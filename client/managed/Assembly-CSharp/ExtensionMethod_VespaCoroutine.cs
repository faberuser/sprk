using System.Collections;
using UnityEngine;

public static class ExtensionMethod_VespaCoroutine
{
	public static Coroutine COROUTINE(this VespaCoroutine.RuntimeData This)
	{
		return This?.ExecuteCo;
	}

	public static Coroutine StartCoroutineThis(this MonoBehaviour This, IEnumerator routine)
	{
		return VespaCoroutine.StartThis(This, routine);
	}
}
