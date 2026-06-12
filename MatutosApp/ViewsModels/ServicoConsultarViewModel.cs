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

        [ObservableProperty] int codigoServico;

        [ObservableProperty]
        private ObservableCollection<Servico> listaServicos = new ObservableCollection<Servico>();

        //[ObservableProperty] private ObservableCollection<Servico_Imagem> listaImagensServico = new ObservableCollection<Servico_Imagem>();

        [ObservableProperty]
        private ObservableCollection<ImagemExibicao> listaImagensServico = new();

        [ObservableProperty]
        private bool mostrarGaleria;

        // Comando para o botão "Fechar" da galeria
        [RelayCommand]
        public void FecharGaleria()
        {
            MostrarGaleria = false;
            LimparImagens();
        }

        public ServicoConsultarViewModel(ServicoService servicoService)
        {
            _servicoService = servicoService;

            _ = ConsultarServico();
        }

        private async Task LimparImagens()
        {
            ListaImagensServico.Clear();
        }

        [RelayCommand]
        public async Task ConsultarServicoImagens(int codigoServico)
        {
            try
            {
                IsBusy = true;

                string token = await SecureStorage.Default.GetAsync("jwt_token");
                if (token == null)
                {
                    await Application.Current.MainPage.DisplayAlert("Atenção", "O usuário está desconectado no Sistema", "Ok");
                    return;
                }



                var resultado = await _servicoService.ConsultarImagemServico(codigoServico, token);

                if(resultado.Sucesso)
                {
                   foreach (var imagem in resultado.Dados)
                   {
                       if(!string.IsNullOrEmpty(imagem.Imagem))
                       {
                           byte[] imagemBytes = Convert.FromBase64String(imagem.Imagem);

                           var fonteParaTela = ImageSource.FromStream(() => new MemoryStream(imagemBytes));

                           ListaImagensServico.Add(new ImagemExibicao
                           {
                               Codigo_Imagem = imagem.Codigo_Imagem,
                               FonteImagem = fonteParaTela
                           });

                       }
                   }
                   MostrarGaleria = true;
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Atenção", resultado.Mensagem, "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Erro", $"Não foi possível carregar os serviços. Erro: {ex.Message}", "OK");
            }
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

        public class ImagemExibicao
        {
            public int Codigo_Imagem { get; set; }
            public ImageSource FonteImagem { get; set; } // O XAML vai ler isso!
        }

     
    }
}
