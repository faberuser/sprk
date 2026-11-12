using System.Collections;
using NGame2.NUI.NWindow.NEntryMenu;
using NGame2.NUI.NWindow.NEntryMenu.NAction;
using NVespa;

namespace NGame2.NLobby.NActivity;

public class OpenMazeTowerList : IActivity
{
	public static IActivity Create(string[] values)
	{
		return new OpenMazeTowerList();
	}

	public OpenMazeTowerList()
	{
		SingletonAuto<ActionContainer>.instance.GetAction(typeof(NGame2.NUI.NWindow.NEntryMenu.NAction.OpenMazeTowerList))?.Run(null);
	}

	IEnumerator IActivity.coStart()
	{
		yield return null;
	}

	void IActivity.OnRequestStop()
	{
	}
}
