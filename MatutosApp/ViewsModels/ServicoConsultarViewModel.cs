using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MatutosApp.Services;
using MatutosApp.Views;
using MatutosDomain;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatutosApp.ViewsModels
{
    public partial class ServicoConsultarViewModel : BaseViewModel
    {
        private readonly ServicoService _servicoService;

        [ObservableProperty] int servicoSelecionado;

        [ObservableProperty]
        private ObservableCollection<Servico> listaServicos = new ObservableCollection<Servico>();
        
        public ServicoConsultarViewModel(ServicoService servicoService)
        {
            _servicoService = servicoService;

            _ = ConsultarServico();
        }



        [RelayCommand]
        public async Task ConsultarServico()
        {
            try 
            { 
                var token = await SecureStorage.Default.GetAsync("jwt_token");

                if(token == null)
                {
                    await Application.Current.MainPage.DisplayAlert("Atenção", "O usuário está desconectado no Sistema", "Ok");
                    return;
                }

                var resultado = await _servicoService.Consultar(token);

                if(resultado.Sucesso)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        ListaServicos.Clear();

                        if (resultado.Dados != null)
                        {
                            foreach (var servico in resultado.Dados)
                            {
                                ListaServicos.Add(servico);
                            }
                        }
                    });
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Atenção", resultado.Mensagem, "OK");
                }
            }
            catch(Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Erro", $"Não foi possível carregar os serviços. Erro: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        public async Task AbrirCadastroServico()
        {
            await Shell.Current.GoToAsync(nameof(ServicoCadastroView));
        }
    }
}
