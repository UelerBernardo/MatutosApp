using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MatutosApp.Services;
using MatutosDomain;
using System;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Microsoft.Maui.ApplicationModel;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

// 👉 RESOLVENDO O CONFLITO: Criamos um apelido para as cores do PDF
using PdfColors = QuestPDF.Helpers.Colors;

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
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

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

            var resultado = await _relatorioService.GerarRelatorioFaturamento(token, filtro);

            if (resultado.Sucesso && resultado.Dados != null && resultado.Dados.Any())
            {
                string caminhoArquivo = Path.Combine(FileSystem.CacheDirectory, $"Faturamento_{DateTime.Now:ddMMyyyy_HHmm}.pdf");
                GerarPdfFaturamento(resultado.Dados, caminhoArquivo);

                await Launcher.Default.OpenAsync(new OpenFileRequest
                {
                    Title = "Visualizar Relatório de Faturamento",
                    File = new ReadOnlyFile(caminhoArquivo)
                });
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Aviso", "Nenhum dado encontrado para gerar o PDF.", "OK");
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

            var resultado = await _relatorioService.GerarRelatorioServicosPorBarbeiro(token, filtro);

            if (resultado.Sucesso && resultado.Dados != null && resultado.Dados.Any())
            {
                // 1. Define onde o arquivo será salvo temporariamente
                string caminhoArquivo = Path.Combine(FileSystem.CacheDirectory, $"ServicosBarbeiro_{DateTime.Now:ddMMyyyy_HHmm}.pdf");

                // 2. Desenha e gera o PDF
                GerarPdfServicosBarbeiro(resultado.Dados, caminhoArquivo);

                // 3. Pede para o Sistema Operacional abrir o PDF
                await Launcher.Default.OpenAsync(new OpenFileRequest
                {
                    Title = "Visualizar Relatório de Barbeiros",
                    File = new ReadOnlyFile(caminhoArquivo)
                });
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Aviso", "Nenhum dado encontrado para gerar o PDF.", "OK");
            }
        }

        private async Task ProcessarClientesNaoFidelizados(string token)
        {
            var resultado = await _relatorioService.GerarRelatorioClientesNaoFidelizados(token, QtdDiasSemAgendamento);

            if (resultado.Sucesso && resultado.Dados != null && resultado.Dados.Any())
            {
                // 1. Define onde o arquivo será salvo temporariamente
                string caminhoArquivo = Path.Combine(FileSystem.CacheDirectory, $"ClientesInativos_{DateTime.Now:ddMMyyyy_HHmm}.pdf");

                // 2. Desenha e gera o PDF
                GerarPdfClientesNaoFidelizados(resultado.Dados, caminhoArquivo);

                // 3. Pede para o Sistema Operacional abrir o PDF
                await Launcher.Default.OpenAsync(new OpenFileRequest
                {
                    Title = "Visualizar Relatório de Clientes",
                    File = new ReadOnlyFile(caminhoArquivo)
                });
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Aviso", "Nenhum dado encontrado para gerar o PDF.", "OK");
            }
        }

        private void GerarPdfFaturamento(List<RelatorioFaturamentoResponseDTO> dados, string caminho)
        {
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);

                    page.PageColor(PdfColors.White);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    // CABEÇALHO DO PDF
                    page.Header().Column(col =>
                    {
                        // 👉 CORREÇÃO: "Blue.Darken2" e "Grey.Darken1" (com o ponto no meio)
                        col.Item().Text("Barbearia Matutos").SemiBold().FontSize(24).FontColor(PdfColors.Blue.Darken2);
                        col.Item().Text("Relatório de Faturamento").FontSize(16);
                        col.Item().Text($"Gerado em: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(10).FontColor(PdfColors.Grey.Darken1);
                        col.Item().PaddingVertical(10).LineHorizontal(1).LineColor(PdfColors.Grey.Lighten2);
                    });

                    // CONTEÚDO (A TABELA)
                    page.Content().PaddingVertical(10).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2); // Cliente
                            columns.RelativeColumn(2); // Serviço
                            columns.RelativeColumn(2); // Barbeiro
                            columns.RelativeColumn(1); // Data
                            columns.RelativeColumn(1); // Valor
                        });

                        table.Header(header =>
                        {
                            header.Cell().BorderBottom(1).PaddingBottom(5).Text("Cliente").SemiBold();
                            header.Cell().BorderBottom(1).PaddingBottom(5).Text("Serviço").SemiBold();
                            header.Cell().BorderBottom(1).PaddingBottom(5).Text("Barbeiro").SemiBold();
                            header.Cell().BorderBottom(1).PaddingBottom(5).Text("Data").SemiBold();
                            header.Cell().BorderBottom(1).PaddingBottom(5).AlignRight().Text("Valor").SemiBold();
                        });

                        foreach (var item in dados)
                        {
                            table.Cell().PaddingVertical(5).Text(item.NomeCliente);
                            table.Cell().PaddingVertical(5).Text(item.DescricaoServico);
                            table.Cell().PaddingVertical(5).Text(item.NomeBarbeiro);
                            table.Cell().PaddingVertical(5).Text(item.DataHoraAgendamento.ToString("dd/MM/yy"));
                            table.Cell().PaddingVertical(5).AlignRight().Text(item.TotalAgendamento.ToString("C"));
                        }
                    });

                    // RODAPÉ (Soma Total e Paginação)
                    page.Footer().Column(col =>
                    {
                        // 👉 CORREÇÃO: "Grey.Lighten2"
                        col.Item().LineHorizontal(1).LineColor(PdfColors.Grey.Lighten2);
                        col.Item().PaddingTop(5).AlignRight().Text($"TOTAL: {dados.Sum(x => x.TotalAgendamento):C}").SemiBold().FontSize(14);

                        col.Item().AlignCenter().Text(x =>
                        {
                            x.Span("Página ");
                            x.CurrentPageNumber();
                            x.Span(" de ");
                            x.TotalPages();
                        });
                    });
                });
            })
            .GeneratePdf(caminho);
        }

        private void GerarPdfServicosBarbeiro(List<RelatorioServicosPorBarbeiroResponseDTO> dados, string caminho)
        {
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);

                    page.PageColor(PdfColors.White);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    // CABEÇALHO DO PDF
                    page.Header().Column(col =>
                    {
                        col.Item().Text("Barbearia Matutos").SemiBold().FontSize(24).FontColor(PdfColors.Blue.Darken2);
                        col.Item().Text("Relatório de Serviços por Barbeiro").FontSize(16);
                        col.Item().Text($"Gerado em: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(10).FontColor(PdfColors.Grey.Darken1);
                        col.Item().PaddingVertical(10).LineHorizontal(1).LineColor(PdfColors.Grey.Lighten2);
                    });

                    // CONTEÚDO (A TABELA)
                    page.Content().PaddingVertical(10).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3); // Barbeiro (maior espaço para o nome)
                            columns.RelativeColumn(2); // Quantidade de Serviços
                            columns.RelativeColumn(2); // Valor Total
                        });

                        table.Header(header =>
                        {
                            header.Cell().BorderBottom(1).PaddingBottom(5).Text("Barbeiro").SemiBold();
                            header.Cell().BorderBottom(1).PaddingBottom(5).AlignCenter().Text("Qtd. Serviços").SemiBold();
                            header.Cell().BorderBottom(1).PaddingBottom(5).AlignRight().Text("Valor Total").SemiBold();
                        });

                        foreach (var item in dados)
                        {
                            table.Cell().PaddingVertical(5).Text(item.NomeBarbeiro);
                            table.Cell().PaddingVertical(5).AlignCenter().Text(item.QuantidadeServicosPrestados.ToString());
                            table.Cell().PaddingVertical(5).AlignRight().Text(item.ValorTotal.ToString("C"));
                        }
                    });

                    // RODAPÉ (Soma Total e Paginação)
                    page.Footer().Column(col =>
                    {
                        col.Item().LineHorizontal(1).LineColor(PdfColors.Grey.Lighten2);

                        // Exibe o total geral de todos os barbeiros juntos no rodapé
                        var totalDinheiro = dados.Sum(x => x.ValorTotal);
                        var totalServicos = dados.Sum(x => x.QuantidadeServicosPrestados);

                        col.Item().PaddingTop(5).AlignRight().Text($"TOTAL GERAL: {totalServicos} serviços | {totalDinheiro:C}").SemiBold().FontSize(14);

                        col.Item().AlignCenter().Text(x =>
                        {
                            x.Span("Página ");
                            x.CurrentPageNumber();
                            x.Span(" de ");
                            x.TotalPages();
                        });
                    });
                });
            })
            .GeneratePdf(caminho);
        }

        private void GerarPdfClientesNaoFidelizados(List<RelatorioClientesNaoFidelizadosResponseDTO> dados, string caminho)
        {
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);

                    page.PageColor(PdfColors.White);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    // CABEÇALHO DO PDF
                    page.Header().Column(col =>
                    {
                        col.Item().Text("Barbearia Matutos").SemiBold().FontSize(24).FontColor(PdfColors.Blue.Darken2);
                        col.Item().Text("Relatório de Clientes Não Fidelizados").FontSize(16);
                        col.Item().Text($"Gerado em: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(10).FontColor(PdfColors.Grey.Darken1);
                        col.Item().PaddingVertical(10).LineHorizontal(1).LineColor(PdfColors.Grey.Lighten2);
                    });

                    // CONTEÚDO (A TABELA)
                    page.Content().PaddingVertical(10).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3); // Nome do Cliente
                            columns.RelativeColumn(2); // Data do Último Agendamento
                            columns.RelativeColumn(2); // Dias Ausente
                        });

                        table.Header(header =>
                        {
                            header.Cell().BorderBottom(1).PaddingBottom(5).Text("Nome do Cliente").SemiBold();
                            header.Cell().BorderBottom(1).PaddingBottom(5).AlignCenter().Text("Último Agendamento").SemiBold();
                            header.Cell().BorderBottom(1).PaddingBottom(5).AlignRight().Text("Dias Ausente").SemiBold();
                        });

                        foreach (var item in dados)
                        {
                            table.Cell().PaddingVertical(5).Text(item.NomeCliente);
                            table.Cell().PaddingVertical(5).AlignCenter().Text(item.DataUltimoAgendamento.ToString("dd/MM/yyyy"));

                            // Coloca a quantidade de dias em vermelho para chamar atenção
                            table.Cell().PaddingVertical(5).AlignRight().Text($"{item.DiasSemAgendamento} dias")
                                 .FontColor(PdfColors.Red.Darken1).SemiBold();
                        }
                    });

                    // RODAPÉ (Soma Total e Paginação)
                    page.Footer().Column(col =>
                    {
                        col.Item().LineHorizontal(1).LineColor(PdfColors.Grey.Lighten2);

                        // Exibe a contagem total de clientes sumidos
                        col.Item().PaddingTop(5).AlignRight().Text($"TOTAL: {dados.Count} clientes inativos").SemiBold().FontSize(14);

                        col.Item().AlignCenter().Text(x =>
                        {
                            x.Span("Página ");
                            x.CurrentPageNumber();
                            x.Span(" de ");
                            x.TotalPages();
                        });
                    });
                });
            })
            .GeneratePdf(caminho);
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