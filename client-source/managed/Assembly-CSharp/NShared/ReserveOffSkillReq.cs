namespace NShared;

[BattlePacket]
public class ReserveOffSkillReq : BaseCreatureActionReq
{
	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (!(obj is ReserveOffSkillReq reserveOffSkillReq))
		{
			return false;
		}
		return base.CreatureUid.Equals(reserveOffSkillReq.CreatureUid);
	}

	public override int GetHashCode()
	{
		return base.CreatureUid.GetHashCode();
	}
}
