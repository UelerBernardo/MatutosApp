using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatutosApp.ViewsModels
{
    public partial class SobreNosViewModel : BaseViewModel
    {
        public SobreNosViewModel()
        {
            // Construtor vazio
        }

        [RelayCommand]
        public async Task VoltarTela()
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}
