using GymHero.Application.Common.Interfaces;
using GymHero.Application.Services;
using GymHero.Domain.Entities;
using GymHero.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace GymHero.Infrastructure.Services;

/// <summary>
/// Service for generating PDF receipts and invoices
/// </summary>
public class ReceiptService : IReceiptService
{
    private readonly IApplicationDbContext _context;

    public ReceiptService(IApplicationDbContext context)
    {
        _context = context;

        // Configure QuestPDF license (Community license for open-source/personal projects)
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<byte[]> GenerateReceiptAsync(Guid transactionId, CancellationToken cancellationToken = default)
    {
        // Fetch transaction with related data
        var transaction = await _context.Transactions
            .Include(t => t.Buyer)
            .Include(t => t.Seller)
            .Include(t => t.WorkoutPlan)
            .FirstOrDefaultAsync(t => t.Id == transactionId, cancellationToken);

        if (transaction == null)
        {
            throw new InvalidOperationException("Transaction not found");
        }

        if (transaction.Status != TransactionStatus.Completed)
        {
            throw new InvalidOperationException("Receipt can only be generated for completed transactions");
        }

        // Generate PDF using QuestPDF
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(50);
                page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                page.Header().Element(ComposeHeader);
                page.Content().Element(content => ComposeContent(content, transaction));
                page.Footer().Element(ComposeFooter);
            });
        });

        return document.GeneratePdf();
    }

    private void ComposeHeader(IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(column =>
            {
                column.Item().Text("GymHero Marketplace").FontSize(20).Bold().FontColor(Colors.Blue.Darken2);
                column.Item().Text("Recibo de Compra").FontSize(14).FontColor(Colors.Grey.Darken2);
            });

            row.RelativeItem().Column(column =>
            {
                column.Item().AlignRight().Text($"Data: {DateTime.Now:dd/MM/yyyy}").FontSize(10);
                column.Item().AlignRight().Text($"Hora: {DateTime.Now:HH:mm:ss}").FontSize(10);
            });
        });
    }

    private void ComposeContent(IContainer container, Transaction transaction)
    {
        container.PaddingVertical(20).Column(column =>
        {
            column.Spacing(15);

            // Transaction Information Section
            column.Item().Element(c => ComposeSectionTitle(c, "Informações da Transação"));
            column.Item().Element(c => ComposeTable(c, new Dictionary<string, string>
            {
                { "ID da Transação", transaction.Id.ToString() },
                { "Data da Compra", transaction.CompletedAt?.ToString("dd/MM/yyyy HH:mm") ?? "N/A" },
                { "Status", GetStatusText(transaction.Status) },
                { "ID do Pagamento", transaction.StripePaymentIntentId ?? "N/A" }
            }));

            // Buyer Information Section
            column.Item().PaddingTop(10).Element(c => ComposeSectionTitle(c, "Informações do Comprador"));
            column.Item().Element(c => ComposeTable(c, new Dictionary<string, string>
            {
                { "Nome", transaction.Buyer.Name },
                { "Email", transaction.Buyer.Email },
                { "ID do Usuário", transaction.BuyerId.ToString() }
            }));

            // Seller Information Section
            column.Item().PaddingTop(10).Element(c => ComposeSectionTitle(c, "Informações do Vendedor"));
            column.Item().Element(c => ComposeTable(c, new Dictionary<string, string>
            {
                { "Nome", transaction.Seller.Name },
                { "Email", transaction.Seller.Email },
                { "ID do Instrutor", transaction.SellerId.ToString() }
            }));

            // Plan Information Section
            column.Item().PaddingTop(10).Element(c => ComposeSectionTitle(c, "Detalhes do Produto"));
            column.Item().Element(c => ComposeTable(c, new Dictionary<string, string>
            {
                { "Nome do Plano", transaction.WorkoutPlan.Name },
                { "Descrição", transaction.WorkoutPlan.Description ?? "N/A" },
                { "Objetivo", transaction.WorkoutPlan.Goal ?? "N/A" },
                { "Duração", transaction.WorkoutPlan.Duration.HasValue ? $"{transaction.WorkoutPlan.Duration} semanas" : "N/A" }
            }));

            // Payment Summary Section
            column.Item().PaddingTop(20).Element(c => ComposePaymentSummary(c, transaction));

            // Footer Note
            column.Item().PaddingTop(20).Text(text =>
            {
                text.Span("Este é um recibo válido de compra realizada através da plataforma GymHero. ")
                    .FontSize(9).FontColor(Colors.Grey.Darken1);
                text.Span("Para questões relacionadas a este recibo, entre em contato através do suporte da plataforma.")
                    .FontSize(9).FontColor(Colors.Grey.Darken1);
            });
        });
    }

    private void ComposeSectionTitle(IContainer container, string title)
    {
        container.Background(Colors.Blue.Lighten4).Padding(8).Text(title).FontSize(13).Bold().FontColor(Colors.Blue.Darken2);
    }

    private void ComposeTable(IContainer container, Dictionary<string, string> data)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(2);
                columns.RelativeColumn(3);
            });

            foreach (var item in data)
            {
                table.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Text(item.Key).Bold();
                table.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Text(item.Value);
            }
        });
    }

    private void ComposePaymentSummary(IContainer container, Transaction transaction)
    {
        container.Background(Colors.Green.Lighten4).Border(2).BorderColor(Colors.Green.Darken1).Padding(15).Column(column =>
        {
            // Total amount
            column.Item().Row(row =>
            {
                row.RelativeItem().Text("Total Pago:").FontSize(16).Bold();
                row.RelativeItem().AlignRight().Text($"{transaction.Currency.ToUpper()} {transaction.Amount:N2}")
                    .FontSize(20).Bold().FontColor(Colors.Green.Darken2);
            });

            // Platform fee breakdown (if applicable)
            if (transaction.PlatformFee > 0)
            {
                column.Item().PaddingTop(10).PaddingBottom(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);

                column.Item().PaddingTop(5).Text("Detalhamento:").FontSize(11).Bold();

                column.Item().Row(row =>
                {
                    row.RelativeItem().Text($"Taxa da Plataforma ({transaction.PlatformFeePercentage:N1}%):").FontSize(10);
                    row.RelativeItem().AlignRight().Text($"{transaction.Currency.ToUpper()} {transaction.PlatformFee:N2}")
                        .FontSize(10).FontColor(Colors.Orange.Darken1);
                });

                column.Item().Row(row =>
                {
                    row.RelativeItem().Text("Valor ao Vendedor:").FontSize(10);
                    row.RelativeItem().AlignRight().Text($"{transaction.Currency.ToUpper()} {transaction.SellerPayout:N2}")
                        .FontSize(10).FontColor(Colors.Green.Darken1);
                });
            }

            var paymentMethod = transaction.Provider == Domain.Entities.PaymentProvider.PayPal ? "PayPal" : "Stripe";
            column.Item().PaddingTop(10).Text($"Método de Pagamento: {paymentMethod}")
                .FontSize(10).FontColor(Colors.Grey.Darken2);
        });
    }

    private void ComposeFooter(IContainer container)
    {
        container.AlignCenter().Text(text =>
        {
            text.Span("GymHero - Plataforma de Treinamento Personalizado | ").FontSize(8).FontColor(Colors.Grey.Medium);
            text.Span("www.gymhero.com").FontSize(8).FontColor(Colors.Blue.Medium);
        });
    }

    private string GetStatusText(TransactionStatus status)
    {
        return status switch
        {
            TransactionStatus.Completed => "Concluído ✓",
            TransactionStatus.Pending => "Pendente",
            TransactionStatus.Failed => "Falhou",
            TransactionStatus.Refunded => "Reembolsado",
            TransactionStatus.Cancelled => "Cancelado",
            _ => status.ToString()
        };
    }
}
