using MatutosApi.Infraestrutura;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using MatutosDomain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MatutosApi.Controllers
{
    [ApiController]
    [Route("relatorio")]
    public class RelatorioController : ControllerBase
    {
        private readonly MatutosDbContext _dbContext;

        public RelatorioController(MatutosDbContext dbContext)
        {
            _dbContext = dbContext;
        }

    [HttpPost("servicos-barbeiro/pdf")]
    [Authorize]
    public async Task<IActionResult> GerarPdfServicosBarbeiro([FromBody] FiltroServicosPorBarbeiroDTO filtro)
    {
        try
        {
            var dados = await _dbContext.Database.SqlQuery<RelatorioServicosPorBarbeiroResponseDTO>(
                $"CALL usp_Relatorio_ServicosPorBarbeiro({filtro.DataInicio}, {filtro.DataFim}, {filtro.NomeBarbeiro})"
            ).ToListAsync();

            if (!dados.Any())
                return BadRequest(new { Mensagem = "Nenhum dado encontrado para o período selecionado." });

            var pdfBytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Header().Column(col =>
                    {
                        col.Item().Text("Barbearia Matutos").SemiBold().FontSize(24).FontColor(Colors.Blue.Darken2);
                        col.Item().Text("Relatório de Serviços por Barbeiro").FontSize(16);
                        col.Item().Text($"Gerado em: {DateTime.Now:dd/MM/yy HH:mm}").FontSize(10).FontColor(Colors.Grey.Darken1);
                        col.Item().PaddingVertical(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                    });

                    page.Content().PaddingVertical(10).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3); // Barbeiro
                            columns.RelativeColumn(2); // Quantidade
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

                    page.Footer().Column(col =>
                    {
                        col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                        var totalServicos = dados.Sum(x => x.QuantidadeServicosPrestados);
                        var totalDinheiro = dados.Sum(x => x.ValorTotal);
                        col.Item().PaddingTop(5).AlignRight().Text($"TOTAL GERAL: {totalServicos} serviços | {totalDinheiro:C}").SemiBold().FontSize(14);
                    });
                });
            }).GeneratePdf();

            return File(pdfBytes, "application/pdf", "ServicosBarbeiro.pdf");
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Mensagem = $"Erro ao gerar PDF: {ex.Message}" });
        }
    }

    [HttpGet("clientes-nao-fidelizados/pdf")] 
    [Authorize]
    public async Task<IActionResult> GerarPdfClientesNaoFidelizados([FromQuery] int? qtdDias)
    {
        try
        {
            var dados = await _dbContext.Database.SqlQuery<RelatorioClientesNaoFidelizadosResponseDTO>(
                $"CALL usp_Relatorio_ClientesNaoFidelizados({qtdDias})"
            ).ToListAsync();

            if (!dados.Any())
                return BadRequest(new { Mensagem = "Nenhum cliente inativo encontrado para este filtro." });

            var pdfBytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Header().Column(col =>
                    {
                        col.Item().Text("Barbearia Matutos").SemiBold().FontSize(24).FontColor(Colors.Blue.Darken2);
                        col.Item().Text("Relatório de Clientes Não Fidelizados").FontSize(16);
                        col.Item().Text($"Gerado em: {DateTime.Now:dd/MM/yy HH:mm}").FontSize(10).FontColor(Colors.Grey.Darken1);
                        col.Item().PaddingVertical(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                    });

                    page.Content().PaddingVertical(10).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3); // Nome
                            columns.RelativeColumn(2); // Data
                            columns.RelativeColumn(2); // Dias
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
                            table.Cell().PaddingVertical(5).AlignCenter().Text(item.DataUltimoAgendamento.ToString("dd/MM/yy"));
                            table.Cell().PaddingVertical(5).AlignRight().Text($"{item.DiasSemAgendamento} dias")
                                 .FontColor(Colors.Red.Darken1).SemiBold(); // Destaque em vermelho
                        }
                    });

                    page.Footer().Column(col =>
                    {
                        col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                        col.Item().PaddingTop(5).AlignRight().Text($"TOTAL: {dados.Count} clientes inativos").SemiBold().FontSize(14);
                    });
                });
            }).GeneratePdf();

            return File(pdfBytes, "application/pdf", "ClientesInativos.pdf");
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Mensagem = $"Erro ao gerar PDF: {ex.Message}" });
        }
    }

    [HttpPost("faturamento/pdf")]
        [Authorize]
        public async Task<IActionResult> GerarPdfFaturamento([FromBody] FiltroFaturamentoDTO filtro)
        {
            try
            {
                // 1. Pega os dados do banco
                var dados = await _dbContext.Database.SqlQuery<RelatorioFaturamentoResponseDTO>(
                    $"CALL usp_Relatorio_Faturamento({filtro.DataInicio}, {filtro.DataFim}, {filtro.DescricaoServico}, {filtro.NomeCliente})"
                ).ToListAsync();

                if (!dados.Any())
                    return BadRequest(new { Mensagem = "Nenhum dado encontrado para o período selecionado." });

                // 2. Desenha o PDF na API
                var pdfBytes = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(11));

                        page.Header().Column(col =>
                        {
                            col.Item().Text("Barbearia Matutos").SemiBold().FontSize(24).FontColor(Colors.Blue.Darken2);
                            col.Item().Text("Relatório de Faturamento").FontSize(16);
                            col.Item().Text($"Gerado em: {DateTime.Now:dd/MM/yy HH:mm}").FontSize(10).FontColor(Colors.Grey.Darken1);
                            col.Item().PaddingVertical(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                        });

                        page.Content().PaddingVertical(10).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1);
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

                        page.Footer().Column(col =>
                        {
                            col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                            col.Item().PaddingTop(5).AlignRight().Text($"TOTAL: {dados.Sum(x => x.TotalAgendamento):C}").SemiBold().FontSize(14);
                        });
                    });
                })
                .GeneratePdf(); // 👉 Em vez de salvar no disco, gera os bytes na memória!

                // 3. Devolve o arquivo binário (PDF) para o aplicativo
                return File(pdfBytes, "application/pdf", "Faturamento.pdf");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Mensagem = $"Erro ao gerar PDF: {ex.Message}" });
            }
        }


    }
}
