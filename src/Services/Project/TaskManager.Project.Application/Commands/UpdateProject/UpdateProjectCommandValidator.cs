using FluentValidation;

namespace TaskManager.Project.Application.Commands.UpdateProject;

public class UpdateProjectCommandValidator : AbstractValidator<UpdateProjectCommand>
{
    public UpdateProjectCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Project ID is required.");

        RuleFor(x => x.Name)
            .MaximumLength(100).WithMessage("Project name must not exceed 100 characters.")
            .When(x => x.Name is not null);
    }
}
