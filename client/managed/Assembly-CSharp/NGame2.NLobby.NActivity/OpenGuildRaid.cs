using System.Collections;
using NGame2.NUI.NWindow.NEntryMenu;
using NGame2.NUI.NWindow.NEntryMenu.NAction;
using NVespa;

namespace NGame2.NLobby.NActivity;

public class OpenGuildRaid : IActivity
{
	public static IActivity Create(string[] values)
	{
		return new OpenGuildRaid();
	}

	public OpenGuildRaid()
	{
		SingletonAuto<ActionContainer>.instance.GetAction(typeof(GuildRaid))?.Run(null);
	}

	IEnumerator IActivity.coStart()
	{
		yield return null;
	}

	void IActivity.OnRequestStop()
	{
	}
}
