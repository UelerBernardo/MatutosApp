using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MatutosApp.Services;
using MatutosDomain;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatutosApp.ViewsModels
{
    [QueryProperty(nameof(RegraSelecionada), "RegraSelecionada")]
    public partial class ConfiguraNotificacaoCadastrarViewModel : BaseViewModel
    {
        private readonly NotificacaoService _notificacaoService;

        [ObservableProperty] private bool ativo = true;
        [ObservableProperty] private string mensagem;
        [ObservableProperty] private string descricao;
        [ObservableProperty] private int? valor;
        [ObservableProperty] private UnidadeTempoEnum? unidadeTempo;
        [ObservableProperty] private ObservableCollection<Tipo_Evento> tiposEventos = new ObservableCollection<Tipo_Evento>();

        [ObservableProperty] private bool isValorVisivel = true;
        [ObservableProperty] private bool isUnidadeTempoVisivel = true;
        // Propriedades de controle de tela (geradas automaticamente pelo Toolkit)
        [ObservableProperty] private bool isValorHabilitado = true;
        [ObservableProperty] private bool isUnidadeTempoHabilitada = true;

        [ObservableProperty] private Configura_Notificacao? regraSelecionada;

        public List<UnidadeTempoEnum> UnidadesDeTempo { get; set; } = Enum.GetValues(typeof(UnidadeTempoEnum)).Cast<UnidadeTempoEnum>().ToList();
        public ConfiguraNotificacaoCadastrarViewModel(NotificacaoService notificacaoService)
        {
            _notificacaoService = notificacaoService;
            _ = ConsultarTipoEvento();
        }

        private Tipo_Evento _tipoSelecionado;
        public Tipo_Evento TipoSelecionado
        {
            get => _tipoSelecionado;
            set
            {
                if (SetProperty(ref _tipoSelecionado, value))
                {
                    AplicarRegrasDeTela();
                }
            }
        }


        private void ModoCadastro()
        {
            Ativo = true;
            Descricao = string.Empty;
            Mensagem = string.Empty;
            Valor = 0;
            UnidadeTempo = UnidadeTempoEnum.Minutos;
            TipoSelecionado
        }

        private void AplicarRegrasDeTela()
        {
            if (TipoSelecionado == null || string.IsNullOrWhiteSpace(TipoSelecionado.Nome)) return;

            // 👉 CORREÇÃO: Voltamos a testar o Nome (igual está no XAML) e ignorando maiúsculas/minúsculas
            if (TipoSelecionado.Nome.Trim().Equals("Promocional", StringComparison.OrdinalIgnoreCase))
            {
                // Regra: Ficam INVISÍVEIS
                IsValorVisivel = false;
                IsUnidadeTempoVisivel = false;

                Valor = null;
                UnidadeTempo = null;
            }
            else if (TipoSelecionado.Nome.Trim().Equals("Inatividade", StringComparison.OrdinalIgnoreCase))
            {
                // Regra: Ficam visíveis, mas a Unidade de tempo fica SOMENTE LEITURA
                IsValorVisivel = true;
                IsUnidadeTempoVisivel = true;

                IsValorHabilitado = true;
                IsUnidadeTempoHabilitada = false; // Trava o campo

                UnidadeTempo = UnidadesDeTempo.FirstOrDefault(u => (int)u == 3);
            }
            else
            {
                // Regra padrão: Tudo visível e liberado
                IsValorVisivel = true;
                IsUnidadeTempoVisivel = true;
                IsValorHabilitado = true;
                IsUnidadeTempoHabilitada = true;
            }
        }


        [RelayCommand]
        public async Task ConsultarTipoEvento()
        {
            string token = await SecureStorage.Default.GetAsync("jwt_token");

            try
            {
                var resultado = await _notificacaoService.ConsultarTipoEvento(token);

                if(resultado.Sucesso && resultado.Dados != null)
                {
                    TiposEventos.Clear();

                    foreach(var evento in resultado.Dados)
                    {
                        TiposEventos.Add(evento);
                    }
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Atenção", "Não foi possível carregar os tipos de evento.", "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Erro", $"Falha ao consultar tipos: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        public async Task CadastrarConfiguracaoNotificacao()
        {
            string? token = await SecureStorage.Default.GetAsync("jwt_token");
            if (string.IsNullOrWhiteSpace(token))
            {
                await Application.Current.MainPage.DisplayAlert("Sessão Expirada", "Faça login novamente para realizar o cadastro.", "Ok");
                await Shell.Current.GoToAsync("///LoginView"); // Redireciona para o login
                return;
            }

            try
            {
                var notificacaoNova = new Configura_Notificacao
                {
                    Ativo = Ativo,
                    Codigo_Tipo = TipoSelecionado.Codigo_Tipo,
                    Mensagem = Mensagem,
                    Descricao = Descricao,
                    Valor = Valor,
                    UnidadeTempo = UnidadeTempo
                };

                var resultado = await _notificacaoService.CadastrarNotificacoes(token, notificacaoNova);

                if(resultado.Sucesso)
                {
                    await Application.Current.MainPage.DisplayAlert("Sucesso", resultado.Mensagem, "Ok");
                    await Shell.Current.GoToAsync("..");

                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Atenção", resultado.Mensagem, "Ok");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Erro", $"Não foi possível cadastrar as regras de notificação. Erro: {ex.Message}", "OK");
            }

        }

        [RelayCommand]
        public async Task AlterarNotificacaoRegra()
        {
            try
            {
                string token = await SecureStorage.Default.GetAsync("jwt_token");


                var notificacaoAlterada = new Configura_Notificacao
                {
                    Codigo_Notificacao = regraSelecionada.Codigo_Notificacao,
                    Ativo = Ativo,
                    Codigo_Tipo = TipoSelecionado.Codigo_Tipo,
                    Mensagem = Mensagem,
                    Descricao = Descricao,
                    Valor = Valor,
                    UnidadeTempo = UnidadeTempo
                };

                var resultado = await _notificacaoService.AlterarRegraNotificacao(token, notificacaoAlterada);

                if(resultado.Sucesso)
                {
                    await Application.Current.MainPage.DisplayAlert("Sucesso", resultado.Mensagem, "Ok");
                    await Shell.Current.GoToAsync("..");
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Falha", resultado.Mensagem, "Ok");
                    return;
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Erro", $"Não foi possível alterar as regras da notificação. Erro: {ex.Message}", "OK");
            }
        }
    }
}
