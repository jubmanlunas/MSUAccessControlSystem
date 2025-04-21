using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace MSUAccessControlSystem
{
    /// <summary>
    /// Interaction logic for DashboardPage.xaml
    /// </summary>
    public partial class DashboardPage : BasePage
    {
        private DispatcherTimer _refreshTimer;

        public DashboardPage()
        {
            InitializeComponent();
            DataContext = DataContext.Instance;

            // Set up a timer to refresh the dashboard every 10 seconds
            _refreshTimer = new DispatcherTimer();
            _refreshTimer.Interval = TimeSpan.FromSeconds(10);
            _refreshTimer.Tick += RefreshTimer_Tick;
            _refreshTimer.Start();

            // Subscribe to access logged events to update the recent logs
            DataContext.Instance.AccessLogged += DataContext_AccessLogged;

            // Load initial stats
            UpdateStats();
        }

        private void DataContext_AccessLogged(object sender, AccessLogEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                UpdateStats();
            });
        }

        private void RefreshTimer_Tick(object sender, EventArgs e)
        {
            UpdateStats();
        }

        private void UpdateStats()
        {
            try
            {
                var today = DateTime.Now.Date;
                var logs = DataContext.Instance.AccessLogs.Where(l => l.TimeStamp.Date == today).ToList();

                // Update counts
                int studentCount = logs.Count(l => l.UserType == "Student" && l.EventType == "ENTRY");
                int employeeCount = logs.Count(l => l.UserType == "Employee" && l.EventType == "ENTRY");
                int visitorCount = logs.Count(l => l.UserType == "Visitor" && l.EventType == "ENTRY");

                StudentCountText.Text = studentCount.ToString();
                EmployeeCountText.Text = employeeCount.ToString();
                VisitorCountText.Text = visitorCount.ToString();

                // Update currently inside count
                InsideCountText.Text = DataContext.Instance.CurrentlyInside.Count.ToString();

                // Update recent logs listview
                var recentLogs = DataContext.Instance.AccessLogs
                    .OrderByDescending(l => l.TimeStamp)
                    .Take(10)
                    .ToList();

                RecentLogsListView.ItemsSource = recentLogs;
            }
            catch (Exception ex)
            {
                // Log the error but don't crash the dashboard
                Console.WriteLine($"Error updating dashboard stats: {ex.Message}");
            }
        }

        protected override void OnNavigatingFrom()
        {
            base.OnNavigatingFrom();

            // Clean up resources when navigating away
            _refreshTimer.Stop();
            DataContext.Instance.AccessLogged -= DataContext_AccessLogged;
        }

        protected override void OnNavigatedTo(object parameter = null)
        {
            base.OnNavigatedTo(parameter);

            // Refresh stats when navigating to this page
            UpdateStats();
            _refreshTimer.Start();
        }
    }
}