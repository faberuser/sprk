using System.Collections.Generic;
using System.Linq;
using MessagePack;
using NShared.NJit;
using NShared.NUtil;

namespace NShared;

[MessagePackObject(false)]
public class SkillData : IDataKeySelector<int>, ITriggerValueGetter
{
	public class TickTimeData
	{
		public int TickIndex;

		public int TickMs;

		public int[] OperationNo;
	}

	public const int MaxOperationCount = 8;

	private Condition[] _activeConditions;

	[IgnoreMember]
	private TickTimeData[] _tickTime;

	int IDataKeySelector<int>.Key => Index;

	[Key(0)]
	public int Index { get; set; }

	[Key(3)]
	public string ReplaceNameKey { get; set; }

	[Key(4)]
	public string[] NameParam { get; set; }

	[Key(5)]
	public string ReplaceDescKey { get; set; }

	[Key(6)]
	public string[] DescParam { get; set; }

	[Key(7)]
	public int ReqIndex { get; set; }

	[Key(8)]
	public int NextIndex { get; set; }

	[Key(9)]
	public int FollowLevelIndex { get; set; }

	[Key(10)]
	public SkillTriggerType TriggerType { get; set; }

	[Key(11)]
	public string[] TriggerValue { get; set; }

	[Key(12)]
	public int TriggerRatio { get; set; }

	[Key(13)]
	public bool TriggerDisabled { get; set; }

	[Key(14)]
	public SkillTriggerFlag[] TriggerFlag { get; set; }

	[Key(15)]
	public int[] CancelableSkillIndex { get; set; }

	[Key(16)]
	public int Mp { get; set; }

	[Key(17)]
	public int ReqHpRatio { get; set; }

	[Key(18)]
	public string[] ActiveCondition { get; set; }

	[Key(19)]
	public int CoolTimeMs { get; set; }

	[Key(20)]
	public int[] CoolTimeGroup { get; set; }

	[Key(21)]
	public int InitialCoolTimeMs { get; set; }

	[Key(22)]
	public int CastingTimeMs { get; set; }

	[Key(23)]
	public int ActingTimeMs { get; set; }

	[Key(24)]
	public int SoloTimeMs { get; set; }

	[Key(25)]
	public int[] DurationTimeMs { get; set; }

	[Key(26)]
	public bool CancelOnWaveEnd { get; set; }

	[Key(27)]
	public string[] TickTimeValue { get; set; }

	[Key(28)]
	public SkillAttrType AttrType { get; set; }

	[Key(29)]
	public bool BaseAttack { get; set; }

	[Key(30)]
	public AttackAttrType AttackType { get; set; }

	[Key(31)]
	public string[] StateType { get; set; }

	[Key(32)]
	public string[] ClearStateType { get; set; }

	[Key(33)]
	public string[] IgnoreStateType { get; set; }

	[Key(34)]
	public string[] DisabledStateType { get; set; }

	[Key(35)]
	public SkillTargetType TargetType { get; set; }

	[Key(36)]
	public short TargetCount { get; set; }

	[Key(37)]
	public string TargetCountState { get; set; }

	[Key(38)]
	public int MinRangeMm { get; set; }

	[Key(39)]
	public int MaxRangeMm { get; set; }

	[Key(40)]
	public bool DeadTarget { get; set; }

	[Key(41)]
	public bool IgnoreBlock { get; set; }

	[Key(42)]
	public bool RetargetOnFire { get; set; }

	[Key(43)]
	public bool ViewTargetWhileUse { get; set; }

	[Key(44)]
	public SkillTargetFlag TargetFlag { get; set; }

	[Key(45)]
	public CreatureTagType CreatureTagType { get; set; }

	[Key(46)]
	public string[] TargetStateType { get; set; }

	[Key(47)]
	public SkillCcFlag CcFlag { get; set; }

	[IgnoreMember]
	public string NAME_KEY => "SKILL_NAME_" + Index;

	[IgnoreMember]
	private string DESC_KEY => "SKILL_DESC_" + Index;

	[IgnoreMember]
	public TickTimeData[] TickTime => GetCachedTickTime();

	[IgnoreMember]
	public string Name { get; set; }

	[IgnoreMember]
	public string Description => GameRoot.LocalizeProxy.GetCache_or_Build(this, "Description", BuildLocalize_Description);

	public override string ToString()
	{
		return "(Index=" + Index + ")";
	}

	public OperationData GetOperationData(int operationNo)
	{
		return TableRoot.Instance.GetDataContainer<SkillDataContainer>().GetOperationData(Index, operationNo);
	}

	public OperationData GetOperationDataUnsafe(int operationNo)
	{
		return TableRoot.Instance.GetDataContainer<SkillDataContainer>().GetOperationData(Index, operationNo);
	}

	public Condition[] GetActiveCondition()
	{
		if (_activeConditions == null)
		{
			_activeConditions = GetActiveConditions(ActiveCondition);
		}
		return _activeConditions;
	}

