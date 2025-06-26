using Application.DTOs.Product;
using Domain.Common;
using Domain.Entities;
using Domain.Interfaces;
using MediatR;
using Application.Common.Exceptions;

namespace Application.Features.Products.Commands;

public record CreateProductCommand(CreateProductRequest Request, Guid UserId) : IRequest<BaseResponse<ProductDto>>;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, BaseResponse<ProductDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateProductCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<BaseResponse<ProductDto>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(request.Request.CategoryId);
        if (category == null)
        {
            throw new NotFoundException("Category", request.Request.CategoryId);
        }

        var product = new Product
        {
            Name = request.Request.Name,
            Description = request.Request.Description,
            Price = request.Request.Price,
            Stock = request.Request.Stock,
            ImageUrl = request.Request.ImageUrl,
            CategoryId = request.Request.CategoryId,
            UserId = request.UserId
        };

        await _unitOfWork.Products.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();

        var productDto = new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Stock = product.Stock,
            ImageUrl = product.ImageUrl,
            CategoryId = product.CategoryId,
            CategoryName = category.Name,
            CreatedAt = product.CreatedAt
        };

        return BaseResponse<ProductDto>.SuccessResult(productDto, "Product created successfully");
    }
}
