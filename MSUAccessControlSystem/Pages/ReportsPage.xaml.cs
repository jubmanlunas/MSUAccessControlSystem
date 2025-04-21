using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.IO;
using System.ComponentModel;

namespace MSUAccessControlSystem
{
    /// <summary>
    /// Interaction logic for ReportsPage.xaml
    /// </summary>
    public partial class ReportsPage : BasePage
    {
        private DateTime _selectedDate = DateTime.Now;
        private string _userTypeFilter = "All";
        private string _eventTypeFilter = "Both";
        private string _fileTypeFilter = "Excel";
        private string _currentSortProperty = "TimeStamp";
        private ListSortDirection _currentSortDirection = ListSortDirection.Descending;

        public ReportsPage()
        {
            InitializeComponent();

            // Initialize date picker with today's date
            DatePicker.SelectedDate = _selectedDate;

            // Initialize radio buttons
            AllRadioButton.IsChecked = true;
            BothRadioButton.IsChecked = true;
            ExcelRadioButton.IsChecked = true;

            // Load initial data
            LoadReportData();
        }

        /// <summary>
        /// Loads report data based on current filters
        /// </summary>
        private void LoadReportData()
        {
            var filteredLogs = GetFilteredLogs();
            LogsListView.ItemsSource = filteredLogs;
        }

