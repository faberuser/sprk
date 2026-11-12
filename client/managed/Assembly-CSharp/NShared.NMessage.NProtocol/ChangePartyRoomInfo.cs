namespace NShared.NMessage.NProtocol;

[JsonSerializable]
public class ChangePartyRoomInfo
{
	public int RaidLevel { get; set; }

	public byte GoalIndex { get; set; }

	public bool IsViolateBanRule { get; set; }

	public int[] AffixIndices { get; set; }
}
