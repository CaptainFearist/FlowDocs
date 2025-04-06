using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using File_Manager.Entities;
using File_Manager.MVVM.ViewModel;
using File_Manager.MVVM.Model;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;

namespace File_Manager.MVVM.View.Messenger
{
    public partial class ChatsWindow : Window
    {
        private readonly IT_DepartmentsContext _context;
        private List<EmployeeViewModel> _allUsers = new List<EmployeeViewModel>();

        private ObservableCollection<ChatModel> _chats = new ObservableCollection<ChatModel>();

        public ChatsWindow()
        {
            InitializeComponent();
            var options = new DbContextOptionsBuilder<IT_DepartmentsContext>()
                .UseSqlServer("Data Source=HoneyPot\\SQLEXPRESS;" +
                              "Initial Catalog=IT_Departments;Integrated Security=True;MultipleActiveResultSets=True;" +
                              "TrustServerCertificate=True")
                .Options;

            _context = new IT_DepartmentsContext(options);
            LoadUsersAsync();

            ChatsListView.ItemsSource = _chats;
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        private void MinimizeWindow(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void MaximizeRestoreWindow(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
            }
            else
            {
                WindowState = WindowState.Maximized;
            }
        }

        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public class Chat
        {
            public string Name { get; set; }
        }

        private async void LoadUsersAsync()
        {
            try
            {
                _allUsers = await _context.Users
                    .Select(u => new EmployeeViewModel
                    {
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                    }).ToListAsync();

                // _chats.Clear();
                // ChatsListView.ItemsSource = _chats;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке пользователей: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                _allUsers = new List<EmployeeViewModel>();
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchQuery = SearchChatsBox.Text?.ToLower() ?? "";

            if (string.IsNullOrWhiteSpace(searchQuery))
            {
                _chats.Clear();
                ChatsListView.ItemsSource = _chats;
            }
            else
            {
                var filteredUsers = _allUsers
                    .Where(user => (user.FirstName + " " + user.LastName).ToLower().Contains(searchQuery))
                    .Select(u => new ChatModel
                    {
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        ImagePath = _context.Users.FirstOrDefault(dbUser => dbUser.FirstName == u.FirstName && dbUser.LastName == u.LastName)?.ImagePath
                    })
                    .ToList();

                _chats.Clear();
                foreach (var user in filteredUsers)
                {
                    _chats.Add(user);
                }
                ChatsListView.ItemsSource = _chats;
            }
        }

        private void ChatsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ChatsListView.SelectedItem is ChatModel selectedChat)
            {
                ChatTitle.Text = $"{selectedChat.FirstName} {selectedChat.LastName}";
            }
        }

        private void MessageInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SendMessage_Click(sender, e);
            }
        }

        private void SendMessage_Click(object sender, RoutedEventArgs e)
        {
            string message = MessageInput.Text;
            if (!string.IsNullOrEmpty(message))
            {
                Console.WriteLine("Отправлено сообщение: " + message);
                MessageInput.Clear();
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

        private void Search_GotFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchChatsBox.Text))
            {
                SearchHint.Visibility = Visibility.Collapsed;
            }
        }

        private void Search_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchChatsBox.Text))
            {
                SearchHint.Visibility = Visibility.Visible;
                SearchChatsBox.Text = string.Empty;
            }
        }
    }
}