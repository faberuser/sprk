using System;

namespace NGame2.NNotify;

public class InviteNotyfiData
{
	public InviteNotifyType Type;

	public object Data;

	public Action<bool> OnAccept;

	public Action OnFailed;

	public float WindowOpenedTime;
}
