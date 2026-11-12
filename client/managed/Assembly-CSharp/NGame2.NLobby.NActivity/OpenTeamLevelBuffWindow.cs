using System.Collections;
using NGame2.NUI.NManager;
using NVespa.NSingleton;

namespace NGame2.NLobby.NActivity;

public class OpenTeamLevelBuffWindow : IActivity
{
	public static IActivity Create(string[] values)
	{
		return new OpenTeamLevelBuffWindow();
	}

	IEnumerator IActivity.coStart()
	{
		SceneAutoBehaviour<TeamLevelBuffManagement>.instance.OpenTeamLevelBuffWindow();
		yield return null;
	}

	void IActivity.OnRequestStop()
	{
	}
}
