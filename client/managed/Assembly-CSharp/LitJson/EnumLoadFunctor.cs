namespace LitJson;

public class EnumLoadFunctor<T> : ILoadFunctor<T>, IFunctor
{
	T ILoadFunctor<T>.CreateFromJson(JsonData json)
	{
		return json.GETENUM(default(T));
	}
}
