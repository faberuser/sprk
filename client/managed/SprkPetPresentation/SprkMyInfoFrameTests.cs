using System.Collections;
using UnityEngine;

public sealed class SprkMyInfoFrameTests : MonoBehaviour
{
	private IEnumerator Start()
	{
		yield return new WaitForSecondsRealtime(3f);
		SprkPetPresentation.RunProbe();
		Application.Quit();
	}
}
