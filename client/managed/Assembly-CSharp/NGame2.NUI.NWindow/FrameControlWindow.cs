using System;
using NGame2.NUtil;
using NVespa.NSingleton;
using NVespa.NTask;

namespace NGame2.NUI.NWindow;

public class FrameControlWindow : BaseWindow
{
	public override void Open()
	{
		GlobalAutoBehaviour<FrameManager>.instance.PushContentFrameInfo(base.name);
		base.Open();
	}

	public override void Open(Action onComplete)
	{
		GlobalAutoBehaviour<FrameManager>.instance.PushContentFrameInfo(base.name);
		base.Open(onComplete);
	}

	public override ITask ShowAsync(bool immediately = false)
	{
		GlobalAutoBehaviour<FrameManager>.instance.PushContentFrameInfo(base.name);
		return base.ShowAsync(immediately);
	}

	public override void Close()
	{
		GlobalAutoBehaviour<FrameManager>.instance.PopContentFrameInfo(base.name);
		base.Close();
	}

	public override void OnClosing()
	{
		GlobalAutoBehaviour<FrameManager>.instance.PopContentFrameInfo(base.name);
		base.OnClosing();
	}
}
