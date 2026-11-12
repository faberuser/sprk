namespace LitJson;

public interface ILoadFunctor<T> : IFunctor
{
	T CreateFromJson(JsonData json);
}
