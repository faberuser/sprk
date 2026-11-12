using System.Collections;
using NGame2.NUI.NWindow.NEntryMenu;
using NGame2.NUI.NWindow.NEntryMenu.NAction;
using NVespa;

namespace NGame2.NLobby.NActivity;

public class OpenGuildSuppress : IActivity
{
	public static IActivity Create(string[] values)
	{
		return new OpenGuildSuppress();
	}

	public OpenGuildSuppress()
	{
		SingletonAuto<ActionContainer>.instance.GetAction(typeof(NGame2.NUI.NWindow.NEntryMenu.NAction.OpenGuildSuppress))?.Run(null);
	}

	IEnumerator IActivity.coStart()
	{
		yield return null;
	}

	void IActivity.OnRequestStop()
	{
	}
}
