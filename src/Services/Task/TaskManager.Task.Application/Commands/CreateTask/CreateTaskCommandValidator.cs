using FluentValidation;

namespace TaskManager.Task.Application.Commands.CreateTask;

public class CreateTaskCommandValidator : AbstractValidator<CreateTaskCommand>
{
    public CreateTaskCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ProjectId).NotEmpty();
        RuleFor(x => x.ReporterId).NotEmpty();
    }
}
