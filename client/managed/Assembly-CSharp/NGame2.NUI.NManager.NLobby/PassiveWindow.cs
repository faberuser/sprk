using System;
using NGame2.NUI.NWindow;
using NVespa.NSingleton;

namespace NGame2.NUI.NManager.NLobby;

internal class PassiveWindow<M, T> : NGame2.NUI.NManager.NLobby.IPassiveWindow where M : BaseManagement<M> where T : BaseWindow
{
	private BaseManagement<M> _management;

	private Action<T> _openAction;

	private Func<bool> _openCondition;

	private EPassiveWindowOpenFlag _openFlags = EPassiveWindowOpenFlag.All;

	private Func<EPassiveWindowOpenFlag, bool> _flagOpenCondition;

	EPassiveWindowOpenFlag NGame2.NUI.NManager.NLobby.IPassiveWindow.OpenFlag => _openFlags;

	public PassiveWindow(BaseManagement<M> management, Action<T> openAction, Func<EPassiveWindowOpenFlag, bool> openCondition, EPassiveWindowOpenFlag openFlags)
	{
		_management = management;
		_openAction = openAction;
		_flagOpenCondition = openCondition;
		_openFlags = openFlags;
	}

	public PassiveWindow(BaseManagement<M> management, Action<T> openAction, Func<bool> openCondition)
	{
		_management = management;
		_openAction = openAction;
		_openCondition = openCondition;
	}

	BaseWindow NGame2.NUI.NManager.NLobby.IPassiveWindow.GetWindow(bool openedOnly)
	{
		return _management.GetInstance<T>(openedOnly);
	}

	void NGame2.NUI.NManager.NLobby.IPassiveWindow.Open()
	{
		if (_flagOpenCondition != null && !Vespa_System_Action_Extension_Method.fire(_flagOpenCondition, _openFlags))
		{
			T val = Vespa_Common_Class_Extension_Method.Interface((NGame2.NUI.NManager.NLobby.IPassiveWindow)this).GetWindow(openedOnly: true) as T;
			if ((bool)val)
			{
				val.Close();
			}
		}
		else
		{
			T val2 = Vespa_Common_Class_Extension_Method.Interface((NGame2.NUI.NManager.NLobby.IPassiveWindow)this).GetWindow(openedOnly: false) as T;
			if (val2 != null && !val2.IsOpen())
			{
				Vespa_System_Action_Extension_Method.fire(_openAction, val2);
			}
		}
	}

	void NGame2.NUI.NManager.NLobby.IPassiveWindow.Close()
	{
		T val = Vespa_Common_Class_Extension_Method.Interface((NGame2.NUI.NManager.NLobby.IPassiveWindow)this).GetWindow(openedOnly: true) as T;
		if (val != null)
		{
			val.Close();
		}
	}
}
internal class PassiveWindow<T> : NGame2.NUI.NManager.NLobby.PassiveWindow<LobbyManagement, T> where T : BaseWindow
{
	private Action<T> _openAction;

	public PassiveWindow(Action<T> openAction, Func<EPassiveWindowOpenFlag, bool> openCondition = null, EPassiveWindowOpenFlag openFlags = EPassiveWindowOpenFlag.All)
		: base((BaseManagement<LobbyManagement>)SceneAutoBehaviour<LobbyManagement>.instance, openAction, openCondition, openFlags)
	{
		_openAction = openAction;
	}

	public PassiveWindow(Action<T> openAction, Func<bool> openCondition = null)
		: base((BaseManagement<LobbyManagement>)SceneAutoBehaviour<LobbyManagement>.instance, openAction, openCondition)
	{
		_openAction = openAction;
	}
}
