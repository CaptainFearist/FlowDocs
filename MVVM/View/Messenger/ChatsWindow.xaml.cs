﻿using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using File_Manager.Entities;
using File_Manager.MVVM.ViewModel;
using File_Manager.MVVM.Model;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Microsoft.Win32;

namespace File_Manager.MVVM.View.Messenger
{
    public partial class ChatsWindow : Window
    {
        private readonly IT_DepartmentsContext _context;
        private User _currentUser;
        private List<EmployeeViewModel> _allUsers = new List<EmployeeViewModel>();
        private ObservableCollection<ChatModel> _chats = new ObservableCollection<ChatModel>();
        private ObservableCollection<FileViewModel> _currentAttachments = new ObservableCollection<FileViewModel>();
        private ChatModel _selectedChat;
        private bool isLoadingMessages = false;

        public ChatsWindow(User currentUser)
        {
            InitializeComponent();
            var options = new DbContextOptionsBuilder<IT_DepartmentsContext>()
                .UseSqlServer("Data Source=HoneyPot\\SQLEXPRESS;" +
                              "Initial Catalog=IT_Departments;Integrated Security=True;MultipleActiveResultSets=True;" +
                              "TrustServerCertificate=True")
                .Options;

            _currentUser = currentUser;
            _context = new IT_DepartmentsContext(options);
            LoadUsersAsync();
            ChatsListView.ItemsSource = _chats;
            MessagesList.ItemsSource = null;
            AttachedFilesListBox.ItemsSource = _currentAttachments;
        }

        private static readonly Dictionary<string, int> FileTypeMappings = new Dictionary<string, int>
        {
            { ".doc", 1 }, { ".docx", 1 }, { ".pdf", 1 }, { ".txt", 1 }, { ".rtf", 1 },
            { ".xls", 1 }, { ".xlsx", 1 }, { ".ppt", 1 }, { ".pptx", 1 },
            { ".jpg", 2 }, { ".jpeg", 2 }, { ".png", 2 }, { ".gif", 2 },
            { ".bmp", 2 }, { ".tiff", 2 }, { ".mp4", 3 }
        };

