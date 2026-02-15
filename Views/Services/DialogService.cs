using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestExample.Views;
using System.Windows;
 namespace TestExample.Views.Services
{
    public class DialogService:IDialogService
    {
        public Task<string> ShowRenameDialogAsync(string currentTitle)
        {
            return Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var dialog = new RenameDialog(currentTitle);
                bool? result = dialog.ShowDialog();
                return result == true ? dialog.NewTitle : null;
            }).Task;
        }
    }
}
