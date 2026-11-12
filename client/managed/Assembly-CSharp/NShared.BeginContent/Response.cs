namespace NShared.BeginContent;

[Response]
public class Response : GenericResponse<ResultType>
{
	public RewardItemInfo[] DropItems { get; set; }

	public int DropGold { get; set; }

	public StaminaResultInfo StaminaResult { get; set; }

	public string BattleServerAddress { get; set; }

	public int BattleServerPort { get; set; }

	public bool CheckIntegrity { get; set; }

	public ItemResultInfo[] ItemResults { get; set; }
}
