using Calidad_API.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Reflection.Metadata;

namespace Calidad_API.Services
{
    public class ValePdfService
    {
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _env;

        public ValePdfService(IConfiguration config, IWebHostEnvironment env)
        {
            _config = config;
            _env = env;
            QuestPDF.Settings.License = LicenseType.Community; // gratis para uso community
        }

        public string GenerarYGuardar(Vale vale, Inspeccion inspeccion, string lote)
        {
            var basePath = _config["Vales:RutaAlmacenamiento"]
                           ?? Path.Combine(_env.ContentRootPath, "Vales");

            var year = vale.FechaGeneracion.Year.ToString();
            var month = vale.FechaGeneracion.Month.ToString("00");
            var folder = Path.Combine(basePath, year, month);
            Directory.CreateDirectory(folder);

            var fileName = $"{vale.Folio}.pdf";
            var fullPath = Path.Combine(folder, fileName);

            var logoPath = _config["Vales:LogoPath"];
            if (!string.IsNullOrWhiteSpace(logoPath) && !Path.IsPathRooted(logoPath))
                logoPath = Path.Combine(_env.ContentRootPath, logoPath);

            var document = QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A5);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Column(col =>
                    {
                        col.Item().Row(row =>
                        {
                            if (!string.IsNullOrWhiteSpace(logoPath) && File.Exists(logoPath))
                            {
                                row.ConstantItem(80).Image(logoPath).FitArea();
                            }
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().AlignRight().Text("VALE DE MATERIAL").Bold().FontSize(16);
                                c.Item().AlignRight().Text($"Folio: {vale.Folio}").FontSize(12);
                            });
                        });
                        col.Item().PaddingTop(5).LineHorizontal(1).LineColor(Colors.Grey.Medium);
                    });

                    page.Content().PaddingVertical(15).Column(col =>
                    {
                        col.Spacing(8);

                        col.Item().Text($"Fecha: {vale.FechaGeneracion:dd/MM/yyyy HH:mm}").FontSize(10);
                        col.Item().Text($"Solicita: {vale.UsuarioSolicita.Nombre}");
                        col.Item().Text($"Lote: {lote}");

                        // —— Defectos de la inspección ——
                        col.Item().PaddingTop(8).Text("Defectos / hallazgos").Bold();

                        if (inspeccion.Detalles == null || !inspeccion.Detalles.Any())
                        {
                            col.Item().Text("Sin detalles de defecto registrados.").FontColor(Colors.Grey.Medium);
                        }
                        else
                        {
                            foreach (var d in inspeccion.Detalles.OrderBy(x => x.FechaRegistro))
                            {
                                var def = d.Defecto;
                                col.Item().Text(
                                    $"• {def.Codigo} - {def.Nombre} | Tipo: {d.TipoRegistro}" +
                                    (string.IsNullOrWhiteSpace(d.Lado) ? "" : $" | Lado: {d.Lado}") +
                                    $" | Cant.: {d.Cantidad:0.##}"
                                );
                            }
                        }

                        col.Item().PaddingTop(10).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten1);

                        // —— Materiales del vale ——
                        col.Item().PaddingTop(10).Text("Materiales solicitados").Bold();

                        col.Item().PaddingTop(6).Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(2);
                                c.RelativeColumn(3);
                                c.RelativeColumn(1);
                            });

                            table.Header(h =>
                            {
                                h.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Código").Bold();
                                h.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Pieza").Bold();
                                h.Cell().Background(Colors.Grey.Lighten3).Padding(5).AlignRight().Text("Cant.").Bold();
                            });

                            foreach (var linea in vale.Detalles.OrderBy(x => x.Pieza.Codigo))
                            {
                                table.Cell().BorderBottom(0.5f).Padding(5).Text(linea.Pieza.Codigo);
                                table.Cell().BorderBottom(0.5f).Padding(5).Text(linea.Pieza.Nombre);
                                table.Cell().BorderBottom(0.5f).Padding(5).AlignRight().Text($"{linea.Cantidad:0.##}");
                            }
                        });

                        col.Item().PaddingTop(20).Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("________________________");
                                c.Item().Text("Firma solicita").FontSize(8).FontColor(Colors.Grey.Medium);
                            });
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().AlignRight().Text("________________________");
                                c.Item().AlignRight().Text("Firma entrega").FontSize(8).FontColor(Colors.Grey.Medium);
                            });
                        });
                    });

                    page.Footer().AlignCenter().Text(t =>
                    {
                        t.Span("Módulo de Calidad — Vale de material").FontSize(8).FontColor(Colors.Grey.Medium);
                    });
                });
            });

            document.GeneratePdf(fullPath);
            return fullPath;
        }
    }
}