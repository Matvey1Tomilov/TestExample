using System.ComponentModel;
using System.Runtime.CompilerServices;
 

namespace TestExample.Models
{
    public class TabModel:INotifyPropertyChanged
    {
        private string _title;
        private string _textContent;
        private bool _isDeleting;
        private int _remainingSeconds;
        public TabModel()
        {
            _title = Title ;
            _textContent = TextContent;
        }
        public TabModel( int index, string textContent,bool isSelected) { 
            _title = "Вкладка "+index.ToString();
            _textContent = textContent; 
        }
        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                OnPropertyChanged("Title");
            }
        }
        public string TextContent {
            get { return _textContent; }
            set
            {
                _textContent = value;
                OnPropertyChanged("TextContent");
            }
        }    

        public bool IsDeleting
        {
            get => _isDeleting;
            set
            {
                _isDeleting = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HeaderColor)); 
            }
        }

        public int RemainingSeconds
        {
            get => _remainingSeconds;
            set
            {
                _remainingSeconds = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CancelButtonText));
            }
        }
        public string CancelButtonText => IsDeleting ? $"Отменить ({RemainingSeconds}с)" : "Отменить";
        public string HeaderColor => IsDeleting ? "Red" : "Black";

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop="") {
            if (PropertyChanged != null) { 
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
            }
        }
    }
}
