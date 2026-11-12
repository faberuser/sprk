using System;
using System.Collections.Generic;
using NGame2.NMessage;
using NGame2.NNodeTree;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NUI.NGame3DWidget;

public class ContentsNodeTree : IContents
{
	private class RuntimeSet
	{
		public List<IRuntime> RuntimeList;

		public Action OnComplete;
	}

	private UIGame3DWidget _root;

	private string _packPathName;

	private string _eventCode;

	private List<RuntimeSet> _runtimeSetList = new List<RuntimeSet>();

	private RenderQueueUpdater _rqUpdater = new RenderQueueUpdater();

	public ContentsNodeTree(UIGame3DWidget root, string packCode, string eventCode)
	{
		_root = root;
		_packPathName = packCode;
		_eventCode = eventCode;
		SceneAutoBehaviour<RuntimeGlobalManager>.instance.LoadNodeTree(_packPathName);
	}

	void IContents.Play(ITransformData extraTransform, ILocalTimer localTimer, Action onComplete, bool playSync)
	{
		WidgetMessage widgetMessage = SmallPooling<WidgetMessage>.CreateInstance();
		widgetMessage.Open(_packPathName, _eventCode, _root, _rqUpdater);
		SingletonAuto<GlobalManager>.instance.HandleDirect(widgetMessage);
		List<IRuntime> list = widgetMessage.PopRuntimeList();
		Vespa_Common_Class_Extension_Method.Interface((IMessage)widgetMessage).ReleaseSelf();
		if (__Extension_Method_ICollection__.IsNullOrEmpty(list))
		{
			Vespa_System_Action_Extension_Method.fire(onComplete);
			return;
		}
		_runtimeSetList.Add(new RuntimeSet
		{
			RuntimeList = list,
			OnComplete = onComplete
		});
		for (int i = 0; i < list.Count; i++)
		{
			SceneAutoBehaviour<RuntimeGlobalManager>.instance.AddRuntime(list[i].CaptureSelf(), immediatelyStart: true);
		}
	}

	void IContents.PlayPhase(string phaseName)
	{
	}

	void IContents.Stop(bool immediatelyClear)
	{
		for (int i = 0; i < _runtimeSetList.Count; i++)
		{
			RuntimeSet runtimeSet = _runtimeSetList[i];
			for (int j = 0; j < runtimeSet.RuntimeList.Count; j++)
			{
				IRuntime runtime = runtimeSet.RuntimeList[j];
				if (!runtime.IsEnd())
				{
					runtime.RequestStop(ERequestStop.Force);
				}
				runtime.ReleaseSelf();
			}
			runtimeSet.RuntimeList.Clear();
		}
		_runtimeSetList.Clear();
	}

	void IContents.ChainUpdate()
	{
		for (int i = 0; i < _runtimeSetList.Count; i++)
		{
			RuntimeSet runtimeSet = _runtimeSetList[i];
			for (int j = 0; j < runtimeSet.RuntimeList.Count; j++)
			{
				IRuntime runtime = runtimeSet.RuntimeList[j];
				if (runtime.IsEnd())
				{
					runtime.ReleaseSelf();
					runtimeSet.RuntimeList.RemoveAt(j);
					j--;
				}
			}
			if (runtimeSet.RuntimeList.Count == 0)
			{
				Vespa_System_Action_Extension_Method.fire(runtimeSet.OnComplete);
				_runtimeSetList.RemoveAt(i);
				i--;
			}
		}
	}

	void IContents.ReleaseAll()
	{
		Vespa_Common_Class_Extension_Method.Interface((IContents)this).Stop(immediatelyClear: true);
	}

	int IContents.GetInstanceCount()
	{
		return _runtimeSetList.Count;
	}

	void IContents.UpdateExtraRenderQueue(UIPanel panel, RenderQueueVisitor visitor)
	{
		_rqUpdater.UpdateExtraRenderQueue(panel, visitor);
	}

	void IContents.UpdateAlpha(UIWidget widget)
	{
	}

	bool IContents.ISDONE()
	{
		return false;
	}
}
