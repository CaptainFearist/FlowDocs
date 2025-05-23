﻿using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using File_Manager.Entities;
using File_Manager.MVVM.ViewModel;
using Microsoft.EntityFrameworkCore;
using System.Windows.Input;

namespace File_Manager.MVVM.View.Technician
{
    /// <summary>
    /// Логика взаимодействия для DepartmentUsersWindow.xaml
    /// </summary>
    public partial class DepartmentUsersWindow : Window
    {
        private readonly IT_DepartmentsContext _context;
        public ObservableCollection<DepartmentUsersViewModel> DeptUsers { get; set; }
        private readonly string _departmentName;
        private WindowResizer _windowResizer;

        public DepartmentUsersWindow(string departmentName)
        {
            var options = new DbContextOptionsBuilder<IT_DepartmentsContext>()
                .UseSqlServer("Data Source=HoneyPot\\SQLEXPRESS;" +
                              "Initial Catalog=IT_Departments;Integrated Security=True;MultipleActiveResultSets=True;" +
                              "TrustServerCertificate=True")
                .Options;

            _context = new IT_DepartmentsContext(options);
            InitializeComponent();

            _departmentName = departmentName;
            DeptUsers = new ObservableCollection<DepartmentUsersViewModel>();
            DepartmentsUsersListView.ItemsSource = DeptUsers;

            DepartmentNameTextBlock.Text = $"Сотрудники отдела: {departmentName}";

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

        private async void LoadUsersAsync()
        {
            var departmentUsers = await _context.Users
                .Include(u => u.Department)
                .Where(u => u.Department.DepartmentName == _departmentName)
                .Select(u => new DepartmentUsersViewModel
                {
                    FirstName = u.FirstName,
                    LastName = u.LastName
                })
                .ToListAsync();

            DeptUsers.Clear();
            foreach (var user in departmentUsers)
            {
                DeptUsers.Add(user);
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searhQuery = SearchBox.Text.ToLower();

            var filteredUsers = DeptUsers
                .Where(u => u.FirstName.ToLower().Contains(searhQuery) ||
                            u.LastName.ToLower().Contains(searhQuery))
                .ToList();

            DepartmentsUsersListView.ItemsSource = filteredUsers;
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
            double totalWidth = DepartmentsUsersListView.ActualWidth - 25;
            if (totalWidth > 0)
            {
                var gridView = DepartmentsUsersListView.View as GridView;
                if (gridView != null && gridView.Columns.Count == 2)
                {
                    double columnWidth = totalWidth / 2;
                    gridView.Columns[0].Width = columnWidth;
                    gridView.Columns[1].Width = columnWidth;
                }
            }
        }
    }
}
