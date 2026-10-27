using NadinSoftTask.Domain.Products;

namespace NadinSoftTask.Application.Common.Abstractions;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Product>> ListAllAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Product>> ListByOwnerAsync(
        string ownerId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByProduceDateAndEmailAsync(
        DateOnly produceDate,
        Email manufactureEmail,
        int? excludeProductId = null,
        CancellationToken cancellationToken = default);

    Task AddAsync(Product product, CancellationToken cancellationToken = default);

    void Delete(Product product);
}
