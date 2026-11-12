namespace NShared;

[JsonSerializable(Javascript = true)]
public class OrdealArenaSkillDataBase : OrdealArenaBuffDataBase, IExtraSkillData
{
	public int SkillIndex { get; set; }

	int IExtraSkillData.SkillIndex => SkillIndex;

	int IExtraSkillData.SkillLevel => 1;

	bool IExtraSkillData.CheckActiveCondition(CampaignDungeonData dungeonData)
	{
		return CheckActiveCondition(dungeonData);
	}
}
