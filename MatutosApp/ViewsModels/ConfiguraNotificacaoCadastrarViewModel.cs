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
    public partial class ConfiguraNotificacaoCadastrarViewModel : BaseViewModel
    {
        private readonly NotificacaoService _notificacaoService;
        [ObservableProperty] private Tipo_Evento tipoEventoSelecionado;
        [ObservableProperty] private bool ativo = true;
        [ObservableProperty] private string mensagem;
        [ObservableProperty] private string descricao;
        [ObservableProperty] private int valor;
        [ObservableProperty] private UnidadeTempoEnum unidadeTempo;
        [ObservableProperty] private ObservableCollection<Tipo_Evento> tiposEventos = new ObservableCollection<Tipo_Evento>();
        public List<UnidadeTempoEnum> UnidadesDeTempo { get; } = Enum.GetValues(typeof(UnidadeTempoEnum)).Cast<UnidadeTempoEnum>().ToList();
        public ConfiguraNotificacaoCadastrarViewModel(NotificacaoService notificacaoService)
        {
            _notificacaoService = notificacaoService;
            _ = ConsultarTipoEvento();
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

            if (TipoEventoSelecionado.Codigo_Tipo <= 0 || Mensagem.IsNullOrEmpty() || Descricao.IsNullOrEmpty() || Valor <= 0)
            {
                await Application.Current.MainPage.DisplayAlert("Atenção", "Por favor preencha todos os campos.", "Ok");
                return;
            }

            try
            {
                var notificacaoNova = new Configura_Notificacao
                {
                    Ativo = Ativo,
                    Codigo_Tipo = TipoEventoSelecionado.Codigo_Tipo,
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
    }
}
