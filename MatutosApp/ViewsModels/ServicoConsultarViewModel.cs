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

        [ObservableProperty] private int servicoAlteracao;

        [ObservableProperty] private bool podeVisualizar;

        [ObservableProperty]
        private ServicoExibicao servicoClicado;

        [ObservableProperty]
        private ObservableCollection<ServicoExibicao> listaServicos = new();

        public ServicoConsultarViewModel(ServicoService servicoService)
        {
            _servicoService = servicoService;

            _ = ConsultarServico();
            PermissaoParaVisualizar();
        }

        private void PermissaoParaVisualizar()
        {
            var usuarioLogado = UsuarioSessaoService.UsuarioLogado;

            if (usuarioLogado.TipoSelecionado == UsuarioTipo.Administrador)
            {
                PodeVisualizar = true;
            }
            else
            {
                PodeVisualizar = false;
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

                if (resultado.Dados != null)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        ListaServicos.Clear();
                        foreach (var servico in resultado.Dados)
                        {

                            // Transforma o 'Servico' que veio da API no 'ServicoExibicao' que a tela entende!
                            ListaServicos.Add(new ServicoExibicao
                            {
                                Codigo_Servico = servico.Codigo_Servico,
                                Descricao = servico.Descricao,
                                Preco = servico.Preco,
                                Duracao = servico.Duracao,
                                Tempo_Servico = servico.Tempo_Servico, // 👉 Mapeado
                                Ativo = servico.Ativo,
                                IsExpanded = false, // Garante que nasce fechado
                                IsCarregando = false
                            });
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
            public ImageSource FonteImagem { get; set; } 
        }

        public partial class ServicoExibicao : ObservableObject
        {
            public int Codigo_Servico { get; set; }
            public string Descricao { get; set; }
            public decimal Preco { get; set; }
            public string Duracao { get; set; }
            public int Tempo_Servico { get; set; }
            public bool Ativo { get; set; }

            [ObservableProperty]
            private bool isExpanded;

            // 👉 Mostra o ícone de carregamento apenas neste item
            [ObservableProperty]
            private bool isCarregando;

            // 👉 A lista de imagens EXCLUSIVA deste serviço
            [ObservableProperty]
            private ObservableCollection<ImagemExibicao> imagensDoServico = new();
        }
        public async Task ExpandirRecolherServico(ServicoExibicao servicoSelecionado)
        {
            if (servicoSelecionado == null)
                return;

            // Se tá aberto, o clique fecha.
            if (servicoSelecionado.IsExpanded)
            {
                servicoSelecionado.IsExpanded = false;
                return;
            }

            // Abre a sanfona
            servicoSelecionado.IsExpanded = true;

            // Se já tem foto guardada na memória, não precisa chamar a API de novo
            if (servicoSelecionado.ImagensDoServico.Count > 0)
                return;

            try
            {
                servicoSelecionado.IsCarregando = true; // Bolinha girando só nele

                string token = await SecureStorage.Default.GetAsync("jwt_token");
                var resultado = await _servicoService.ConsultarImagemServico(servicoSelecionado.Codigo_Servico, token);

                if (resultado.Sucesso && resultado.Dados != null)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        foreach (var imagemBanco in resultado.Dados)
                        {
                            if (!string.IsNullOrEmpty(imagemBanco.Imagem))
                            {
                                byte[] imageBytes = Convert.FromBase64String(imagemBanco.Imagem);
                                var fonteParaTela = ImageSource.FromStream(() => new MemoryStream(imageBytes));

                                servicoSelecionado.ImagensDoServico.Add(new ImagemExibicao
                                {
                                    Codigo_Imagem = imagemBanco.Codigo_Imagem,
                                    FonteImagem = fonteParaTela
                                });
                            }
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Erro", "Falha ao carregar fotos.", "OK");
            }
            finally
            {
                servicoSelecionado.IsCarregando = false; // Tira a bolinha
            }
        }

        partial void OnServicoClicadoChanged(ServicoExibicao value)
        {
            if (value != null)
            {
                _ = ExpandirRecolherServico(value);

          
                App.Current.MainPage.Dispatcher.Dispatch(() =>
                {
                    ServicoClicado = null;
                });
            }
        }

        [RelayCommand]
        public async Task AbrirAlteracaoServico(ServicoExibicao servicoDaTela)
        {
            if (servicoDaTela == null) return;

            // 👉 TRANSFORMA O ESPELHO DA TELA NA ENTIDADE PURA DO BANCO DE DADOS
            var servicoPuro = new Servico
            {
                Codigo_Servico = servicoDaTela.Codigo_Servico,
                Descricao = servicoDaTela.Descricao,
                Preco = servicoDaTela.Preco,
                Duracao = servicoDaTela.Duracao,
                Tempo_Servico = servicoDaTela.Tempo_Servico,
                Ativo = servicoDaTela.Ativo
            };

            // Agora enviamos o objeto que a tela de destino realmente espera receber!
                    var parametros = new Dictionary<string, object>
            {
                { "ServicoSelecionado", servicoPuro }
            };

            await Shell.Current.GoToAsync(nameof(ServicoCadastroView), parametros);
        }
    }
}
