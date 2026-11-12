using System;
using LitJson;
using NGame2.NNodeTree.NRuntime;
using NShared;

namespace NGame2.NNodeTree.NTypeValue.NOperator.NData.NSkillData;

public class OperationFireCount : ITypeValue<decimal>, ITypeValueBase
{
	private ArgumentContainer _argumentContainer = new ArgumentContainer();

	private int _skillIndex;

	private int _operationNo;

	string ITypeValueBase.ValuePath { get; set; }

	public OperationFireCount()
	{
	}

	public OperationFireCount(OperationFireCount prototype)
	{
		_skillIndex = prototype._skillIndex;
		_operationNo = prototype._operationNo;
	}

	private bool TryGetNativeValue(out decimal o_value)
	{
		o_value = default(decimal);
		SkillData data = GameRoot.Instance.SkillDataContainer.GetData(_skillIndex);
		if (data == null)
		{
			return true;
		}
		if (__Extension_Method_ICollection__.IsNullOrEmpty(data.TickTime))
		{
			o_value = 1m;
			return true;
		}
		for (int i = 0; i < data.TickTime.Length; i++)
		{
			SkillData.TickTimeData tickTimeData = data.TickTime[i];
			if (__Extension_Method_ICollection__.IsNullOrEmpty(tickTimeData.OperationNo))
			{
				++o_value;
				continue;
			}
			for (int j = 0; j < tickTimeData.OperationNo.Length; j++)
			{
				if (tickTimeData.OperationNo[j] == _operationNo)
				{
					++o_value;
					break;
				}
			}
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
		return new OperationFireCount(this);
	}

	bool ITypeValueBase.LoadFromBodyJson(JsonData json)
	{
		_argumentContainer.LoadFromJson(json["ArgumentContainer"], null);
		_skillIndex = json["SkillIndex"].GETINT(_skillIndex);
		_operationNo = json["OperationNo"].GETINT(_operationNo);
		return true;
	}

	JsonData ITypeValueBase.MakeBodyJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddField("ArgumentContainer", _argumentContainer.MakeJson(null));
		jsonData.AddFieldLong("SkillIndex", _skillIndex);
		jsonData.AddFieldLong("OperationNo", _operationNo);
		return jsonData;
	}
}
