namespace LitJson;

public interface IMakeFunctor<T> : IFunctor
{
	JsonData MakeJson(T item);
}
