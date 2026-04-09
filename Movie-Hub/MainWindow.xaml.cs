using Movie_Hub.Commands;
using Movie_Hub.Data;
using Movie_Hub.Models;
using Movie_Hub.Services;
using Movie_Hub.ViewModels;
using Movie_Hub.Views;
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

namespace Movie_Hub
{
    public partial class MainWindow : Window
    {
        private IMovieService _movieService;
        private IGenreService _genreService;

        public MainWindow()
        {
            InitializeComponent();

            var context = new ImdbContext();
            _movieService = new MovieService(context);
            _genreService = new GenreService(context);

            ShowMovieList();
        }

        //show movies
        private void ShowMovieList()
        {
            // navigate command to go to details page when a movie is selected
            var navigateCommand = new RelayCommand(parameter =>
            {
                if (parameter is Title movie)
                {
                    ShowMovieDetails(movie.TitleId);
                }
            });

            var viewModel = new MovieListViewModel(_movieService, _genreService, navigateCommand);
            var view = new MovieListView();
            view.DataContext = viewModel;

            MainFrame.Navigate(view);
        }

        //show details of a movie 
        private void ShowMovieDetails(string titleId)
        {
            var viewModel = new MovieDetailsViewModel(_movieService);
            viewModel.TitleId = titleId;
            viewModel.NavigateBack = ShowMovieList;  //back button returns to list

            var view = new MovieDetailsView();
            view.DataContext = viewModel;

            MainFrame.Navigate(view);
        }
    }
}


//=======list of movies that has cast since the others dont
//The Godfather / The Godfather Part II, Star Wars: Episode IV - A New Hope, Star Wars: Episode V - The Empire Strikes Back
    