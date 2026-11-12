namespace NShared;

[JsonSerializable(Javascript = true)]
public enum CreatureOpenType
{
	None,
	RecruitOnly,
	CommingSoon,
	Opened,
	Scenario,
	MileageOnly,
	ChapterReward
}
