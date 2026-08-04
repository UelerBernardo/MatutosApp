using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MatutosApp.Services;
using MatutosApp.Views;
using MatutosDomain;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatutosApp.ViewsModels
{
    public partial class ServicoItemWrapper : ObservableObject
    {
        public Servico ServicoOriginal { get; set; }

        [ObservableProperty]
        private int quantidadeSelecionada;

        [RelayCommand]
        public void Aumentar() => QuantidadeSelecionada++;

        [RelayCommand]
        public void Diminuir()
        {
            if (QuantidadeSelecionada > 0)
                QuantidadeSelecionada--;
        }
    }

    [QueryProperty(nameof(Agendamento), "Agendamento")]
    public partial class AgendamentoServicoViewModel : BaseViewModel
    {
        private readonly ServicoService _servicoService;
        private readonly AgendamentoService _agendamentoService;

        private List<ServicoItemWrapper> _listaOriginalServicos = new();

        [ObservableProperty]  private string textoBusca;

        [ObservableProperty]
        private ObservableCollection<ServicoItemWrapper> listaServico = new();

        [ObservableProperty] private int agendamento;
        [ObservableProperty] private decimal valor_Total_Item;
        //public ObservableCollection<ServicoItemWrapper> ListaServico { get; set; } = new();

        public AgendamentoServicoViewModel(ServicoService servicoService, AgendamentoService agendamentoService)
        {
            _servicoService = servicoService;
            _agendamentoService = agendamentoService;

            _ = ConsultarServico();
        }

        [RelayCommand]
        public async Task ConsultarServico()
        {
            try
            {
                string token = await SecureStorage.Default.GetAsync("jwt_token");

                var listaDb = await _servicoService.ServicoConsultar(token);
                if (listaDb != null)
                {
                    // Limpa as duas listas para evitar duplicação
                    ListaServico.Clear();
                    _listaOriginalServicos.Clear();

                    foreach (var servico in listaDb)
                    {
                        var itemWrapper = new ServicoItemWrapper
                        {
                            ServicoOriginal = servico,
                            QuantidadeSelecionada = 0
                        };

                        // 👉 3. ADICIONA O MESMO WRAPPER NA TELA E NO BACKUP
                        ListaServico.Add(itemWrapper);
                        _listaOriginalServicos.Add(itemWrapper);
                    }
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Erro", $"Não foi possível carregar os serviços. Erro: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        public async Task Cadastrar()
        {
            var itensEscolhidos = _listaOriginalServicos.Where(x => x.QuantidadeSelecionada > 0).ToList();

            if (!itensEscolhidos.Any())
            {
                await Application.Current.MainPage.DisplayAlert("Atenção", "É necessário selecionar ao menos um serviço.", "OK");
                return;
            }

            string token = await SecureStorage.Default.GetAsync("jwt_token");
            if (string.IsNullOrEmpty(token))
            {
                await Application.Current.MainPage.DisplayAlert("Atenção", "Sessão expirada. Faça login novamente.", "OK");
                return;
            }

            var novosAgendamentosServicos = new List<Agendamento_Servico>();

            foreach (var item in itensEscolhidos)
            {
                novosAgendamentosServicos.Add(new Agendamento_Servico
                {
                    Codigo_Agendamento = Agendamento,
                    Codigo_Servico = item.ServicoOriginal.Codigo_Servico,
                    Quantidade_Servico = item.QuantidadeSelecionada,
                    Valor_Total_Item = item.QuantidadeSelecionada * item.ServicoOriginal.Preco,
                    Tempo_Servico_Item = item.QuantidadeSelecionada * item.ServicoOriginal.Tempo_Servico
                });
            }

            bool confirmar = await Application.Current.MainPage.DisplayAlert("Atenção", "Deseja realmente finalizar o agendamento? Após a confimação não será possível alterar as informações.", "Sim", "Não");
            if(!confirmar)
            {
                return;
            }

            var resultado = await _agendamentoService.AgendamentoServicoCadastrar(novosAgendamentosServicos, token);

            if (resultado.Sucesso)
            {
                LiberarAgendamento();
                await Application.Current.MainPage.DisplayAlert("Sucesso", "Agendamento Cadastrado com sucesso.", "OK");

                var parametros = new Dictionary<string, object>
                {
                    {"CadastroAgendamento", true }
                };
                await Shell.Current.GoToAsync($"{nameof(AgendamentoDetalhesView)}?Agendamento={resultado.IdAgendamento}", parametros);
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Atenção", resultado.Mensagem, "OK");
            }
        }

        [RelayCommand]
        public async Task CancelarAgendamento()
        {
            string token = await SecureStorage.Default.GetAsync("jwt_token");
            if (Agendamento <= 0)
            {
                await Application.Current.MainPage.DisplayAlert("Atenção", "O agendamento não foi encontrado", "Ok");
                return;
            }

            var resultado = await _agendamentoService.AgendamentoExcluir(Agendamento, token);

            if (resultado.Sucesso)
            {
                await Application.Current.MainPage.DisplayAlert("Sucesso", "Agendamento cancelado.", "Ok");
                await Shell.Current.GoToAsync("///PrincipalView");
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Atenção", resultado.Mensagem, "Ok");
            }
        }

        public async Task LiberarAgendamento()
        {
  
            string token = await SecureStorage.Default.GetAsync("jwt_token");

            if (Agendamento <= 0)
            {
                await Application.Current.MainPage.DisplayAlert("Atenção", "O agendamento não foi encontrado", "Ok");
                return;
            }

            var resultado = await _agendamentoService.AgendamentoAlterarSituacao(Agendamento, token, AgendamentoSituacao.Liberado);

            if (resultado.Sucesso)
            {

            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Atenção", resultado.Mensagem, "Ok");
            }
            
        }


        partial void OnTextoBuscaChanged(string value)
        {
            // Se a barra estiver vazia, restaura todos os serviços na tela
            if (string.IsNullOrWhiteSpace(value))
            {
                ListaServico = new ObservableCollection<ServicoItemWrapper>(_listaOriginalServicos);
                return;
            }

            // Filtra procurando a descrição ignorando letras maiúsculas ou minúsculas
            var itensFiltrados = _listaOriginalServicos
                .Where(s => s.ServicoOriginal.Descricao.Contains(value, StringComparison.OrdinalIgnoreCase))
                .ToList();

            // Atualiza a tela com os resultados
            ListaServico = new ObservableCollection<ServicoItemWrapper>(itensFiltrados);
        }
    }
}
