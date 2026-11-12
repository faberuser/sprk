using NShared;

namespace NGame2.NScene.NLobby;

public class InspectUserReturnToArenaParam : InspectUserReturnParam
{
	public MatchPvpType MatchType { get; set; }

	public MatchTierType MatchTierType { get; set; }

	public int MatchPage { get; set; }

	public InspectUserReturnToArenaParam(MatchPvpType matchType, MatchTierType tierType, int page, CachedAccountInfo accountInfo, int heroIndex, int[] deckArray)
		: base(accountInfo, heroIndex, deckArray)
	{
		MatchType = matchType;
		MatchTierType = tierType;
		MatchPage = page;
	}
}
