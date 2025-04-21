using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;

namespace MSUAccessControlSystem
{
    /// <summary>
    /// Interaction logic for AdminsPage.xaml
    /// </summary>
    public partial class AdminsPage : BasePage
    {
        public AdminsPage()
        {
            InitializeComponent();

            // Set data context and load administrators
            DataContext = DataContext.Instance;
            RefreshAdminsList();
        }

        /// <summary>
        /// Refreshes the administrators list from the database
        /// </summary>
        private void RefreshAdminsList()
        {
            // Display admins sorted by username
            AdminsListView.ItemsSource = DataContext.Instance.Administrators
                .OrderBy(a => a.Username)
                .ToList();
        }

        /// <summary>
        /// Opens the Add Administrator window
        /// </summary>
        private void AddAdminButton_Click(object sender, RoutedEventArgs e)
        {
            var addAdminWindow = new AddAdminWindow();

            // If dialog returns true, admin was added
            if (addAdminWindow.ShowDialog() == true)
            {
                // Refresh list after adding an admin
                RefreshAdminsList();
            }
        }

        /// <summary>
        /// Opens the Edit Administrator window for the selected admin
        /// </summary>
        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var admin = button?.DataContext as Administrator;

            if (admin != null)
            {
                var editAdminWindow = new EditAdminWindow(admin);

                // If dialog returns true, admin was edited
                if (editAdminWindow.ShowDialog() == true)
                {
                    // Refresh list after editing
                    RefreshAdminsList();
                }
            }
            else if (AdminsListView.SelectedItem is Administrator selectedAdmin)
            {
                // Alternative selection method if button's DataContext is not set
                var editAdminWindow = new EditAdminWindow(selectedAdmin);

                if (editAdminWindow.ShowDialog() == true)
                {
                    RefreshAdminsList();
                }
            }
            else
            {
                MessageBox.Show("Please select an administrator to edit.",
                             "Edit Administrator",
                             MessageBoxButton.OK,
                             MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Deletes the selected administrator
        /// </summary>
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var admin = button?.DataContext as Administrator;

            if (admin == null && AdminsListView.SelectedItem is Administrator selectedAdmin)
            {
                // Alternative selection method if button's DataContext is not set
                admin = selectedAdmin;
            }

            if (admin != null)
            {
                // Don't allow deleting the last Super Admin
                if (admin.Role == "Super Admin" &&
                    DataContext.Instance.Administrators.Count(a => a.Role == "Super Admin") <= 1)
                {
                    MessageBox.Show("Cannot delete the last Super Admin account.",
                                    "Delete Error",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                    return;
                }

                var result = MessageBox.Show($"Are you sure you want to delete {admin.Username}?",
                                           "Confirm Delete",
                                           MessageBoxButton.YesNo,
                                           MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    // Delete admin from database
                    try
                    {
                        using (var connection = DatabaseManager.GetConnection())
                        {
                            connection.Open();

                            string query = "DELETE FROM Administrators WHERE Username = @username";
                            var cmd = new MySqlCommand(query, connection);
                            cmd.Parameters.AddWithValue("@username", admin.Username);

                            cmd.ExecuteNonQuery();

                            // Remove from in-memory collection
                            DataContext.Instance.Administrators.Remove(admin);

                            // Refresh list after delete
                            RefreshAdminsList();

                            MessageBox.Show("Administrator deleted successfully.",
                                          "Delete Successful",
                                          MessageBoxButton.OK,
                                          MessageBoxImage.Information);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting administrator: {ex.Message}",
                                       "Delete Error",
                                       MessageBoxButton.OK,
                                       MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select an administrator to delete.",
                              "Delete Administrator",
                              MessageBoxButton.OK,
                              MessageBoxImage.Information);
            }
        }

        protected override void OnNavigatedTo(object parameter = null)
        {
            base.OnNavigatedTo(parameter);

            // Refresh the list when navigating to this page
            RefreshAdminsList();
        }
    }
}