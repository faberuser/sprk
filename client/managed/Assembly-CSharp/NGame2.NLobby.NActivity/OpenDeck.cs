using System.Collections;
using NGame2.NUI.NManager;
using NVespa.NSingleton;

namespace NGame2.NLobby.NActivity;

public class OpenDeck : IActivity
{
	public static IActivity Create(string[] values)
	{
		return new OpenDeck();
	}

	IEnumerator IActivity.coStart()
	{
		SceneAutoBehaviour<MyHeroManagement>.instance.OpenMyDeck();
		yield return null;
	}

	void IActivity.OnRequestStop()
	{
	}
}
