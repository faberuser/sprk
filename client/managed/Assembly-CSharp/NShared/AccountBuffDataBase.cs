namespace NShared;

[JsonSerializable(Javascript = true)]
public class AccountBuffDataBase : IExtraSkillData
{
	int IExtraSkillData.SkillIndex => SkillIndex;

	int IExtraSkillData.SkillLevel => SkillLevel;

	public int AccountBuffIndex { get; set; }

	public int SkillIndex { get; set; }

	public int SkillLevel { get; set; }

	bool IExtraSkillData.CheckActiveCondition(CampaignDungeonData dungeonData)
	{
		if (dungeonData == null)
		{
			return false;
		}
		AccountBuffData data = GameRoot.Instance.AccountBuffDataContainer.GetData(AccountBuffIndex);
		if (data == null)
		{
			return false;
		}
		if (data.BattleType == null || data.BattleType.Length == 0)
		{
			return false;
		}
		for (int i = 0; i < data.BattleType.Length; i++)
		{
			if (data.BattleType[i] == dungeonData.BattleType)
			{
				return true;
			}
		}
		return false;
	}
}
