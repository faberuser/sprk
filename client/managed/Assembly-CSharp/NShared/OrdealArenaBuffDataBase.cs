namespace NShared;

[JsonSerializable(Javascript = true)]
public class OrdealArenaBuffDataBase
{
	public int Index { get; set; }

	public bool CheckActiveCondition(CampaignDungeonData dungeonData)
	{
		if (dungeonData == null || dungeonData.BattleDefineCode != (HashString)"OrdealArena")
		{
			return false;
		}
		return true;
	}
}
