namespace JsonBin;

public interface IFunctorJsonRead<T> : IFunctorJsonReadBase
{
	T CreateFrom(JsonRead json);
}
