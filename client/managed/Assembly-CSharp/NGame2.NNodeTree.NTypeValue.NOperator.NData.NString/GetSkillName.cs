using System;
using LitJson;
using NGame2.NNodeTree.NRuntime;
using NGame2.NUI.NDescription;
using NShared;

namespace NGame2.NNodeTree.NTypeValue.NOperator.NData.NString;

public class GetSkillName : ITypeValue<string>, ITypeValueBase
{
	private ArgumentContainer _argumentContainer = new ArgumentContainer();

	private int _skillIndex;

	string ITypeValueBase.ValuePath { get; set; }

	public GetSkillName()
	{
	}

	public GetSkillName(GetSkillName prototype)
	{
		_skillIndex = prototype._skillIndex;
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
		SkillData data = GameRoot.Instance.SkillDataContainer.GetData(_skillIndex);
		if (data == null)
		{
			o_value = string.Empty;
			return true;
		}
		o_value = SkillDescContainer.GetSkillName(data);
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
		return new GetSkillName(this);
	}

	bool ITypeValueBase.LoadFromBodyJson(JsonData json)
	{
		_argumentContainer.LoadFromJson(json["ArgumentContainer"], null);
		_skillIndex = json["SkillIndex"].GETINT(_skillIndex);
		return true;
	}

	JsonData ITypeValueBase.MakeBodyJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddField("ArgumentContainer", _argumentContainer.MakeJson(null));
		jsonData.AddFieldLong("SkillIndex", _skillIndex);
		return jsonData;
	}
}
