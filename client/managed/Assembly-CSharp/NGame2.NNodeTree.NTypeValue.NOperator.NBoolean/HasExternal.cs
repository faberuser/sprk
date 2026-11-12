using System;
using LitJson;
using NGame2.NBattleView;
using NGame2.NNodeTree.NRuntime;

namespace NGame2.NNodeTree.NTypeValue.NOperator.NBoolean;

public class HasExternal : ITypeValue<bool>, ITypeValueBase
{
	private StringKey _name = StringKey.Empty;

	private ArgumentContainer _argumentContainer = new ArgumentContainer();

	string ITypeValueBase.ValuePath { get; set; }

	public HasExternal()
	{
	}

	public HasExternal(HasExternal prototype)
	{
		_name = prototype._name;
	}

	bool ITypeValue<bool>.GetValue(IVariable variable)
	{
		if (!Vespa_Common_Class_Extension_Method.Interface((ITypeValue<bool>)this).TryGetValue(variable, out var o_value))
		{
			return false;
		}
		return o_value;
	}

	bool ITypeValue<bool>.TryGetValue(IVariable variable, out bool o_value)
	{
		if (_name.IsNullOrEmpty())
		{
			o_value = false;
			return true;
		}
		ITypeValue<long> typeValue = _argumentContainer.GetTypeValue<long>(0);
		if (typeValue == null)
		{
			o_value = false;
			return true;
		}
		long value = typeValue.GetValue(variable);
		Creature bATTLECREATURE = variable.RUNTIMEINSTANCE().SCENECONTAINER().GETBATTLECREATURE((int)value);
		if (bATTLECREATURE == null)
		{
			o_value = false;
			return true;
		}
		IVariableValueBase valueBase = bATTLECREATURE.Variable.GetValueBase(_name);
		o_value = valueBase != null;
		return true;
	}

	Type ITypeValueBase.GetValueType()
	{
		return typeof(bool);
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
		return new HasExternal(this);
	}

	bool ITypeValueBase.LoadFromBodyJson(JsonData json)
	{
		_name = StringKey.CreateFromJson(json["Name"]);
		_argumentContainer.LoadFromJson(json["ArgumentContainer"], null);
		return true;
	}

	JsonData ITypeValueBase.MakeBodyJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddField("Name", _name.MakeJson());
		jsonData.AddField("ArgumentContainer", _argumentContainer.MakeJson(null));
		return jsonData;
	}
}
