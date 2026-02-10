using System.ComponentModel;
using System.Runtime.CompilerServices;
 

namespace TestExample.Models
{
    public class TabModel:INotifyPropertyChanged
    {
        private string _title;
        private string _textContent;
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

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop="") {
            if (PropertyChanged != null) { 
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
            }
        }
    }
}
