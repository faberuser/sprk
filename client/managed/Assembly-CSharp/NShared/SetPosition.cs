using NShared.NUtil;

namespace NShared;

[BattleCallback]
public sealed class SetPosition : BaseBattleCallback
{
	public enum SetPositionMethodType
	{
		None,
		WaveReady,
		DungeonChangeMove,
		DungeonChangeMoveBackward,
		Knockback,
		Pulling,
		Down,
		Spawn,
		Warp,
		WakeUp
	}

	public int CreatureUid { get; set; }

	public Vector3 Position { get; set; }

	public SetPositionMethodType MethodType { get; set; }

	public long OperationUid { get; set; }

	public override bool IsEquals(BaseBattleCallback callbackBase)
	{
		if (!(callbackBase is SetPosition setPosition))
		{
			return false;
		}
		if (base.GlobalTimestamp == setPosition.GlobalTimestamp && CreatureUid == setPosition.CreatureUid && OperationUid == setPosition.OperationUid && Position == setPosition.Position)
		{
			return MethodType == setPosition.MethodType;
		}
		return false;
	}
}
