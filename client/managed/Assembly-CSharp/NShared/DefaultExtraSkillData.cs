namespace NShared;

public class DefaultExtraSkillData : IExtraSkillData
{
	public int SkillIndex { get; set; }

	public int SkillLevel { get; set; }

	bool IExtraSkillData.CheckActiveCondition(CampaignDungeonData dungeonData)
	{
		return true;
	}
}
