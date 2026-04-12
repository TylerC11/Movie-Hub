using System.Collections.ObjectModel;
using Movie_Hub.Commands;
using Movie_Hub.Models;

namespace Movie_Hub.ViewModels
{
    public class FavouritesViewModel : BaseViewModel
    {
        private Title? _selectedFavourite;
        private string _statusMessage = string.Empty;

        public ObservableCollection<Title> Favourites { get; } = new();

        public Title? SelectedFavourite
        {
            get => _selectedFavourite;
            set => SetProperty(ref _selectedFavourite, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            private set => SetProperty(ref _statusMessage, value);
        }

        public bool HasFavourites => Favourites.Count > 0;

        public RelayCommand RemoveCommand { get; }
        public RelayCommand ClearAllCommand { get; }

        public FavouritesViewModel()
        {
            RemoveCommand = new RelayCommand(
                execute: param => Remove(param as Title ?? SelectedFavourite),
                canExecute: _ => Favourites.Count > 0);

            ClearAllCommand = new RelayCommand(
                execute: _ => ClearAll(),
                canExecute: _ => Favourites.Count > 0);

            Favourites.CollectionChanged += (_, _) =>
            {
                OnPropertyChanged(nameof(HasFavourites));
                RemoveCommand.RaiseCanExecuteChanged();
                ClearAllCommand.RaiseCanExecuteChanged();
                RefreshStatus();
            };

            RefreshStatus();
        }

        public bool Add(Title? title)
        {
            if (title is null) return false;
            if (Favourites.Any(f => f.TitleId == title.TitleId)) return false;
            Favourites.Add(title);
            return true;
        }

        public bool Remove(Title? title)
        {
            if (title is null) return false;
            var existing = Favourites.FirstOrDefault(f => f.TitleId == title.TitleId);
            if (existing is null) return false;
            Favourites.Remove(existing);
            if (SelectedFavourite?.TitleId == title.TitleId)
                SelectedFavourite = null;
            return true;
        }

        public bool IsFavourite(Title? title)
            => title is not null && Favourites.Any(f => f.TitleId == title.TitleId);

        private void ClearAll()
        {
            Favourites.Clear();
            SelectedFavourite = null;
        }

        private void RefreshStatus()
        {
            StatusMessage = Favourites.Count switch
            {
                0 => "No favourites yet — click ★ on any movie to save it here.",
                1 => "1 movie saved.",
                _ => $"{Favourites.Count} movies saved."
            };
        }
    }
}