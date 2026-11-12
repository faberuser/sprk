using System.Collections;
using NGame2.NUI.NManager;
using NVespa.NSingleton;

namespace NGame2.NLobby.NActivity;

public class OpenWorldmap : IActivity
{
	public static IActivity Create(string[] values)
	{
		return new OpenWorldmap();
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
