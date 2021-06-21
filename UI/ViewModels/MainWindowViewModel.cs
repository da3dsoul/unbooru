using System.ComponentModel;
using ReactiveUI;

namespace UI.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private string _address;
        private string _currentAddress;

        public MainWindowViewModel()
        {
            _address = _currentAddress = string.Empty;
            PropertyChanged += OnPropertyChanged;
        }

        private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CurrentAddress)) {
                Address = CurrentAddress;
            }
        }

        public string Address
        {
            get => _address;
            set => this.RaiseAndSetIfChanged(ref _address, value);
        }

        public string CurrentAddress
        {
            get => _currentAddress;
            set => this.RaiseAndSetIfChanged(ref _currentAddress, value);
        }
    }
}
