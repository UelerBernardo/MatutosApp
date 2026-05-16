using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MatutosApp.Services;
using MatutosApp.Views;
using MatutosDomain;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MatutosApp.ViewsModels
{
    [QueryProperty(nameof(Agendamento), "Codigo_Agendamento")]
    public partial class AgendamentoConsultarViewModel : BaseViewModel
    {
        private readonly AgendamentoService _agendamentoService;
        [ObservableProperty] private int agendamento;

        [ObservableProperty]
        private ObservableCollection<Agendamento> listaAgendamentos = new ObservableCollection<Agendamento>();

      
        public AgendamentoConsultarViewModel(AgendamentoService service) 
        {
            _agendamentoService = service;

            _ = ConsultarAgendamentos();

        }

        [RelayCommand]
        public async Task ConsultarAgendamentos()
        {
            try
            {
                string token = await SecureStorage.Default.GetAsync("jwt_token");

                var resultado = await _agendamentoService.AgendamentoConsultar(token);

                if(resultado.Sucesso)
                {
                    if(resultado.Dados != null && resultado.Dados.Any() )
                    {
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            ListaAgendamentos = new ObservableCollection<Agendamento>(resultado.Dados);
                        });
                    }
                    else
                    {
                        await Application.Current.MainPage.DisplayAlert("Atenção", "Você Ainda não possui Agendamentos!", "Ok");
                        return;
                    }
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Erro", resultado.Mensagem, "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Erro", $"Não foi possível carregar os serviços. Erro: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        public async Task AbrirDetalhes(int codigoAgendamento)
        {
            bool confirmar = await Shell.Current.DisplayAlert("Atenção", $"Deseja visualizar os Detalhes do Agendamento {codigoAgendamento}?", "Sim", "Não");
            if(!confirmar)
            {
                return;
            }
            else
            { 
                await Shell.Current.GoToAsync($"{nameof(AgendamentoDetalhesView)}?Agendamento={codigoAgendamento}");
            }
        }
    }
}
