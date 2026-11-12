using System.Collections;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow;
using NVespa.NSingleton;

namespace NGame2.NLobby.NActivity;

public class OpenFriend : IActivity
{
	public static IActivity Create(string[] values)
	{
		if (!SceneAutoBehaviour<LobbyManagement>.isValidInstance)
		{
			return null;
		}
		return new OpenFriend();
	}

	IEnumerator IActivity.coStart()
	{
		SceneAutoBehaviour<MyFriendManagement>.instance.OpenFriendMenu(FriendMenu.EToggle.List);
		yield return null;
	}

	void IActivity.OnRequestStop()
	{
	}
}
