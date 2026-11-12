using UnityEngine;

namespace NGame2.NUI.NWindow.NBattle.NAnim;

public class KarmaCardMoveTarget : BaseKarmAnimState
{
	private Transform _moveTransform;

	private Transform _targetTransform;

	private Vector3 _targetPos;

	private Vector3 _startPos;

	public KarmaCardMoveTarget(KarmaCardSelectWindow owner)
		: base(owner)
	{
	}

	public override void OnStart()
	{
		_targetTransform = _owner.GetTargetTransform();
		int select = 0;
		_owner.ForCardPlate(delegate(KarmaCardSelectWindow.CardPlate cardPlate)
		{
			if (cardPlate.Select)
			{
				_moveTransform = cardPlate.Transform;
				select = cardPlate.CardPlateIndex;
			}
		});
		string stateName = $"Base Layer.Anim_KarmaCard_CardPlate{1 + select}Holer";
		_animator.Play(stateName, 0, 0f);
		_startPos = ((_moveTransform != null) ? _moveTransform.position : default(Vector3));
		_targetPos = ((_targetTransform != null) ? _targetTransform.position : default(Vector3));
	}

	public override void OnUpdate()
	{
		if (SetClipLength())
		{
			_animPlayTime += Time.deltaTime;
			float num = _animPlayTime / _clipLength;
			float num2 = Mathf.Clamp(num, 0f, 1f);
			float curve = _owner.GetCurve(num2);
			float x = _startPos.x + (_targetPos.x - _startPos.x) * num2;
			float y = _startPos.y + (_targetPos.y - _startPos.y) * curve;
			_moveTransform.position = new Vector3(x, y, 0f);
			if (num >= 1f)
			{
				_owner.PlayCardHitEffect();
				SetEnd(isEnd: true);
			}
		}
	}

	public override State GetNextState()
	{
		return new KarmaCardEnd(_owner);
	}
}
