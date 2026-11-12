using NGame2.NUI.NWindow;
using NVespa;
using NVespa.NTask;
using NVespa.NTask.NRuntime;
using UnityEngine;

namespace NGame2.NUtil;

public class WindowOpenTask : DecoratorTask
{
	private readonly WeakRef<BaseWindow> _windowRef;

	private readonly float _expireAt;

	public WindowOpenTask(BaseWindow window, float timeout, ITask task)
		: base(task)
	{
		_windowRef = new WeakRef<BaseWindow>();
		_windowRef.Value = window;
		_expireAt = Time.time + timeout;
	}

	public BaseWindow GetWindow()
	{
		return _windowRef.Value;
	}

	public override bool IsValid()
	{
		if (GetWindow() == null)
		{
			return false;
		}
		if (Time.time > _expireAt)
		{
			return false;
		}
		return true;
	}
}
