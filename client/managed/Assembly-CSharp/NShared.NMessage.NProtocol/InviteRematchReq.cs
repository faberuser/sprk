namespace NShared.NMessage.NProtocol;

[JsonSerializable]
public class InviteRematchReq
{
	public BattleType BattleType { get; set; }

	public string Nick { get; set; }

	public int TeamLevel { get; set; }

	public int AvatarIndex { get; set; }

	public int ChatperIndex { get; set; }

	public int DungeonIndex { get; set; }

	public MatchInfo LastMatchInfo { get; set; }
}
