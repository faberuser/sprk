using System;
using LitJson;
using NGame2.NNodeTree.NRuntime;
using NVespa;

namespace NGame2.NNodeTree.NTypeValue.NOperator.NString;

public class FormatFunc : ITypeValue<string>, ITypeValueBase
{
	private string _format = string.Empty;

	private ArgumentContainer _argumentContainer = new ArgumentContainer();

	string ITypeValueBase.ValuePath { get; set; }

	public FormatFunc()
	{
	}

	public FormatFunc(FormatFunc prototype)
	{
		_format = prototype._format;
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
		object[] array = new object[__Extension_Method_ICollection__.COUNT(_argumentContainer.HolderList)];
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_argumentContainer.HolderList); i++)
		{
			ITypeValueBase typeValueBase = _argumentContainer.GetTypeValueBase(i);
			array[i] = SingletonAuto<TypeFactory>.instance.TypeToObject(typeValueBase, variable);
		}
		try
		{
			o_value = string.Format(_format, array);
		}
		catch (Exception)
		{
			o_value = "Error";
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
		return new FormatFunc(this);
	}

	bool ITypeValueBase.LoadFromBodyJson(JsonData json)
	{
		_format = json["Format"].GETSTRING(_format);
		_argumentContainer.LoadFromJson(json["ArgumentContainer"], null);
		return true;
	}

	JsonData ITypeValueBase.MakeBodyJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("Format", _format);
		jsonData.AddField("ArgumentContainer", _argumentContainer.MakeJson(null));
		return jsonData;
	}
}
