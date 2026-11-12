using NGame2.NDispatchBattle;
using UnityEngine;

public class BehaviorOnEndDispatchAnimation : StateMachineBehaviour
{
	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		PopupUIDispatchBattleFx.CloseGlobal();
	}
}
