using System;
using System.Collections.Generic;
using MessagePack;
using NShared.NUtil;
using NVespa;

namespace NShared;

public class OperationData : IDataKeySelector<OperationDataKey>
{
	public class SkillOperationDataSnapshotSerializer : ISnapshotMemberSerializer
	{
		object ISnapshotMemberSerializer.From(string value, DumpedSnapshotContainer container)
		{
			return FromSnapshotData(value);
		}

		string ISnapshotMemberSerializer.To(object value)
		{
			return ToSnapshotData(value as OperationData);
		}
	}

	[MessagePackObject(false)]
	public class LoadData : IDataKeySelector<OperationDataKey>
	{
		public static LoadData Empty = new LoadData();

		OperationDataKey IDataKeySelector<OperationDataKey>.Key => new OperationDataKey(SkillIndex, OperationNo);

		[Key(0)]
		public int SkillIndex { get; set; }

		[Key(1)]
		public int OperationNo { get; set; }

		[Key(2)]
		public OperationType Type { get; set; }

		[Key(3)]
		public OperationTargetType TargetType { get; set; }

		[Key(4)]
		public OperationTargetSelectType TargetSelectType { get; set; }

		[Key(5)]
		public int MaxRangeMm { get; set; }

		[Key(6)]
		public int AreaOffsetMm { get; set; }

		[Key(7)]
		public int RadiusMm { get; set; }

		[Key(8)]
		public int Width { get; set; }

		[Key(9)]
		public int[] AreaDelayMss { get; set; }

		[Key(10)]
		public int Angle { get; set; }

		[Key(11)]
		public string[] Value { get; set; }

		[Key(12)]
		public int[] ValueFactor { get; set; }

		[Key(13)]
		public int DurationMs { get; set; }

		[Key(14)]
		public int[] DurationFactor { get; set; }

		[Key(15)]
		public int[] DelayMs { get; set; }

		[Key(16)]
		public int TickTimeMs { get; set; }

		[Key(17)]
		public int[] TickTimeFactor { get; set; }

		[Key(18)]
		public int ReqSkillIndex { get; set; }

		[Key(19)]
		public OperationConditionType ConditionType { get; set; }

		[Key(20)]
		public string[] ConditionValue { get; set; }

		[Key(21)]
		public bool ConditionReverseCheck { get; set; }

		[Key(22)]
		public OperationConditionActionType ConditionActionType { get; set; }

		[Key(23)]
		public string[] ConditionTrueActionValue { get; set; }

		[Key(24)]
		public string[] ConditionFalseActionValue { get; set; }

		[Key(25)]
		public int HitChance { get; set; }

		[Key(26)]
		public int MaxAccCount { get; set; }

		[Key(27)]
		public int Parent { get; set; }

		[Key(28)]
		public string State { get; set; }

		[Key(29)]
		public int AddStateCount { get; set; }

		[Key(30)]
		public OperationFlag[] Flags { get; set; }

		[Key(31)]
		public StatType[] RecalculateValueByStats { get; set; }

		[Key(32)]
		public byte Group { get; set; }

		[Key(33)]
		public OperationGroupConditionType GroupConditionType { get; set; }

		[Key(34)]
		public string[] GroupConditionValues { get; set; }

		[Key(35)]
		public string[] RecalculateValueByStates { get; set; }
	}

	private LoadData _load;

	private readonly Dictionary<string, OperationValueExpression> _cachedOperationValueExpressions = new Dictionary<string, OperationValueExpression>();

	public const int SkillIndexMultiplier = 100;

	public static SkillOperationDataSnapshotSerializer SnapshotSerializer = new SkillOperationDataSnapshotSerializer();

	OperationDataKey IDataKeySelector<OperationDataKey>.Key => new OperationDataKey(SkillIndex, OperationNo);

	public int SkillIndex => _load.SkillIndex;

	public int OperationNo => _load.OperationNo;

	public OperationType Type => _load.Type;

	public OperationTargetType TargetType => _load.TargetType;

	public OperationTargetSelectType TargetSelectType => _load.TargetSelectType;

	public int MaxRangeMm => _load.MaxRangeMm;

	public int AreaOffsetMm => _load.AreaOffsetMm;

	public int RadiusMm => _load.RadiusMm;

	public int Width => _load.Width;

	public int[] AreaDelayMss => _load.AreaDelayMss;

	public int Angle => _load.Angle;

	public string[] Values => _load.Value;

	public int[] ValueFactors => _load.ValueFactor;

	public int DurationMs => _load.DurationMs;

	public int[] DurationFactors => _load.DurationFactor;

	public int[] DelayMss => _load.DelayMs;

	public int TickTimeMs => _load.TickTimeMs;

	public int[] TickTimeFactors => _load.TickTimeFactor;

	public int ReqSkillIndex => _load.ReqSkillIndex;

	public OperationConditionType ConditionType => _load.ConditionType;

	public string[] ConditionValues => _load.ConditionValue;

