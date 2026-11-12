namespace NVespa;

public interface IGenericVisitor<T> : IVisitor
{
	void Visit(T t);
}
