using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MatutosApp.Services;
using MatutosDomain;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatutosApp.ViewsModels
{
    public partial class NotificacaoConsultarViewModel : BaseViewModel
    {
        private readonly NotificacaoService? _notificacaoService;

        [ObservableProperty]
        private DateTime dataInicial = DateTime.Today.AddDays(-7);

        [ObservableProperty]
        private DateTime dataFinal = DateTime.Today;

        [ObservableProperty] ObservableCollection<NotificacaoResponseDTO> listaNotificacao = new();
        [ObservableProperty]
        private bool isPopupVisible;
        [ObservableProperty] private NotificacaoDetalhes notificacaoDetalhes = new();

        public NotificacaoConsultarViewModel(NotificacaoService? notificacaoService)
        {
            _notificacaoService = notificacaoService;
        }

        [RelayCommand]
        public async Task MarcarTodasLidas()
        {
            try
            {
                bool confirmacao = await Application.Current.MainPage.DisplayAlert("Atenção", "Deseja realmente marcar todas as notificações como \"Visualizada\"?", "Sim", "Não");
                if(!confirmacao)
                {
                    return;
                }

                string? token = await SecureStorage.Default.GetAsync("jwt_token");

                var resultado = await _notificacaoService.NotificacaoVisualizarTodas(token);

                if(resultado.Sucesso)
                {
                    await Application.Current.MainPage.DisplayAlert("Sucesso", resultado.Mensagem, "Ok");
                    _=ConsultarNotificacao();
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Atenção", resultado.Mensagem, "Ok");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Erro", $"Não foi possível alterar as notificações. Erro: {ex.Message}", "OK");
            }
        }


        [RelayCommand]
        public void FecharPopup()
        {
            IsPopupVisible = false;
            NotificacaoDetalhes = null; // Limpa os dados ao fechar
        }

        [RelayCommand]
        public async Task Voltar()
        {
            await Shell.Current.GoToAsync("..");
        }

        [RelayCommand]
        public async Task ConsultarDetalhesNotificacao(NotificacaoResponseDTO notificacao)
        {
            try
            {
                string? token = await SecureStorage.Default.GetAsync("jwt_token");


                var resultado = await _notificacaoService.ConsultarNotificacaoDetalhes(token, notificacao.Codigo_Historico);

                if (resultado.Sucesso && resultado.Dados != null)
                {
                    NotificacaoDetalhes = resultado.Dados; 
                    IsPopupVisible = true;                 
                    ConsultarNotificacao();


                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Atenção", resultado.Mensagem, "Ok");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Erro", $"Não foi possível carregar as notificações. Erro: {ex.Message}", "OK");
            }
        }


        [RelayCommand]
        public async Task ConsultarNotificacao()
        {
            try
            {
                string? token = await SecureStorage.Default.GetAsync("jwt_token");
                DateTime inicial = DataInicial.Date;

                DateTime final = DataFinal.Date.AddHours(23).AddMinutes(59);

                var resposta = await _notificacaoService.ConsultarNotificacao(token, inicial, final);

                if(resposta.Sucesso && resposta.Dados != null)
                {
                    ListaNotificacao.Clear();

                    foreach(var item in resposta.Dados)
                    {
                        ListaNotificacao.Add(item);
                    }
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Atenção", "Falha para carrega as notificações.", "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Erro", $"Não foi possível carregar as notificações. Erro: {ex.Message}", "OK");
            }
        }

    }
}

