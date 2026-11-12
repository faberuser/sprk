using System.Collections.Generic;
using NShared.NOperationCondition;
using NVespa;

namespace NShared;

public class OperationConditionFactory
{
	private readonly Dictionary<OperationConditionType, IOperationCondition> _conditionDictionary = new Dictionary<OperationConditionType, IOperationCondition>();

	public OperationConditionFactory()
	{
		RegisterCreator<And>(OperationConditionType.And);
		RegisterCreator<Or>(OperationConditionType.Or);
		RegisterCreator<Random>(OperationConditionType.Random);
		RegisterCreator<State>(OperationConditionType.State);
		RegisterCreator<NotState>(OperationConditionType.NotState);
		RegisterCreator<HpRatio>(OperationConditionType.HpRatio);
		RegisterCreator<HpRatioMore>(OperationConditionType.HpRatioMore);
		RegisterCreator<HpRatioMoreOrEqual>(OperationConditionType.HpRatioMoreOrEqual);
		RegisterCreator<HpRatioLess>(OperationConditionType.HpRatioLess);
		RegisterCreator<HpRatioLessOrEqual>(OperationConditionType.HpRatioLessOrEqual);
		RegisterCreator<SummoneeCount>(OperationConditionType.SummoneeCount);
		RegisterCreator<PrevCritical>(OperationConditionType.PrevCritical);
		RegisterCreator<MpRatio>(OperationConditionType.MpRatio);
		RegisterCreator<DebuffCount>(OperationConditionType.DebuffCount);
		RegisterCreator<CompareStat>(OperationConditionType.CompareStat);
		RegisterCreator<StateCount>(OperationConditionType.StateCount);
		RegisterCreator<SkillLevel>(OperationConditionType.SkillLevel);
		RegisterCreator<IsCoolTime>(OperationConditionType.IsCoolTime);
		RegisterCreator<Race>(OperationConditionType.Race);
		RegisterCreator<BattleDefineCode>(OperationConditionType.BattleDefineCode);
		RegisterCreator<Boss>(OperationConditionType.Boss);
		RegisterCreator<NShared.NOperationCondition.StaminaType>(OperationConditionType.StaminaType);
		RegisterCreator<Chapter>(OperationConditionType.Chapter);
		RegisterCreator<HeroClassCount>(OperationConditionType.HeroClassCount);
		RegisterCreator<HeroClass>(OperationConditionType.HeroClass);
		RegisterCreator<HeroClassMyself>(OperationConditionType.HeroClassMyself);
		RegisterCreator<NShared.NOperationCondition.CreaturePositionType>(OperationConditionType.CreaturePositionType);
		RegisterCreator<CreatureIndex>(OperationConditionType.CreatureIndex);
		RegisterCreator<HeroEquipTypeCount>(OperationConditionType.HeroEquipTypeCount);
		RegisterCreator<CompareDamage>(OperationConditionType.CompareDamage);
		RegisterCreator<CheckMPS>(OperationConditionType.CheckMPS);
	}

	private void RegisterCreator<T>(OperationConditionType conditionType) where T : IOperationCondition, new()
	{
		if (_conditionDictionary.ContainsKey(conditionType))
		{
			VespaDebug.LogError("Already registered operation condition : " + conditionType);
			return;
		}
		_ = conditionType.ToString() != typeof(T).Name;
		_conditionDictionary.Add(conditionType, new T());
	}

	private IOperationCondition GetCondition(OperationConditionType conditionType)
	{
		IOperationCondition value = null;
		_conditionDictionary.TryGetValue(conditionType, out value);
		return value;
	}

	public bool MeetCondition(OperationConditionType conditionType, FiredOperationInfo firedOperationInfo, BattleCreature appliedCreature, string[] values, bool reverseCheck)
	{
		if (conditionType == OperationConditionType.None)
		{
			return true;
		}
		IOperationCondition condition = GetCondition(conditionType);
		if (condition == null)
		{
			return true;
		}
		bool flag = condition.Meet(firedOperationInfo, appliedCreature, values);
		if (reverseCheck)
		{
			flag = !flag;
		}
		return flag;
	}
}
