using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Movie_Hub.Views
{
    /// <summary>
    /// Interaction logic for MovieDetailsView.xaml
    /// </summary>
    public partial class MovieDetailsView : Page
    {
        public MovieDetailsView()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // Safety check: Ensure ViewModel is set
            if (DataContext == null)
            {
                // Log error or handle gracefully
                System.Diagnostics.Debug.WriteLine("MovieDetailsView: DataContext is null");
            }
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            // Clean up any resources if needed
            // This prevents memory leaks with event handlers
            if (DataContext is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}