        private async void LoadUsersAsync()
        {
            try
            {
                _allUsers = await _context.Users
                    .Where(u => u.UserId != _currentUser.UserId)
                    .Select(u => new EmployeeViewModel
                    {
                        UserId = u.UserId,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        ImagePath = u.ImagePath
                    }).ToListAsync();

                _chats.Clear();
                foreach (var user in _allUsers)
                {
                    _chats.Add(new ChatModel
                    {
                        ChatId = user.UserId,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        ImagePath = user.ImagePath,
                        Messages = new ObservableCollection<MessageModel>()
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке пользователей: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Debug.WriteLine($"LoadUsersAsync ERROR: {ex.ToString()}");
                _allUsers = new List<EmployeeViewModel>();
            }
        }

        private async void ChatsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ChatsListView.SelectedItem is ChatModel selectedUserEntry)
            {
                int otherUserId = selectedUserEntry.ChatId;

                try
                {
                    int actualChatId = await GetOrCreateChatAsync(otherUserId);

                    if (actualChatId > 0)
                    {
                        _selectedChat = selectedUserEntry;
                        _selectedChat.ChatId = actualChatId;

                        _selectedChat.Messages.Clear();
                        MessagesList.ItemsSource = _selectedChat.Messages;

                        ChatTitle.Text = $"{_selectedChat.FirstName} {_selectedChat.LastName}";

                        await LoadMessagesForChat(actualChatId);
                    }
                    else
                    {
                        _selectedChat = null;
                        MessagesList.ItemsSource = null;
                        ChatTitle.Text = "Выберите чат";
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при выборе чата: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    Debug.WriteLine($"ChatsListView_SelectionChanged ОШИБКА: {ex.ToString()}");
                    _selectedChat = null;
                    MessagesList.ItemsSource = null;
                    ChatTitle.Text = "Выберите чат";
                }
            }
            else
            {
                _selectedChat = null;
                MessagesList.ItemsSource = null;
                ChatTitle.Text = "Выберите чат";
            }
        }

        private async Task<int> GetOrCreateChatAsync(int otherUserId)
        {
            var existingChat = await _context.Chats
                .Include(c => c.ChatParticipants)
                .Where(c => c.ChatParticipants.Any(p => p.UserId == _currentUser.UserId) &&
                            c.ChatParticipants.Any(p => p.UserId == otherUserId) &&
                            c.ChatParticipants.Count == 2)
                .FirstOrDefaultAsync();

            if (existingChat != null)
            {
                Debug.WriteLine($"Найден существующий чат. ChatID: {existingChat.ChatId}");
                return existingChat.ChatId;
            }
            else
            {
                Debug.WriteLine($"Существующий чат не найден. Создание нового чата между пользователем {_currentUser.UserId} и пользователем {otherUserId}.");
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var newChat = new Chat
                    {
                        ChatName = $"Чат между {_currentUser.UserId} и {otherUserId}",
                        CreatedDate = DateTime.Now
                    };
                    _context.Chats.Add(newChat);
                    await _context.SaveChangesAsync();

                    int newChatId = newChat.ChatId;
                    Debug.WriteLine($"Создана новая сущность Chat. ChatID: {newChatId}");

                    var participant1 = new ChatParticipant { ChatId = newChatId, UserId = _currentUser.UserId };
                    var participant2 = new ChatParticipant { ChatId = newChatId, UserId = otherUserId };
                    _context.ChatParticipants.AddRange(participant1, participant2);
                    await _context.SaveChangesAsync();
                    Debug.WriteLine($"Добавление участников в ChatID: {newChatId}");

                    await transaction.CommitAsync();
                    return newChatId;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Debug.WriteLine($"ОШИБКА при создании чата: {ex.ToString()}");
                    MessageBox.Show($"Не удалось создать запись чата в базе данных: {ex.Message}", "Ошибка БД", MessageBoxButton.OK, MessageBoxImage.Error);
                    return 0;
                }
            }
        }

        private void MessagesList_Loaded(object sender, RoutedEventArgs e)
        {
            if (_selectedChat != null && _selectedChat.Messages.Count > 0)
            {
                MessagesList.ScrollIntoView(_selectedChat.Messages.Last());
            }
        }

        private async Task LoadMessagesForChat(int chatId)
        {
            if (isLoadingMessages) return;
            isLoadingMessages = true;

            try
            {
                var messages = await _context.Messages
                    .Where(m => m.ChatId == chatId)
                    .OrderBy(m => m.SentDate)
                    .Include(m => m.Sender)
                    .Include(m => m.ChatAttachments)
                        .ThenInclude(ca => ca.File) // Включаем связанный File
                    .ToListAsync();

                var messageModels = messages.Select(m => new MessageModel
                {
                    SenderName = $"{m.Sender.FirstName} {m.Sender.LastName}",
                    Content = m.Content,
                    SentDate = m.SentDate,
                    IsSenderCurrentUser = m.SenderId == _currentUser.UserId,
                    Attachment = m.ChatAttachments.Count > 0 && m.ChatAttachments.First().File != null ? new AttachedFileInMessage
                    {
                        AttachmentId = m.ChatAttachments.First().ChatAttachmentId,
                        FileName = m.ChatAttachments.First().File.FileName,
                        FilePath = m.ChatAttachments.First().File.FilePath,
                        FileId = m.ChatAttachments.First().File.FileId
                    } : null
                }).ToList();

                _selectedChat.Messages.Clear();
                foreach (var message in messageModels)
                {
                    _selectedChat.Messages.Add(message);
                }

                MessagesList.ItemsSource = null;
                MessagesList.ItemsSource = _selectedChat.Messages;

                if (_selectedChat.Messages.Count > 0)
                {
                    MessagesList.ScrollIntoView(_selectedChat.Messages.Last());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при загрузке сообщений: {ex.Message}");
            }
            finally
            {
                isLoadingMessages = false;
            }
        }

        private async void SendMessage_Click(object sender, RoutedEventArgs e)
        {
            string message = MessageInput.Text;

            if (!string.IsNullOrEmpty(message) && _selectedChat != null)
            {
                var currentUser = _context.Users.FirstOrDefault(u => u.UserId == _currentUser.UserId);

                if (currentUser != null)
                {
                    var newMessage = new Message
                    {
                        SenderId = currentUser.UserId,
                        ChatId = _selectedChat.ChatId,
                        Content = message,
                        SentDate = DateTime.Now
                    };

                    _context.Messages.Add(newMessage);
                    await _context.SaveChangesAsync();

                    foreach (var attachmentViewModel in _currentAttachments)
                    {
                        string fileExtension = System.IO.Path.GetExtension(attachmentViewModel.FileName).ToLowerInvariant();
                        int? fileTypeId = FileTypeMappings.TryGetValue(fileExtension, out var typeId) ? typeId : null;

                        if (!fileTypeId.HasValue)
                        {
                            MessageBox.Show($"Не удалось определить тип файла для '{attachmentViewModel.FileName}'.");
                            continue; // Пропустить этот файл
                        }

                        var newFile = await _context.Files.FirstOrDefaultAsync(f => f.FilePath == attachmentViewModel.FilePath && f.FileName == attachmentViewModel.FileName && f.UserId == _currentUser.UserId && f.FileTypeId == fileTypeId.Value);

                        if (newFile == null)
                        {
                            newFile = new File
                            {
                                FileName = attachmentViewModel.FileName,
                                FilePath = attachmentViewModel.FilePath,
                                UploadDate = DateTime.Now,
                                UserId = _currentUser.UserId,
                                FileTypeId = fileTypeId.Value
                            };
                            _context.Files.Add(newFile);
                            await _context.SaveChangesAsync();
                        }

                        var newChatAttachment = new ChatAttachment
                        {
                            MessageId = newMessage.MessageId,
                            FileId = newFile.FileId
                        };
                        _context.ChatAttachments.Add(newChatAttachment);
                    }
                    await _context.SaveChangesAsync();

                    LoadMessagesForChat(_selectedChat.ChatId);
                    MessageInput.Clear();
                }
                else
                {
                    MessageBox.Show("Не удалось найти пользователя для отправки сообщения.");
                }
            }
            _currentAttachments.Clear();
        }

        private void AttachFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                _currentAttachments.Add(new FileViewModel
                {
                    FileName = System.IO.Path.GetFileName(openFileDialog.FileName),
                    FilePath = openFileDialog.FileName
                });
            }
        }

