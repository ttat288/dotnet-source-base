using Application.DTOs.Product;
using Domain.Common;
using Domain.Interfaces;
using MediatR;
using Application.Common.Exceptions;
using Application.Common.Constants;
using Application.Common.Interfaces;

namespace Application.Features.Products.Queries;

public record GetProductByIdQuery(Guid Id) : IRequest<BaseResponse<ProductDto>>;

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, BaseResponse<ProductDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;

    public GetProductByIdQueryHandler(IUnitOfWork unitOfWork, ICacheService cacheService)
    {
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
    }

    public async Task<BaseResponse<ProductDto>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = CacheKeys.ProductById(request.Id);
        
        var cachedProduct = await _cacheService.GetAsync<ProductDto>(cacheKey, cancellationToken);
        if (cachedProduct != null)
        {
            return BaseResponse<ProductDto>.SuccessResult(cachedProduct, "Product retrieved from cache");
        }

        var product = await _unitOfWork.Products.GetByIdAsync(request.Id);
        if (product == null)
        {
            throw new NotFoundException("Product", request.Id);
        }

        var category = await _unitOfWork.Categories.GetByIdAsync(product.CategoryId);

        var productDto = new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Stock = product.Stock,
            ImageUrl = product.ImageUrl,
            CategoryId = product.CategoryId,
            CategoryName = category?.Name ?? "Unknown",
            CreatedAt = product.CreatedAt
        };

        // Cache the result
        await _cacheService.SetAsync(cacheKey, productDto, CacheSettings.ProductCacheExpiry, cancellationToken);

        return BaseResponse<ProductDto>.SuccessResult(productDto, "Product retrieved successfully");
    }
}
