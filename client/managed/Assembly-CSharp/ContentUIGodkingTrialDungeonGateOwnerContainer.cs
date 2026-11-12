using UnityEngine;

[RequireComponent(typeof(Animator))]
public class ContentUIGodkingTrialDungeonGateOwnerContainer : MonoBehaviour
{
	public ContentUIGodkingTrialDungeonGate ownerGate;

	public ContentUIGodkingTrialDungeonGate GetOwner()
	{
		return ownerGate;
	}
}
