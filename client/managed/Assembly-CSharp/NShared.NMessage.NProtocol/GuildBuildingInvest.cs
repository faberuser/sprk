namespace NShared.NMessage.NProtocol;

[JsonSerializable]
public class GuildBuildingInvest
{
	public int GuildBuildingIndex { get; set; }

	public int InvestWood { get; set; }

	public int InvestStone { get; set; }

	public int InvestMetal { get; set; }

	public int Wood { get; set; }

	public int Stone { get; set; }

	public int Metal { get; set; }
}
