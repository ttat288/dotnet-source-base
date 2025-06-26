using Application.DTOs.Product;
using Domain.Common;
using Domain.Interfaces;
using MediatR;

namespace Application.Features.Products.Queries;

public record GetProductsQuery(int Page = 1, int PageSize = 10) : IRequest<BaseResponse<List<ProductDto>>>;

public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, BaseResponse<List<ProductDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetProductsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<BaseResponse<List<ProductDto>>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
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

        return BaseResponse<List<ProductDto>>.SuccessResult(productDtos, "Products retrieved successfully");
    }
}
