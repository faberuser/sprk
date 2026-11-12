using NGame2.NUI.NWindow;
using NShared;

namespace NGame2.NScene.NLobby;

public class InspectUserReturnToFriendParam : InspectUserReturnParam
{
	public FriendMenu.EToggle _lastViewToggle { get; set; }

	public InspectUserReturnToFriendParam(FriendMenu.EToggle lastViewToggle, CachedAccountInfo accountInfo, int heroIndex, int[] deckArray)
		: base(accountInfo, heroIndex, deckArray)
	{
		_lastViewToggle = lastViewToggle;
	}
}
