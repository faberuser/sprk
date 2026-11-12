using System;
using NGame2.NUI.NManager.NLobby;
using NVespa.NSingleton;
using NVespa.NTask;
using NVespa.NTask.NRuntime;

namespace NGame2.NUI.NWindow;

public class LobbyFullWindow : FrameControlWindow
{
	public bool AutoHideWorldMap = true;

	public void NotifyOpen()
	{
		if (AutoHideWorldMap && SceneAutoBehaviour<LobbyManagement>.isValidInstance)
		{
			SceneAutoBehaviour<LobbyManagement>.instance.OnEnterFullUi(this);
		}
	}

	public void NotifyClose()
	{
		if (SceneAutoBehaviour<LobbyManagement>.isValidInstance)
		{
			SceneAutoBehaviour<LobbyManagement>.instance.OnExitFullUi(this);
		}
	}

	public override void Open()
	{
		Open(null, NotifyOpen);
	}

	public override void Open(Action onComplete)
	{
		onComplete = (Action)Delegate.Combine(onComplete, new Action(NotifyOpen));
		Open(null, onComplete);
	}

	public override ITask ShowAsync(bool immediately = false)
	{
		return Vespa_Task_ExtensionMethod.ADDTASK(new SequenceTask().ADDTASK(base.ShowAsync(immediately)), delegate
		{
			if (SceneAutoBehaviour<LobbyManagement>.isValidInstance)
			{
				SceneAutoBehaviour<LobbyManagement>.instance.OnEnterFullUi(this);
			}
		});
	}

	public override void Close()
	{
		NotifyClose();
		base.Close();
	}

	public override ITask CloseAsync()
	{
		NotifyClose();
		return base.CloseAsync();
	}

	public override void OnClosing()
	{
		NotifyClose();
		base.Close();
	}
}
