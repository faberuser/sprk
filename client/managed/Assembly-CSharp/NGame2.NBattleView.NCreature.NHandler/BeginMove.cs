using NGame2.NBattleField;
using NShared;
using NShared.NUtil;
using NVespa;
using UnityEngine;

namespace NGame2.NBattleView.NCreature.NHandler;

public class BeginMove : BaseHandler, IHandler
{
	private NShared.BeginMove _packet;

	private float _speedMultiply = 1f;

	public float PositionThreshold
	{
		get
		{
			BattleFacade.BattleCacheData battleCache = SingletonAuto<BattleFacade>.instance.GetBattleCache();
			if (!battleCache.IsBattleEnabled)
			{
				return 999f;
			}
			float t = (float)battleCache.ElapsedBattleTimeMs * 0.001f;
			return Mathf.Lerp(5f, 1f, t);
		}
	}

	public float MaxSpeedMultiply
	{
		get
		{
			BattleFacade.BattleCacheData battleCache = SingletonAuto<BattleFacade>.instance.GetBattleCache();
			if (!battleCache.IsBattleEnabled)
			{
				return 1f;
			}
			float t = (float)battleCache.ElapsedBattleTimeMs * 0.001f;
			return Mathf.Lerp(2f, 1.05f, t);
		}
	}

	public BeginMove(Creature creature, NShared.BeginMove packet)
		: base(creature)
	{
		_packet = packet;
	}

	private void Close()
	{
		_packet = null;
	}

	void IHandler.Begin()
	{
		UnityEngine.Vector3 vector = _packet.StartPosition;
		UnityEngine.Vector3 vector2 = Owner.Position;
		if (UnityEngine.Vector3.Distance(vector, vector2) > PositionThreshold)
		{
			Owner.SetBattlePos(vector);
			vector2 = vector;
		}
		UnityEngine.Vector3 b = _packet.TargetPosition;
		float value = UnityEngine.Vector3.Distance(vector, b);
		UnityEngine.Vector3 realDestinationFromPacket = GetRealDestinationFromPacket();
		float value2 = (UnityEngine.Vector3.Distance(vector2, realDestinationFromPacket) - 0.5f) / Vespa_Float_Extension_Method.NoneZero(value);
		_speedMultiply = Mathf.Clamp(value2, 1f, MaxSpeedMultiply);
	}

	bool IHandler.IsEnd()
	{
		return _packet == null;
	}

	void IHandler.Cancel()
	{
		Close();
	}

	void IHandler.ChainUpdate()
	{
		if (Vespa_Common_Class_Extension_Method.Interface((IHandler)this).IsEnd())
		{
			return;
		}
		VectorXZ dest = Vespa_Unity_Vector3_Extension_Method.VecXZ(GetRealDestinationFromPacket());
		MoveTo(dest, (float)_packet.SpeedMms * 0.001f * _speedMultiply, idleActionIfEnd: true, out var endMove);
		if (endMove)
		{
			Close();
			return;
		}
		VectorXZ b = Vespa_Unity_Vector3_Extension_Method.VecXZ(Owner.Position);
		Creature creature = BattleViewGenericSingleton<CreatureManager>.instance.GetCreature(_packet.TargetCreatureUid);
		if (creature != null && VectorXZ.Distance(Vespa_Unity_Vector3_Extension_Method.VecXZ(creature.Position), b) < (float)_packet.ApproachDistanceMm * 0.001f)
		{
			Close();
		}
	}

	private UnityEngine.Vector3 GetDestination()
	{
		return GetRealDestinationFromPacket();
	}

	private UnityEngine.Vector3 GetRealDestinationFromPacket()
	{
		Creature creature = BattleViewGenericSingleton<CreatureManager>.instance.GetCreature(_packet.TargetCreatureUid);
		if (creature == null)
		{
			return _packet.TargetPosition;
		}
		UnityEngine.Vector3 position = creature.Position;
		int num = (Owner.CreatureTableData.SizeMm + creature.CreatureTableData.SizeMm) / 2 + 1000;
		int targetOffsetIndex = _packet.TargetOffsetIndex;
		NShared.NUtil.Vector3 vector = MoveHandle.MoveTargetOffsets[targetOffsetIndex] * num / 1000L;
		return position + vector;
	}
}
