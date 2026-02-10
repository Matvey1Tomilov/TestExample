using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using TestExample.Commands;
using TestExample.Models;

namespace TestExample.ViewModels
{
    public class TabViewModel:INotifyPropertyChanged
    {
        private TabModel _selectedTab;
        public ObservableCollection<TabModel> TabModels { get; set; }
       
        private RelayCommand addTab;
        public RelayCommand AddTab
        {
            get{
                return addTab ??
                    (addTab = new RelayCommand(obj =>
                    {
                        int index = TabModels.Count + 1;
                        TabModel tabModel = new TabModel(index, "",true);
                        TabModels.Add(tabModel);
                        SelectedTab = tabModel;
                    }));
            }
        }
        private RelayCommand moveTab;
        public RelayCommand MoveTab
        {
            get {
                return moveTab ?? 
                    (moveTab=new RelayCommand(obj=>
                    {
                        if (obj is Tuple<TabModel, TabModel> payload)
                        { 
                            var (startTab, endTab) = payload;
                            int startIndex=TabModels.IndexOf(startTab);
                            int endIndex=TabModels.IndexOf(endTab);
                            if (startIndex >=0 && endIndex >=0 && startIndex != endIndex)
                            { 
                                int insertIndex= endIndex>startIndex?endIndex-1:endIndex;
                                TabModels.Move(startIndex, insertIndex);
                                SelectedTab = startTab; 
                            }
                        }
                    },obj=>obj is Tuple<TabModel,TabModel>));
            }
        }
        //private RelayCommand removeTab;
        //public RelayCommand RemoveTab
        //{
        //    get
        //    {
        //        return removeTab ??
        //          (removeTab = new RelayCommand(obj =>
        //          {
        //              TabModel tabModel = obj as TabModel;
        //              if (tabModel != null )
        //              {
        //                  TabModels.Remove(tabModel);
        //              }
        //          },
        //         (obj) => TabModels.Count > 0));
        //    }
        //}
        private RelayCommand _removeTab;
        public RelayCommand RemoveTab => _removeTab ?? new RelayCommand(obj =>
        {
            if (obj is TabModel tab && TabModels.Contains(tab))
            {
                int idx = TabModels.IndexOf(tab);
                TabModels.Remove(tab);

                if (TabModels.Count > 0)
                {
                    SelectedTab = TabModels[Math.Min(idx, TabModels.Count - 1)];
                }
            }
        }, _ => TabModels.Count > 0);
        public TabModel SelectedTab
        {
            get { return _selectedTab; }
            set
            {
                if (_selectedTab != value)
                {
                    _selectedTab = value;
                    OnPropertyChanged("SelectedTab");
                }
            }
        }
        public TabViewModel()
        {
            TabModels = new ObservableCollection<TabModel>
            {
                new TabModel{ Title="Вкладка 1", TextContent="Содержимое текстбокса 1" },
                new TabModel{ Title="Вкладка 2", TextContent="Содержимое текстбокса 2" }
            };
            SelectedTab = TabModels[0];
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop="") {
            if (PropertyChanged != null) { 
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
            }
        }
    }
}
