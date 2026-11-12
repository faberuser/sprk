using System;
using UnityEngine;

public static class Vespa_Unity_GameObject_Extension_Method
{
	public static void SENDMESSAGE(this GameObject This, string functionName, SendMessageOptions options)
	{
		if (!(This == null))
		{
			This.SendMessage(functionName, options);
		}
	}

	public static void SENDMESSAGE(this GameObject This, string functionName, object value, SendMessageOptions options)
	{
		if (!(This == null))
		{
			This.SendMessage(functionName, value, options);
		}
	}

	public static Component GetComponentAuto(this GameObject go, Type componentType)
	{
		Component component = go.GetComponent(componentType);
		if (component != null)
		{
			return component;
		}
		return go.AddComponent(componentType);
	}

	public static T GetComponentAuto<T>(this GameObject go) where T : Component
	{
		T component = go.GetComponent<T>();
		if (component != null)
		{
			return component;
		}
		return go.AddComponent(typeof(T)) as T;
	}

	public static T GetComponentRecursive<T>(this GameObject i_go) where T : Component
	{
		T component = i_go.GetComponent<T>();
		if (component != null)
		{
			return component;
		}
		return i_go.GetComponentInChildren<T>();
	}

	public static void ChangeLayerRecursive(this GameObject gameObject, int layer, bool forceRecursive = false)
	{
		if (!(gameObject == null) && layer >= 0 && (gameObject.layer != layer || forceRecursive))
		{
			ChangeLayerRecursiveInternal(gameObject, layer);
		}
	}

	private static void ChangeLayerRecursiveInternal(GameObject gameObject, int layer)
	{
		gameObject.layer = layer;
		for (int i = 0; i < gameObject.transform.childCount; i++)
		{
			ChangeLayerRecursiveInternal(gameObject.transform.GetChild(i).gameObject, layer);
		}
	}

	private static bool VerifyLayerRecursive(GameObject gameObject, int layer, out GameObject invalidGo)
	{
		if (gameObject.layer != layer)
		{
			invalidGo = gameObject;
			return false;
		}
		for (int i = 0; i < gameObject.transform.childCount; i++)
		{
			if (!VerifyLayerRecursive(gameObject.transform.GetChild(i).gameObject, layer, out var invalidGo2))
			{
				invalidGo = invalidGo2;
				return false;
			}
		}
		invalidGo = null;
		return true;
	}
}
