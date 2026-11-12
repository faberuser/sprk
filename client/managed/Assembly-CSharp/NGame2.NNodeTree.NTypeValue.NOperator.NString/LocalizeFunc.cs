using System;
using LitJson;
using NGame2.NNodeTree.NRuntime;
using NVespa;
using NVespa.NUtil;

namespace NGame2.NNodeTree.NTypeValue.NOperator.NString;

public class LocalizeFunc : ITypeValue<string>, ITypeValueBase
{
	private LocalizeLinkData _localizeLink = new LocalizeLinkData();

	private ArgumentContainer _argumentContainer = new ArgumentContainer();

	string ITypeValueBase.ValuePath { get; set; }

	public LocalizeFunc()
	{
	}

	public LocalizeFunc(LocalizeFunc prototype)
	{
		_localizeLink.LOADFROMJSON(prototype._localizeLink.MAKEJSON());
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
		o_value = _localizeLink.GetLocalize(array);
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
		return new LocalizeFunc(this);
	}

	bool ITypeValueBase.LoadFromBodyJson(JsonData json)
	{
		_localizeLink.LOADFROMJSON(json["LocalizeLink"]);
		_argumentContainer.LoadFromJson(json["ArgumentContainer"], null);
		return true;
	}

	JsonData ITypeValueBase.MakeBodyJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldJsonMake("LocalizeLink", _localizeLink);
		jsonData.AddField("ArgumentContainer", _argumentContainer.MakeJson(null));
		return jsonData;
	}
}
