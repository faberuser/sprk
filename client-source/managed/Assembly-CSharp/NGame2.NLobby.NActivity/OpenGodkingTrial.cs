using System.Collections;
using NGame2.NUI.NManager.NLobby;
using NVespa.NSingleton;

namespace NGame2.NLobby.NActivity;

public class OpenGodkingTrial : IActivity
{
	public static IActivity Create(string[] values)
	{
		SceneAutoBehaviour<SoulWeaponManagement>.CreateInstance();
		return new OpenGodkingTrial();
	}

	IEnumerator IActivity.coStart()
	{
		SceneAutoBehaviour<SoulWeaponManagement>.instance.OpenGodkingTrialDungeonList();
		yield return null;
	}

	void IActivity.OnRequestStop()
	{
	}
}
