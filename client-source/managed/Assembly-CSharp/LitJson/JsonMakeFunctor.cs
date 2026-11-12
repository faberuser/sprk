namespace LitJson;

public class JsonMakeFunctor<T> : IMakeFunctor<T>, IFunctor
{
	JsonData IMakeFunctor<T>.MakeJson(T item)
	{
		if (item is IMakeJson makeJson)
		{
			return makeJson.MakeJson();
		}
		return null;
	}
}
