namespace GymHero.Application.Services;

/// <summary>
/// Service for generating receipts and invoices
/// </summary>
public interface IReceiptService
{
    /// <summary>
    /// Generates a PDF receipt for a transaction
    /// </summary>
    /// <param name="transactionId">The transaction ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>PDF receipt as byte array</returns>
    Task<byte[]> GenerateReceiptAsync(Guid transactionId, CancellationToken cancellationToken = default);
}
