using NVespa.NUtil;
using UnityEngine;

public class VespaObject
{
	public static void DestroyWithAssets(Object obj)
	{
		DestroyObject(obj);
	}

	public static void DestroyObject(Object obj)
	{
		if (obj != null)
		{
			Object.Destroy(obj);
		}
	}

	public static void DestroyObject(Object obj, float t)
	{
		if (obj != null)
		{
			Object.Destroy(obj, t);
		}
	}

	public static void DestroyGoAndDetachFromParent(GameObject go, Transform otherParent = null)
	{
		if (!(go == null))
		{
			if (otherParent == null)
			{
				go.transform.parent = go.transform.parent.parent;
			}
			else
			{
				go.transform.parent = otherParent;
			}
			DestroyGo(go);
		}
	}

	public static void PendingDestroyGo(GameObject go)
	{
		GameObjectDestroyer.PendingDestroy(go);
	}

	public static void PendingDestroyGo(Component component)
	{
		if (!(component == null))
		{
			PendingDestroyGo(component.gameObject);
		}
	}

	public static void DestroyGo(GameObject go)
	{
		DestroyObject(go);
	}

	public static void DestroyGo(GameObject go, float t)
	{
		DestroyObject(go, t);
	}

	public static void DestroyGo(Component component)
	{
		if (!(component == null))
		{
			DestroyGo(component.gameObject);
		}
	}

	public static void DestroyGo(Component component, float t)
	{
		if (!(component == null))
		{
			DestroyGo(component.gameObject, t);
		}
	}

	public static void DestroyComponent(Component component)
	{
		if (!(component == null))
		{
			Object.Destroy(component);
		}
	}
}
