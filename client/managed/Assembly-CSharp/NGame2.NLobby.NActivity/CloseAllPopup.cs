using System.Collections;
using NGame2.NUI.NManager;
using NVespa.NSingleton;

namespace NGame2.NLobby.NActivity;

public class CloseAllPopup : IActivity
{
	public static IActivity Create(string[] values)
	{
		return new CloseAllPopup();
	}

	IEnumerator IActivity.coStart()
	{
		GlobalAutoBehaviour<PopupManegement>.instance.CloseAll();
		yield return null;
	}

	void IActivity.OnRequestStop()
	{
	}
}
