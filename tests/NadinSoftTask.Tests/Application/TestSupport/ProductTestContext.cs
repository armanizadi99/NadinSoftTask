using AutoMapper;
using Moq;
using NadinSoftTask.Application.Common.Abstractions;
using NadinSoftTask.Application.Common.Dtos;
using NadinSoftTask.Domain.Products;
using NadinSoftTask.Tests.Application.TestData;

namespace NadinSoftTask.Tests.Application.TestSupport;

internal sealed class ProductTestContext
{
    public ProductTestContext(string currentUserId = ProductTestData.OwnerId)
    {
        CurrentUser.SetupGet(user => user.UserId).Returns(currentUserId);
        UnitOfWork
            .Setup(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
    }

    public CancellationToken CancellationToken { get; } = new CancellationTokenSource().Token;

    public Mock<ICurrentUser> CurrentUser { get; } = new(MockBehavior.Strict);

    public Mock<IMapper> Mapper { get; } = new(MockBehavior.Strict);

    public Mock<IProductRepository> ProductRepository { get; } = new(MockBehavior.Strict);

    public Mock<IUserRepository> UserRepository { get; } = new(MockBehavior.Strict);

    public Mock<IUnitOfWork> UnitOfWork { get; } = new(MockBehavior.Strict);

    public void GivenUser(string userId) =>
        UserRepository
            .Setup(repository => repository.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserDto(userId, "test-user"));

    public void GivenMissingUser(string userId) =>
        UserRepository
            .Setup(repository => repository.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserDto?)null);

    public void GivenProduct(Product product) =>
        ProductRepository
            .Setup(repository => repository.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

    public void GivenMissingProduct(int productId) =>
        ProductRepository
            .Setup(repository => repository.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

    public void GivenKeyExists(
        DateOnly produceDate,
        string manufactureEmail,
        bool exists,
        int? excludeProductId = null) =>
        ProductRepository
            .Setup(repository => repository.ExistsByProduceDateAndEmailAsync(
                produceDate,
                It.Is<Email>(email => email.Value == manufactureEmail),
                excludeProductId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(exists);

    public void GivenAddAssignsId(int productId, Func<Product, bool>? predicate = null) =>
        ProductRepository
            .Setup(repository => repository.AddAsync(
                It.Is<Product>(product => predicate == null || predicate(product)),
                It.IsAny<CancellationToken>()))
            .Callback<Product, CancellationToken>((product, _) => ProductTestData.SetId(product, productId))
            .Returns(Task.CompletedTask);

    public void GivenDelete(Product product) =>
        ProductRepository
            .Setup(repository => repository.Delete(
                It.Is<Product>(candidate => candidate.Id == product.Id)));

    public void GivenListAll(IReadOnlyList<Product> products) =>
        ProductRepository
            .Setup(repository => repository.ListAllAsync(CancellationToken))
            .ReturnsAsync(products);

    public void GivenListByOwner(string userId, IReadOnlyList<Product> products) =>
        GivenListByOwner(userId, (_, _) => products);

    public void GivenListByOwner(
        string userId,
        Func<string, CancellationToken, IReadOnlyList<Product>> productsFactory) =>
        ProductRepository
            .Setup(repository => repository.ListByOwnerAsync(userId, CancellationToken))
            .ReturnsAsync(productsFactory);

    public void GivenMap(ProductDto dto, Func<Product, bool>? predicate = null) =>
        Mapper
            .Setup(mapper => mapper.Map<ProductDto>(
                It.Is<Product>(product => predicate == null || predicate(product))))
            .Returns(dto);

    public void GivenListMap(
        IReadOnlyList<ProductDto> dtos,
        Func<IReadOnlyList<Product>, bool>? predicate = null) =>
        Mapper
            .Setup(mapper => mapper.Map<IReadOnlyList<ProductDto>>(
                It.Is<IReadOnlyList<Product>>(products => predicate == null || predicate(products))))
            .Returns(dtos);

    public void VerifyGetByIdOnce(int productId) =>
        ProductRepository.Verify(
            repository => repository.GetByIdAsync(productId, It.IsAny<CancellationToken>()),
            Times.Once);

    public void VerifyKeyExistsOnce(
        DateOnly produceDate,
        string manufactureEmail,
        int? excludeProductId = null) =>
        ProductRepository.Verify(
            repository => repository.ExistsByProduceDateAndEmailAsync(
                produceDate,
                It.Is<Email>(email => email.Value == manufactureEmail),
                excludeProductId,
                It.IsAny<CancellationToken>()),
            Times.Once);

    public void VerifyKeyExistsNever() =>
        ProductRepository.Verify(
            repository => repository.ExistsByProduceDateAndEmailAsync(
                It.IsAny<DateOnly>(),
                It.IsAny<Email>(),
                It.IsAny<int?>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

    public void VerifyAddedOnce() =>
        ProductRepository.Verify(
            repository => repository.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()),
            Times.Once);

    public void VerifyNotAdded() =>
        ProductRepository.Verify(
            repository => repository.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()),
            Times.Never);

    public void VerifyDeletedOnce() =>
        ProductRepository.Verify(
            repository => repository.Delete(It.IsAny<Product>()),
            Times.Once);

    public void VerifyNotDeleted() =>
        ProductRepository.Verify(
            repository => repository.Delete(It.IsAny<Product>()),
            Times.Never);

    public void VerifySavedOnce() =>
        UnitOfWork.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);

    public void VerifyNotSaved() =>
        UnitOfWork.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);

    public void VerifyProductMapUsedOnce() =>
        Mapper.Verify(
            mapper => mapper.Map<ProductDto>(It.IsAny<object>()),
            Times.Once);

    public void VerifyProductMapNotUsed() =>
        Mapper.Verify(
            mapper => mapper.Map<ProductDto>(It.IsAny<object>()),
            Times.Never);

    public void VerifyListMapUsedOnce() =>
        Mapper.Verify(
            mapper => mapper.Map<IReadOnlyList<ProductDto>>(It.IsAny<object>()),
            Times.Once);

    public void VerifyListMapNotUsed() =>
        Mapper.Verify(
            mapper => mapper.Map<IReadOnlyList<ProductDto>>(It.IsAny<object>()),
            Times.Never);

    public void VerifyListAllOnce() =>
        ProductRepository.Verify(
            repository => repository.ListAllAsync(CancellationToken),
            Times.Once);

    public void VerifyUserLookupOnce(string userId) =>
        UserRepository.Verify(
            repository => repository.GetByIdAsync(userId, It.IsAny<CancellationToken>()),
            Times.Once);

    public void VerifyListByOwnerOnce(string userId) =>
        ProductRepository.Verify(
            repository => repository.ListByOwnerAsync(userId, CancellationToken),
            Times.Once);

    public void VerifyListByOwnerNever() =>
        ProductRepository.Verify(
            repository => repository.ListByOwnerAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
}
