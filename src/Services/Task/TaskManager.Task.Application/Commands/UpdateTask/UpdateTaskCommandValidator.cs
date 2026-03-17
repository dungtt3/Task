using FluentValidation;

namespace TaskManager.Task.Application.Commands.UpdateTask;

public class UpdateTaskCommandValidator : AbstractValidator<UpdateTaskCommand>
{
    public UpdateTaskCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Title).MaximumLength(200).When(x => x.Title is not null);
    }
}
