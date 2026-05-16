using CommunityToolkit.Mvvm.Input;
using MatutosApp.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatutosApp.ViewsModels
{
    public partial class PrincipalViewModel : BaseViewModel
    {


        [RelayCommand]
        public async Task AbrirAgendamento()
        {
            await Shell.Current.GoToAsync(nameof(AgendamentoCadastroView));
        }

        [RelayCommand]
        public async Task MeusAgendamentos()
        {
            await Shell.Current.GoToAsync(nameof(AgendamentoConsultarView));
        }

        [RelayCommand]
        public async Task PerfilAbrir()
        {
            await Shell.Current.GoToAsync(nameof(ClientePerfilConsultarView));
        }
    }
}
