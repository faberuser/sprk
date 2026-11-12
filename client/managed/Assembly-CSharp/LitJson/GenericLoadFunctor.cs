using System;

namespace LitJson;

public class GenericLoadFunctor<T> : ILoadFunctor<T>, IFunctor
{
	public Func<JsonData, T> Func;

	T ILoadFunctor<T>.CreateFromJson(JsonData json)
	{
		return Func(json);
	}
}
