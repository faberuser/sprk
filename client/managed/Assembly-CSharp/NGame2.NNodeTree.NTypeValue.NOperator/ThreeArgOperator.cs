using System;
using LitJson;
using NGame2.NNodeTree.NRuntime;

namespace NGame2.NNodeTree.NTypeValue.NOperator;

public class ThreeArgOperator<T, TArg0, TArg1, TArg2> : ITypeValue<T>, ITypeValueBase
{
	public Func<TArg0, TArg1, TArg2, T> Func;

	private ArgumentContainer _argumentContainer = new ArgumentContainer();

	string ITypeValueBase.ValuePath { get; set; }

	public ThreeArgOperator(Func<TArg0, TArg1, TArg2, T> func)
	{
		Func = func;
	}

	public ThreeArgOperator(Func<TArg0, TArg1, TArg2, T> func, string arg0Name, string arg1Name, string arg2Name)
	{
		Func = func;
	}

	public ThreeArgOperator(ThreeArgOperator<T, TArg0, TArg1, TArg2> prototype)
	{
		Func = prototype.Func;
	}

	T ITypeValue<T>.GetValue(IVariable variable)
	{
		if (!Vespa_Common_Class_Extension_Method.Interface((ITypeValue<T>)this).TryGetValue(variable, out var o_value))
		{
			return default(T);
		}
		return o_value;
	}

	bool ITypeValue<T>.TryGetValue(IVariable variable, out T o_value)
	{
		ITypeValue<TArg0> typeValue = _argumentContainer.GetTypeValue<TArg0>(0);
		ITypeValue<TArg1> typeValue2 = _argumentContainer.GetTypeValue<TArg1>(1);
		ITypeValue<TArg2> typeValue3 = _argumentContainer.GetTypeValue<TArg2>(2);
		if (typeValue == null || typeValue2 == null || typeValue3 == null)
		{
			o_value = default(T);
			return true;
		}
		if (!typeValue.TryGetValue(variable, out var o_value2) || !typeValue2.TryGetValue(variable, out var o_value3) || !typeValue3.TryGetValue(variable, out var o_value4))
		{
			o_value = default(T);
			return true;
		}
		o_value = Vespa_System_Action_Extension_Method.fire(Func, o_value2, o_value3, o_value4);
		return true;
	}

	Type ITypeValueBase.GetValueType()
	{
		return typeof(T);
	}

	object ITypeValueBase.GetObjectValue(IVariable variable)
	{
		if (!this.TRYGETVALUE(variable, out var o_value))
		{
			return null;
		}
		return o_value;
	}

	ArgumentContainer ITypeValueBase.GetArgumentContainer()
	{
		return _argumentContainer;
	}

	ITypeValueBase ITypeValueBase.CreateFromPrototype()
	{
		return new ThreeArgOperator<T, TArg0, TArg1, TArg2>(this);
	}

	bool ITypeValueBase.LoadFromBodyJson(JsonData json)
	{
		_argumentContainer.LoadFromJson(json, null);
		return true;
	}

	JsonData ITypeValueBase.MakeBodyJson()
	{
		return _argumentContainer.MakeJson(null);
	}
}
