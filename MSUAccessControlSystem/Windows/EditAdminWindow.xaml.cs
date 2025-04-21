public partial class EditAdminWindow : Window
{
    private Administrator _admin;

    public EditAdminWindow(Administrator admin)
    {
        InitializeComponent();
        _admin = admin;

        // Populate role combo box
        var roles = new[] { "Super Admin", "Security Personnel" };
        RoleComboBox.ItemsSource = roles;

        // Fill in existing admin data
        RFIDTextBox.Text = _admin.RFIDUID;
        UsernameTextBox.Text = _admin.Username;
        RoleComboBox.SelectedItem = roles.Contains(_admin.Role) ? _admin.Role : roles[1];
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        // Validate input fields
        if (string.IsNullOrWhiteSpace(RFIDTextBox.Text))
        {
            MessageBox.Show("Please tap RFID card on the reader.",
                         "Validation Error",
                         MessageBoxButton.OK,
                         MessageBoxImage.Warning);
            RFIDTextBox.Focus();
            return;
        }

        // Check if this is the last Super Admin and user is trying to change role
        if (_admin.Role == "Super Admin" &&
            RoleComboBox.SelectedItem.ToString() != "Super Admin" &&
            DataContext.Instance.Administrators.Count(a => a.Role == "Super Admin") <= 1)
        {
            MessageBox.Show("Cannot change role of the last Super Admin.",
                          "Validation Error",
                          MessageBoxButton.OK,
                          MessageBoxImage.Warning);
            RoleComboBox.SelectedItem = "Super Admin";
            return;
        }

        // Check if password should be updated
        bool updatePassword = !string.IsNullOrEmpty(PasswordBox.Password);

        if (updatePassword && PasswordBox.Password != ConfirmPasswordBox.Password)
        {
            MessageBox.Show("Passwords do not match.",
                         "Validation Error",
                         MessageBoxButton.OK,
                         MessageBoxImage.Warning);
            ConfirmPasswordBox.Focus();
            return;
        }

        try
        {
            // Update administrator in database
            using (var connection = DatabaseManager.GetConnection())
            {
                connection.Open();

                string query = "UPDATE Administrators SET RFIDUID = @rfid, Role = @role";

                if (updatePassword)
                {
                    query += ", Password = @password";
                }

                query += " WHERE Username = @username";

                var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@rfid", RFIDTextBox.Text.Trim());
                cmd.Parameters.AddWithValue("@role", RoleComboBox.SelectedItem.ToString());
                cmd.Parameters.AddWithValue("@username", _admin.Username);

                if (updatePassword)
                {
                    cmd.Parameters.AddWithValue("@password", PasswordBox.Password);
                }

                cmd.ExecuteNonQuery();

                // Update in-memory object
                _admin.RFIDUID = RFIDTextBox.Text.Trim();
                _admin.Role = RoleComboBox.SelectedItem.ToString();

                if (updatePassword)
                {
                    _admin.Password = PasswordBox.Password;
                }
            }

            MessageBox.Show("Administrator updated successfully.",
                         "Update Complete",
                         MessageBoxButton.OK,
                         MessageBoxImage.Information);
            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error updating administrator: {ex.Message}",
                         "Update Error",
                         MessageBoxButton.OK,
                         MessageBoxImage.Error);
        }
    }
}