namespace NShared;

[BattlePacket]
public class ReserveOnSkillReq : BaseCreatureActionReq
{
	public int SkillSlotIndex { get; set; }

	public int ManualTargetUid { get; set; }

	public bool AutoReserved { get; set; }

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (!(obj is ReserveOnSkillReq reserveOnSkillReq))
		{
			return false;
		}
		if (base.CreatureUid.Equals(reserveOnSkillReq.CreatureUid) && SkillSlotIndex.Equals(reserveOnSkillReq.SkillSlotIndex) && ManualTargetUid.Equals(reserveOnSkillReq.ManualTargetUid))
		{
			return AutoReserved.Equals(reserveOnSkillReq.AutoReserved);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return base.CreatureUid.GetHashCode() ^ SkillSlotIndex.GetHashCode() ^ ManualTargetUid.GetHashCode() ^ AutoReserved.GetHashCode();
	}
}