	public static Condition[] GetActiveConditions(string[] conditionStrings)
	{
		List<Condition> list = new List<Condition>();
		if (conditionStrings != null)
		{
			foreach (string text in conditionStrings)
			{
				if (text != null && text.Length != 0)
				{
					Condition condition = Condition.GetCondition(text);
					if (condition != null)
					{
						list.Add(condition);
					}
				}
			}
		}
		return list.ToArray();
	}

	public bool HasTriggerFlag(SkillTriggerFlag flag)
	{
		if (TriggerFlag != null)
		{
			SkillTriggerFlag[] triggerFlag = TriggerFlag;
			for (int i = 0; i < triggerFlag.Length; i++)
			{
				if ((triggerFlag[i] & flag) != SkillTriggerFlag.None)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasCcFlag(SkillCcFlag flag)
	{
		return (CcFlag & flag) != 0;
	}

	public bool HasTargetFlag(SkillTargetFlag flag)
	{
		return (TargetFlag & flag) != 0;
	}

	public int GetTotalTargetCount(BattleCreature firedCreature)
	{
		int num = TargetCount;
		if (!string.IsNullOrEmpty(TargetCountState) && firedCreature != null)
		{
			List<IStateModifier> allStateModifier = firedCreature.StateController.GetAllStateModifier(TargetCountState);
			if (allStateModifier != null)
			{
				for (int i = 0; i < allStateModifier.Count; i++)
				{
					num += allStateModifier[i]?.GetStackCount() ?? 0;
				}
			}
		}
		return firedCreature.SkillController.ModifierContainer.GetModifiedInt(Index, 0, OperationFieldType.SkillTargetCount, 0, num);
	}

	public SkillTargetType GetSkillTargetType(BattleCreature firedCreature)
	{
		return firedCreature?.SkillController.ModifierContainer.GetModifiedT(Index, 0, OperationFieldType.SkillTargetType, 0, TargetType) ?? SkillTargetType.Myself;
	}

	public int GetSoloTimeMs(bool isIgnoreSoloTime)
	{
		if (SoloTimeMs > 0 && isIgnoreSoloTime)
		{
			return 0;
		}
		return SoloTimeMs;
	}

	public int GetCoolTimeMs(BattleCreature firedCreature)
	{
		return firedCreature?.SkillController.ModifierContainer.GetModifiedInt(Index, 0, OperationFieldType.SkillCoolTimeMs, 0, CoolTimeMs) ?? 0;
	}

	public int GetMp(BattleCreature firedCreature)
	{
		return firedCreature?.SkillController.ModifierContainer.GetModifiedInt(Index, 0, OperationFieldType.SkillMp, 0, Mp) ?? 0;
	}

	public int GetMinRangeMm(BattleCreature firedCreature)
	{
		return firedCreature?.SkillController.ModifierContainer.GetModifiedInt(Index, 0, OperationFieldType.SkillMinRangeMm, 0, MinRangeMm) ?? 0;
	}

	public int GetMaxRangeMm(BattleCreature firedCreature)
	{
		return firedCreature?.SkillController.ModifierContainer.GetModifiedInt(Index, 0, OperationFieldType.SkillMaxRangeMm, 0, MaxRangeMm) ?? 0;
	}

	public bool GetRetargetOnFire(BattleCreature firedCreature)
	{
		return firedCreature?.SkillController.ModifierContainer.GetModifiedBool(Index, 0, OperationFieldType.SkillRetargetOnFire, 0, RetargetOnFire) ?? false;
	}

	public bool GetViewTargetWhileUse(BattleCreature firedCreature)
	{
		return firedCreature?.SkillController.ModifierContainer.GetModifiedBool(Index, 0, OperationFieldType.SkillViewTargetWhileUse, 0, ViewTargetWhileUse) ?? false;
	}

	public string GetTriggerValue(BattleCreature firedCreature, int valueIndex)
	{
		string text = ((TriggerValue != null && valueIndex < TriggerValue.Length) ? TriggerValue[valueIndex] : null);
		if (firedCreature == null)
		{
			return text;
		}
		return firedCreature.SkillController.ModifierContainer.GetModifiedString(Index, 0, OperationFieldType.SkillTriggerValue, valueIndex, text);
	}

	public bool HasTriggerValue(int valueIndex)
	{
		if (TriggerValue != null)
		{
			return valueIndex < TriggerValue.Length;
		}
		return false;
	}

	public HashSet<string> GetClearStateTypeList(BattleCreature firedCreature)
	{
		return GetModifiedStateTypeList(firedCreature, OperationFieldType.SkillClearStateType, ClearStateType);
	}

	public HashSet<string> GetIgnoreStateTypeList(BattleCreature firedCreature)
	{
		return GetModifiedStateTypeList(firedCreature, OperationFieldType.SkillIgnoreStateType, IgnoreStateType);
	}

	private HashSet<string> GetModifiedStateTypeList(BattleCreature firedCreature, OperationFieldType fieldType, string[] initialStates)
	{
		if (firedCreature == null)
		{
			return null;
		}
		if (initialStates == null || initialStates.Length == 0)
		{
			return null;
		}
		HashSet<string> hashSet = null;
		for (int i = 0; i < initialStates.Length; i++)
		{
			string modifiedString = firedCreature.SkillController.ModifierContainer.GetModifiedString(Index, 0, fieldType, i, initialStates[i]);
			if (!string.IsNullOrEmpty(modifiedString))
			{
				hashSet = hashSet ?? new HashSet<string>();
				hashSet.Add(modifiedString);
			}
		}
		return hashSet;
	}

	private TickTimeData[] GetCachedTickTime()
	{
		if (_tickTime != null)
		{
			return _tickTime;
		}
		ParseTickTime();
		return _tickTime;
	}

	public void ParseTickTime()
	{
		if (TickTimeValue == null || TickTimeValue.Length == 0)
		{
			_tickTime = null;
			return;
		}
		lock (TickTimeValue)
		{
			TickTimeData[] array = new TickTimeData[TickTimeValue.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = CreateTickTimeData(i, TickTimeValue[i]);
			}
			_tickTime = array;
		}
	}

	public int GetFixedDurationMs()
	{
		if (DurationTimeMs == null || DurationTimeMs.Length == 0)
		{
			return 0;
		}
		return DurationTimeMs[0];
	}

	public int GetDurationMsPerDistance()
	{
		if (DurationTimeMs == null || DurationTimeMs.Length <= 1)
		{
			return 0;
		}
		return DurationTimeMs[1];
	}

	public int GetDurationMs(int distanceMm)
	{
		int fixedDurationMs = GetFixedDurationMs();
		int durationMsPerDistance = GetDurationMsPerDistance();
		return fixedDurationMs + durationMsPerDistance * distanceMm / 1000;
	}

	public bool IsDurationalSkill()
	{
		int fixedDurationMs = GetFixedDurationMs();
		int durationMsPerDistance = GetDurationMsPerDistance();
		if (fixedDurationMs <= 0)
		{
			return durationMsPerDistance > 0;
		}
		return true;
	}

	private string BuildLocalize_Description()
	{
		string[] cache_or_Build = GameRoot.LocalizeProxy.GetCache_or_Build(this, "Description", BuildLocalize_DescParam);
		ILocalizeProxy localizeProxy = GameRoot.LocalizeProxy;
		string dESC_KEY = DESC_KEY;
		string replaceDescKey = ReplaceDescKey;
		object[] values = cache_or_Build;
		return localizeProxy.GetStringWithReplaceKey(dESC_KEY, replaceDescKey, values);
	}

	private string[] BuildLocalize_DescParam()
	{
		return GameRoot.LocalizeProxy.ConvertArrayByCultureInfo(DescParam);
	}

	public string BuildLocalize_CustomDescription(params string[] param)
	{
		List<string> list = param.ToList();
		if (DescParam != null)
		{
			list.AddRange(DescParam.ToList());
		}
		string[] array = GameRoot.LocalizeProxy.ConvertArrayByCultureInfo(list.ToArray());
		ILocalizeProxy localizeProxy = GameRoot.LocalizeProxy;
		string dESC_KEY = DESC_KEY;
		string replaceDescKey = ReplaceDescKey;
		object[] values = array;
		return localizeProxy.GetStringWithReplaceKey(dESC_KEY, replaceDescKey, values);
	}

	public static bool IsValidOperationNo(int operationNo)
	{
		if (operationNo <= 0)
		{
			return false;
		}
		if (operationNo > 8)
		{
			return false;
		}
		return true;
	}

	public static TickTimeData CreateTickTimeData(int tickIndex, string tickTimeValue)
	{
		TickTimeData tickTimeData = new TickTimeData();
		tickTimeData.TickIndex = tickIndex;
		if (!SplitString(tickTimeValue, '[', out var front, out var back))
		{
			tickTimeData.TickMs = ParseInt(tickTimeValue, 0);
			return tickTimeData;
		}
		tickTimeData.TickMs = ParseInt(front, 0);
		if (!SplitString(back, ']', out var front2, out back))
		{
			return tickTimeData;
		}
		string[] array = front2.Split(':');
		if (array == null || array.Length == 0)
		{
			return tickTimeData;
		}
		tickTimeData.OperationNo = new int[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			tickTimeData.OperationNo[i] = ParseInt(array[i], 0);
		}
		return tickTimeData;
	}

	private static int ParseInt(string str, int defaultValue)
	{
		if (!int.TryParse(str, out var result))
		{
			return defaultValue;
		}
		return result;
	}

	private static bool SplitString(string str, char separator, out string front, out string back)
	{
		int num = str.IndexOf(separator);
		if (num < 0)
		{
			front = string.Empty;
			back = string.Empty;
			return false;
		}
		front = str.Substring(0, num);
		back = str.Substring(num + 1);
		return true;
	}
}
