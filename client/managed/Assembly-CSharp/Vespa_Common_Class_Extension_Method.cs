using System;
using System.Reflection;
using System.Text;
using UnityEngine;

public static class Vespa_Common_Class_Extension_Method
{
	public static bool IsUnityNull<T>(this T This) where T : class
	{
		if (This == null)
		{
			return true;
		}
		if (This is UnityEngine.Object)
		{
			return This as UnityEngine.Object == null;
		}
		return false;
	}

	public static TInterface Interface<TInterface>(this TInterface This)
	{
		return This;
	}

	public static string TOSTRING<TypeCls>(this TypeCls i_object) where TypeCls : class
	{
		if (i_object == null)
		{
			return "null";
		}
		return i_object.ToString();
	}

	public static void Clear(this StringBuilder stringBuilder)
	{
		stringBuilder.Length = 0;
		stringBuilder.Capacity = 0;
	}

	public static bool EQUALS<TypeCls>(this TypeCls i_this, TypeCls i_other) where TypeCls : class
	{
		if (i_this == null && i_other == null)
		{
			return true;
		}
		if (i_this == null && i_other != null)
		{
			return false;
		}
		if (i_this != null && i_other == null)
		{
			return false;
		}
		return i_this.Equals(i_other);
	}

	public static object RunMethod<T>(this T i_this, string funcName, params object[] objects) where T : class
	{
		if (i_this == null)
		{
			return null;
		}
		Type[] interfaces = i_this.GetType().GetInterfaces();
		if (interfaces == null || interfaces.Length == 0)
		{
			return null;
		}
		Type[] array = interfaces;
		for (int i = 0; i < array.Length; i++)
		{
			MethodInfo method = array[i].GetMethod(funcName);
			if (method != null)
			{
				return method.Invoke(i_this, objects);
			}
		}
		return null;
	}
}
