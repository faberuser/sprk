using System;
using LitJson;
using NGame2.NNodeTree.NRuntime;
using NVespa;

namespace NGame2.NNodeTree.NTypeValue.NOperator.NString;

public class ConcatFunc : ITypeValue<string>, ITypeValueBase
{
	private ArgumentContainer _argumentContainer = new ArgumentContainer();

	string ITypeValueBase.ValuePath { get; set; }

	public ConcatFunc()
	{
	}

	public ConcatFunc(ConcatFunc prototype)
	{
	}

	string ITypeValue<string>.GetValue(IVariable variable)
	{
		if (!Vespa_Common_Class_Extension_Method.Interface((ITypeValue<string>)this).TryGetValue(variable, out var o_value))
		{
			return string.Empty;
		}
		return o_value;
	}

	bool ITypeValue<string>.TryGetValue(IVariable variable, out string o_value)
	{
		o_value = string.Empty;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_argumentContainer.HolderList); i++)
		{
			ITypeValueBase typeValueBase = _argumentContainer.GetTypeValueBase(i);
			object obj = SingletonAuto<TypeFactory>.instance.TypeToObject(typeValueBase, variable);
			if (obj != null)
			{
				o_value += obj.ToString();
			}
		}
		return true;
	}

	Type ITypeValueBase.GetValueType()
	{
		return typeof(string);
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
		return new ConcatFunc(this);
	}

	bool ITypeValueBase.LoadFromBodyJson(JsonData json)
	{
		_argumentContainer.LoadFromJson(json["ArgumentContainer"], null);
		return true;
	}

	JsonData ITypeValueBase.MakeBodyJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddField("ArgumentContainer", _argumentContainer.MakeJson(null));
		return jsonData;
	}
}
