using System;
using LitJson;
using NGame2.NNodeTree.NRuntime;
using NShared;

namespace NGame2.NNodeTree.NTypeValue.NOperator.NData;

public class ValueFromStatData : ITypeValue<decimal>, ITypeValueBase
{
	private ArgumentContainer _argumentContainer = new ArgumentContainer();

	private StatType _statType = StatType.PhysicalAttack;

	string ITypeValueBase.ValuePath { get; set; }

	public ValueFromStatData()
	{
	}

	public ValueFromStatData(ValueFromStatData prototype)
	{
		_statType = prototype._statType;
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
		ITypeValue<object> typeValue = _argumentContainer.GetTypeValue<object>(0);
		if (typeValue == null)
		{
			return true;
		}
		if (!(typeValue.GetValue(variable) is StatData statData))
		{
			return true;
		}
		o_value = statData.GetStat(_statType);
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
		return new ValueFromStatData(this);
	}

	bool ITypeValueBase.LoadFromBodyJson(JsonData json)
	{
		_argumentContainer.LoadFromJson(json["ArgumentContainer"], null);
		_statType = json["StatType"].GETENUM(_statType);
		return true;
	}

	JsonData ITypeValueBase.MakeBodyJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddField("ArgumentContainer", _argumentContainer.MakeJson(null));
		jsonData.AddFieldEnum("StatType", _statType);
		return jsonData;
	}
}
