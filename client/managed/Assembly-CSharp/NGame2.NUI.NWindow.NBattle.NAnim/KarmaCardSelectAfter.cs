using UnityEngine;

namespace NGame2.NUI.NWindow.NBattle.NAnim;

public class KarmaCardSelectAfter : BaseKarmAnimState
{
	public KarmaCardSelectAfter(KarmaCardSelectWindow owner)
		: base(owner)
	{
	}

	public override void OnStart()
	{
		int select = _owner.SelectCardIndex;
		_owner.ForCardPlate(delegate(KarmaCardSelectWindow.CardPlate cardPlate)
		{
			cardPlate.Select = cardPlate.CardPlateIndex == select;
		});
		string stateName = $"Base Layer.Anim_KarmaCard_CardPlate{1 + select}";
		_animator.Play(stateName, 0, 0f);
	}

	public override void OnUpdate()
	{
		if (SetClipLength())
		{
			_animPlayTime += Time.deltaTime;
			if (_animPlayTime >= _clipLength)
			{
				_isEnd = true;
			}
		}
	}

	public override State GetNextState()
	{
		return new KarmaCardMoveTarget(_owner);
	}
}
