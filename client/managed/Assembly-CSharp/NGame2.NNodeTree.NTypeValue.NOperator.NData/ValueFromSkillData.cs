using System;
using LitJson;
using NGame2.NNodeTree.NRuntime;
using NShared;

namespace NGame2.NNodeTree.NTypeValue.NOperator.NData;

public class ValueFromSkillData : ITypeValue<decimal>, ITypeValueBase
{
	public enum EValueType
	{
		TriggerValue,
		TriggerRatio,
		Cooltime,
		TargetCount
	}

	private ArgumentContainer _argumentContainer = new ArgumentContainer();

	private EValueType _valueType;

	private int _skillIndex;

	private int _valueIndex;

	string ITypeValueBase.ValuePath { get; set; }

	public ValueFromSkillData()
	{
	}

	public ValueFromSkillData(ValueFromSkillData prototype)
	{
		_valueType = prototype._valueType;
		_skillIndex = prototype._skillIndex;
		_valueIndex = prototype._valueIndex;
	}

	private bool TryGetNativeValue(out decimal o_value)
	{
		o_value = default(decimal);
		SkillData data = GameRoot.Instance.SkillDataContainer.GetData(_skillIndex);
		if (data == null)
		{
			return false;
		}
		if (_valueType == EValueType.TriggerValue)
		{
			if (!Vespa_Generic_IList_Extension_Method.IsValidIndex(data.TriggerValue, _valueIndex))
			{
				return false;
			}
			string text = data.TriggerValue[_valueIndex];
			if (String_Extension_Method.IsNullOrEmpty(text))
			{
				return false;
			}
			return decimal.TryParse(text, out o_value);
		}
		if (_valueType == EValueType.TriggerRatio)
		{
			o_value = data.TriggerRatio;
			return true;
		}
		if (_valueType == EValueType.Cooltime)
		{
			o_value = data.CoolTimeMs;
			return true;
		}
		if (_valueType == EValueType.TargetCount)
		{
			o_value = data.TargetCount;
			return true;
		}
		return true;
	}

	decimal ITypeValue<decimal>.GetValue(IVariable variable)
	{
		if (!TryGetNativeValue(out var o_value))
		{
			return 0m;
		}
		return o_value;
	}

	bool ITypeValue<decimal>.TryGetValue(IVariable variable, out decimal o_value)
	{
		if (!TryGetNativeValue(out o_value))
		{
			return false;
		}
		return true;
	}

	Type ITypeValueBase.GetValueType()
	{
		return typeof(decimal);
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
		return new ValueFromSkillData(this);
	}

	bool ITypeValueBase.LoadFromBodyJson(JsonData json)
	{
		_argumentContainer.LoadFromJson(json["ArgumentContainer"], null);
		_valueType = json["ValueType"].GETENUM(_valueType);
		_skillIndex = json["SkillIndex"].GETINT(_skillIndex);
		_valueIndex = json["ValueIndex"].GETINT(_valueIndex);
		return true;
	}

	JsonData ITypeValueBase.MakeBodyJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddField("ArgumentContainer", _argumentContainer.MakeJson(null));
		jsonData.AddFieldEnum("ValueType", _valueType);
		jsonData.AddFieldLong("SkillIndex", _skillIndex);
		jsonData.AddFieldLong("ValueIndex", _valueIndex);
		return jsonData;
	}
}
