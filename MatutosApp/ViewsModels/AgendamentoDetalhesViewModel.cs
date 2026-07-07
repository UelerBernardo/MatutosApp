using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MatutosApp.Services;
using MatutosApp.Views;
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

        [ObservableProperty]
        Agendamento dadosDoAgendamento = new Agendamento
        {
            Cliente = new Cliente(),
            Barbeiro = new Barbeiro(),
            Agendamento_Servicos = new List<Agendamento_Servico>()
        };

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

        [RelayCommand]
        public async Task CancelarAgendamento()
        {
            if (DadosDoAgendamento != null && DadosDoAgendamento.Codigo_Situacao_Agendamento != AgendamentoSituacao.Aberto && 
                                              DadosDoAgendamento.Codigo_Situacao_Agendamento != AgendamentoSituacao.Liberado)
            {
                await Application.Current.MainPage.DisplayAlert("Aviso", "Somente é possível cancelar registros na situação 'Aberto' ou 'Liberado'.", "Ok");
                return;
            }

            bool confirmar = await Shell.Current.DisplayAlert("Atenção", "Deseja realmente cancelar o agendamento?", "Sim", "Não");

            if (!confirmar)
            {
                return;
            }
            else
            {
                string token = await SecureStorage.Default.GetAsync("jwt_token");
                if (AgendamentoID == null)
                {
                    await Application.Current.MainPage.DisplayAlert("Atenção", "O agendamento não foi encontrado.", "Ok");
                    return;
                }

                if(DadosDoAgendamento.Data_Fim_Agendamento != null || DadosDoAgendamento.Valor_Total_Agendamento > 0)
                {
                    var resultado = await _agendamentoService.AgendamentoAlterarSituacao(AgendamentoID, token, AgendamentoSituacao.Cancelado);

                    if(resultado.Sucesso)
                    {
                        await Application.Current.MainPage.DisplayAlert("Aviso", "Agendamento cancelado com sucesso!", "Ok");
                        await Shell.Current.GoToAsync("///PrincipalView");
                    }
                }
                else
                { 

                    var resultado = await _agendamentoService.AgendamentoExcluir(AgendamentoID, token);

                if (resultado.Sucesso)
                {
                    await Application.Current.MainPage.DisplayAlert("Sucesso", "Agendamento cancelado.", "Ok");
                    await Shell.Current.GoToAsync("///PrincipalView");
                    }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Atenção", resultado.Mensagem, "Ok");
                }

                }
            }
        }

        [RelayCommand]
        public async Task LiberarAgendamento()
        {
            if (DadosDoAgendamento != null && DadosDoAgendamento.Codigo_Situacao_Agendamento != AgendamentoSituacao.Aberto)
            {
                await Application.Current.MainPage.DisplayAlert("Aviso", "Somente é possível liberar registros na situção 'Aberto'.", "Ok");
                return; 
            }
            bool confirmar = await Shell.Current.DisplayAlert("Atenção", "Deseja realmente liberar o agendamento?", "Sim", "Não");
            if (!confirmar)
            {
                return;
            }
            else { 
                string token = await SecureStorage.Default.GetAsync("jwt_token");

                if (AgendamentoID <= 0)
                {
                    await Application.Current.MainPage.DisplayAlert("Atenção", "O agendamento não foi encontrado", "Ok");
                    return;
                }

                var resultado = await _agendamentoService.AgendamentoAlterarSituacao(AgendamentoID, token, AgendamentoSituacao.Liberado);

                if (resultado.Sucesso)
                {
                    await Application.Current.MainPage.DisplayAlert("Sucesso", "Agendamento liberado.", "Ok");
                    //await Shell.Current.GoToAsync(nameof(PrincipalView));
                    await Shell.Current.GoToAsync("///PrincipalView");
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Atenção", resultado.Mensagem, "Ok");
                }
            }
        }
    }
}
