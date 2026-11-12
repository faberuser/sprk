using System;
using System.Collections.Generic;
using LitJson;
using NGame2.NNodeTree.NRuntime;
using NShared;
using NShared.NOperationRuntime;

namespace NGame2.NNodeTree.NTypeValue.NOperator.NData.NOperationData;

public class CalcDot : ITypeValue<decimal>, ITypeValueBase
{
	private static CalcDamage.FuncLinker _funcLinker;

	private ArgumentContainer _argumentContainer = new ArgumentContainer();

	private int _skillIndex;

	private string _inputOperationNo = string.Empty;

	private List<int> _operationNoList = new List<int>();

	string ITypeValueBase.ValuePath { get; set; }

	public CalcDot()
	{
		if (_funcLinker == null)
		{
			_funcLinker = new CalcDamage.FuncLinker((IOperationRuntime runtime) => runtime is GetDot || runtime is GetHot);
		}
	}

	public CalcDot(CalcDot prototype)
	{
		_skillIndex = prototype._skillIndex;
		_inputOperationNo = prototype._inputOperationNo;
		_operationNoList = new List<int>(prototype._operationNoList);
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
		o_value = default(decimal);
		SkillData data = GameRoot.Instance.SkillDataContainer.GetData(_skillIndex);
		if (data == null)
		{
			return true;
		}
		ITypeValue<object> typeValue = _argumentContainer.GetTypeValue<object>(0);
		if (typeValue == null)
		{
			return true;
		}
		if (!(typeValue.GetValue(variable) is StatData statData))
		{
			return true;
		}
		int skillLevel = (int)variable.GetValue<decimal>(VariableKeyword.SkillLevel);
		int key = (int)variable.GetValue<decimal>(VariableKeyword.CreatureIndex);
		int creatureStar = (int)variable.GetValue<decimal>(VariableKeyword.CreatureStar);
		CreatureData data2 = GameRoot.Instance.CreatureDataContainer.GetData(key);
		_funcLinker.InitCreature(data2, creatureStar, statData);
		for (int i = 0; i < _operationNoList.Count; i++)
		{
			int operationNo = _operationNoList[i];
			if (!SkillData.IsValidOperationNo(operationNo))
			{
				continue;
			}
			OperationData operationDataUnsafe = data.GetOperationDataUnsafe(operationNo);
			if (operationDataUnsafe == null)
			{
				continue;
			}
			IOperationRuntime operationRuntime = _funcLinker.InitRuntime(data, skillLevel, operationDataUnsafe);
			if (operationRuntime != null)
			{
				long num = 0L;
				if (operationRuntime is GetDot)
				{
					num = (operationRuntime as GetDot).GetDamageAmount();
				}
				else if (operationRuntime is GetHot)
				{
					num = (operationRuntime as GetHot).GetHealAmount();
				}
				if (num > 0)
				{
					int num2 = operationDataUnsafe.TickTimeMs + operationDataUnsafe.GetOperationLevelTickTime(skillLevel);
					int num3 = operationDataUnsafe.DurationMs + operationDataUnsafe.GetOperationLevelDuration(skillLevel);
					int num4 = ((num2 > 0) ? (num3 / num2) : 0);
					o_value += (decimal)(num * num4);
				}
			}
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
		return new CalcDot(this);
	}

	bool ITypeValueBase.LoadFromBodyJson(JsonData json)
	{
		_argumentContainer.LoadFromJson(json["ArgumentContainer"], null);
		_skillIndex = json["SkillIndex"].GETINT(_skillIndex);
		_inputOperationNo = json["RawOperationNo"].GETSTRING(_inputOperationNo);
		_operationNoList.LoadFromJson(json["OperationNoList"]);
		return true;
	}

	JsonData ITypeValueBase.MakeBodyJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddField("ArgumentContainer", _argumentContainer.MakeJson(null));
		jsonData.AddFieldLong("SkillIndex", _skillIndex);
		jsonData.AddFieldString("RawOperationNo", _inputOperationNo);
		jsonData.AddField("OperationNoList", Json_Extension_Method_IList.MakeJson(_operationNoList));
		return jsonData;
	}
}
