using System;
using LitJson;
using NGame2.NNodeTree.NRuntime;

namespace NGame2.NNodeTree.NTypeValue.NVariable;

public abstract class ValueBase<T> : ITypeValue<T>, ITypeValueBase
{
	private ArgumentContainer _argumentContainer = new ArgumentContainer();

	string ITypeValueBase.ValuePath { get; set; }

	public StringKey Name { get; set; }

	public ValueBase()
	{
	}

	public ValueBase(ValueBase<T> prototype)
	{
		Name = prototype.Name;
	}

	T ITypeValue<T>.GetValue(IVariable variable)
	{
		if (!TryGetValueDerived(variable, out var o_value))
		{
			return default(T);
		}
		return o_value;
	}

	bool ITypeValue<T>.TryGetValue(IVariable variable, out T o_value)
	{
		return TryGetValueDerived(variable, out o_value);
	}

	protected abstract bool TryGetValueDerived(IVariable variable, out T o_value);

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
		Name = StringKey.CreateFromJson(json["Name"]);
		_argumentContainer.LoadFromJson(json["ArgumentContainer"], null);
		return true;
	}

	JsonData ITypeValueBase.MakeBodyJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddField("Name", Name.MakeJson());
		jsonData.AddField("ArgumentContainer", _argumentContainer.MakeJson(null));
		return jsonData;
	}
}
