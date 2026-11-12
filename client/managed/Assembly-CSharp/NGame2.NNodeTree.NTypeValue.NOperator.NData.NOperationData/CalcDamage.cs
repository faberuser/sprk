using System;
using System.Collections.Generic;
using LitJson;
using NGame2.NNodeTree.NRuntime;
using NShared;
using NShared.NOperationRuntime;
using NVespa;

namespace NGame2.NNodeTree.NTypeValue.NOperator.NData.NOperationData;

public class CalcDamage : ITypeValue<decimal>, ITypeValueBase
{
	public class FuncLinker
	{
		private static object SyncObj = new object();

		private static BattleCreature _creatureForCalculation;

		private readonly Dictionary<OperationType, IOperationRuntime> _runtimeDictionary;

		private readonly OperationInfo _operationForCalculation;

		public FuncLinker(Func<IOperationRuntime, bool> selector)
		{
			_runtimeDictionary = new Dictionary<OperationType, IOperationRuntime>();
			using (new SnapshotWritableBlock())
			{
				EnumLookup<OperationType>.ValueData[] valueArray = EnumLookup<OperationType>.ValueArray;
				for (int i = 0; i < valueArray.Length; i++)
				{
					OperationType enumValue = valueArray[i].EnumValue;
					IOperationRuntime operationRuntime = GameRoot.Instance.OperationRuntimeFactory.Create(enumValue);
					if (operationRuntime != null && Vespa_System_Action_Extension_Method.fire(selector, operationRuntime))
					{
						_runtimeDictionary.Add(enumValue, operationRuntime);
					}
				}
				lock (SyncObj)
				{
					if (_creatureForCalculation == null)
					{
						BattleCreatureCreateInfo createInfo = new BattleCreatureCreateInfo
						{
							HeroInfo = new CachedHeroInfo()
						};
						_creatureForCalculation = new BattleCreature();
						_creatureForCalculation.Init(null, new BattleCreatureStockData
						{
							CreateInfo = createInfo
						});
						_creatureForCalculation.InitSnapshot(0);
					}
				}
				_operationForCalculation = new OperationInfo();
				_operationForCalculation.Creature = _creatureForCalculation;
				_operationForCalculation.FiredInfo = new FiredOperationInfo();
				_operationForCalculation.FiredInfo.TargetCreatures = new BattleCreature[1] { _creatureForCalculation };
				_operationForCalculation.FiredInfo.FiredCreature = _creatureForCalculation;
				_operationForCalculation.FiredInfo.TargetCreature = _creatureForCalculation;
				_operationForCalculation.FiredInfo.Init();
			}
		}

		public IOperationRuntime GetRuntime(OperationType operationType)
		{
			IOperationRuntime value = null;
			_runtimeDictionary.TryGetValue(operationType, out value);
			return value;
		}

		public IOperationRuntime InitRuntime(SkillData skillData, int skillLevel, OperationData operationData)
		{
			if (skillData == null || operationData == null)
			{
				return null;
			}
			IOperationRuntime runtime = GetRuntime(operationData.Type);
			if (runtime == null)
			{
				return null;
			}
			using (new SnapshotWritableBlock())
			{
				_operationForCalculation.FiredInfo.OperationData = operationData;
				_operationForCalculation.FiredInfo.SkillData = skillData;
				_operationForCalculation.FiredInfo.FiredSkillData = skillData;
				_operationForCalculation.FiredInfo.SkillLevel = skillLevel;
				_operationForCalculation.Runtime = runtime;
				_operationForCalculation.Init();
				runtime.Init(_operationForCalculation);
				return runtime;
			}
		}

		public void InitCreature(CreatureData creatureData, int creatureStar, StatData statData)
		{
			_creatureForCalculation.ChangeInitInfo(creatureData, creatureStar, statData);
		}
	}

	private static FuncLinker _funcLinker;

	private ArgumentContainer _argumentContainer = new ArgumentContainer();

	private int _skillIndex;

	private string _inputOperationNo = string.Empty;

	private List<int> _operationNoList = new List<int>();

	string ITypeValueBase.ValuePath { get; set; }

	public CalcDamage()
	{
		if (_funcLinker == null)
		{
			_funcLinker = new FuncLinker((IOperationRuntime runtime) => runtime is GetDamage || runtime is GetHeal);
		}
	}

	public CalcDamage(CalcDamage prototype)
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
				long amount = 0L;
				if (operationRuntime is GetDamage)
				{
					DamageFlag damageFlag = DamageFlag.None;
					(operationRuntime as GetDamage).GetInstantDamageInfo(out amount, out damageFlag);
				}
				else if (operationRuntime is GetHeal)
				{
					HealFlag healFlag = HealFlag.None;
					int overHealShieldRatio = 0;
					int overHealShieldMinHpRatio = 0;
					(operationRuntime as GetHeal).GetInstantHealInfo(out amount, out healFlag, out overHealShieldRatio, out overHealShieldMinHpRatio);
				}
				o_value += (decimal)amount;
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
		return new CalcDamage(this);
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
