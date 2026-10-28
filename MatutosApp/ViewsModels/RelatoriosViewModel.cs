using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MatutosApp.Services;
using MatutosDomain;
using System;
using Microsoft.Maui.ApplicationModel;
using System.Collections.ObjectModel;
using System.Threading.Tasks;


namespace MatutosApp.ViewsModels
{
    public partial class RelatoriosViewModel : BaseViewModel
    {
        private readonly RelatorioService _relatorioService;

        // ==========================================
        // 1. SELEÇÃO DO RELATÓRIO
        // ==========================================
        [ObservableProperty] private RelatorioOpcao relatorioSelecionado;
        public ObservableCollection<RelatorioOpcao> ListaOpcoesRelatorio { get; } = new();

        // ==========================================
        // 2. VARIÁVEIS DE FILTRO (Compartilhadas e Específicas)
        // ==========================================
        [ObservableProperty] private DateTime dataInicio = DateTime.Today.AddDays(-30);
        [ObservableProperty] private DateTime dataFim = DateTime.Today;
        [ObservableProperty] private string? descricaoServico;
        [ObservableProperty] private string? nomeCliente;
        [ObservableProperty] private string? nomeBarbeiro;
        [ObservableProperty] private int? qtdDiasSemAgendamento;

        // ==========================================
        // 3. CONTROLES DE VISIBILIDADE (Esconde/Mostra na Tela)
        // ==========================================
        [ObservableProperty] private bool isFiltrosFaturamentoVisible;
        [ObservableProperty] private bool isFiltrosBarbeiroVisible;
        [ObservableProperty] private bool isFiltrosClientesVisible;
        [ObservableProperty] private bool isBotaoGerarVisible;

        // As listas e booleanos antigos de resultados foram removidos, 
        // já que agora vamos gerar PDF!

        public RelatoriosViewModel(RelatorioService relatorioService)
        {
            _relatorioService = relatorioService;

            CarregarOpcoesRelatorio();
        }

        private void CarregarOpcoesRelatorio()
        {
            ListaOpcoesRelatorio.Clear();
            ListaOpcoesRelatorio.Add(new RelatorioOpcao { Descricao = "1. Relatório de Faturamento", Valor = TipoRelatorioEnum.Faturamento });
            ListaOpcoesRelatorio.Add(new RelatorioOpcao { Descricao = "2. Serviços por Barbeiro", Valor = TipoRelatorioEnum.ServicosPorBarbeiro });
            ListaOpcoesRelatorio.Add(new RelatorioOpcao { Descricao = "3. Clientes Não Fidelizados", Valor = TipoRelatorioEnum.ClientesNaoFidelizados });
        }

        // ==========================================
        // O "PULO DO GATO": Esse método roda sozinho 
        // sempre que o Picker de Relatório mudar!
        // ==========================================
        partial void OnRelatorioSelecionadoChanged(RelatorioOpcao value)
        {
            // 1. Esconde tudo primeiro
            IsFiltrosFaturamentoVisible = false;
            IsFiltrosBarbeiroVisible = false;
            IsFiltrosClientesVisible = false;

            IsBotaoGerarVisible = value != null;

            if (value == null) return;

            // 2. Mostra só os filtros do relatório escolhido
            switch (value.Valor)
            {
                case TipoRelatorioEnum.Faturamento:
                    IsFiltrosFaturamentoVisible = true;
                    break;
                case TipoRelatorioEnum.ServicosPorBarbeiro:
                    IsFiltrosBarbeiroVisible = true;
                    break;
                case TipoRelatorioEnum.ClientesNaoFidelizados:
                    IsFiltrosClientesVisible = true;
                    break;
            }
        }