	public bool ReverseCheck => _load.ConditionReverseCheck;

	public OperationConditionActionType ConditionActionType => _load.ConditionActionType;

	public string[] ConditionTrueActionValues => _load.ConditionTrueActionValue;

	public string[] ConditionFalseActionValues => _load.ConditionFalseActionValue;

	public int HitChance => _load.HitChance;

	public int MaxAccCount => _load.MaxAccCount;

	public int Parent => _load.Parent;

	public string State => _load.State;

	public int AddStateCount => _load.AddStateCount;

	public OperationFlag[] Flags => _load.Flags;

	public StatType[] RecalculateValueByStats => _load.RecalculateValueByStats;

	public byte Group => _load.Group;

	public OperationGroupConditionType GroupConditionType => _load.GroupConditionType;

	public string[] GroupConditionValues => _load.GroupConditionValues;

	public string[] RecalculateValueByStates => _load.RecalculateValueByStates;

	public OperationData()
	{
		_load = LoadData.Empty;
	}

	public OperationData(LoadData load)
	{
		_load = load;
	}

	public long GetOperationValueInt(OperationInfo operationInfo, int valueIndex, long defaultValue = 0L)
	{
		BattleCreature firedCreature = operationInfo.FiredCreature;
		int skillLevel = operationInfo.SkillLevel;
		string[] values = operationInfo.Values;
		int creatureIndex = firedCreature?.CreatureIndex ?? 0;
		int creatureStar = firedCreature?.CreateInfo.HeroInfo.Star ?? 0;
		int creatureTranscended = firedCreature?.CreateInfo.HeroInfo.Transcended ?? 0;
		if (values == null)
		{
			values = Values;
		}
		string[] array = values;
		if (array == null)
		{
			return 0L;
		}
		string valueString = ArgParser.GetValueString(array, valueIndex);
		long num = 0L;
		num = ((string.IsNullOrEmpty(valueString) || !valueString.StartsWith("$")) ? ArgParser.GetValueLong(array, valueIndex, defaultValue) : CalculateOperationValueExpression(valueIndex, valueString, defaultValue, operationInfo));
		int operationLevelValue = GetOperationLevelValue(creatureIndex, creatureStar, creatureTranscended, valueIndex, skillLevel);
		return num + operationLevelValue;
	}

	public int GetOperationValueInt(int creatureIndex, int creatureStar, int creatureTranscended, int skillLevel, int valueIndex, int defaultValue = 0)
	{
		if (Values == null)
		{
			return 0;
		}
		int valueInt = ArgParser.GetValueInt(Values, valueIndex, defaultValue);
		int operationLevelValue = GetOperationLevelValue(creatureIndex, creatureStar, creatureTranscended, valueIndex, skillLevel);
		return valueInt + operationLevelValue;
	}

	public int GetOperationValueInt(int valueIndex, int defaultValue = 0)
	{
		return ArgParser.GetValueInt(Values, valueIndex, defaultValue);
	}

	public string[] GetOperationValues(string[] overrideValues = null)
	{
		return overrideValues ?? Values;
	}

	public bool GetOperationValueBool(int index, string[] overrideValues = null, bool defaultValue = false)
	{
		string operationValueString = GetOperationValueString(index, overrideValues);
		if (string.IsNullOrEmpty(operationValueString))
		{
			return defaultValue;
		}
		return string.Equals(operationValueString, "true", StringComparison.OrdinalIgnoreCase);
	}

	public string GetOperationValueString(int index, string[] overrideValues = null)
	{
		return ArgParser.GetValueString(overrideValues ?? Values, index);
	}

	public T GetOperationValueEnum<T>(int index, string[] overrideValues = null, T defaultValue = default(T)) where T : struct
	{
		return ArgParser.GetValueEnum<string, T>((IList<string>)(overrideValues ?? Values), index, defaultValue);
	}

	public T[] GetOperationValueEnums<T>(int index, string[] overrideValues = null) where T : struct
	{
		string operationValueString = GetOperationValueString(index, overrideValues);
		if (string.IsNullOrEmpty(operationValueString))
		{
			return new T[0];
		}
		List<T> list = new List<T>();
		string[] array = operationValueString.Split(new char[1] { '|' });
		for (int i = 0; i < array.Length; i++)
		{
			try
			{
				list.Add(EnumParser.Parse<T>(array[i]));
			}
			catch (Exception)
			{
			}
		}
		return list.ToArray();
	}

	public T GetOperationValueFlag<T>(int index, string[] overrideValues = null) where T : struct
	{
		T[] operationValueEnums = GetOperationValueEnums<T>(index, overrideValues);
		if (operationValueEnums == null || operationValueEnums.Length == 0)
		{
			return default(T);
		}
		int num = 0;
		for (int i = 0; i < operationValueEnums.Length; i++)
		{
			num |= (int)(object)operationValueEnums[i];
		}
		return (T)(object)num;
	}

