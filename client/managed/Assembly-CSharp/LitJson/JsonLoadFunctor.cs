namespace LitJson;

public class JsonLoadFunctor<T> : ILoadFunctor<T>, IFunctor where T : new()
{
	T ILoadFunctor<T>.CreateFromJson(JsonData json)
	{
		T val = new T();
		if (val is ILoadJson loadJson)
		{
			loadJson.Load(json);
		}
		return val;
	}
}
