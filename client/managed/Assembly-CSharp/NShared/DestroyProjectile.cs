namespace NShared;

[BattleCallback]
public sealed class DestroyProjectile : BaseBattleCallback
{
	public long ProjectileUid { get; set; }

	public override bool IsEquals(BaseBattleCallback callbackBase)
	{
		if (!(callbackBase is DestroyProjectile destroyProjectile))
		{
			return false;
		}
		if (base.GlobalTimestamp == destroyProjectile.GlobalTimestamp)
		{
			return ProjectileUid == destroyProjectile.ProjectileUid;
		}
		return false;
	}
}
