namespace NShared;

public interface IExtraSkillData
{
	int SkillIndex { get; }

	int SkillLevel { get; }

	bool CheckActiveCondition(CampaignDungeonData dungeonData);
}
