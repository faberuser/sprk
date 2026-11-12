using UnityEngine;

namespace NGame2.NUI.NWindow.NBattle.NAnim;

public class KarmaCardSuffleWait : BaseKarmAnimState
{
	public KarmaCardSuffleWait(KarmaCardSelectWindow owner)
		: base(owner)
	{
	}

	public override void OnStart()
	{
		_owner.ShowCardRoot(show: true);
		_animator.Play("Base Layer.Anim_KarmaCard_Open", 0, 0f);
	}

	public override void OnUpdate()
	{
		if (SetClipLength())
		{
			_animPlayTime += Time.deltaTime;
			if (!(_clipLength >= _animPlayTime))
			{
				_isEnd = true;
			}
		}
	}

	public override State GetNextState()
	{
		return new KarmaCardSelectWait(_owner);
	}
}
