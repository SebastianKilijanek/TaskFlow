namespace TaskFlow.Application.Common.Interfaces;

public interface IOutbox
{
    void Add(string type, object payload);
}