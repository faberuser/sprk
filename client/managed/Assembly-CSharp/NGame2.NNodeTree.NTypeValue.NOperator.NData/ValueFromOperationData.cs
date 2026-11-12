using System;
using LitJson;
using NGame2.NNodeTree.NRuntime;
using NShared;

namespace NGame2.NNodeTree.NTypeValue.NOperator.NData;

public class ValueFromOperationData : ITypeValue<decimal>, ITypeValueBase
{
	public enum EValueType
	{
		Value,
		Duration,
		TickTime,
		ConditionValue
	}

	private ArgumentContainer _argumentContainer = new ArgumentContainer();

	private EValueType _valueType;

	private int _skillIndex;

	private int _operationNo;

	private int _valueIndex;

	string ITypeValueBase.ValuePath { get; set; }

	public ValueFromOperationData()
	{
	}

	public ValueFromOperationData(ValueFromOperationData prototype)
	{
		_valueType = prototype._valueType;
		_skillIndex = prototype._skillIndex;
		_operationNo = prototype._operationNo;
		_valueIndex = prototype._valueIndex;
	}

	private bool TryGetNativeValue(int creatureIndex, int creatureStar, int creatureTranscended, int skillLevel, out decimal o_value)
	{
		o_value = default(decimal);
		SkillData data = GameRoot.Instance.SkillDataContainer.GetData(_skillIndex);
		if (data == null)
		{
			return true;
		}
		if (!SkillData.IsValidOperationNo(_operationNo))
		{
			return true;
		}
		OperationData operationDataUnsafe = data.GetOperationDataUnsafe(_operationNo);
		if (operationDataUnsafe == null || operationDataUnsafe.Type == OperationType.None)
		{
			return true;
		}
		if (_valueType == EValueType.Value)
		{
			o_value = operationDataUnsafe.GetOperationValueInt(creatureIndex, creatureStar, creatureTranscended, skillLevel, _valueIndex);
			return true;
		}
		if (_valueType == EValueType.Duration)
		{
			o_value = operationDataUnsafe.DurationMs + operationDataUnsafe.GetOperationLevelDuration(skillLevel);
			return true;
		}
		if (_valueType == EValueType.TickTime)
		{
			o_value = operationDataUnsafe.TickTimeMs + operationDataUnsafe.GetOperationLevelTickTime(skillLevel);
			return true;
		}
		if (_valueType == EValueType.ConditionValue && Vespa_Generic_IList_Extension_Method.IsValidIndex(operationDataUnsafe.ConditionValues, _valueIndex))
		{
			return decimal.TryParse(operationDataUnsafe.ConditionValues[_valueIndex], out o_value);
		}
		return false;
	}

	decimal ITypeValue<decimal>.GetValue(IVariable variable)
	{
		if (!Vespa_Common_Class_Extension_Method.Interface((ITypeValue<decimal>)this).TryGetValue(variable, out var o_value))
		{
			return 0m;
		}
		return o_value;
	}

	bool ITypeValue<decimal>.TryGetValue(IVariable variable, out decimal o_value)
	{
		decimal value = variable.GetValue<decimal>(VariableKeyword.SkillLevel);
		decimal value2 = variable.GetValue<decimal>(VariableKeyword.CreatureIndex);
		decimal value3 = variable.GetValue<decimal>(VariableKeyword.CreatureStar);
		decimal value4 = variable.GetValue<decimal>(VariableKeyword.CreatureTranscended);
		if (!TryGetNativeValue((int)value2, (int)value3, (int)value4, (int)value, out o_value))
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
		return new ValueFromOperationData(this);
	}

	bool ITypeValueBase.LoadFromBodyJson(JsonData json)
	{
		_argumentContainer.LoadFromJson(json["ArgumentContainer"], null);
		_valueType = json["ValueType"].GETENUM(_valueType);
		_skillIndex = json["SkillIndex"].GETINT(_skillIndex);
		_operationNo = json["OperationNo"].GETINT(_operationNo);
		_valueIndex = json["ValueIndex"].GETINT(_valueIndex);
		return true;
	}

	JsonData ITypeValueBase.MakeBodyJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddField("ArgumentContainer", _argumentContainer.MakeJson(null));
		jsonData.AddFieldEnum("ValueType", _valueType);
		jsonData.AddFieldLong("SkillIndex", _skillIndex);
		jsonData.AddFieldLong("OperationNo", _operationNo);
		jsonData.AddFieldLong("ValueIndex", _valueIndex);
		return jsonData;
	}
}
