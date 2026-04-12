using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace Movie_Hub.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        protected bool SetProperty<T>(ref T field, T value,
            [CallerMemberName] string? propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected bool SetProperty<T>(ref T field, T value, Action onChanged,
            [CallerMemberName] string? propertyName = null)
        {
            if (!SetProperty(ref field, value, propertyName)) return false;
            onChanged();
            return true;
        }

        protected static void RunOnUI(Action action)
        {
            if (Application.Current?.Dispatcher is { } d && !d.CheckAccess())
                d.Invoke(action);
            else
                action();
        }
    }
}