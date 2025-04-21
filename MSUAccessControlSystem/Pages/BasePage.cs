// BasePage.cs
using System.Windows.Controls;

namespace MSUAccessControlSystem
{
    /// <summary>
    /// Base class for all pages in the application
    /// </summary>
    public class BasePage : Page
    {
        public BasePage()
        {
            // Common initialization for all pages
            this.Loaded += BasePage_Loaded;
        }

        private void BasePage_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            // You can add common functionality that should run when any page loads
            // For example, logging page navigation or setting up global event handlers
        }

        /// <summary>
        /// Called when navigating away from the page
        /// </summary>
        protected virtual void OnNavigatingFrom()
        {
            // This method can be overridden in derived pages to perform cleanup
        }

        /// <summary>
        /// Called when navigating to the page
        /// </summary>
        /// <param name="parameter">Optional navigation parameter</param>
        protected virtual void OnNavigatedTo(object parameter = null)
        {
            // This method can be overridden in derived pages to initialize with navigation parameters
        }
    }
}