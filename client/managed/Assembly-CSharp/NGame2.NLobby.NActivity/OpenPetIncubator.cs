using System.Collections;
using NGame2.NAccount;
using NGame2.NUI.NManager.NPet;
using NGame2.NUI.NPet;
using NVespa.NSingleton;

namespace NGame2.NLobby.NActivity;

public class OpenPetIncubator : IActivity
{
	public static IActivity Create(string[] values)
	{
		return new OpenPetIncubator();
	}

	IEnumerator IActivity.coStart()
	{
		if (GlobalAutoBehaviour<AccountManager>.instance.TutorialManager != null && GlobalAutoBehaviour<AccountManager>.instance.TutorialManager.IsComplete(11400))
		{
			SceneAutoBehaviour<PetManagement>.instance.OpenPetHouse(EPetHouseObjectType.IncubatorHouse);
			yield break;
		}
		SceneAutoBehaviour<PetManagement>.instance.OpenPetHouse();
		yield return null;
	}

	void IActivity.OnRequestStop()
	{
	}
}
