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

        [ObservableProperty] private int agendamento;
        [ObservableProperty] private decimal valor_Total_Item;
        public ObservableCollection<ServicoItemWrapper> ListaServico { get; set; } = new();

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
                var listaDb = await _servicoService.ServicoConsultar();
                if (listaDb != null)
                {
                    ListaServico.Clear();
                    foreach (var servico in listaDb)
                    {                    
                        ListaServico.Add(new ServicoItemWrapper
                        {
                            ServicoOriginal = servico,
                            QuantidadeSelecionada = 0
                        });
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
            var itensEscolhidos = ListaServico.Where(x => x.QuantidadeSelecionada > 0).ToList();

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

            var resultado = await _agendamentoService.AgendamentoServicoCadastrar(novosAgendamentosServicos, token);

            if (resultado.Sucesso)
            {
                await Application.Current.MainPage.DisplayAlert("Sucesso", "Agendamento Cadastrado com sucesso.", "OK");
                await Shell.Current.GoToAsync($"{nameof(AgendamentoDetalhesView)}?Agendamento={resultado.IdAgendamento}");
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
            if (Agendamento == null)
            {
                await Application.Current.MainPage.DisplayAlert("Atenção", "O agendamento não foi encontrado", "Ok");
                return;
            }

            var resultado = await _agendamentoService.AgendamentoInativar(Agendamento, token);

            if (resultado.Sucesso)
            {
                await Application.Current.MainPage.DisplayAlert("Sucesso", "Agendamento cancelado.", "Ok");
                await Shell.Current.GoToAsync(nameof(PrincipalView));
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Atenção", resultado.Mensagem, "Ok");
            }
        }
    }
}
