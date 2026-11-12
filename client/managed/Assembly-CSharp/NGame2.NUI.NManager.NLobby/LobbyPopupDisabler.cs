using System;
using System.Collections.Generic;

namespace NGame2.NUI.NManager.NLobby;

public class LobbyPopupDisabler : IDisposable
{
	private static List<LobbyPopupDisabler> _disablerList = new List<LobbyPopupDisabler>();

	public LobbyPopupDisabler()
	{
		_disablerList.Add(this);
	}

	public void Dispose()
	{
		_disablerList.Remove(this);
	}

	public static bool IsDisabled()
	{
		return __Extension_Method_ICollection__.COUNT(_disablerList) > 0;
	}
}
