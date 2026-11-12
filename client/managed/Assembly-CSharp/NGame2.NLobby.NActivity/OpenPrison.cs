using System.Collections;
using NGame2.NUI.NManager;
using NVespa.NSingleton;

namespace NGame2.NLobby.NActivity;

public class OpenPrison : IActivity
{
	public static IActivity Create(string[] values)
	{
		return new OpenPrison();
	}

	public OpenPrison()
	{
		SceneAutoBehaviour<MultipleDungeonManagement>.instance.OpenPrisonList();
	}

	IEnumerator IActivity.coStart()
	{
		yield return null;
	}

	void IActivity.OnRequestStop()
	{
	}
}
