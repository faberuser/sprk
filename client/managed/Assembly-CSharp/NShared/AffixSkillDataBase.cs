namespace NShared;

[JsonSerializable(Javascript = true)]
public class AffixSkillDataBase : AffixDataBase, IExtraSkillData
{
	public int SkillIndex { get; set; }

	public int SkillLevel { get; set; }

	int IExtraSkillData.SkillIndex => SkillIndex;

	int IExtraSkillData.SkillLevel => SkillLevel;

	bool IExtraSkillData.CheckActiveCondition(CampaignDungeonData dungeonData)
	{
		return CheckActiveCondition(dungeonData);
	}
}