	private OperationValueExpression GetOperationValueExpression(int valueIndex, string stringValue)
	{
		lock (_cachedOperationValueExpressions)
		{
			OperationValueExpression value = null;
			if (_cachedOperationValueExpressions.TryGetValue(stringValue, out value))
			{
				return value;
			}
			OperationValueExpression operationValueExpression = OperationValueExpression.Create(stringValue);
			_cachedOperationValueExpressions.Add(stringValue, operationValueExpression);
			return operationValueExpression;
		}
	}

	private long CalculateOperationValueExpression(int valueIndex, string stringValue, long defaultValue, OperationInfo operationInfo)
	{
		OperationValueExpression operationValueExpression = GetOperationValueExpression(valueIndex, stringValue);
		if (operationValueExpression == null)
		{
			return defaultValue;
		}
		List<TokenInfo> rpnTokens = operationValueExpression.RpnTokens;
		if (rpnTokens == null)
		{
			VespaDebug.LogError("Null rpn tokens.");
			return defaultValue;
		}
		OperationValueExpression.VariableFuncDelegate variableFunc = operationValueExpression.VariableFunc;
		if (variableFunc == null)
		{
			VespaDebug.LogError("Null variable func.");
			return defaultValue;
		}
		return ExpressionCalculator.Calculate(rpnTokens, (string key) => variableFunc(operationInfo, key));
	}

	public int GetFixedDelayMs()
	{
		return ArgParser.GetValueInt(DelayMss, 0);
	}

	public int GetDistanceDelayMs()
	{
		return ArgParser.GetValueInt(DelayMss, 1);
	}

	public int GetDistanceFixedDelayMs()
	{
		return ArgParser.GetValueInt(DelayMss, 2);
	}

	public int GetDistanceDelayOffsetMm()
	{
		return ArgParser.GetValueInt(DelayMss, 3);
	}

	public int GetOperationLevelValue(int creatureIndex, int creatureStar, int creatureTranscended, int valueIndex, int skillLevel)
	{
		if (ValueFactors == null)
		{
			return 0;
		}
		int num = valueIndex * 2;
		int num2 = valueIndex * 2 + 1;
		if (num2 >= ValueFactors.Length)
		{
			return 0;
		}
		int type = ValueFactors[num];
		int num3 = ValueFactors[num2];
		int levelFactor = GameRoot.Instance.SkillLevelFactorDataContainer.GetLevelFactor(type, skillLevel);
		int num4 = num3 * levelFactor / 1000;
		if (creatureIndex > 0 && creatureStar > 0)
		{
			int levelStatFactor = GameRoot.Instance.CreatureStatDataContainer.GetLevelStatFactor(creatureIndex, creatureStar, creatureTranscended);
			num4 = num4 * levelStatFactor / 1000;
		}
		return num4;
	}

	public int GetOperationLevelDuration(int skillLevel)
	{
		if (DurationFactors == null || DurationFactors.Length < 2)
		{
			return 0;
		}
		int type = DurationFactors[0];
		int num = DurationFactors[1];
		int levelFactor = GameRoot.Instance.SkillLevelFactorDataContainer.GetLevelFactor(type, skillLevel);
		return num * levelFactor / 1000;
	}

	public int GetOperationLevelTickTime(int skillLevel)
	{
		if (TickTimeFactors == null || TickTimeFactors.Length < 2)
		{
			return 0;
		}
		int type = TickTimeFactors[0];
		int num = TickTimeFactors[1];
		int levelFactor = GameRoot.Instance.SkillLevelFactorDataContainer.GetLevelFactor(type, skillLevel);
		return num * levelFactor / 1000;
	}

	public bool HasFlag(OperationFlag flag)
	{
		if (Flags != null)
		{
			for (int i = 0; i < Flags.Length; i++)
			{
				if (Flags[i] == flag)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasDelayedConditionCheckFlag()
	{
		if (Flags != null)
		{
			for (int i = 0; i < Flags.Length; i++)
			{
				if (IsDelayedConditionCheckFlag(Flags[i]))
				{
					return true;
				}
			}
		}
		return false;
	}

	public static OperationData CreateDummy(string[] values)
	{
		return new OperationData(new LoadData
		{
			Value = values
		});
	}

	public static bool IsDelayedConditionCheckFlag(OperationFlag flag)
	{
		if (flag != OperationFlag.CheckConditionOnBegin)
		{
			return flag == OperationFlag.CheckConditionOnReady;
		}
		return true;
	}

	public static OperationData FromSnapshotData(string value)
	{
		long result = 0L;
		long.TryParse(value, out result);
		int key = (int)(result / 100);
		int operationNo = (int)(result % 100);
		return GameRoot.Instance.SkillDataContainer.GetData(key)?.GetOperationData(operationNo);
	}

	public static string ToSnapshotData(OperationData operationData)
	{
		if (operationData == null)
		{
			return null;
		}
		return ((long)operationData.SkillIndex * 100L + operationData.OperationNo).ToString();
	}
}
