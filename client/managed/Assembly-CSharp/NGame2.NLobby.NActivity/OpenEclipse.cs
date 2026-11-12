using System.Collections;
using NGame2.NUI.NManager.NLobby;
using NVespa.NSingleton;

namespace NGame2.NLobby.NActivity;

public class OpenEclipse : IActivity
{
	public static IActivity Create(string[] values)
	{
		SceneAutoBehaviour<SoulWeaponManagement>.CreateInstance();
		return new OpenEclipse();
	}

	IEnumerator IActivity.coStart()
	{
		SceneAutoBehaviour<SoulWeaponManagement>.instance.OpenEclipseMainWithReqeust(openByEntryMenu: false);
		yield return null;
	}

	void IActivity.OnRequestStop()
	{
	}
}
