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

        [ObservableProperty] private bool podeEditar = true;

        [ObservableProperty] private Configura_Notificacao? regraSelecionada;

        public List<UnidadeTempoEnum> UnidadesDeTempo { get; set; } = Enum.GetValues(typeof(UnidadeTempoEnum)).Cast<UnidadeTempoEnum>().ToList();

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(NomeBotaoAcao))]
        [NotifyPropertyChangedFor(nameof(Titulo))]
        private AcaoTela _acaoTela = AcaoTela.Cadastro;

        public string Titulo => AcaoTela == AcaoTela.Cadastro ? "Configurar Avisos e Alertas" : "Alterar Avisos e Alertas";
        public string NomeBotaoAcao => AcaoTela == AcaoTela.Cadastro ? "Cadastrar" : "Alterar Regra";

        public ConfiguraNotificacaoCadastrarViewModel(NotificacaoService notificacaoService)
        {
            _notificacaoService = notificacaoService;
            _ = ConsultarTipoEvento();
            DefinirModoTela();
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

        partial void OnRegraSelecionadaChanged(Configura_Notificacao? value)
        {
            if (value != null)
            {
                AcaoTela = AcaoTela.Alteração;
            }
            DefinirModoTela();
        }

        private void DefinirModoTela()
        {
            if (AcaoTela == AcaoTela.Cadastro)
            {
                PodeEditar = true;
                Ativo = true;
                Mensagem = string.Empty;
                Descricao = string.Empty;
                Valor = 0;
                UnidadeTempo = UnidadeTempoEnum.Dias;
                TipoSelecionado = TiposEventos.FirstOrDefault();
            }
            else
            {
                if (RegraSelecionada != null)
                {
                    PodeEditar = false;
                    Ativo = RegraSelecionada.Ativo;
                    Mensagem = RegraSelecionada.Mensagem ?? string.Empty;
                    Descricao = RegraSelecionada.Descricao ?? string.Empty;
                    Valor = RegraSelecionada.Valor;
                    UnidadeTempo = RegraSelecionada.UnidadeTempo;

                    // 👉 CORREÇÃO 1: Mudança de .Where para .FirstOrDefault para bater com a tipagem
                    TipoSelecionado = TiposEventos.FirstOrDefault(tipo => tipo.Codigo_Tipo == RegraSelecionada.Codigo_Tipo);
                }
            }
        }

        private void AplicarRegrasDeTela()
        {
            if (TipoSelecionado == null || string.IsNullOrWhiteSpace(TipoSelecionado.Nome)) return;

            if (TipoSelecionado.Nome.Trim().Equals("Promocional", StringComparison.OrdinalIgnoreCase))
            {
                IsValorVisivel = false;
                IsUnidadeTempoVisivel = false;
                Valor = null;
                UnidadeTempo = null;
            }
            else if (TipoSelecionado.Nome.Trim().Equals("Inatividade", StringComparison.OrdinalIgnoreCase))
            {
                IsValorVisivel = true;
                IsUnidadeTempoVisivel = true;
                IsValorHabilitado = true;
                IsUnidadeTempoHabilitada = false; // Trava o campo
                UnidadeTempo = UnidadesDeTempo.FirstOrDefault(u => (int)u == 3);
            }
            else
            {
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

                if (resultado.Sucesso && resultado.Dados != null)
                {
                    TiposEventos.Clear();

                    foreach (var evento in resultado.Dados)
                    {
                        TiposEventos.Add(evento);
                    }

                    if(AcaoTela == AcaoTela.Cadastro)
                    {
                        TipoSelecionado = TiposEventos.FirstOrDefault();
                    }
                    if (AcaoTela == AcaoTela.Alteração && RegraSelecionada != null)
                    {
                        TipoSelecionado = TiposEventos.FirstOrDefault(tipo => tipo.Codigo_Tipo == RegraSelecionada.Codigo_Tipo);
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
        public async Task SalvarRegra()
        {
            // Validações básicas (não deixe enviar se o tipo ou a mensagem estiverem vazios)
            if (TipoSelecionado == null || string.IsNullOrWhiteSpace(Mensagem))
            {
                await Application.Current.MainPage.DisplayAlert("Atenção", "O Tipo de Evento e a Mensagem são obrigatórios.", "OK");
                return;
            }

            if (AcaoTela == AcaoTela.Cadastro)
            {
                await CadastrarConfiguracaoNotificacao();
            }
            else
            {
                await AlterarNotificacaoRegra();
            }
        }

        // Tirei a tag [RelayCommand] daqui, pois ele não deve ser chamado direto pelo XAML
        private async Task CadastrarConfiguracaoNotificacao()
        {
            string? token = await SecureStorage.Default.GetAsync("jwt_token");
            if (string.IsNullOrWhiteSpace(token))
            {
                await Application.Current.MainPage.DisplayAlert("Sessão Expirada", "Faça login novamente para realizar o cadastro.", "Ok");
                await Shell.Current.GoToAsync("///LoginView");
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

                if (resultado.Sucesso)
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

        // Tirei a tag [RelayCommand] daqui, pois ele não deve ser chamado direto pelo XAML
        private async Task AlterarNotificacaoRegra()
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

                if (resultado.Sucesso)
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