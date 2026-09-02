using FluentValidation;
using ProductService.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.Validators;
public class CreateCategoryRequestValidator : AbstractValidator<CreateCategoryRequest>
{
    public CreateCategoryRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Category name is required.")
            .MaximumLength(150)
            .WithMessage("Category name can not exceed 150 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("Description can not exceed 500 characters.");
    }
}