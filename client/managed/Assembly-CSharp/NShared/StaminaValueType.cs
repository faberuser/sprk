namespace NShared;

[JsonSerializable(Javascript = true)]
public enum StaminaValueType
{
	None,
	Fixed,
	TeamLevelData,
	TeamLevelDataAndHeroCount,
	FixedMaxCount,
	OverValueRemain,
	OverValueRemainMaxCount
}
