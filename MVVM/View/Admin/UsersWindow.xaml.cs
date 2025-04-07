using File_Manager.Entities;
using File_Manager.MVVM.View.Admin;
using File_Manager.MVVM.ViewModel;
using Microsoft.EntityFrameworkCore;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace File_Manager
{
    public partial class UsersWindow : Window
    {
        private readonly IT_DepartmentsContext _context;
        private List<EmployeeViewModel> _allUsers;
        private WindowResizer _windowResizer;

        public UsersWindow()
        {
            InitializeComponent();

            var options = new DbContextOptionsBuilder<IT_DepartmentsContext>()
                            .UseSqlServer("Data Source=HoneyPot\\SQLEXPRESS;" +
                                         "Initial Catalog=IT_Departments;Integrated Security=True;MultipleActiveResultSets=True;" +
                                         "TrustServerCertificate=True")
                            .Options;

            _context = new IT_DepartmentsContext(options);
            LoadUsersAsync();
            _windowResizer = new WindowResizer(this);
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

        private async Task ReloadUsersAsync()
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

            UsersListView.ItemsSource = _allUsers;
        }

        private async void LoadUsersAsync()
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

            UsersListView.ItemsSource = _allUsers;
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchQuery = SearchBox.Text.ToLower();
            var filteredUsers = _allUsers
                .Where(user => user.FirstName.ToLower().Contains(searchQuery) ||
                               user.LastName.ToLower().Contains(searchQuery) ||
                               user.Email.ToLower().Contains(searchQuery))
                .ToList();

            UsersListView.ItemsSource = filteredUsers;
        }

        private async void UpdateUserButton_Click(object sender, RoutedEventArgs e)
        {
            if (UsersListView.SelectedItem is EmployeeViewModel selectedUser)
            {
                var userToEdit = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == selectedUser.Email);

                if (userToEdit == null)
                {
                    MessageBox.Show("Пользователь не найден в базе данных.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var updateWindow = new UpdateUserProfile(userToEdit, _context);
                if (updateWindow.ShowDialog() == true)
                {
                    await ReloadUsersAsync();
                }
            }
        }

        private async void DeleteUserButton_Click(object sender, RoutedEventArgs e)
        {
            if (UsersListView.SelectedItem is EmployeeViewModel selectedUser)
            {
                var result = MessageBox.Show($"Вы уверены, что хотите удалить {selectedUser.FirstName} {selectedUser.LastName}?",
                                             "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        var userToDelete = await _context.Users
                            .Include(u => u.Files)
                            .FirstOrDefaultAsync(emp => emp.Email == selectedUser.Email);

                        if (userToDelete != null)
                        {
                            var departmentFiles = await _context.DepartmentFiles
                                .Where(df => df.File.UserId == userToDelete.UserId)
                                .ToListAsync();
                            _context.DepartmentFiles.RemoveRange(departmentFiles);

                            var userFiles = userToDelete.Files.ToList();
                            foreach (var file in userFiles)
                            {
                                _context.Files.Remove(file);
                            }

                            await _context.SaveChangesAsync();

                            _context.Users.Remove(userToDelete);
                            await _context.SaveChangesAsync();

                            _allUsers.Remove(selectedUser);
                            UsersListView.ItemsSource = _allUsers.ToList();
                        }
                        else
                        {
                            MessageBox.Show("Пользователь не найден в базе данных.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    catch (DbUpdateException dbEx)
                    {
                        MessageBox.Show($"Ошибка при удалении: {dbEx.Message}\n{dbEx.InnerException?.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите сотрудника для удаления.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }


        private async void UsersListView_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (UsersListView.SelectedItem is EmployeeViewModel selectedUser)
            {
                var result = MessageBox.Show($"Вы уверены, что хотите удалить {selectedUser.FirstName} {selectedUser.LastName}?",
                                             "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        var userToDelete = await _context.Users
                            .Include(u => u.Files)
                            .FirstOrDefaultAsync(emp => emp.Email == selectedUser.Email);

                        if (userToDelete != null)
                        {
                            // Удаление записей в связующей таблице
                            var departmentFiles = await _context.DepartmentFiles
                                .Where(df => df.File.UserId == userToDelete.UserId)
                                .ToListAsync();
                            _context.DepartmentFiles.RemoveRange(departmentFiles);

                            var userFiles = userToDelete.Files.ToList();
                            foreach (var file in userFiles)
                            {
                                _context.Files.Remove(file);
                            }

                            await _context.SaveChangesAsync();

                            _context.Users.Remove(userToDelete);
                            await _context.SaveChangesAsync();

                            _allUsers.Remove(selectedUser);
                            UsersListView.ItemsSource = _allUsers.ToList();
                        }
                    }
                    catch (DbUpdateException dbEx)
                    {
                        foreach (var entry in dbEx.Entries)
                        {
                            Console.WriteLine($"Entity: {entry.Entity.GetType().Name}, State: {entry.State}");
                        }

                        MessageBox.Show($"Ошибка при удалении: {dbEx.Message}\n{dbEx.InnerException?.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                }
            }
        }

        private void Search_GotFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchBox.Text))
            {
                SearchHint.Visibility = Visibility.Collapsed;
            }
        }

        private void Search_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchBox.Text))
            {
                SearchHint.Visibility = Visibility.Visible;
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double totalWidth = UsersListView.ActualWidth - 25;
            if (totalWidth > 0)
            {
                var gridView = UsersListView.View as GridView;
                if (gridView != null && gridView.Columns.Count == 4)
                {
                    double columnWidth = totalWidth / 4; 
                    gridView.Columns[0].Width = columnWidth;
                    gridView.Columns[1].Width = columnWidth;
                    gridView.Columns[2].Width = columnWidth;
                    gridView.Columns[3].Width = columnWidth;
                }
            }
        }
    }
}
