using System.Collections;
using NGame2.NUI.NManager.NPet;
using NVespa.NSingleton;

namespace NGame2.NLobby.NActivity;

public class OpenPetHouse : IActivity
{
	public static IActivity Create(string[] values)
	{
		return new OpenPetHouse();
	}

	IEnumerator IActivity.coStart()
	{
		SceneAutoBehaviour<PetManagement>.instance.OpenPetHouse();
		yield return null;
	}

	void IActivity.OnRequestStop()
	{
	}
}
