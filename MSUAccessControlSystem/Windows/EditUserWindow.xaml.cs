using MSUAccessControlSystem.Data;
using MSUAccessControlSystem.Models;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows;

public partial class EditUserWindow : Window
{
    private User _user;
    private byte[] _photoBytes;

    public EditUserWindow(User user)
    {
        InitializeComponent();
        _user = user;

        // Populate department combo box
        var departments = new[] { "DCS", "DHESS", "DIT", "BSHMT", "SECONDARY", "DNSM" };
        DepartmentComboBox.ItemsSource = departments;

        // Populate user type combo box
        var userTypes = new[] { "Student", "Employee", "Visitor" };
        UserTypeComboBox.ItemsSource = userTypes;

        // Fill in existing user data
        RFIDTextBox.Text = _user.RFIDUID;
        IDNumberTextBox.Text = _user.IDNumber;
        FirstNameTextBox.Text = _user.FirstName;
        LastNameTextBox.Text = _user.LastName;
        DepartmentComboBox.SelectedItem = departments.Contains(_user.Department) ? _user.Department : departments[0];
        UserTypeComboBox.SelectedItem = userTypes.Contains(_user.UserType) ? _user.UserType : userTypes[0];

        // Populate course/year/position combo box based on user type
        UpdateCourseYrPositionComboBox();

        // Load existing photo if available
        if (_user.Photo != null)
        {
            _photoBytes = _user.Photo;
            try
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = new MemoryStream(_photoBytes);
                bitmap.EndInit();
                UserPhotoImage.Source = bitmap;
            }
            catch
            {
                // Ignore errors loading photo
            }
        }
    }

    private void UserTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        UpdateCourseYrPositionComboBox();
    }

    private void UpdateCourseYrPositionComboBox()
    {
        if (UserTypeComboBox.SelectedItem != null)
        {
            string userType = UserTypeComboBox.SelectedItem.ToString();

            if (userType == "Student")
            {
                var courses = new[]
                {
                        "BSCS 1", "BSCS 2", "BSCS 3", "BSCS 4",
                        "BEED 1", "BEED 2", "BEED 3", "BEED 4",
                        "BSET 1", "BSET 2", "BSET 3", "BSET 4",
                        "BTLeD 1", "BTLeD 2", "BTLeD 3", "BTLeD 4"
                    };
                CourseYrPositionComboBox.ItemsSource = courses;
            }
            else if (userType == "Employee")
            {
                var positions = new[]
                {
                        "Instructor I", "Instructor II", "Instructor III",
                        "Assistant Professor I", "Assistant Professor II",
                        "Associate Professor I", "Associate Professor II",
                        "Professor I", "Professor II",
                        "Staff", "Administrative", "Security", "Maintenance"
                    };
                CourseYrPositionComboBox.ItemsSource = positions;
            }
            else // Visitor
            {
                var purposes = new[]
                {
                        "Guest", "Parent", "Applicant", "Supplier", "Contractor", "Others"
                    };
                CourseYrPositionComboBox.ItemsSource = purposes;
            }

            // Try to select the user's current course/year/position
            foreach (var item in CourseYrPositionComboBox.Items)
            {
                if (item.ToString() == _user.CourseYrPosition)
                {
                    CourseYrPositionComboBox.SelectedItem = item;
                    break;
                }
            }

            // If not found, select the first item
            if (CourseYrPositionComboBox.SelectedItem == null && CourseYrPositionComboBox.Items.Count > 0)
            {
                CourseYrPositionComboBox.SelectedIndex = 0;
            }
        }
    }

    private void ImportPhotoButton_Click(object sender, RoutedEventArgs e)
    {
        var openFileDialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "Image Files (*.jpg, *.jpeg, *.png, *.bmp)|*.jpg;*.jpeg;*.png;*.bmp|All Files (*.*)|*.*"
        };

        if (openFileDialog.ShowDialog() == true)
        {
            try
            {
                // Load the image and display it
                BitmapImage bitmap = new BitmapImage(new Uri(openFileDialog.FileName));
                UserPhotoImage.Source = bitmap;

                // Convert to byte array for storage
                _photoBytes = File.ReadAllBytes(openFileDialog.FileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading image: {ex.Message}",
                              "Image Error",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }
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

        if (string.IsNullOrWhiteSpace(IDNumberTextBox.Text))
        {
            MessageBox.Show("Please enter an ID number.",
                         "Validation Error",
                         MessageBoxButton.OK,
                         MessageBoxImage.Warning);
            IDNumberTextBox.Focus();
            return;
        }

        if (string.IsNullOrWhiteSpace(FirstNameTextBox.Text))
        {
            MessageBox.Show("Please enter a first name.",
                         "Validation Error",
                         MessageBoxButton.OK,
                         MessageBoxImage.Warning);
            FirstNameTextBox.Focus();
            return;
        }

        if (string.IsNullOrWhiteSpace(LastNameTextBox.Text))
        {
            MessageBox.Show("Please enter a last name.",
                         "Validation Error",
                         MessageBoxButton.OK,
                         MessageBoxImage.Warning);
            LastNameTextBox.Focus();
            return;
        }

        try
        {
            // Update user in database
            using (var connection = DatabaseManager.GetConnection())
            {
                connection.Open();

                string query = @"UPDATE Users SET 
                                RFIDUID = @rfid,
                                FirstName = @fname,
                                LastName = @lname,
                                Department = @dept,
                                CourseYrPosition = @course,
                                UserType = @type";

                if (_photoBytes != null)
                {
                    query += ", Photo = @photo";
                }

                query += " WHERE IDNumber = @id";

                var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@rfid", RFIDTextBox.Text.Trim());
                cmd.Parameters.AddWithValue("@id", _user.IDNumber); // Use original ID for WHERE clause
                cmd.Parameters.AddWithValue("@fname", FirstNameTextBox.Text.Trim());
                cmd.Parameters.AddWithValue("@lname", LastNameTextBox.Text.Trim());
                cmd.Parameters.AddWithValue("@dept", DepartmentComboBox.SelectedItem.ToString());
                cmd.Parameters.AddWithValue("@course", CourseYrPositionComboBox.SelectedItem.ToString());
                cmd.Parameters.AddWithValue("@type", UserTypeComboBox.SelectedItem.ToString());

                if (_photoBytes != null)
                {
                    cmd.Parameters.AddWithValue("@photo", _photoBytes);
                }

                cmd.ExecuteNonQuery();

                // Update in-memory object
                _user.RFIDUID = RFIDTextBox.Text.Trim();
                _user.FirstName = FirstNameTextBox.Text.Trim();
                _user.LastName = LastNameTextBox.Text.Trim();
                _user.Department = DepartmentComboBox.SelectedItem.ToString();
                _user.CourseYrPosition = CourseYrPositionComboBox.SelectedItem.ToString();
                _user.UserType = UserTypeComboBox.SelectedItem.ToString();

                if (_photoBytes != null)
                {
                    _user.Photo = _photoBytes;
                }

                // If ID number changed, update ID in logs too
                if (_user.IDNumber != IDNumberTextBox.Text.Trim())
                {
                    string updateLogsQuery = "UPDATE AccessLogs SET IDNumber = @newId WHERE IDNumber = @oldId";
                    var updateLogsCmd = new MySqlCommand(updateLogsQuery, connection);
                    updateLogsCmd.Parameters.AddWithValue("@newId", IDNumberTextBox.Text.Trim());
                    updateLogsCmd.Parameters.AddWithValue("@oldId", _user.IDNumber);
                    updateLogsCmd.ExecuteNonQuery();

                    // Update ID last (as it's used as key)
                    _user.IDNumber = IDNumberTextBox.Text.Trim();
                }
            }

            MessageBox.Show("User updated successfully.",
                         "Update Complete",
                         MessageBoxButton.OK,
                         MessageBoxImage.Information);
            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error updating user: {ex.Message}",
                         "Update Error",
                         MessageBoxButton.OK,
                         MessageBoxImage.Error);
        }
    }
}