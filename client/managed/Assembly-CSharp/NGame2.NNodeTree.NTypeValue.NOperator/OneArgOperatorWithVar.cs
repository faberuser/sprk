using System;
using LitJson;
using NGame2.NNodeTree.NRuntime;

namespace NGame2.NNodeTree.NTypeValue.NOperator;

public class OneArgOperatorWithVar<T, TArg0> : ITypeValue<T>, ITypeValueBase
{
	public Func<IVariable, TArg0, T> Func;

	private ArgumentContainer _argumentContainer = new ArgumentContainer();

	string ITypeValueBase.ValuePath { get; set; }

	public OneArgOperatorWithVar(Func<IVariable, TArg0, T> func)
	{
		Func = func;
	}

	public OneArgOperatorWithVar(OneArgOperatorWithVar<T, TArg0> prototype)
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
		if (typeValue == null)
		{
			o_value = default(T);
			return true;
		}
		if (!typeValue.TryGetValue(variable, out var o_value2))
		{
			o_value = default(T);
			return true;
		}
		o_value = Vespa_System_Action_Extension_Method.fire(Func, variable, o_value2);
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
		return new OneArgOperatorWithVar<T, TArg0>(this);
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
