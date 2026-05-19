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
    public partial class UsuarioTelefoneConsultarViewModel : BaseViewModel
    {
        private readonly TelefoneService _telefoneService;

        [ObservableProperty] int telefoneSelecionado;

        [ObservableProperty]
        private ObservableCollection<UsuarioTelefone> listaTelefone = new ObservableCollection<UsuarioTelefone>();

        public UsuarioTelefoneConsultarViewModel( TelefoneService telefoneService) 
        { 
            _telefoneService = telefoneService;

            _ = ConsultarTelefone();
        }

        [RelayCommand]
        public async Task ConsultarTelefone()
        {
            try
            {
                string token = await SecureStorage.Default.GetAsync("jwt_token");

                var resultado = await _telefoneService.TelefoneConsultar(token);

                if (resultado.Sucesso)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        ListaTelefone.Clear();

                        if (resultado.Dados != null)
                        {
                            foreach (var tel in resultado.Dados)
                            {
                                ListaTelefone.Add(tel);
                            }
                        }
                    });
                }
                else
                {
                    if (resultado.Mensagem.Contains("não possui telefone"))
                    {
                        MainThread.BeginInvokeOnMainThread(() => ListaTelefone.Clear());
                    }
                    else
                    {
                        await Application.Current.MainPage.DisplayAlert("Atenção", resultado.Mensagem, "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Erro", $"Não foi possível carregar os serviços. Erro: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        public async Task AbrirAdicionarTelefone()
        {
            await Shell.Current.GoToAsync(nameof(TelefoneCadastroView));
        }

        [RelayCommand]
        public async Task ExcluirTelefone(int codigoTelefone)
        {
            try
            {
                bool confirmar = await Application.Current.MainPage.DisplayAlert("Excluir", "Deseja apagar este telefone?", "Sim", "Não");
                if (!confirmar) return;


                string token = await SecureStorage.Default.GetAsync("jwt_token");

                var resultado = await _telefoneService.TelefoneExcluir(token, codigoTelefone);


                if(resultado.Sucesso)
                {
                    await Application.Current.MainPage.DisplayAlert("Sucesso", resultado.Mensagem, "Ok");
                    await ConsultarTelefone();
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Atenção", resultado.Mensagem, "Ok");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Erro", $"Não foi possível carregar os serviços. Erro: {ex.Message}", "OK");
            }
        }
    }
}
