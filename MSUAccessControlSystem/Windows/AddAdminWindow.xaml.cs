using MSUAccessControlSystem.Models;
using System.Windows.Controls;
using System.Windows;

public partial class AddAdminWindow : Window
{
    public AddAdminWindow()
    {
        InitializeComponent();

        // Populate role combo box
        var roles = new[] { "Super Admin", "Security Personnel" };
        RoleComboBox.ItemsSource = roles;
        RoleComboBox.SelectedIndex = 1; // Default to Security Personnel
    }

    private void RFIDTextBox_GotFocus(object sender, RoutedEventArgs e)
    {
        // When RFID field is focused, start listening for RFID input
        // This would typically be handled automatically by the RFID reader
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

        if (string.IsNullOrWhiteSpace(UsernameTextBox.Text))
        {
            MessageBox.Show("Please enter a username.",
                         "Validation Error",
                         MessageBoxButton.OK,
                         MessageBoxImage.Warning);
            UsernameTextBox.Focus();
            return;
        }

        if (string.IsNullOrWhiteSpace(PasswordBox.Password))
        {
            MessageBox.Show("Please enter a password.",
                         "Validation Error",
                         MessageBoxButton.OK,
                         MessageBoxImage.Warning);
            PasswordBox.Focus();
            return;
        }

        if (PasswordBox.Password != ConfirmPasswordBox.Password)
        {
            MessageBox.Show("Passwords do not match.",
                         "Validation Error",
                         MessageBoxButton.OK,
                         MessageBoxImage.Warning);
            ConfirmPasswordBox.Focus();
            return;
        }

        // Create new administrator
        var admin = new Administrator
        {
            RFIDUID = RFIDTextBox.Text.Trim(),
            Username = UsernameTextBox.Text.Trim(),
            Password = PasswordBox.Password,
            Role = RoleComboBox.SelectedItem.ToString()
        };

        // Save administrator to database
        if (DataContext.Instance.RegisterAdmin(admin))
        {
            MessageBox.Show("Administrator registered successfully.",
                         "Registration Complete",
                         MessageBoxButton.OK,
                         MessageBoxImage.Information);
            DialogResult = true;
            Close();
        }
        else
        {
            MessageBox.Show("An administrator with this username already exists.",
                         "Registration Error",
                         MessageBoxButton.OK,
                         MessageBoxImage.Error);
        }
    }
}
