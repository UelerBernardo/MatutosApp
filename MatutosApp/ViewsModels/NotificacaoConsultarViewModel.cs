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

        // Antes era: ObservableCollection<Notificacao>
        [ObservableProperty] ObservableCollection<NotificacaoResponseDTO> listaNotificacao = new();
        public NotificacaoConsultarViewModel(NotificacaoService? notificacaoService)
        {
            _notificacaoService = notificacaoService;
        }

        [RelayCommand]
        public async Task ConsultarNotificacao()
        {
            try
            {
                string? token = await SecureStorage.Default.GetAsync("jwt_token");

                var resposta = await _notificacaoService.ConsultarNotificacao(token);

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
