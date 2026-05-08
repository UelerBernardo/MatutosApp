using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MatutosApp.Services;
using MatutosDomain;
using System;
using System.Threading.Tasks;

namespace MatutosApp.ViewsModels
{
    [QueryProperty(nameof(AgendamentoID), "Agendamento")]
    public partial class AgendamentoDetalhesViewModel : BaseViewModel
    {
        private readonly AgendamentoService _agendamentoService;

        [ObservableProperty] private int agendamentoID;

        [ObservableProperty] Agendamento dadosAgendamento; 

        public AgendamentoDetalhesViewModel(AgendamentoService agendamento) 
        {
            _agendamentoService = agendamento;
        }
        partial void OnAgendamentoIDChanged(int oldValue, int newValue)
        {
            _ = ConsultarDetalhesAgendamento();
        }

        [RelayCommand]
        public async Task ConsultarDetalhesAgendamento()
        {
            string token = await SecureStorage.Default.GetAsync("jwt_token");

            // Chamada com 'await' aguardando o resultado
            var resultado = await _agendamentoService.AgendamentoConsultarDetalhes(AgendamentoID, token);

            if (resultado.Sucesso)
            {
                // Guarda os dados na variável que o XAML está enxergando
                DadosAgendamento = resultado.Dados;
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Erro", resultado.Mensagem, "OK");
            }
        }
    }
}
