using System;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class StartEffectWindow : BaseWindow
{
	public const string EclipseStartPrefabPath = "Resources/Prefab2/Window/SoulWeapon/EclipseBattleStart";

	public Animator Animator;

	private Action _onClose;

	public void OpenEffect(Action OnClose)
	{
		Open(null, null);
		_onClose = OnClose;
	}

	public override void UpdateDerived()
	{
		if (IsEnd())
		{
			Vespa_System_Action_Extension_Method.fire(_onClose);
			Close();
		}
	}

	public bool IsEnd()
	{
		if (Animator == null)
		{
			return true;
		}
		return Animator.GetCurrentAnimatorStateInfo(0).IsName("End");
	}
}
