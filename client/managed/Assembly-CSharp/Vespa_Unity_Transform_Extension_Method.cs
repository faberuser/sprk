using System.Collections.Generic;
using System.Text;
using NVespa;
using UnityEngine;

public static class Vespa_Unity_Transform_Extension_Method
{
	public static string GetTransformPath(this Transform This, Transform rootTm)
	{
		if (This == null || Vespa_Common_Class_Extension_Method.EQUALS(This, rootTm))
		{
			return string.Empty;
		}
		using PoolingList<Transform> poolingList = default(PoolingList<Transform>);
		poolingList.Add(This);
		Transform parent = This.parent;
		for (int i = 0; i < 100; i++)
		{
			if (parent == null)
			{
				break;
			}
			if (Vespa_Common_Class_Extension_Method.EQUALS(rootTm, parent))
			{
				break;
			}
			poolingList.Add(parent);
			parent = parent.parent;
		}
		StringBuilder stringBuilder = new StringBuilder();
		for (int num = poolingList.Count - 1; num >= 0; num--)
		{
			stringBuilder.Append(poolingList[num].name);
			if (num > 0)
			{
				stringBuilder.Append('/');
			}
		}
		return stringBuilder.ToString();
	}

	public static string DumpTransformPath(this Transform This, Transform rootTm, int maxDump = 100)
	{
		if (This == null || Vespa_Common_Class_Extension_Method.EQUALS(This, rootTm))
		{
			return string.Empty;
		}
		string text = $"{This.name} ({This.GetSiblingIndex()})";
		Transform parent = This.parent;
		for (int i = 0; i < maxDump; i++)
		{
			if (parent == null)
			{
				break;
			}
			if (Vespa_Common_Class_Extension_Method.EQUALS(rootTm, parent))
			{
				break;
			}
			text = $"{parent.name} ({parent.GetSiblingIndex()})" + "/" + text;
			parent = parent.parent;
		}
		return text;
	}

	public static void ParentRelative(this Transform childTm, Transform parentTm)
	{
		childTm.SetParent(parentTm, worldPositionStays: false);
	}

	public static void SetLayerRecursively(this Transform childTm, Transform parentTm)
	{
		SetLayerRecursively(childTm.gameObject, parentTm.gameObject.layer);
	}

	public static void SetLayerRecursively(this GameObject go, int layerNumber)
	{
		Transform[] componentsInChildren = go.GetComponentsInChildren<Transform>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].gameObject.layer = layerNumber;
		}
	}

	public static void ParentResetTM(this Transform i_child, Transform i_parent)
	{
		i_child.parent = i_parent;
		i_child.localPosition = Vector3.zero;
		i_child.localRotation = Quaternion.identity;
		i_child.localScale = Vector3.one;
	}

	public static void ResetLocalTM(this Transform i_this)
	{
		i_this.localPosition = Vector3.zero;
		i_this.localRotation = Quaternion.identity;
		i_this.localScale = Vector3.one;
	}

	public static Transform FindFlexible(this Transform tm, string path, int findIndex = 0, bool includeInactive = false)
	{
		if (tm == null)
		{
			return null;
		}
		if (!includeInactive && !tm.gameObject.activeInHierarchy)
		{
			return null;
		}
		string[] array = path.Split('/');
		if (__Extension_Method_ICollection__.IsNullOrEmpty(array))
		{
			return null;
		}
		Transform transform = tm;
		for (int i = 0; i < array.Length - 1; i++)
		{
			transform = FindChildRecursively(transform, array[i], includeInactive);
			if (transform == null)
			{
				return null;
			}
		}
		List<Transform> list = new List<Transform>();
		CollectChildRecursively(transform, array[^1], includeInactive, list);
		if (__Extension_Method_ICollection__.IsNullOrEmpty(list))
		{
			return null;
		}
		list.Sort(delegate(Transform lhs, Transform rhs)
		{
			Vector3 position = lhs.transform.position;
			Vector3 position2 = rhs.transform.position;
			if (!Mathf.Approximately(position.y, position2.y))
			{
				if (!(position.y > position2.y))
				{
					return 1;
				}
				return -1;
			}
			return (!Mathf.Approximately(position.x, position2.x)) ? ((!(position.x < position2.x)) ? 1 : (-1)) : 0;
		});
		int index = Math.ClampOver(findIndex, 0, list.Count);
		return list[index];
	}

	public static void CollectChildRecursively(this Transform tm, string name, bool includeInactive, List<Transform> resultList)
	{
		if (!(tm == null) && (includeInactive || tm.gameObject.activeInHierarchy))
		{
			if (string.Equals(Vespa_Unity_Object_Extension_Method.NAME(tm), name))
			{
				resultList.Add(tm);
			}
			for (int i = 0; i < tm.childCount; i++)
			{
				CollectChildRecursively(tm.GetChild(i), name, includeInactive, resultList);
			}
		}
	}

	public static Transform FindChildRecursively(this Transform tm, string name, bool includeInactive = true)
	{
		if (tm == null)
		{
			return null;
		}
		if (!includeInactive && !tm.gameObject.activeInHierarchy)
		{
			return null;
		}
		if (string.Equals(Vespa_Unity_Object_Extension_Method.NAME(tm), name))
		{
			return tm;
		}
		for (int i = 0; i < tm.childCount; i++)
		{
			Transform transform = FindChildRecursively(tm.GetChild(i), name, includeInactive);
			if (transform != null)
			{
				return transform;
			}
		}
		return null;
	}
}
