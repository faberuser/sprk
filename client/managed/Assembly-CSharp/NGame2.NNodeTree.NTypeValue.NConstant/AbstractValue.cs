using System;
using LitJson;
using NGame2.NNodeTree.NRuntime;

namespace NGame2.NNodeTree.NTypeValue.NConstant;

public abstract class AbstractValue<T> : ITypeValue<T>, ITypeValueBase
{
	private ArgumentContainer _argumentContainer = new ArgumentContainer();

	string ITypeValueBase.ValuePath { get; set; }

	public abstract T Value { get; set; }

	T ITypeValue<T>.GetValue(IVariable variable)
	{
		return Value;
	}

	bool ITypeValue<T>.TryGetValue(IVariable variable, out T o_value)
	{
		o_value = Value;
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
		return CreateFromPrototypeDerived();
	}

	protected abstract ITypeValueBase CreateFromPrototypeDerived();

	bool ITypeValueBase.LoadFromBodyJson(JsonData json)
	{
		LoadFromBodyJsonDerived(json["Derived"]);
		_argumentContainer.LoadFromJson(json["ArgumentContainer"], null);
		return true;
	}

	protected abstract bool LoadFromBodyJsonDerived(JsonData json);

	JsonData ITypeValueBase.MakeBodyJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddField("Derived", MakeBodyJsonDerived());
		jsonData.AddField("ArgumentContainer", _argumentContainer.MakeJson(null));
		return jsonData;
	}

	protected abstract JsonData MakeBodyJsonDerived();
}