        // ==========================================
        // COMANDO DO BOTÃO "GERAR RELATÓRIO"
        // ==========================================
        [RelayCommand]
        public async Task ConsultarRelatorio()
        {
            if (RelatorioSelecionado == null)
            {
                await Application.Current.MainPage.DisplayAlert("Atenção", "Selecione um relatório primeiro.", "OK");
                return;
            }

            IsBusy = true; // Mostra o ActivityIndicator (Loading)
            try
            {
                string? token = await SecureStorage.Default.GetAsync("jwt_token");

                switch (RelatorioSelecionado.Valor)
                {
                    case TipoRelatorioEnum.Faturamento:
                        await ProcessarFaturamento(token);
                        break;
                    case TipoRelatorioEnum.ServicosPorBarbeiro:
                        await ProcessarServicosBarbeiro(token);
                        break;
                    case TipoRelatorioEnum.ClientesNaoFidelizados:
                        await ProcessarClientesNaoFidelizados(token);
                        break;
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Erro", $"Falha ao gerar relatório: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false; // Esconde o loading
            }
        }

        // --- LÓGICA INDIVIDUAL DE CADA RELATÓRIO ---

        private async Task ProcessarFaturamento(string token)
        {
            var filtro = new FiltroFaturamentoDTO
            {
                DataInicio = DataInicio,
                DataFim = DataFim.Date.AddHours(23).AddMinutes(59),
                DescricaoServico = DescricaoServico,
                NomeCliente = NomeCliente
            };

            var resultado = await _relatorioService.GerarRelatorioFaturamentoPdf(token, filtro);

            if (resultado.Sucesso && resultado.ArquivoPdf != null)
            {
                string caminhoArquivo = Path.Combine(FileSystem.CacheDirectory, $"Faturamento_{DateTime.Now:ddMMyyyy_HHmm}.pdf");
                File.WriteAllBytes(caminhoArquivo, resultado.ArquivoPdf);

                // 2. Abre o PDF
                await Launcher.Default.OpenAsync(new OpenFileRequest
                {
                    Title = "Visualizar Relatório de Faturamento",
                    File = new ReadOnlyFile(caminhoArquivo)
                });
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Aviso", resultado.Mensagem, "OK");
            }
        }

        private async Task ProcessarServicosBarbeiro(string token)
        {
            var filtro = new FiltroServicosPorBarbeiroDTO
            {
                DataInicio = DataInicio,
                DataFim = DataFim.Date.AddHours(23).AddMinutes(59),
                NomeBarbeiro = NomeBarbeiro
            };

            var resultado = await _relatorioService.GerarRelatorioServicosPorBarbeiroPdf(token, filtro);

            if (resultado.Sucesso && resultado.ArquivoPdf != null)
            {
                string caminhoArquivo = Path.Combine(FileSystem.CacheDirectory, $"ServicosBarbeiro_{DateTime.Now:ddMMyyyy_HHmm}.pdf");
                File.WriteAllBytes(caminhoArquivo, resultado.ArquivoPdf);

                await Launcher.Default.OpenAsync(new OpenFileRequest
                {
                    Title = "Visualizar Relatório de Barbeiros",
                    File = new ReadOnlyFile(caminhoArquivo)
                });
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Aviso", resultado.Mensagem, "OK");
            }
        }

        private async Task ProcessarClientesNaoFidelizados(string token)
        {
            var resultado = await _relatorioService.GerarRelatorioClientesNaoFidelizadosPdf(token, QtdDiasSemAgendamento);

            if (resultado.Sucesso && resultado.ArquivoPdf != null)
            {
                string caminhoArquivo = Path.Combine(FileSystem.CacheDirectory, $"ClientesInativos_{DateTime.Now:ddMMyyyy_HHmm}.pdf");
                File.WriteAllBytes(caminhoArquivo, resultado.ArquivoPdf);

                await Launcher.Default.OpenAsync(new OpenFileRequest
                {
                    Title = "Visualizar Relatório de Clientes",
                    File = new ReadOnlyFile(caminhoArquivo)
                });
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Aviso", resultado.Mensagem, "OK");
            }
        }
    }

        // Classe auxiliar para o Picker de seleção
        public class RelatorioOpcao
    {
        public string Descricao { get; set; }
        public TipoRelatorioEnum Valor { get; set; }

        public override string ToString() => Descricao;
    }
}