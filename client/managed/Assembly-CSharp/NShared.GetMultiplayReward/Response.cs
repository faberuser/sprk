namespace NShared.GetMultiplayReward;

[Response]
public class Response : GenericResponse<ResultType>
{
	public RewardItemInfo[] ReceivedRewardItems { get; set; }

	public RewardWinner[] RewardWinners { get; set; }

	public ItemResultInfo[] ItemResults { get; set; }

	public EquipItemInfo[] EquipItemInfos { get; set; }
}
