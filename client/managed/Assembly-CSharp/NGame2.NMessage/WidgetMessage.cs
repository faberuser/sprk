using System.Collections.Generic;
using NGame2.NNodeTree;
using NGame2.NUI;
using NVespa;

namespace NGame2.NMessage;

public class WidgetMessage : SmallPooling<WidgetMessage>, ISmallPoolingItem, IMessage
{
	private List<IRuntime> _runtimeList;

	public bool IsDummy { get; set; }

	public string PackPathName { get; private set; }

	public string EventCode { get; private set; }

	public UIGame3DWidget Widget { get; private set; }

	public IObserver Observer { get; private set; }

	void ISmallPoolingItem.OnCreateNew()
	{
	}

	void ISmallPoolingItem.OnDestroy()
	{
	}

	void ISmallPoolingItem.OnCreate()
	{
	}

	void ISmallPoolingItem.OnRelease()
	{
		Widget = null;
		Observer = null;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_runtimeList); i++)
		{
			_runtimeList[i].ReleaseSelf();
		}
		_runtimeList = null;
	}

	void IMessage.ReleaseSelf()
	{
		SmallPooling<WidgetMessage>.ReleaseInstance(this);
	}

	public void Open(string packPathName, string eventCode, UIGame3DWidget widget, IObserver observer)
	{
		PackPathName = packPathName;
		EventCode = eventCode;
		Widget = widget;
		Observer = observer;
	}

	public void AddRuntimeInstance(IRuntime runtime)
	{
		if (_runtimeList == null)
		{
			_runtimeList = new List<IRuntime>();
		}
		_runtimeList.Add(runtime);
	}

	public List<IRuntime> PopRuntimeList()
	{
		List<IRuntime> runtimeList = _runtimeList;
		_runtimeList = null;
		return runtimeList;
	}
}
