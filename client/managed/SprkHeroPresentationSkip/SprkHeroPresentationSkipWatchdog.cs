using UnityEngine;

/// <summary>Ends a hero-presentation fast-forward that outlived its window.</summary>
public sealed class SprkHeroPresentationSkipWatchdog : MonoBehaviour
{
	private void Update()
	{
		SprkHeroPresentationSkip.CheckFastForward();
	}
}
