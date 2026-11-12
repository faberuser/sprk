using UnityEngine;

namespace NGame2.NUI.NWindow.NBattle.NAnim;

public class KarmaCardSelectWait : BaseKarmAnimState
{
	private float _waitTime;

	private float _cardSelectLeftTime;

	public KarmaCardSelectWait(KarmaCardSelectWindow owner)
		: base(owner)
	{
	}

	public override void OnStart()
	{
		_cardSelectLeftTime = _owner.GetCardSelectLeftTime();
	}

	public override void OnUpdate()
	{
		if (_owner.GetSelectCard())
		{
			SetEnd(isEnd: true);
			return;
		}
		_waitTime += Time.deltaTime;
		float num = _cardSelectLeftTime - _waitTime;
		if (num < 0f)
		{
			num = 0f;
		}
		_owner.SetCardSelectLeftTime((int)num);
		if (num <= 0f)
		{
			int index = Random.Range(0, 2);
			_owner.AutoSelect(index);
			SetEnd(isEnd: true);
		}
	}

	public override State GetNextState()
	{
		return new KarmaCardSelectAfter(_owner);
	}
}
