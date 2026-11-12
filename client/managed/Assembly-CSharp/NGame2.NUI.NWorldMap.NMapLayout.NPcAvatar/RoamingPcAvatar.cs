using System.Collections.Generic;
using System.Linq;
using NShared;
using NVespa;
using UnityEngine;

namespace NGame2.NUI.NWorldMap.NMapLayout.NPcAvatar;

public class RoamingPcAvatar : BasePcAvatar
{
	public int[] _moveNodes;

	public float[] _stayTimes;

	public string[] _touchActions;

	public float _stayTime;

	public float _curStayTime;

	public override void ChainUpdate()
	{
		base.ChainUpdate();
		if (!Hide)
		{
			UpdateCheckNextNode();
		}
	}

	protected override void SetAvatarBehavior(BasePcMediator mediator, PcAvatarData pcAvatarData)
	{
		base.SetAvatarBehavior(mediator, pcAvatarData);
		_moveNodes = pcAvatarData.MoveNodes;
		_stayTimes = pcAvatarData.StayTimes;
		_touchActions = pcAvatarData.TouchActions;
		int moveDungeonIndex = GetMoveDungeonIndex();
		mediator.SetDungeonIndex(moveDungeonIndex);
		SetPositionByDungeonIndex(moveDungeonIndex);
		_stayTime = GetStayTime();
		Show(show: true);
	}

	private int GetMoveDungeonIndex()
	{
		int dungeonIndex = GetDungeonIndex();
		List<int> list = _moveNodes.ToList();
		list.Remove(dungeonIndex);
		int index = Random.Range(0, __Extension_Method_ICollection__.COUNT(list));
		if (__Extension_Method_ICollection__.COUNT(list) == 0)
		{
			return -1;
		}
		return list[index];
	}

	private float GetStayTime()
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_stayTimes) || _stayTimes.Length < 2)
		{
			return 5f;
		}
		return Random.Range(_stayTimes[0], _stayTimes[1]);
	}

	private void UpdateCheckNextNode()
	{
		if (IsMoving())
		{
			return;
		}
		_curStayTime += MainTimer.deltaTime;
		if (_curStayTime > _stayTime)
		{
			int moveDungeonIndex = GetMoveDungeonIndex();
			if (moveDungeonIndex != -1)
			{
				_curStayTime = 0f;
				_stayTime = GetStayTime();
				MoveToDungeonIndex(moveDungeonIndex, userInput: false);
			}
		}
	}

	protected override void OnClickAvatar()
	{
		if (!IsMoving())
		{
			SetMotion(_touchActions);
			ClickAvatarDerived();
		}
	}

	protected virtual void ClickAvatarDerived()
	{
	}
}
