using System;
using LitJson;
using NGame2.NBattleField;
using NGame2.NNodeTree.NRuntime;
using NShared;
using NVespa;

namespace NGame2.NNodeTree.NTypeValue.NOperator.NDecimal;

public class CreatureCustomStat : ITypeValue<decimal>, ITypeValueBase
{
	private ArgumentContainer _argumentContainer = new ArgumentContainer();

	private StatType _statType = StatType.PhysicalAttack;

	string ITypeValueBase.ValuePath { get; set; }

	public CreatureCustomStat()
	{
	}

	public CreatureCustomStat(CreatureCustomStat prototype)
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
		ITypeValue<long> typeValue = _argumentContainer.GetTypeValue<long>(0);
		long num = 0L;
		if (typeValue != null)
		{
			num = typeValue.GetValue(variable);
		}
		if (!SingletonAuto<BattleFacade>.isValidInstance || num == 0L)
		{
			return true;
		}
		o_value = SingletonAuto<BattleFacade>.instance.CreatureCustomStatValue(num, _statType);
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
		return new CreatureCustomStat(this);
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
