using System.Collections;
using NGame2.NUI.NManager;
using NVespa.NSingleton;

namespace NGame2.NLobby.NActivity;

public class OpenClearMissionPopup : IActivity
{
	public static IActivity Create(string[] values)
	{
		SceneAutoBehaviour<ClearMissionPopupManagement>.CreateInstance();
		return new OpenClearMissionPopup();
	}

	IEnumerator IActivity.coStart()
	{
		SceneAutoBehaviour<ClearMissionPopupManagement>.instance.OpenClearMissionPopup();
		yield return null;
	}

	void IActivity.OnRequestStop()
	{
	}
}
