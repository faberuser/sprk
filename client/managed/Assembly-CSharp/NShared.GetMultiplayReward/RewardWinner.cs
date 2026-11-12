namespace NShared.GetMultiplayReward;

[Response(Serialize = false, Deserialize = true, Javascript = true)]
public class RewardWinner
{
	public long[] ApplicantIds { get; set; }

	public int[] DiceNumbers { get; set; }

	public long WinnerId { get; set; }
}
