namespace NShared.NMessage.NProtocol;

[JsonSerializable]
public class SeasonChanged
{
	public MatchSeasonData SeasonData { get; set; }
}
