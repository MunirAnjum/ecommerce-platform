using ProductService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.Interfaces;
public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(Guid id);

    Task<List<Category>> GetAllAsync();

    Task AddAsync(Category category);

    Task UpdateAsync(Category category);

    Task DeleteAsync(Category category);
}

