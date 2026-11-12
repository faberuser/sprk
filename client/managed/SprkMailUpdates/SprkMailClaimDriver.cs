using UnityEngine;

public sealed class SprkMailClaimDriver : MonoBehaviour
{
	private void Update()
	{
		SprkMailClaimAll.Pump();
	}
}
