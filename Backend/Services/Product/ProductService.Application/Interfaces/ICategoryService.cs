using ProductService.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.Interfaces;
public interface ICategoryService
{
    Task<CategoryResponse?> GetByIdAsync(Guid id);

    Task<List<CategoryResponse>> GetAllAsync();

    Task<CategoryResponse> CreateAsync(
        CreateCategoryRequest request);

    Task<CategoryResponse?> UpdateAsync(
        Guid id,
        UpdateCategoryRequest request);

    Task<bool> DeleteAsync(Guid id);
}

