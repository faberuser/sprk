using System.Collections;
using NGame2.NUI.NManager.NLobby;
using NVespa.NSingleton;

namespace NGame2.NLobby.NActivity;

public class OpenWorldBoss : IActivity
{
	public static IActivity Create(string[] values)
	{
		if (!SceneAutoBehaviour<WorldBossManagement>.isValidInstance)
		{
			return null;
		}
		return new OpenWorldBoss();
	}

	IEnumerator IActivity.coStart()
	{
		SceneAutoBehaviour<WorldBossManagement>.instance.OpenWorldBossRanking(null);
		yield return null;
	}

	void IActivity.OnRequestStop()
	{
	}
}
