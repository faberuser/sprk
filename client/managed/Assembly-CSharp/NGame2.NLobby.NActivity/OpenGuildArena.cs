using System.Collections;
using NGame2.NUI.NManager;
using NVespa.NSingleton;

namespace NGame2.NLobby.NActivity;

public class OpenGuildArena : IActivity
{
	public static IActivity Create(string[] values)
	{
		return new OpenGuildArena();
	}

	public OpenGuildArena()
	{
		SceneAutoBehaviour<GuildArenaManagement>.instance.OpenGuildArena();
	}

	IEnumerator IActivity.coStart()
	{
		yield return null;
	}

	void IActivity.OnRequestStop()
	{
	}
}
