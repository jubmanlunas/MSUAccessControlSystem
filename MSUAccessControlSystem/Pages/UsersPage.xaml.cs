using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;

namespace MSUAccessControlSystem
{
    /// <summary>
    /// Interaction logic for UsersPage.xaml
    /// </summary>
    public partial class UsersPage : BasePage
    {
        public object DataContext { get; }

        public UsersPage()
        {
            InitializeComponent();

            // Set data context and load users
            DataContext = DataContext.Instance;
            RefreshUsersList();
        }

        /// <summary>
        /// Refreshes the users list from the database
        /// </summary>
        private void RefreshUsersList()
        {
            // Display users sorted by ID
            UsersListView.ItemsSource = DataContext.Instance.Users
                .OrderBy(u => u.IDNumber)
                .ToList();
        }

        /// <summary>
        /// Opens the Add User window
        /// </summary>
        private void AddUserButton_Click(object sender, RoutedEventArgs e)
        {
            var addUserWindow = new AddUserWindow();

            // If dialog returns true, user was added
            if (addUserWindow.ShowDialog() == true)
            {
                // Refresh list after adding a user
                RefreshUsersList();
            }
        }

        /// <summary>
        /// Opens a file dialog for importing users from CSV/Excel
        /// </summary>
        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            // Open file dialog for CSV/Excel import
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "CSV Files (*.csv)|*.csv|Excel Files (*.xlsx)|*.xlsx|All Files (*.*)|*.*",
                Title = "Import Users"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    // Import users from file
                    ImportUsers(openFileDialog.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error importing users: {ex.Message}",
                                  "Import Error",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Imports users from a CSV file
        /// </summary>
        private void ImportUsers(string filePath)
        {
            // Simple CSV import implementation (can be expanded for Excel)
            if (Path.GetExtension(filePath).ToLower() == ".csv")
            {
                var lines = File.ReadAllLines(filePath);

                if (lines.Length > 1) // Has header row
                {
                    // Show progress dialog
                    var progressWindow = new ProgressWindow("Importing Users", lines.Length - 1);
                    progressWindow.Show();

                    int successCount = 0;
                    int failCount = 0;

                    for (int i = 1; i < lines.Length; i++) // Skip header
                    {
                        progressWindow.UpdateProgress(i, $"Importing user {i} of {lines.Length - 1}");

                        var parts = lines[i].Split(',');
                        if (parts.Length >= 6)
                        {
                            try
                            {
                                var user = new User
                                {
                                    RFIDUID = parts[0].Trim(),
                                    IDNumber = parts[1].Trim(),
                                    FirstName = parts[2].Trim(),
                                    LastName = parts[3].Trim(),
                                    Department = parts[4].Trim(),
                                    CourseYrPosition = parts[5].Trim(),
                                    UserType = parts.Length > 6 ? parts[6].Trim() : "Student"
                                };

                                if (DataContext.Instance.RegisterUser(user))
                                {
                                    successCount++;
                                }
                                else
                                {
                                    failCount++;
                                }
                            }
                            catch (Exception)
                            {
                                failCount++;
                            }
                        }
                        else
                        {
                            failCount++;
                        }
                    }

                    progressWindow.Close();

                    // Refresh list after import
                    RefreshUsersList();

                    MessageBox.Show($"Import complete.\nSuccessfully imported: {successCount} users\nFailed: {failCount} users",
                                  "Import Complete",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("The CSV file is empty or invalid.",
                                  "Import Error",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Warning);
                }
            }
            else if (Path.GetExtension(filePath).ToLower() == ".xlsx")
            {
                MessageBox.Show("Excel import requires additional libraries. Please use CSV format for now.",
                              "Import Error",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
            }
            else
            {
                MessageBox.Show("Unsupported file format. Please use CSV files for import.",
                              "Import Error",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// Creates a backup of the users database
        /// </summary>
        private void BackupButton_Click(object sender, RoutedEventArgs e)
        {
            // Create backup of users database
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "CSV Files (*.csv)|*.csv|Excel Files (*.xlsx)|*.xlsx",
                FileName = $"MSU_Users_Backup_{DateTime.Now:yyyyMMdd}",
                Title = "Backup Users Database"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    ExportUsers(saveFileDialog.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error backing up users: {ex.Message}",
                                  "Backup Error",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Exports users to a CSV file
        /// </summary>
        private void ExportUsers(string filePath)
        {
            if (Path.GetExtension(filePath).ToLower() == ".csv")
            {
                using (var writer = new StreamWriter(filePath))
                {
                    // Write header
                    writer.WriteLine("RFIDUID,IDNumber,FirstName,LastName,Department,CourseYrPosition,UserType");

                    // Write data
                    foreach (var user in DataContext.Instance.Users)
                    {
                        writer.WriteLine($"{user.RFIDUID},{user.IDNumber},{user.FirstName},{user.LastName},{user.Department},{user.CourseYrPosition},{user.UserType}");
                    }
                }

                MessageBox.Show("Users exported successfully.",
                             "Export Complete",
                             MessageBoxButton.OK,
                             MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Only CSV export is currently supported.",
                             "Export Error",
                             MessageBoxButton.OK,
                             MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// Opens the Edit User window for the selected user
        /// </summary>
        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var user = button?.DataContext as User;

            if (user != null)
            {
                var editUserWindow = new EditUserWindow(user);

                // If dialog returns true, user was edited
                if (editUserWindow.ShowDialog() == true)
                {
                    // Refresh list after editing
                    RefreshUsersList();
                }
            }
            else if (UsersListView.SelectedItem is User selectedUser)
            {
                // Alternative selection method if button's DataContext is not set
                var editUserWindow = new EditUserWindow(selectedUser);

                if (editUserWindow.ShowDialog() == true)
                {
                    RefreshUsersList();
                }
            }
            else
            {
                MessageBox.Show("Please select a user to edit.",
                             "Edit User",
                             MessageBoxButton.OK,
                             MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Deletes the selected user
        /// </summary>
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var user = button?.DataContext as User;

            if (user == null && UsersListView.SelectedItem is User selectedUser)
            {
                // Alternative selection method if button's DataContext is not set
                user = selectedUser;
            }

            if (user != null)
            {
                var result = MessageBox.Show($"Are you sure you want to delete {user.Name}?",
                                           "Confirm Delete",
                                           MessageBoxButton.YesNo,
                                           Me}
