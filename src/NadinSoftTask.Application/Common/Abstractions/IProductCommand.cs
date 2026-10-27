namespace NadinSoftTask.Application.Common.Abstractions;

public interface IProductCommand
{
    string Name { get; }

    string ManufacturePhone { get; }

    string ManufactureEmail { get; }

    bool? IsAvailable { get; }

    DateOnly? ProduceDate { get; }
}
