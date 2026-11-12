using UnityEngine;

public class BehviorGodkingTrialGateAnim_Closed : StateMachineBehaviour
{
	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (animator == null)
		{
			return;
		}
		ContentUIGodkingTrialDungeonGateOwnerContainer component = animator.GetComponent<ContentUIGodkingTrialDungeonGateOwnerContainer>();
		if (!(component == null))
		{
			ContentUIGodkingTrialDungeonGate owner = component.GetOwner();
			if (!(owner == null))
			{
				owner.OnChangeAniState(ContentUIGodkingTrialDungeonGate.State.Closed);
			}
		}
	}
}
