using System.Collections;
using UnityEngine;

public sealed class SprkFreeCameraTests : MonoBehaviour
{
	private IEnumerator Start()
	{
		yield return new WaitForSecondsRealtime(3f);
		SprkFreeCamera.RunProbe();
		Application.Quit();
	}
}
