using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ProjectManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            frame.Navigate(new MainPage());
        }

        private void projects(object sender, RoutedEventArgs e)
        {
            if (!(frame.Content is Projects))
            {
                frame.Navigate(new Projects());
            }
            else { }
        }

        private void mainpage(object sender, RoutedEventArgs e)
        {
            frame.Navigate(new MainPage());
        }
    }
}