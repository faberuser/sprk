using UnityEngine;

namespace NGame2.NUI.NWindow.NBattle.NAnim;

public abstract class BaseKarmAnimState : State
{
	protected KarmaCardSelectWindow _owner;

	protected Animator _animator;

	protected float _clipLength;

	protected float _animPlayTime;

	public BaseKarmAnimState(KarmaCardSelectWindow owner)
	{
		_owner = owner;
		if (IsValid())
		{
			_animator = _owner.Animator;
		}
	}

	protected bool IsValid()
	{
		return _owner != null;
	}

	protected bool SetClipLength()
	{
		if (_clipLength == 0f)
		{
			AnimatorClipInfo[] currentAnimatorClipInfo = _animator.GetCurrentAnimatorClipInfo(0);
			if (__Extension_Method_ICollection__.IsNullOrEmpty(currentAnimatorClipInfo))
			{
				return false;
			}
			for (int i = 0; i < currentAnimatorClipInfo.Length; i++)
			{
				AnimatorClipInfo animatorClipInfo = currentAnimatorClipInfo[i];
				if (animatorClipInfo.clip != null)
				{
					_clipLength = animatorClipInfo.clip.length;
				}
			}
			if (_clipLength == 0f)
			{
				_clipLength = 1f;
			}
		}
		return true;
	}
}
