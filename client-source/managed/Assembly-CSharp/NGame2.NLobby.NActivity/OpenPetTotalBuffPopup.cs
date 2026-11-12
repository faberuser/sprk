using System.Collections;
using NGame2.NUI.NManager.NPet;
using NVespa.NSingleton;

namespace NGame2.NLobby.NActivity;

public class OpenPetTotalBuffPopup : IActivity
{
	public static IActivity Create(string[] values)
	{
		return new OpenPetTotalBuffPopup();
	}

	IEnumerator IActivity.coStart()
	{
		SceneAutoBehaviour<PetManagement>.instance.OpenPetTotalBuffPopup();
		yield return null;
	}

	void IActivity.OnRequestStop()
	{
	}
}
