using System;
using LitJson;

namespace NGame2.NNodeTree.NRuntime.NVariable;

public class ValueGenericFunc<T> : IVariableValue<T>, IVariableValueBase
{
	private Func<T> _func;

	public ValueGenericFunc(Func<T> func)
	{
		_func = func;
	}

	T IVariableValue<T>.GetValue()
	{
		if (_func == null)
		{
			return default(T);
		}
		return _func();
	}

	Type IVariableValueBase.GetValueType()
	{
		return typeof(T);
	}

	JsonData IVariableValueBase.DumpJson()
	{
		return new JsonData(Vespa_Common_Class_Extension_Method.Interface((IVariableValue<T>)this).GetValue().ToString());
	}
}
