using AutoMapper;
using MediatR;
using NadinSoftTask.Application.Common.Abstractions;
using NadinSoftTask.Application.Common.Dtos;
using NadinSoftTask.Domain.Common;
using NadinSoftTask.Domain.Products;

namespace NadinSoftTask.Application.Commands.CreateProduct;

public sealed class CreateProductCommandHandler(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IMapper mapper) : IRequestHandler<CreateProductCommand, Result<ProductDto>>
{
    public async Task<Result<ProductDto>> Handle(
        CreateProductCommand request,
        CancellationToken cancellationToken)
    {
        Product product = Product.Create(
            currentUser.UserId,
            request.Name,
            new Phone(request.ManufacturePhone),
            new Email(request.ManufactureEmail),
            request.IsAvailable!.Value,
            request.ProduceDate!.Value).Value!;

        bool keyExists = await productRepository.ExistsByProduceDateAndEmailAsync(
            product.ProduceDate,
            product.ManufactureEmail);

        if (keyExists)
        {
            return new DomainError(
                "A product with the same produce date and manufacture email already exists.",
                DomainErrorKind.Conflict);
        }

        await productRepository.AddAsync(product);
        await unitOfWork.SaveChangesAsync();

        return mapper.Map<ProductDto>(product);
    }
}