        /// <summary>
        /// Updates when date selection changes
        /// </summary>
        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DatePicker.SelectedDate.HasValue)
            {
                _selectedDate = DatePicker.SelectedDate.Value;
                LoadReportData();
            }
        }

        /// <summary>
        /// Opens calendar popup for date selection
        /// </summary>
        private void CalendarButton_Click(object sender, RoutedEventArgs e)
        {
            DatePicker.IsDropDownOpen = true;
        }

        /// <summary>
        /// Updates user type filter when selection changes
        /// </summary>
        private void UserTypeRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton radioButton)
            {
                if (radioButton == AllRadioButton)
                    _userTypeFilter = "All";
                else if (radioButton == StudentsOnlyRadioButton)
                    _userTypeFilter = "Students";
                else if (radioButton == EmployeesOnlyRadioButton)
                    _userTypeFilter = "Employees";

                LoadReportData();
            }
        }

        /// <summary>
        /// Updates event type filter when selection changes
        /// </summary>
        private void EventTypeRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton radioButton)
            {
                if (radioButton == BothRadioButton)
                    _eventTypeFilter = "Both";
                else if (radioButton == EntriesOnlyRadioButton)
                    _eventTypeFilter = "Entries";
                else if (radioButton == ExitsOnlyRadioButton)
                    _eventTypeFilter = "Exits";

                LoadReportData();
            }
        }

        /// <summary>
        /// Updates file type filter when selection changes
        /// </summary>
        private void FileTypeRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton radioButton)
            {
                if (radioButton == ExcelRadioButton)
                    _fileTypeFilter = "Excel";
                else if (radioButton == PDFRadioButton)
                    _fileTypeFilter = "PDF";
            }
        }

        /// <summary>
        /// Sorts the data when column header is clicked
        /// </summary>
        private void SortButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string propertyName)
            {
                // Map display property names to actual model property names
                string sortProperty = propertyName;
                if (propertyName == "Time")
                    sortProperty = "TimeStamp";

                // Toggle sort direction if clicking the same column
                if (sortProperty == _currentSortProperty)
                {
                    _currentSortDirection = _currentSortDirection == ListSortDirection.Ascending ?
                        ListSortDirection.Descending : ListSortDirection.Ascending;
                }
                else
                {
                    _currentSortProperty = sortProperty;
                    _currentSortDirection = ListSortDirection.Ascending;
                }

                // Re-sort the data
                var filteredLogs = GetFilteredLogs();

                // Apply sort
                filteredLogs = SortLogs(filteredLogs, _currentSortProperty, _currentSortDirection);

                LogsListView.ItemsSource = filteredLogs;
            }
        }

        /// <summary>
        /// Sorts the log data by property and direction
        /// </summary>
        private List<AccessLog> SortLogs(List<AccessLog> logs, string propertyName, ListSortDirection direction)
        {
            IOrderedEnumerable<AccessLog> sortedLogs;

            switch (propertyName)
            {
                case "TimeStamp":
                    sortedLogs = direction == ListSortDirection.Ascending ?
                        logs.OrderBy(l => l.TimeStamp) : logs.OrderByDescending(l => l.TimeStamp);
                    break;
                case "IDNumber":
                    sortedLogs = direction == ListSortDirection.Ascending ?
                        logs.OrderBy(l => l.IDNumber) : logs.OrderByDescending(l => l.IDNumber);
                    break;
                case "Name":
                    sortedLogs = direction == ListSortDirection.Ascending ?
                        logs.OrderBy(l => l.Name) : logs.OrderByDescending(l => l.Name);
                    break;
                case "UserType":
                    sortedLogs = direction == ListSortDirection.Ascending ?
                        logs.OrderBy(l => l.UserType) : logs.OrderByDescending(l => l.UserType);
                    break;
                case "EventType":
                    sortedLogs = direction == ListSortDirection.Ascending ?
                        logs.OrderBy(l => l.EventType) : logs.OrderByDescending(l => l.EventType);
                    break;
                default:
                    sortedLogs = direction == ListSortDirection.Ascending ?
                        logs.OrderBy(l => l.TimeStamp) : logs.OrderByDescending(l => l.TimeStamp);
                    break;
            }

            return sortedLogs.ToList();
        }

        /// <summary>
        /// Exports the report data
        /// </summary>
        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            ExportReport();
        }

        /// <summary>
        /// Prints the report data
        /// </summary>
        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            PrintReport();
        }

        /// <summary>
        /// Exports report to a file
        /// </summary>
        private void ExportReport()
        {
            try
            {
                // Get filtered access logs
                var logs = GetFilteredLogs();

                // If there are no logs, show message and return
                if (!logs.Any())
                {
                    MessageBox.Show("No data found for the selected filters.",
                                  "Export Report",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Information);
                    return;
                }

                // Create save dialog
                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    FileName = $"MSU_Access_Report_{_selectedDate:yyyyMMdd}"
                };

                if (_fileTypeFilter == "Excel")
                {
                    saveFileDialog.Filter = "Excel Files (*.xlsx)|*.xlsx|CSV Files (*.csv)|*.csv";
                    saveFileDialog.DefaultExt = ".csv";
                }
                else // PDF
                {
                    saveFileDialog.Filter = "PDF Files (*.pdf)|*.pdf";
                    saveFileDialog.DefaultExt = ".pdf";
                }

                if (saveFileDialog.ShowDialog() == true)
                {
                    if (_fileTypeFilter == "Excel" || Path.GetExtension(saveFileDialog.FileName).ToLower() == ".csv")
                    {
                        ExportToCSV(logs, saveFileDialog.FileName);
                    }
                    else // PDF
                    {
                        ExportToPDF(logs, saveFileDialog.FileName);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting report: {ex.Message}",
                              "Export Error",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Exports logs to CSV file
        /// </summary>
        private void ExportToCSV(List<AccessLog> logs, string filePath)
        {
            // Simple CSV export that can be opened in Excel
            try
            {
                using (var writer = new StreamWriter(filePath))
                {
                    // Write header
                    writer.WriteLine("Time,ID Number,Name,Department,User Type,Event Type");

                    // Write data
                    foreach (var log in logs)
                    {
                        writer.WriteLine($"{log.TimeStamp:yyyy-MM-dd HH:mm:ss},{log.IDNumber},{log.Name},{log.Department},{log.UserType},{log.EventType}");
                    }
                }

                MessageBox.Show("Report exported successfully.",
                             "Export Complete",
                             MessageBoxButton.OK,
                             MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error exporting to CSV: {ex.Message}");
            }
        }

        /// <summary>
        /// Exports logs to PDF file
        /// </summary>
        private void ExportToPDF(List<AccessLog> logs, string filePath)
        {
            // For actual implementation, a PDF library would be needed
            MessageBox.Show("PDF export requires a third-party library. Please use Excel export instead.",
                         "Feature Not Implemented",
                         MessageBoxButton.OK,
                         MessageBoxImage.Information);
        }

        /// <summary>
        /// Prints the report
        /// </summary>
        private void PrintReport()
        {
            try
            {
                // Get filtered access logs
                var logs = GetFilteredLogs();

                // If there are no logs, show message and return
                if (!logs.Any())
                {
                    MessageBox.Show("No data found for the selected filters.",
                                  "Print Report",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Information);
                    return;
                }

                // Create a print dialog
                var printDialog = new PrintDialog();
                if (printDialog.ShowDialog() == true)
                {
                    // Create a FlowDocument for printing
                    var document = CreatePrintDocument(logs);

                    // Print the document
                    var paginator = ((IDocumentPaginatorSource)document).DocumentPaginator;
                    printDialog.PrintDocument(paginator, "MSU Access Report");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error printing report: {ex.Message}",
                              "Print Error",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Creates a FlowDocument for printing
        /// </summary>
        private FlowDocument CreatePrintDocument(List<AccessLog> logs)
        {
            var document = new FlowDocument();

            // Add title
            var title = new Paragraph(new Run($"MSU Access Report - {_selectedDate:yyyy-MM-dd}"))
            {
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center
            };
            document.Blocks.Add(title);

            // Add filters info
            var filtersInfo = new Paragraph(new Run($"Filters: User Type: {_userTypeFilter}, Event Type: {_eventTypeFilter}"))
            {
                FontSize = 10,
                TextAlignment = TextAlignment.Center
            };
            document.Blocks.Add(filtersInfo);

            // Add table
            var table = new Table();

            // Add columns
            for (int i = 0; i < 6; i++)
            {
                table.Columns.Add(new TableColumn());
            }

            // Add header row
            var headerRow = new TableRow();
            headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Time"))));
            headerRow.Cells.Add(new TableCell(new Paragraph(new Run("ID Number"))));
            headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Name"))));
            headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Department"))));
            headerRow.Cells.Add(new TableCell(new Paragraph(new Run("User Type"))));
            headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Event Type"))));

            foreach (var cell in headerRow.Cells)
            {
                cell.FontWeight = FontWeights.Bold;
            }

            var tableRowGroup = new TableRowGroup();
            tableRowGroup.Rows.Add(headerRow);

            // Add data rows
            foreach (var log in logs)
            {
                var row = new TableRow();
                row.Cells.Add(new TableCell(new Paragraph(new Run(log.TimeStamp.ToString("HH:mm:ss")))));
                row.Cells.Add(new TableCell(new Paragraph(new Run(log.IDNumber))));
                row.Cells.Add(new TableCell(new Paragraph(new Run(log.Name))));
                row.Cells.Add(new TableCell(new Paragraph(new Run(log.Department))));
                row.Cells.Add(new TableCell(new Paragraph(new Run(log.UserType))));
                row.Cells.Add(new TableCell(new Paragraph(new Run(log.EventType))));
                tableRowGroup.Rows.Add(row);
            }

            table.RowGroups.Add(tableRowGroup);
            document.Blocks.Add(table);

            return document;
        }

        /// <summary>
        /// Gets filtered logs based on current filters
        /// </summary>
        private List<AccessLog> GetFilteredLogs()
        {
            // Get logs for the selected date
            var logs = DataContext.Instance.AccessLogs
                .Where(l => l.TimeStamp.Date == _selectedDate.Date)
                .ToList();

            // Apply user type filter
            if (_userTypeFilter == "Students")
            {
                logs = logs.Where(l => l.UserType == "Student").ToList();
            }
            else if (_userTypeFilter == "Employees")
            {
                logs = logs.Where(l => l.UserType == "Employee").ToList();
            }

            // Apply event type filter
            if (_eventTypeFilter == "Entries")
            {
                logs = logs.Where(l => l.EventType == "ENTRY").ToList();
            }
            else if (_eventTypeFilter == "Exits")
            {
                logs = logs.Where(l => l.EventType == "EXIT").ToList();
            }

            // Apply current sort
            logs = SortLogs(logs, _currentSortProperty, _currentSortDirection);

            return logs;
        }

        protected override void OnNavigatedTo(object parameter = null)
        {
            base.OnNavigatedTo(parameter);

            // Refresh data when navigating to this page
            LoadReportData();
        }
    }
}