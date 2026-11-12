namespace NShared.NMessage.NProtocol;

[JsonSerializable]
public class BeginCampaign
{
	public int ChapterIndex { get; set; }

	public int DungeonIndex { get; set; }
}
