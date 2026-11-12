using NShared.NMessage.NProtocol;

namespace NGame2.NUI.NWindow.NBattle;

public class AutoBattle : BaseWindow, IGlobalWindow
{
	public UIButton StopButton;

	protected bool _repeatEnabled;

	protected void OpenInternal()
	{
		base.Open();
		EnableRepeat(isEnabled: true);
	}

	public void OnBattleWin()
	{
	}

	public virtual void OnBattleLose()
	{
		EnableRepeat(isEnabled: false);
	}

	public virtual void OnClickStop(bool sendRoomRepeat = false, PartyRoomRepeat.ResultType resultType = PartyRoomRepeat.ResultType.NONE)
	{
		ShowRepeateCancelMessage();
		EnableRepeat(isEnabled: false);
	}

	protected virtual void ShowRepeateCancelMessage()
	{
	}

	public void StopRepeat()
	{
		EnableRepeat(isEnabled: false);
	}

	protected virtual void EnableRepeat(bool isEnabled)
	{
		_repeatEnabled = isEnabled;
		UIHelper.EnableButton(StopButton, _repeatEnabled);
	}

	public static void Stop()
	{
		if (AutoRepeatBattle.IsEnabled())
		{
			AutoRepeatBattle.instance.OnClickStop(sendRoomRepeat: true);
		}
		if (AutoNextBattle.IsEnabled())
		{
			AutoNextBattle.instance.OnClickStop();
		}
	}
}
