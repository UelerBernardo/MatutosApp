using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MatutosApp.Services;
using MatutosDomain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatutosApp.ViewsModels
{
    [QueryProperty(nameof(IdLog), "Log")]
    public partial class AuditoriaLogDetalhesViewModel : BaseViewModel
    {
        private readonly AuditoriaLogService _auditoriaLogService;
        [ObservableProperty] private int idLog;
        [ObservableProperty] private AuditoriaLogDetalhesDTO auditoriaDetalhes;

        public AuditoriaLogDetalhesViewModel( AuditoriaLogService auditoriaLogService) 
        {
            _auditoriaLogService = auditoriaLogService;
        }

        [RelayCommand]
        public async Task LogConsultarDetalhes()
        {
            try
            {
                string? token = await SecureStorage.Default.GetAsync("jwt_token");


                var resultado = await _auditoriaLogService.LogConsultarDetalhes(token, IdLog);

                if(resultado.Sucesso)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        AuditoriaDetalhes = resultado.Dados;
                    });
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Antenção", resultado.Mensagem, "Ok");
                    return;
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Erro", $"Não foi possível carregar os detalhes do log. Erro: {ex.Message}", "OK");
                return;
            }
        }
    }
}
