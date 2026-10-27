using Microsoft.EntityFrameworkCore;
using NadinSoftTask.Application.Common.Abstractions;
using NadinSoftTask.Domain.Products;

namespace NadinSoftTask.Infrastructure.Persistence.Repositories;

public sealed class ProductRepository(AppDbContext dbContext) : IProductRepository
{
    public Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        dbContext.Products.FirstOrDefaultAsync(product => product.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Product>> ListAllAsync(
        CancellationToken cancellationToken = default) =>
        await dbContext.Products
            .AsNoTracking()
            .OrderBy(product => product.Id)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Product>> ListByOwnerAsync(
        string ownerId,
        CancellationToken cancellationToken = default) =>
        await dbContext.Products
            .AsNoTracking()
            .Where(product => product.OwnerId == ownerId)
            .OrderBy(product => product.Id)
            .ToListAsync(cancellationToken);

    public Task<bool> ExistsByProduceDateAndEmailAsync(
        DateOnly produceDate,
        Email manufactureEmail,
        int? excludeProductId = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Product> query = dbContext.Products
            .AsNoTracking()
            .Where(product => product.ProduceDate == produceDate);

        if (excludeProductId.HasValue)
        {
            query = query.Where(product => product.Id != excludeProductId.Value);
        }

        return query.AnyAsync(
            product => product.ManufactureEmail == manufactureEmail,
            cancellationToken);
    }

    public async Task AddAsync(Product product, CancellationToken cancellationToken = default) =>
        await dbContext.Products.AddAsync(product, cancellationToken);

    public void Delete(Product product) => dbContext.Products.Remove(product);
}
