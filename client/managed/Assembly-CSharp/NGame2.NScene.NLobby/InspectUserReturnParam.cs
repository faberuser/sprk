using NShared;

namespace NGame2.NScene.NLobby;

public class InspectUserReturnParam
{
	public CachedAccountInfo AccountInfo { get; set; }

	public int HeroIndex { get; set; }

	public int[] DeckArray { get; set; }

	public InspectUserReturnParam(CachedAccountInfo accountInfo, int heroIndex, int[] deckArray)
	{
		AccountInfo = accountInfo;
		HeroIndex = heroIndex;
		DeckArray = deckArray;
	}
}
