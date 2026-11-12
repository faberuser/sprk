using NVespa;

namespace LitJson;

public class EnumMakeFunctor<T> : IMakeFunctor<T>, IFunctor
{
	JsonData IMakeFunctor<T>.MakeJson(T item)
	{
		return new JsonData(EnumTools.GetName(item));
	}
}
