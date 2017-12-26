using System.Windows;
using System.Windows.Controls;

namespace MSiSvInfT
{
    /// <summary>
    /// Логика взаимодействия для WindowSpenMetric.xaml
    /// </summary>
    public partial class WindowSpenMetric : Window
    {
        public WindowSpenMetric()
        {
            InitializeComponent();
            Background = Data.ApplicationBackground;
            ScrollViewer scroll = new ScrollViewer() { Content = SecondTask.GetSpenGrid() };
            DataGrid.Children.Add(scroll);
        }
    }
}
