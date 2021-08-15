using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GUIBase;
namespace TestClient
{
    class TemplateWindowService : IWindowService
    {
        public void OpenWindow(ViewModelBase vm)
        {
            TemplateViewModel templateVm = vm as TemplateViewModel;
            TemplateWindow window = new TemplateWindow();
            templateVm.CloseAction = window.Close;
            window.DataContext = templateVm;
            window.Focus();
            
            window.ShowDialog();
        }
    }
}
