using System.Collections;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.NLobby;
using NVespa.NSingleton;

namespace NGame2.NLobby.NActivity;

public class OpenPortalSelect : IActivity
{
	public static IActivity Create(string[] values)
	{
		if (!SceneAutoBehaviour<LobbyManagement>.isValidInstance)
		{
			return null;
		}
		return new OpenPortalSelect();
	}

	IEnumerator IActivity.coStart()
	{
		SceneAutoBehaviour<PortalManagement>.instance.OpenPortalSelectWindow();
		yield return null;
	}

	void IActivity.OnRequestStop()
	{
	}
}
