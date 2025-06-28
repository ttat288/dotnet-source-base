using Application.DTOs.Product;
using Domain.Common;
using Domain.Interfaces;
using MediatR;
using Application.Common.Constants;
using Application.Common.Interfaces;

namespace Application.Features.Products.Queries;

public record GetProductsQuery(int Page = 1, int PageSize = 10) : IRequest<BaseResponse<List<ProductDto>>>;

public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, BaseResponse<List<ProductDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;

    public GetProductsQueryHandler(IUnitOfWork unitOfWork, ICacheService cacheService)
    {
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
    }

    public async Task<BaseResponse<List<ProductDto>>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = CacheKeys.ProductsList(request.Page, request.PageSize);
    
        var cachedProducts = await _cacheService.GetAsync<List<ProductDto>>(cacheKey, cancellationToken);
        if (cachedProducts != null)
        {
            return BaseResponse<List<ProductDto>>.SuccessResult(cachedProducts, "Products retrieved from cache");
        }

        var products = await _unitOfWork.Products.GetAllAsync();
    
        var productDtos = products.Skip((request.Page - 1) * request.PageSize)
                             .Take(request.PageSize)
                             .Select(p => new ProductDto
                             {
                                 Id = p.Id,
                                 Name = p.Name,
                                 Description = p.Description,
                                 Price = p.Price,
                                 Stock = p.Stock,
                                 ImageUrl = p.ImageUrl,
                                 CategoryId = p.CategoryId,
                                 CreatedAt = p.CreatedAt
                             }).ToList();

        // Cache the result
        await _cacheService.SetAsync(cacheKey, productDtos, CacheSettings.ProductCacheExpiry, cancellationToken);

        return BaseResponse<List<ProductDto>>.SuccessResult(productDtos, "Products retrieved successfully");
    }
}
