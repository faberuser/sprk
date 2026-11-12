using System.Collections.Generic;
using UnityEngine;

namespace NVespa;

public class LinkedTransform<T> : SingletonAuto<LinkedTransform<T>>, ILinkedTransform where T : Component
{
	private class LinkPairData
	{
		public int TransformId;

		public int ComponentId;

		public Transform Tm;

		public T Comp;
	}

	private Dictionary<int, LinkPairData> _tmDic = new Dictionary<int, LinkPairData>();

	private Dictionary<int, LinkPairData> _componentDic = new Dictionary<int, LinkPairData>();

	void ILinkedTransform.OnSceneChanged()
	{
		_tmDic.Clear();
		_componentDic.Clear();
	}

	public LinkedTransform()
	{
		SingletonAuto<LinkedTransformContainer>.instance.Add(this);
	}

	public T Get(Transform tm, bool isFast = false)
	{
		if (_tmDic.TryGetValue(tm.GetInstanceID(), out var value))
		{
			return value.Comp;
		}
		if (isFast)
		{
			return null;
		}
		T component = tm.GetComponent<T>();
		Register(tm, component);
		return component;
	}

	public T GetAuto(Transform tm)
	{
		int instanceID = tm.GetInstanceID();
		if (!_tmDic.TryGetValue(instanceID, out var value))
		{
			value = new LinkPairData();
			value.Tm = tm;
			value.TransformId = instanceID;
			_tmDic.Add(instanceID, value);
		}
		if (value.Comp != null)
		{
			return value.Comp;
		}
		value.Comp = Vespa_Unity_Component_Extension_Method.GetComponentAuto<T>(tm);
		if (value.Comp != null)
		{
			value.ComponentId = value.Comp.GetInstanceID();
			_componentDic.Add(value.ComponentId, value);
		}
		return value.Comp;
	}

	public void OnAwake(T component)
	{
		if (!(component == null))
		{
			Transform transform = component.transform;
			Register(transform, component);
		}
	}

	public void OnDestroy(T component)
	{
		if (!(component == null))
		{
			_componentDic.Remove(component.GetInstanceID());
			Transform transform = component.transform;
			if (transform != null)
			{
				_tmDic.Remove(transform.GetInstanceID());
			}
		}
	}

	private void Register(Transform tm, T comp)
	{
		if (tm == null)
		{
			return;
		}
		int instanceID = tm.GetInstanceID();
		int componentId = ((!(comp == null)) ? comp.GetInstanceID() : 0);
		if (!_tmDic.ContainsKey(instanceID))
		{
			LinkPairData linkPairData = new LinkPairData();
			linkPairData.Tm = tm;
			linkPairData.Comp = comp;
			linkPairData.TransformId = instanceID;
			linkPairData.ComponentId = componentId;
			_tmDic.Add(linkPairData.TransformId, linkPairData);
			if (linkPairData.ComponentId != 0)
			{
				_componentDic.Add(linkPairData.ComponentId, linkPairData);
			}
		}
	}
}
