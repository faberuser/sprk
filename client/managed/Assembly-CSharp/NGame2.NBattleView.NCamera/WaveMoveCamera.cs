using System.Collections.Generic;
using NGame2.NBattleView.NCreature.NHandler;
using NShared;
using UnityEngine;

namespace NGame2.NBattleView.NCamera;

public class WaveMoveCamera : CameraBase
{
	private List<NGame2.NBattleView.NCreature.NHandler.WaveReady> _handlerList;

	private float _waveMoveElapsed;

	public void SetHandlerList(List<NGame2.NBattleView.NCreature.NHandler.WaveReady> handlerList)
	{
		_handlerList = handlerList;
		_waveMoveElapsed = 0f;
	}

	public override void ChainUpdate()
	{
		bool flag = false;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_handlerList); i++)
		{
			if (Vespa_Common_Class_Extension_Method.Interface((ILooting)_handlerList[i]).IsLootingEnd())
			{
				flag = true;
			}
		}
		if (flag)
		{
			_waveMoveElapsed = Mathf.Clamp01(_waveMoveElapsed + (float)Timer.deltaTick * 0.001f);
		}
		base.ChainUpdate();
	}

	protected override bool IsAcceptable(Creature creature)
	{
		if (creature.IsDead)
		{
			return false;
		}
		if (creature.CreateInfo.IsAi)
		{
			return false;
		}
		if (creature.TeamType != CameraBase.GetMyTeam())
		{
			return false;
		}
		return true;
	}

	protected override void ModifyCreatureBounds(Creature creature, ref Bounds bounds)
	{
		Vector3 min = Vector3.zero;
		Vector3 max = Vector3.zero;
		if (CameraBase.GetMyTeam() == TeamType.Right)
		{
			min = Vector3.left * _waveMoveElapsed * GameSetting.data.BattleView.BattleCamera_WaveMoveFrontSpece;
		}
		if (CameraBase.GetMyTeam() == TeamType.Left)
		{
			max = Vector3.right * _waveMoveElapsed * GameSetting.data.BattleView.BattleCamera_WaveMoveFrontSpece;
		}
		bounds = Vespa_Unity_Bounds_Extension_Method.AddMinMax(bounds, min, max);
	}
}
