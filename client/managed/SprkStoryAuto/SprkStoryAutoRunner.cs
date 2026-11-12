using UnityEngine;

public sealed class SprkStoryAutoRunner : MonoBehaviour
{
	private void Update()
	{
		SprkStoryAuto.Tick();
	}
}
