using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MatutosApp.Services;
using MatutosApp.Views;
using MatutosDomain;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace MatutosApp.ViewsModels
{
    public partial class AgendamentoMeusServicosViewModel : BaseViewModel
    {
        private readonly AgendamentoService _agendamentoService;

        [ObservableProperty] private DateTime dataInicial = DateTime.Today;
        [ObservableProperty] private DateTime dataFinal = DateTime.Today;
        [ObservableProperty] private ObservableCollection<MeusServicos> listaAgendamneto = new();
        [ObservableProperty] private string? nomeUsuario;
        [ObservableProperty] private string situacaoSelecionada = "Todos";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Mensagem))]
        private bool isAdm = false;
        public string Mensagem => IsAdm == false ? "Cliente:" : "Barbeiro:";
        public ObservableCollection<string> FiltroSituacao { get; }

        public AgendamentoMeusServicosViewModel(AgendamentoService agendamentoService)
        {
            _agendamentoService = agendamentoService;

            var usuarioLogado = UsuarioSessaoService.UsuarioLogado;

            if (usuarioLogado.TipoSelecionado == UsuarioTipo.Administrador)
            {
                IsAdm = true;
            }

            // 3. Montamos a lista: colocamos o "Todos" primeiro e depois os itens do Enum
            FiltroSituacao = new ObservableCollection<string> { "Todos" };
            foreach (var nomeSituacao in Enum.GetNames(typeof(AgendamentoSituacao)))
            {
                FiltroSituacao.Add(nomeSituacao);
            }
        }

        [RelayCommand]
        public async Task AbrirDetalhes(int codigoAgendamento)
        {
            await Shell.Current.GoToAsync($"{nameof(AgendamentoDetalhesView)}?Agendamento={codigoAgendamento}");
        }

        [RelayCommand]
        public async Task MeusServicos()
        {
            try
            {
                string token = await SecureStorage.Default.GetAsync("jwt_token");
                DateTime inicial = DataInicial.Date;

                DateTime final = DataFinal.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                AgendamentoSituacao? situacaoParaEnviar = null;

                // Se não for "Todos", tentamos converter a string exata para o Enum
                if (SituacaoSelecionada != "Todos" && Enum.TryParse<AgendamentoSituacao>(SituacaoSelecionada, out var valorEnum))
                {
                    situacaoParaEnviar = valorEnum;
                }

                // 5. Enviamos a variável convertida (que será null se o usuário escolheu "Todos")
                var resultado = await _agendamentoService.ConsultarMeusServicos(token, inicial, final, NomeUsuario, situacaoParaEnviar, IsAdm);

                if (resultado.Sucesso && resultado.Dados != null)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        ListaAgendamneto.Clear();
                        foreach (var item in resultado.Dados)
                        {
                            ListaAgendamneto.Add(item);
                        }
                    });
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Atenção", resultado.Mensagem, "Ok");
                    return;
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Erro", $"Não foi possível carregar os agendamentos. Erro: {ex.Message}", "OK");
            }
        }
    }
}