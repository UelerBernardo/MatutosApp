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

                var resultado = await _notificacaoService.ConsultarRegraNotificacao(token);

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
    }
}
