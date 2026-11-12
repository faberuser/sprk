namespace NShared;

[JsonSerializable(Javascript = true)]
public class GuildArenaBuffDataBase : IExtraSkillData
{
	int IExtraSkillData.SkillIndex => SkillIndex;

	int IExtraSkillData.SkillLevel => 1;

	public int SkillIndex { get; set; }

	bool IExtraSkillData.CheckActiveCondition(CampaignDungeonData dungeonData)
	{
		if (dungeonData == null || dungeonData.BattleType == BattleType.None)
		{
			return false;
		}
		return true;
	}
}
