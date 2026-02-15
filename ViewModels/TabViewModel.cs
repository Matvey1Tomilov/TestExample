using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TestExample.Commands;
using TestExample.Models;
using System.Windows;
using TestExample.Views;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Concurrent;
using System.Linq;
using System.Windows.Input;

namespace TestExample.ViewModels
{
    public class TabViewModel : INotifyPropertyChanged
    {
        private TabModel _selectedTab;
        public ObservableCollection<TabModel> TabModels { get; set; }
        private TabModel _draggedTab;

        private readonly ConcurrentDictionary<TabModel, CancellationTokenSource> _pendingDeletions =
            new ConcurrentDictionary<TabModel, CancellationTokenSource>();

        private RelayCommand _addTab;
        private RelayCommand _removeTab;
        private RelayCommand _removeTabAwait;
        private RelayCommand _cancelDelayedDelete;
        private RelayCommand _renameTab;
        private RelayCommand _startDragCommand;
        private RelayCommand _cancelDragCommand;
        private RelayCommand _moveTabCommand;

        public TabModel SelectedTab
        {
            get { return _selectedTab; }
            set
            {
                if (_selectedTab != value)
                {
                    _selectedTab = value;
                    OnPropertyChanged(nameof(SelectedTab));
                }
            }
        }

        public TabViewModel()
        {
            TabModels = new ObservableCollection<TabModel>
            {
                new TabModel { Title = "Вкладка 1", TextContent = "Содержимое текстбокса 1" },
                new TabModel { Title = "Вкладка 2", TextContent = "Содержимое текстбокса 2" }
            };
            SelectedTab = TabModels[0];
        }
        public RelayCommand AddTab
        {
            get
            {
                if (_addTab == null)
                {
                    _addTab = new RelayCommand(obj =>
                    {
                        int index = TabModels.Count + 1;
                        var tabModel = new TabModel(index, "", true);
                        TabModels.Add(tabModel);
                        SelectedTab = tabModel;
                    });
                }
                return _addTab;
            }
        }

        public RelayCommand RemoveTab
        {
            get
            {
                if (_removeTab == null)
                {
                    _removeTab = new RelayCommand(
                        execute: obj =>
                        {
                            if (obj is TabModel tab && TabModels.Contains(tab))
                            {
                                if (_pendingDeletions.TryGetValue(tab, out var cts))
                                {
                                    cts.Cancel();
                                }

                                int index = TabModels.IndexOf(tab);
                                TabModels.Remove(tab);

                                if (TabModels.Count > 0)
                                    SelectedTab = TabModels[Math.Min(index, TabModels.Count - 1)];
                            }
                        },
                        canExecute: _ => TabModels.Count > 0
                    );
                }
                return _removeTab;
            }
        }

        public RelayCommand RemoveTabAwait
        {
            get
            {
                if (_removeTabAwait == null)
                {
                    _removeTabAwait = new RelayCommand(
                        execute: obj =>
                        {
                            if (obj is TabModel tab && TabModels.Contains(tab) && !tab.IsDeleting)
                            {
                                StartDelayedRemovalAsync(tab);
                            }
                        },
                        canExecute: obj => obj is TabModel tab && TabModels.Contains(tab) && !tab.IsDeleting
                    );
                }
                return _removeTabAwait;
            }
        }

        public RelayCommand CancelDelayedDelete
        {
            get
            {
                if (_cancelDelayedDelete == null)
                {
                    _cancelDelayedDelete = new RelayCommand(
                        execute: obj =>
                        {
                            if (obj is TabModel tab && _pendingDeletions.TryGetValue(tab, out var cts))
                            {
                                cts.Cancel();
                            }
                        },
                        canExecute: obj => obj is TabModel tab && tab.IsDeleting
                    );
                }
                return _cancelDelayedDelete;
            }
        }

        public RelayCommand RenameTab
        {
            get
            {
                if (_renameTab == null)
                {
                    _renameTab = new RelayCommand(
                        execute: obj =>
                        {
                            if (obj is TabModel tab)
                            {
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    var dialog = new RenameDialog(tab.Title);
                                    bool? result = dialog.ShowDialog();

                                    if (result == true && !string.IsNullOrWhiteSpace(dialog.NewTitle))
                                    {
                                        tab.Title = dialog.NewTitle;
                                    }
                                });
                            }
                        },
                        canExecute: obj => obj is TabModel && TabModels.Count > 0
                    );
                }
                return _renameTab;
            }
        }

        public RelayCommand StartDragCommand
        {
            get
            {
                if (_startDragCommand == null)
                {
                    _startDragCommand = new RelayCommand(obj =>
                    {
                        if (obj is TabModel tab)
                        {
                            _draggedTab = tab;
                        }
                    });
                }
                return _startDragCommand;
            }
        }

        public RelayCommand MoveTabCommand
        {
            get
            {
                if (_moveTabCommand == null)
                {
                    _moveTabCommand = new RelayCommand(
                        execute: obj =>
                        {
                            if (obj is TabModel targetTab && _draggedTab != null && _draggedTab != targetTab)
                            {
                                int startIndex = TabModels.IndexOf(_draggedTab);
                                int endIndex = TabModels.IndexOf(targetTab);

                                if (startIndex >= 0 && endIndex >= 0 && startIndex != endIndex)
                                {
                                    int insertIndex = endIndex > startIndex ? endIndex - 1 : endIndex;
                                    TabModels.Move(startIndex, insertIndex);
                                    SelectedTab = _draggedTab;
                                }

                                _draggedTab = null;
                            }
                        },
                        canExecute: obj => obj is TabModel tab && _draggedTab != null && _draggedTab != tab
                    );
                }
                return _moveTabCommand;
            }
        }

        public RelayCommand CancelDragCommand
        {
            get
            {
                if (_cancelDragCommand == null)
                {
                    _cancelDragCommand = new RelayCommand(_ =>
                    {
                        _draggedTab = null;
                    });
                }
                return _cancelDragCommand;
            }
        }

        private async void StartDelayedRemovalAsync(TabModel tab)
        {
            var cts = new CancellationTokenSource();

            if (!_pendingDeletions.TryAdd(tab, cts))
            {
                cts.Dispose();
                return;
            }

            tab.IsDeleting = true;
            tab.RemainingSeconds = 5;

            try
            {
                await Task.Run(async () =>
                {
                    for (int i = 5; i > 0; i--)
                    {
                        if (cts.Token.IsCancellationRequested)
                            break;

                        await Application.Current.Dispatcher.InvokeAsync(() =>
                        {
                            if (TabModels.Contains(tab))
                                tab.RemainingSeconds = i;
                        });

                        try
                        {
                            await Task.Delay(1000, cts.Token);
                        }
                        catch (OperationCanceledException)
                        {
                            break;
                        }
                    }
                }, cts.Token);

                if (!cts.Token.IsCancellationRequested && TabModels.Contains(tab))
                {
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        int index = TabModels.IndexOf(tab);
                        TabModels.Remove(tab);
                        if (TabModels.Count > 0)
                            SelectedTab = TabModels[Math.Min(index, TabModels.Count - 1)];
                    });
                }
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    if (TabModels.Contains(tab))
                    {
                        tab.IsDeleting = false;
                        tab.RemainingSeconds = 0;
                    }
                });

                if (_pendingDeletions.TryRemove(tab, out var removedCts))
                {
                    removedCts.Dispose();
                }
            }
        }

        public void CancelAllPendingDeletions()
        {
            foreach (var kvp in _pendingDeletions.ToArray())
            {
                kvp.Value.Cancel();  
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}