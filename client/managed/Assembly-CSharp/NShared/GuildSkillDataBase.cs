using MessagePack;

namespace NShared;

[MessagePackObject(false)]
[JsonSerializable(Javascript = true)]
public class GuildSkillDataBase : IExtraSkillData
{
	int IExtraSkillData.SkillIndex => SkillIndex;

	int IExtraSkillData.SkillLevel => GuildSkillLevel;

	[Key(0)]
	public int GuildSkillIndex { get; set; }

	[Key(1)]
	public int SkillIndex { get; set; }

	[Key(2)]
	public byte GuildSkillLevel { get; set; }

	bool IExtraSkillData.CheckActiveCondition(CampaignDungeonData dungeonData)
	{
		if (dungeonData == null)
		{
			return false;
		}
		GuildSkillData data = GameRoot.Instance.GuildSkillDataContainer.GetData(GuildSkillIndex);
		if (data == null)
		{
			return false;
		}
		if (data.BattleTypes == null || data.BattleTypes.Length == 0)
		{
			return false;
		}
		for (int i = 0; i < data.BattleTypes.Length; i++)
		{
			if (data.BattleTypes[i] == dungeonData.BattleType)
			{
				return true;
			}
		}
		return false;
	}
}
