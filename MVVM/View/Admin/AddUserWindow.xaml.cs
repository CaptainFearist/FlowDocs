using File_Manager.Entities;
using File_Manager.MVVM.Model;
using System.Windows;
using System.Windows.Input;
using Microsoft.Extensions.Configuration;
using System.IO;
using MimeKit;

namespace File_Manager
{
    public partial class AddUserWindow : Window
    {
        private readonly IT_DepartmentsContext _context;
        private readonly EmailSettings _emailSettings;

        public AddUserWindow(IT_DepartmentsContext context)
        {
            InitializeComponent();
            _context = context;

            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("Config/appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            _emailSettings = config.GetSection("EmailSettings").Get<EmailSettings>();
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

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var firstName = FirstNameTextBox.Text;
            var lastName = LastNameTextBox.Text;
            var email = EmailTextBox.Text;
            var username = UsernameTextBox.Text;
            var password = PasswordBox.Password;
            var departmentIdText = DepartmentIdTextBox.Text;

            if (!string.IsNullOrWhiteSpace(firstName) &&
                !string.IsNullOrWhiteSpace(lastName) &&
                !string.IsNullOrWhiteSpace(email) &&
                !string.IsNullOrWhiteSpace(username) &&
                !string.IsNullOrWhiteSpace(password) &&
                int.TryParse(departmentIdText, out int departmentId))
            {
                var newUser = new User
                {
                    FirstName = firstName,
                    LastName = lastName,
                    Email = email,
                    Username = username,
                    Password = password,
                    DepartmentId = departmentId
                };

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                try
                {
                    var message = new MimeMessage();
                    message.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));

                    message.To.Add(new MailboxAddress($"{firstName} {lastName}", email));
                    message.Subject = "Добро пожаловать в систему!";
                    message.Body = new TextPart(MimeKit.Text.TextFormat.Text) { Text = $"Здравствуйте, {firstName}!\n\nВаши учетные данные:\nИмя пользователя: {username}\nПароль: {password}" };

                    using (var client = new MailKit.Net.Smtp.SmtpClient())
                    {
                        client.Connect(_emailSettings.SmtpHost, _emailSettings.SmtpPort, true); // true для SSL
                        client.Authenticate(_emailSettings.SenderEmail.Split('@')[0], _emailSettings.SenderPassword);
                        await client.SendAsync(message);
                        await client.DisconnectAsync(true);
                        MessageBox.Show("Письмо успешно отправлено!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Пользователь добавлен, но не удалось отправить письмо:\n{ex.Message}", "Ошибка отправки", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

                Close();
            }
            else
            {
                MessageBox.Show("Пожалуйста, заполните все поля.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
