using ProductService.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.Interfaces;
 public interface IProductService
 {
    Task<ProductResponse?> GetByIdAsync(Guid id);

    Task<List<ProductResponse>> GetAllAsync();

    Task<ProductResponse> CreateAsync(CreateProductRequest request);

    Task<ProductResponse?> UpdateAsync(Guid id, UpdateProductRequest request);

    Task<bool> DeleteAsync(Guid id);
 }

