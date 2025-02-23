using System.ComponentModel;

namespace YATT
{
    public class AddTorrentDialogViewModel : INotifyPropertyChanged
    {
        private string _selectedDirectory;

        public string SelectedDirectory
        {
            get => _selectedDirectory;
            set
            {
                if (_selectedDirectory != value)
                {
                    _selectedDirectory = value;
                    OnPropertyChanged(nameof(SelectedDirectory));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}