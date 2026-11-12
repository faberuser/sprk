using System.Collections.Generic;
using NGameAsset;
using NVespa.NPrefab2.NEvent;
using UnityEngine;

namespace NVespa.NPrefab2;

public class LinkFactoryComponent : MonoBehaviour, IReleaseEvent
{
	public interface IReleaseEvent
	{
		void OnRelease();
	}

	public FactoryBase Link;

	public LockCounter Lock = new LockCounter();

	internal ILinkData _linkData;

	internal List<IReleaseEvent> _releaseEventList;

	public void SetLinkData(ILinkData linkData)
	{
		if (_linkData != linkData)
		{
			if (_linkData != null)
			{
				_linkData.OnDestroy();
			}
			_linkData = linkData;
		}
	}

	public void AddReleaseEvent(IReleaseEvent releaseEvent)
	{
		if (_releaseEventList == null)
		{
			_releaseEventList = new List<IReleaseEvent>();
		}
		if (!_releaseEventList.Contains(releaseEvent))
		{
			_releaseEventList.Add(releaseEvent);
		}
	}

	void NVespa.NPrefab2.NEvent.IReleaseEvent.OnRelease()
	{
		if (_releaseEventList == null)
		{
			return;
		}
		for (int i = 0; i < _releaseEventList.Count; i++)
		{
			IReleaseEvent releaseEvent = _releaseEventList[i];
			if (!Vespa_Common_Class_Extension_Method.IsUnityNull(releaseEvent))
			{
				releaseEvent.OnRelease();
			}
		}
		_releaseEventList.Clear();
	}

	protected virtual void Awake()
	{
		SingletonAuto<LinkedTransform<LinkFactoryComponent>>.instance.OnAwake(this);
	}

	protected virtual void OnDestroy()
	{
		if (!SystemStatus.IsApplicationQuit)
		{
			Lock.OnDestroy();
			if (_linkData != null)
			{
				_linkData.OnDestroy();
			}
			Vespa_Common_Class_Extension_Method.Interface((NVespa.NPrefab2.NEvent.IReleaseEvent)this).OnRelease();
			SingletonAuto<LinkedTransform<LinkFactoryComponent>>.instance.OnDestroy(this);
		}
	}

	public static void IncreaseLock(Component component)
	{
		if (component != null)
		{
			IncreaseLock(component.gameObject);
		}
	}

	public static void IncreaseLock(GameObject go)
	{
		if (!(go == null))
		{
			LinkFactoryComponent linkFactoryComponent = SingletonAuto<LinkedTransform<LinkFactoryComponent>>.instance.Get(go.transform);
			if (!(linkFactoryComponent == null))
			{
				linkFactoryComponent.Lock.Increase();
			}
		}
	}
}
