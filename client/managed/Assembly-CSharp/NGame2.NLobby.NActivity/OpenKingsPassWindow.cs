using System.Collections;
using NGame2.NUI.NManager;
using NVespa.NSingleton;

namespace NGame2.NLobby.NActivity;

public class OpenKingsPassWindow : IActivity
{
	public static IActivity Create(string[] values)
	{
		SceneAutoBehaviour<KingsPassManagement>.CreateInstance();
		return new OpenKingsPassWindow();
	}

	IEnumerator IActivity.coStart()
	{
		SceneAutoBehaviour<KingsPassManagement>.instance.OpenKingsPassWindow();
		yield return null;
	}

	void IActivity.OnRequestStop()
	{
	}
}
