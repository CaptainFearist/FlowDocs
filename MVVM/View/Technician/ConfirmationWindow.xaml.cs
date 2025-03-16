using System.Windows;

namespace File_Manager.MVVM.View.Technician
{
    /// <summary>
    /// Логика взаимодействия для ConfirmationWindow.xaml
    /// </summary>
    public partial class ConfirmationWindow : Window
    {
        public bool IsConfirmed { get; private set; } = false;

        public ConfirmationWindow()
        {
            InitializeComponent();
        }

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            IsConfirmed = true;
            this.Close();
            MainWindow mainWin = new MainWindow();
            mainWin.Show();

            Application.Current.Windows
                .OfType<Window>()
                .Where(w => !(w is MainWindow))
                .ToList()
                .ForEach(w => w.Close());
        }

        private void NoButton_Click(Object sender, RoutedEventArgs e)
        {
            IsConfirmed = false;
            this.Close();
        }
    }
}
