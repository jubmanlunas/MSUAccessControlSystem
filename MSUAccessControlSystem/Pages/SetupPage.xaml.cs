using System;
using System.IO.Ports;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MSUAccessControlSystem
{
    /// <summary>
    /// Interaction logic for SetupPage.xaml
    /// </summary>
    public partial class SetupPage : BasePage
    {
        private string _selectedComPort = "COM7";

        public SetupPage()
        {
            InitializeComponent();

            // Populate COM port dropdown with available ports
            LoadComPorts();

            // Set the selected port from settings if available
            _selectedComPort = Properties.Settings.Default.ComPort;
        }

        /// <summary>
        /// Loads available COM ports into the combo box
        /// </summary>
        private void LoadComPorts()
        {
            try
            {
                var ports = SerialPort.GetPortNames();
                ComPortComboBox.ItemsSource = ports;

                if (ports.Contains(_selectedComPort))
                {
                    ComPortComboBox.SelectedItem = _selectedComPort;
                }
                else if (ports.Any())
                {
                    ComPortComboBox.SelectedIndex = 0;
                    _selectedComPort = ports[0];
                }
                else
                {
                    // No ports available
                    MessageBox.Show("No COM ports detected. Please connect the flap barrier device.",
                                  "COM Port Error",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading COM ports: {ex.Message}",
                              "COM Port Error",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Updates selected COM port when selection changes
        /// </summary>
        private void ComPortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComPortComboBox.SelectedItem != null)
            {
                _selectedComPort = ComPortComboBox.SelectedItem.ToString();
            }
        }

        /// <summary>
        /// Navigates to Users Database page
        /// </summary>
        private void UsersButton_Click(object sender, RoutedEventArgs e)
        {
            // Navigate to Users Database page
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.Navigate(new UsersPage());
            }
        }

        /// <summary>
        /// Navigates to Administrators page
        /// </summary>
        private void AdminsButton_Click(object sender, RoutedEventArgs e)
        {
            // Navigate to Administrators page
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.Navigate(new AdminsPage());
            }
        }

        /// <summary>
        /// Saves the current settings
        /// </summary>
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Save COM port setting
                Properties.Settings.Default.ComPort = _selectedComPort;
                Properties.Settings.Default.Save();

                // Update global settings
                Settings.ComPort = _selectedComPort;

                // Reconnect the Arduino if needed
                var mainWindow = Window.GetWindow(this) as MainWindow;
                if (mainWindow != null)
                {
                    mainWindow.ReconnectArduino(_selectedComPort);

                    MessageBox.Show("Settings saved successfully.",
                                  "Save Settings",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving settings: {ex.Message}",
                              "Settings Error",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Refreshes the list of available COM ports
        /// </summary>
        private void RefreshPortsButton_Click(object sender, RoutedEventArgs e)
        {
            LoadComPorts();
        }

        protected override void OnNavigatedTo(object parameter = null)
        {
            base.OnNavigatedTo(parameter);

            // Refresh COM port list when navigating to this page
            LoadComPorts();
        }
    }
}