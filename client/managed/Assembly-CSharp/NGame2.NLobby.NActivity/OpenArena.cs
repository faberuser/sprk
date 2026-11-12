using System.Collections;
using NGame2.NUI.NWindow.NEntryMenu;
using NGame2.NUI.NWindow.NEntryMenu.NAction;
using NShared;
using NVespa;

namespace NGame2.NLobby.NActivity;

public class OpenArena : IActivity
{
	public static IActivity Create(string[] values)
	{
		return new OpenArena(ArgParser.GetValueEnum(values, 0, MatchArenaType.Normal));
	}

	public OpenArena(MatchArenaType matchArenaType)
	{
		NGame2.NUI.NWindow.NEntryMenu.IAction action = null;
		switch (matchArenaType)
		{
		case MatchArenaType.Normal:
			action = SingletonAuto<ActionContainer>.instance.GetAction(typeof(NGame2.NUI.NWindow.NEntryMenu.NAction.OpenArena));
			break;
		case MatchArenaType.BanPick:
			action = SingletonAuto<ActionContainer>.instance.GetAction(typeof(OpenArenaBanPick));
			break;
		case MatchArenaType.Ordeal:
			action = SingletonAuto<ActionContainer>.instance.GetAction(typeof(OpenOrdealArena));
			break;
		}
		action?.Run(null);
	}

	IEnumerator IActivity.coStart()
	{
		yield return null;
	}

	void IActivity.OnRequestStop()
	{
	}
}
