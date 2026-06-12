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
    public partial class ServicoCadastrarViewModel : BaseViewModel
    {
        ServicoService _servicoService;

        [ObservableProperty] private string descricao;
        [ObservableProperty] private int tempo_servico;
        [ObservableProperty] private bool ativo;
        [ObservableProperty] private decimal preco;
        [ObservableProperty] private string duracao;

        [ObservableProperty] private int codigoServico;

        [ObservableProperty]
        private int quantidadeImagensSalvas;

        //[ObservableProperty]
        //private ObservableCollection<string> imagensNovasBase64 = new();

        [ObservableProperty]
        private ObservableCollection<ImagemServicoTemp> imagensNovasBase64 = new();

        public ServicoCadastrarViewModel(ServicoService servicoService)
        {
            _servicoService = servicoService;
        }
        [RelayCommand]
        public async Task CadastrarServico()
        {
            if (Descricao.IsNullOrEmpty() || Preco <= 0 || Duracao.IsNullOrEmpty() || Tempo_servico <= 0)
            {
                await Application.Current.MainPage.DisplayAlert("Atenção", "Por favor preencha todos os campos", "Ok");
                return;
            }

            try
            {
                IsBusy = true;
                var token = await SecureStorage.Default.GetAsync("jwt_token");

                var servicoNovo = new Servico
                {
                    Descricao = Descricao,
                    Ativo = Ativo,
                    Duracao = Duracao,
                    Preco = Preco,
                    Tempo_Servico = Tempo_servico,

                    // 👉 A MÁGICA: Transforma as strings Base64 na coleção de entidades associadas
                    Imagens = ImagensNovasBase64.Select(imgTemp => new Servico_Imagem
                    {
                        Imagem = imgTemp.Base64 // Agora o tipo bate perfeitamente (String com String)
                    }).ToList()
                };

                // Envia o pacote completo (serviço + imagens internas) para a API
                var resultado = await _servicoService.ServicoCadastrar(servicoNovo, token);

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
                await Application.Current.MainPage.DisplayAlert("Erro Crítico", $"Falha de comunicação: {ex.Message}", "Ok");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        public async Task SelecionarImagemServico()
        {
            if (ImagensNovasBase64.Count >= 3)
            {
                await Application.Current.MainPage.DisplayAlert("Atenção", "Limite de 3 imagens atingido.", "OK");
                return;
            }

            try
            {
                var foto = await MediaPicker.Default.PickPhotoAsync(new MediaPickerOptions { Title = "Selecione a foto" });
                if (foto == null) return;

                IsBusy = true;

                // 👉 2. Lemos o arquivo da foto uma única vez
                using var stream = await foto.OpenReadAsync();
                using var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);
                byte[] imageBytes = memoryStream.ToArray();

                // 👉 3. Montamos o objeto duplo (Base64 para API, ImageSource para a Tela)
                var novaImagem = new ImagemServicoTemp
                {
                    Base64 = Convert.ToBase64String(imageBytes),
                    FonteImagem = ImageSource.FromStream(() => new MemoryStream(imageBytes))
                };

                ImagensNovasBase64.Add(novaImagem);
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Erro", $"Falha ao processar: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }


        [RelayCommand]
        public async Task AdicioanarImagemServico()
        {
            // 1. Trava de segurança no App (Opcional, mas melhora a experiência)
            if (QuantidadeImagensSalvas >= 3)
            {
                await Application.Current.MainPage.DisplayAlert("Atenção", "Este serviço já atingiu o limite de 3 imagens.", "OK");
                return;
            }

            try
            {
                // 2. Abre a galeria do celular para o usuário escolher a foto
                var foto = await MediaPicker.Default.PickPhotoAsync(new MediaPickerOptions
                {
                    Title = "Selecione uma foto para o serviço"
                });

                // Se o usuário fechar a galeria sem escolher nada, encerra aqui
                if (foto == null)
                    return;

                IsBusy = true; // Mostra o loading na tela (se você tiver o ActivityIndicator)

                // 3. Converte a foto escolhida para uma String Base64
                string base64String = await ConverterFotoParaBase64Async(foto);

                // 4. Pega o Token do usuário logado
                var token = await SecureStorage.Default.GetAsync("jwt_token");

                if (string.IsNullOrEmpty(token))
                {
                    await Application.Current.MainPage.DisplayAlert("Atenção", "Usuário não autenticado.", "OK");
                    return;
                }

                // 5. Envia para a sua Service
                var resultado = await _servicoService.CadastrarImagemServico(token, CodigoServico, base64String);

                if (resultado.Sucesso)
                {
                    QuantidadeImagensSalvas++; // Aumenta o contador local
                    await Application.Current.MainPage.DisplayAlert("Sucesso!", resultado.Mensagem, "OK");

                    // Aqui você pode chamar o método que recarrega a lista de imagens da tela
                    // ex: await CarregarImagensDoServicoAsync();
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Ops!", resultado.Mensagem, "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Erro", $"Falha ao processar imagem: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false; // Tira o loading
            }
        }

        private async Task<string> ConverterFotoParaBase64Async(FileResult foto)
        {
            using var stream = await foto.OpenReadAsync();

            // Copia para a memória do aplicativo
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);

            // Transforma a memória em um Array de Bytes
            byte[] imageBytes = memoryStream.ToArray();

            // Transforma os Bytes no texto Base64
            return Convert.ToBase64String(imageBytes);
        }

        public class ImagemServicoTemp
        {
            public string Base64 { get; set; }
            public ImageSource FonteImagem { get; set; } // O XAML vai ler isso aqui!
        }
    }
}
