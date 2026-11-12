using System;

namespace LitJson;

public class GenericMakeFunctor<T> : IMakeFunctor<T>, IFunctor
{
	public Func<T, JsonData> Func;

	JsonData IMakeFunctor<T>.MakeJson(T item)
	{
		return Func(item);
	}
}
