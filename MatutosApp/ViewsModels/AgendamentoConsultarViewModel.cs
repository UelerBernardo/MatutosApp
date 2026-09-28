using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MatutosApp.Services;
using MatutosApp.Views;
using MatutosDomain;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MatutosApp.ViewsModels
{
    [QueryProperty(nameof(Agendamento), "Codigo_Agendamento")]
    public partial class AgendamentoConsultarViewModel : BaseViewModel
    {
        private readonly AgendamentoService _agendamentoService;

        [ObservableProperty] private int agendamento;
        [ObservableProperty] private string? nomeBarbeiro;
        [ObservableProperty] private AgendamentoSituacao _agendamentoSituacao;
        [ObservableProperty] private DateTime dataInicial = DateTime.Today.AddDays(-7);
        [ObservableProperty] private DateTime dataFinal = DateTime.Today;
        [ObservableProperty] private string situacaoSelecionada = "Todos";

        public ObservableCollection<string> FiltroSituacao { get; }

        [ObservableProperty]
        private ObservableCollection<MeusAgendamentos> listaAgendamentos = new ObservableCollection<MeusAgendamentos>();

        public AgendamentoConsultarViewModel(AgendamentoService service)
        {
            _agendamentoService = service;
            _ = ConsultarAgendamentos();

            FiltroSituacao = new ObservableCollection<string> { "Todos" };
            foreach (var nomeSituacao in Enum.GetNames(typeof(AgendamentoSituacao)))
            {
                FiltroSituacao.Add(nomeSituacao);
            }
        }

        [RelayCommand]
        public async Task ConsultarAgendamentos()
        {
            try
            {
                string token = await SecureStorage.Default.GetAsync("jwt_token");
                DateTime inicial = DataInicial.Date;
                DateTime final = DataFinal.Date.AddHours(23).AddMinutes(59).AddSeconds(59);

                AgendamentoSituacao? situacaoParaEnviar = null;

                if (SituacaoSelecionada != "Todos" && Enum.TryParse<AgendamentoSituacao>(SituacaoSelecionada, out var valorEnum))
                {
                    situacaoParaEnviar = valorEnum;
                }

                var resultado = await _agendamentoService.AgendamentoConsultar(token, nomeBarbeiro, situacaoParaEnviar, inicial, final);

                if (resultado.Sucesso)
                {
                    // Unificamos o MainThread para atualizar a UI independente de ter dados ou não
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        if (resultado.Dados != null && resultado.Dados.Any())
                        {
                            ListaAgendamentos = new ObservableCollection<MeusAgendamentos>(resultado.Dados);
                        }
                        else
                        {
                            ListaAgendamentos.Clear();
                        }
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

        [RelayCommand]
        public async Task AbrirDetalhes(int codigoAgendamento)
        {
            bool confirmar = await Shell.Current.DisplayAlert("Atenção", $"Deseja visualizar os Detalhes do Agendamento {codigoAgendamento}?", "Sim", "Não");

            if (!confirmar)
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