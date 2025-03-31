using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using File_Manager.Entities;
using File_Manager.MVVM.ViewModel;
using Microsoft.EntityFrameworkCore;

namespace File_Manager.MVVM.View.Messenger
{
    public partial class ChatsWindow : Window
    {
        private readonly IT_DepartmentsContext _context;
        private List<EmployeeViewModel> _allUsers;

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
                    .Select(emp => new EmployeeViewModel
                    {
                        FirstName = emp.FirstName,
                        LastName = emp.LastName,
                        Email = emp.Email,
                        DepartmentName = emp.Department.DepartmentName
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке пользователей: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                _allUsers = new List<EmployeeViewModel>(); // Инициализация пустого списка в случае ошибки
            }
        }

        private void ChatsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ChatsListView.SelectedItem != null)
            {
                if (ChatsListView.SelectedItem is Chat selectedChat)
                {
                    ChatTitle.Text = selectedChat.Name;
                }
                else
                {
                    ChatTitle.Text = "Выбран чат";
                }

                MessagesList.ItemsSource = null;
                Console.WriteLine("Выбран чат: " + ChatsListView.SelectedItem);
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

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchQuery = "";
            if (SearchChatsBox.Text != null)
            {
                searchQuery = SearchChatsBox.Text.ToLower();
            }

            if (_allUsers != null)
            {
                var filteredUsers = _allUsers
                    .Where(user => user != null &&
                                   (user.FirstName != null && user.FirstName.ToLower().Contains(searchQuery) ||
                                    user.LastName != null && user.LastName.ToLower().Contains(searchQuery)))
                    .ToList();

                var displayUsers = filteredUsers
                    .Select(user => new { Name = $"{user.FirstName} {user.LastName}" })
                    .ToList();

                ChatsListView.ItemsSource = displayUsers;
            }
            else
            {
                ChatsListView.ItemsSource = new List<object>();
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