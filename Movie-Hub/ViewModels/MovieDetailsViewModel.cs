using Movie_Hub.Commands;
using Movie_Hub.Models;
using Movie_Hub.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace Movie_Hub.ViewModels
{
    public class MovieDetailsViewModel : BaseViewModel
    {
        private readonly IMovieService _movieService;
        private string _titleId;
        private Title _movie;
        private bool _isLoading;
        private string _errorMessage;
        private bool _isAlreadyFavourite;

        public Action NavigateBack { get; set; }
        public Action? AddToFavourites { get; set; }

        public ObservableCollection<CastMemberDisplay> Cast { get; } = new();
        public ObservableCollection<Genre> Genres { get; } = new();

        public ICommand LoadMovieCommand { get; }
        public ICommand NavigateBackCommand { get; }
        public ICommand AddFavouriteCommand { get; }

        public bool IsAlreadyFavourite
        {
            get => _isAlreadyFavourite;
            set
            {
                if (SetProperty(ref _isAlreadyFavourite, value))
                    ((RelayCommand)AddFavouriteCommand).RaiseCanExecuteChanged();
            }
        }

        public MovieDetailsViewModel(IMovieService movieService)
        {
            _movieService = movieService;
            LoadMovieCommand = new RelayCommand(LoadMovie);
            NavigateBackCommand = new RelayCommand(() => NavigateBack?.Invoke());
            AddFavouriteCommand = new RelayCommand(
                execute: () => AddToFavourites?.Invoke(),
                canExecute: () => !IsAlreadyFavourite);
        }

        public string TitleId
        {
            get => _titleId;
            set
            {
                _titleId = value;
                LoadMovieCommand.Execute(null);
            }
        }

        public Title Movie
        {
            get => _movie;
            set => SetProperty(ref _movie, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        private void LoadMovie()
        {
            if (string.IsNullOrEmpty(TitleId)) return;

            IsLoading = true;
            ErrorMessage = null;

            try
            {
                Cast.Clear();
                Genres.Clear();

                Movie = _movieService.GetMovieDetails(TitleId);

                if (Movie == null)
                {
                    ErrorMessage = "Movie not found";
                    return;
                }

                if (Movie.Genres != null)
                {
                    foreach (var genre in Movie.Genres)
                        Genres.Add(genre);
                }

                var castMembers = _movieService.GetCast(TitleId);

                if (castMembers != null && castMembers.Any())
                {
                    foreach (var principal in castMembers)
                    {
                        Cast.Add(new CastMemberDisplay
                        {
                            Name = principal.Name?.PrimaryName ?? "Unknown",
                            CharacterName = string.IsNullOrEmpty(principal.Characters)
                                ? principal.JobCategory ?? "Cast"
                                : principal.Characters
                        });
                    }
                }
                else
                {
                    Cast.Add(new CastMemberDisplay
                    {
                        Name = "No cast information available",
                        CharacterName = "This movie doesn't have cast data in the database"
                    });
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }
    }

    public class CastMemberDisplay
    {
        public string Name { get; set; }
        public string CharacterName { get; set; }
        public string DisplayText => $"{Name} as \"{CharacterName}\"";
    }
}