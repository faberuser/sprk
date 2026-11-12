using System.Collections;
using NGame2.NUI.NManager;
using NVespa.NSingleton;

namespace NGame2.NLobby.NActivity;

public class OpenShakmehDungeon : IActivity
{
	public static IActivity Create(string[] values)
	{
		if (RestoredPortal.OpenShakmehBranch(values))
		{
			return null;
		}
		return new OpenShakmehDungeon();
	}

	public OpenShakmehDungeon()
	{
		SceneAutoBehaviour<NGame2.NUI.NManager.ShakmehDungeonManagement>.instance.OpenShakmehDungeonWindow(needMove: false);
	}

	IEnumerator IActivity.coStart()
	{
		yield return null;
	}

	void IActivity.OnRequestStop()
	{
	}
}
