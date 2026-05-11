using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MatutosApp.Services;
using MatutosDomain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MatutosApp.ViewsModels
{
    public partial class AgendamentoConsultarViewModel : BaseViewModel
    {
        private readonly AgendamentoService _agendamentoService;
        [ObservableProperty]
        Agendamento dadosDoAgendamento = new Agendamento;

        public AgendamentoConsultarViewModel(AgendamentoService service) 
        {
            _agendamentoService = service;

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
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        DadosDoAgendamento = resultado.Dados;
                    });
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
    }
}
