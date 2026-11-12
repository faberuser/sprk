using NShared.NUtil;

namespace NShared;

[BattleCallback]
public sealed class SpawnProjectile : BaseBattleCallback
{
	public long ProjectileUid { get; set; }

	public int CreatureUid { get; set; }

	public int SkillIndex { get; set; }

	public int ProjectileIndex { get; set; }

	public int FollowTargetUid { get; set; }

	public Vector3 Position { get; set; }

	public Vector3 Direction { get; set; }

	public override bool IsEquals(BaseBattleCallback callbackBase)
	{
		if (!(callbackBase is SpawnProjectile spawnProjectile))
		{
			return false;
		}
		if (base.GlobalTimestamp == spawnProjectile.GlobalTimestamp && ProjectileUid == spawnProjectile.ProjectileUid && CreatureUid == spawnProjectile.CreatureUid && SkillIndex == spawnProjectile.SkillIndex && ProjectileIndex == spawnProjectile.ProjectileIndex && FollowTargetUid == spawnProjectile.FollowTargetUid && Position == spawnProjectile.Position)
		{
			return Direction == spawnProjectile.Direction;
		}
		return false;
	}
}
