using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections;
using Microsoft.VisualBasic;
using System.Timers;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Axes;
using OxyPlot.Annotations;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Reflection;

namespace IOt_TEST
{
    public partial class MainWindow : Window
    {
        // Добавляем поля для ThingsBoard
        private ThingsBoardClient _thingsboardClient;
        private readonly string _deviceId = Config.GetDeviceID(); //
        private readonly string _telemetryKey = Config.GetTelemetryKey();
        private bool _isConnected = false;

        // Остальные существующие поля оставляем без изменений...
        private LineSeries _lineSeries;
        private LineSeries _lineSeriesMax;
        private LineSeries _lineSeriesMin;
        private Timer _timer;
        private int _index = 0;
        private readonly int _indexIndMax = 0;
        private readonly int _indexIndMin = 0;
        private Random _random;
        private readonly bool MaximumStatus = false;
        private readonly bool MinimumStatus = false;
        private float Maximum;
        private float Minimum;
        private LineAnnotation _maximumLine;
        private LineAnnotation _minimumLine;

        public MainWindow()
        {
            InitializeComponent();
            InitializeThingsBoard();
            InitializePlot();
            StartTimer();
        }

        private void InitializePlot()
        {
            var plotModel = new PlotModel { Title = "" };

            // Create axes and set their properties to disable zoom and enable horizontal pan
            var xAxis = new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Minimum = 0,
                Maximum = 100,
                IsZoomEnabled = false,
                IsPanEnabled = true
            };

            var yAxis = new LinearAxis
            {
                Position = AxisPosition.Left,
                Minimum = -15,
                Maximum = 100,
                IsZoomEnabled = false,
                IsPanEnabled = false
            };

            // Add axes to the plot model
            plotModel.Axes.Add(xAxis);
            plotModel.Axes.Add(yAxis);

            // Create the line series
            _lineSeries = new LineSeries
            {
                Color = OxyColors.CornflowerBlue
            };
            plotModel.Series.Add(_lineSeries);

            _lineSeriesMax = new LineSeries
            {
                Color = OxyColors.Red
            };
            plotModel.Series.Add(_lineSeriesMax);

            _lineSeriesMin = new LineSeries
            {
                Color = OxyColors.Green
            };
            plotModel.Series.Add(_lineSeriesMin);

            plotView.Model = plotModel;
            _random = new Random();
        }

        private void StartTimer()
        {
            _timer = new Timer(1000); // Обновление каждые 1000 мс (1 секунда)
            _timer.Elapsed += UpdatePlot;
            _timer.Start();
        }

        private async void InitializeThingsBoard()
        {
            try
            {
                _thingsboardClient = new ThingsBoardClient(Config.GetHost()); //хост
                await _thingsboardClient.AuthenticateAsync("tenant@thingsboard.org", "tenant"); //логин и пароль
                _isConnected = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to connect to ThingsBoard: {ex.Message}",
                              "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
                _isConnected = false;
            }
        }

        // Модифицируем метод UpdatePlot
        private async void UpdatePlot(object sender, ElapsedEventArgs e)
        {
            if (!_isConnected) return;

            try
            {
                var value = await _thingsboardClient.GetLatestTelemetryValueAsync(_deviceId, _telemetryKey);

                Dispatcher.Invoke(() =>
                {
                    _lineSeries.Points.Add(new DataPoint(_index += 6, value));

                    if (_maximumLine != null && value > Maximum)
                    {
                        MessageBox.Show("Достигнуты МАКСИМАЛЬНЫЕ критические значения!");
                    }

                    if (_minimumLine != null && value < Minimum)
                    {
                        MessageBox.Show("Достигнуты МИНИМАЛЬНЫЕ критические значения!");
                    }

                    plotView.InvalidatePlot(true);
                });
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    MessageBox.Show($"Error getting data: {ex.Message}",
                                  "Data Error", MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
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

        private void AddMaximumLine(double yPosition)
        {
            _maximumLine = new LineAnnotation
            {
                Type = LineAnnotationType.Horizontal,
                Y = yPosition,
                Color = OxyColors.Red,
                LineStyle = LineStyle.Solid,
                Text = $"Y = {yPosition}",
                TextColor = OxyColors.Red,
                TextHorizontalAlignment = OxyPlot.HorizontalAlignment.Center
            };
            plotView.Model.Annotations.Add(_maximumLine);
            plotView.InvalidatePlot(true);
        }

        private void AddMinimumLine(double yPosition)
        {
            _minimumLine = new LineAnnotation
            {
                Type = LineAnnotationType.Horizontal,
                Y = yPosition,
                Color = OxyColors.Green,
                LineStyle = LineStyle.Solid,
                Text = $"Y = {yPosition}",
                TextColor = OxyColors.Green,
                TextHorizontalAlignment = OxyPlot.HorizontalAlignment.Center
            };
            plotView.Model.Annotations.Add(_minimumLine);
            plotView.InvalidatePlot(true);
        }

        private void RemoveHorizontalLine()
        {
            if (_maximumLine != null)
            {
                plotView.Model.Annotations.Remove(_maximumLine);
                plotView.InvalidatePlot(true);
                _maximumLine = null;
            }

            if (_minimumLine != null)
            {
                plotView.Model.Annotations.Remove(_minimumLine);
                plotView.InvalidatePlot(true);
                _minimumLine = null;
            }
        }


        private void AddMinimum_Click(object sender, RoutedEventArgs e)
        {
            if (_minimumLine != null)
            {
                RemoveHorizontalLine();
            }

            string MinimumStr = Interaction.InputBox("Введите минимум (для дробных используйте запятую)", "Окно ввода");

            float.TryParse(MinimumStr, out Minimum);

            AddMinimumLine(Minimum);
        }

        private void AddMaximum_Click(object sender, RoutedEventArgs e)
        {
            if (_maximumLine != null)
            {
                RemoveHorizontalLine();
            }

            string MaximumStr = Interaction.InputBox("Введите максимум (для дробных используйте запятую)", "Окно ввода");

            float.TryParse(MaximumStr, out Maximum);

            AddMaximumLine(Maximum);
        }

    }
}

