using Movie_Hub.Data;
using Movie_Hub.Services;
using Movie_Hub.ViewModels;
using Movie_Hub.Views;
using System.Windows;

namespace Movie_Hub
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var context = new ImdbContext();
            var movieService = new MovieService(context);
            var genreService = new GenreService(context);

            var viewModel = new MainViewModel(movieService, genreService);
            var sidebarPage = new Views.MainWindow();
            sidebarPage.DataContext = viewModel;

            MainFrame.Content = sidebarPage;
        }
    }
}