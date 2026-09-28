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

namespace MatutosApp.ViewsModels
{
    public partial class ConfiguraNotificacaoConsultarViewModel : BaseViewModel
    {
        private readonly NotificacaoService _notificacaoService;

        [ObservableProperty] ObservableCollection<Configura_Notificacao> listaRegraNotificacao = new();
        [ObservableProperty] private bool? ativo;
        [ObservableProperty] private DateTime dataInicial = DateTime.Today.AddDays(-15);
        [ObservableProperty] private DateTime dataFinal = DateTime.Today;
        [ObservableProperty] private Configura_Notificacao regraClicada;
        public List<string> OpcoesStatus { get; } = new List<string> { "Todos", "Ativos", "Inativos" };
        [ObservableProperty] private string statusSelecionado = "Todos";
        public ConfiguraNotificacaoConsultarViewModel(NotificacaoService notificacaoService)
        {
            _notificacaoService = notificacaoService;
        }

        [RelayCommand]
        public async Task AbrirRegraNotificacao()
        {
           await Shell.Current.GoToAsync(nameof(ConfiguraNotificacaoCadastrarView));
        }

        [RelayCommand]
        public async Task ConsultarRegraNotificacao()
        {
            try
            {
                string token = await SecureStorage.Default.GetAsync("jwt_token");

                if (StatusSelecionado == "Ativos")
                    Ativo = true;
                else if (StatusSelecionado == "Inativos")
                    Ativo = false;
                else
                    Ativo = null;

                DateTime inicial = DataInicial;
                DateTime final = DataFinal.Date.AddHours(23).AddMinutes(59);

                var resultado = await _notificacaoService.ConsultarRegraNotificacao(token, inicial, final, Ativo);

                if(resultado.Sucesso && resultado.Dados != null)
                {
                    ListaRegraNotificacao.Clear();

                    foreach(var notificacao in resultado.Dados)
                    {
                        ListaRegraNotificacao.Add(notificacao);
                    }
                }
                 else
                {
                    await Application.Current.MainPage.DisplayAlert("Atenção", "Falha para carrega as regras de notificações.", "OK");
                }
            }
            catch(Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Erro", $"Não foi possível carregar as regras de notificações. Erro: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        public async Task AbrirAlterarRegra(Configura_Notificacao regra)
        {
            bool confirmar = await Application.Current.MainPage.DisplayAlert("Atenção", "Deseja realizar a alteração do registro selecionado?", "Sim", "Não");

            if(!confirmar)
            {
                return;
            }

            if (regra == null) return;

            var regraAlterar = new Configura_Notificacao
            {
                Codigo_Notificacao = regra.Codigo_Notificacao,
                Ativo = regra.Ativo,
                Descricao = regra.Descricao,
                Mensagem = regra.Mensagem,
                Codigo_Tipo = regra.Codigo_Tipo,
                Valor = regra.Valor,
                UnidadeTempo = regra.UnidadeTempo
            };

            var parametro = new Dictionary<string, object>
            {
                {"RegraSelecionada", regraAlterar }
            };

            await Shell.Current.GoToAsync(nameof(ConfiguraNotificacaoCadastrarView), parametro );
        }

        [RelayCommand]
        public async Task Voltar()
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}
