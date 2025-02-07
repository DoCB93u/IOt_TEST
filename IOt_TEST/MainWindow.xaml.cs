using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections;
using Microsoft.VisualBasic;

namespace IOt_TEST
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) { this.DragMove(); }
        }

        private void Button_Click_Shutdown(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Button_Click_Minimalize(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void AddNewSensor_Click(object sender, RoutedEventArgs e)
        {
            // Используем InputBox для ввода имени нового датчика
            string sensorName = Interaction.InputBox("Введите имя нового датчика:", "Окно ввода");
            if (!string.IsNullOrEmpty(sensorName))
            {
                // Добавляем новый RadioButton с введенным именем в StackPanel
                RadioButton newRadioButton = new RadioButton
                {
                    MinHeight = 60,
                    Background = new SolidColorBrush(Colors.CornflowerBlue),
                    Content = sensorName,
                    Margin = new Thickness(10),
                    FontSize = 22,
                    Padding = new Thickness(10),
                    Foreground = new SolidColorBrush(Colors.White)
                };
                stackPanel.Children.Add(newRadioButton);
            }
        }
    }
}