        private void RemoveAttachment_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.CommandParameter is FileViewModel fileToRemove)
            {
                _currentAttachments.Remove(fileToRemove);
            }
        }

        private void DownloadFileTextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBlock textBlock && textBlock.DataContext is MessageModel message && message.Attachment != null)
            {
                string filePath = message.Attachment.FilePath;
                try
                {
                    Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Не удалось открыть файл: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void MessageInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(MessageInput.Text))
            {
                MessageHint.Visibility = Visibility.Visible;
            }
            else
            {
                MessageHint.Visibility = Visibility.Collapsed;
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchQuery = SearchChatsBox.Text?.ToLower() ?? "";

            var filteredUserModels = _allUsers
                     .Where(user => (user.FirstName + " " + user.LastName).ToLower().Contains(searchQuery))
                     .Select(u => new ChatModel
                     {
                         ChatId = u.UserId,
                         FirstName = u.FirstName,
                         LastName = u.LastName,
                         ImagePath = u.ImagePath,
                         Messages = new ObservableCollection<MessageModel>()
                     }).ToList();

            _chats.Clear();
            foreach (var userModel in filteredUserModels)
            {
                _chats.Add(userModel);
            }

            if (string.IsNullOrWhiteSpace(searchQuery))
            {
                _chats.Clear();
                foreach (var user in _allUsers)
                {
                    _chats.Add(new ChatModel
                    {
                        ChatId = user.UserId,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        ImagePath = user.ImagePath,
                        Messages = new ObservableCollection<MessageModel>()
                    });
                }
            }
        }

        private void Search_GotFocus(object sender, RoutedEventArgs e)
        {
            SearchHint.Visibility = Visibility.Collapsed;
        }

        private void Search_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchChatsBox.Text))
            {
                SearchHint.Visibility = Visibility.Visible;
            }
        }

        private void MessageInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !string.IsNullOrWhiteSpace(MessageInput.Text))
            {
                SendMessage_Click(sender, e);
            }
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void MinimizeWindow(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void MaximizeRestoreWindow(object sender, RoutedEventArgs e)
        {
            WindowState = (WindowState == WindowState.Maximized) ? WindowState.Normal : WindowState.Maximized;
        }

        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}