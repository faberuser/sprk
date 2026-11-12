using NShared.NUtil;

namespace NShared;

[BattleCallback]
public sealed class FireSkill : BaseBattleCallback
{
	[BattleCallback]
	public class OperationInfo
	{
		public long OperationUid { get; set; }

		public int OperationNo { get; set; }

		public int TargetCreatureUid { get; set; }

		public Vector3 TargetPosition { get; set; }

		public int SoloTimeMs { get; set; }

		public bool IsIgnoreSoloTime { get; set; }
	}

	public int CreatureUid { get; set; }

	public int SkillId { get; set; }

	public int FiredSkillId { get; set; }

	public int FirstTargetCreatureUid { get; set; }

	public Vector3 TargetPosition { get; set; }

	public int TickIndex { get; set; }

	public OperationInfo[] OperationInfos { get; set; }

	public override bool IsEquals(BaseBattleCallback callbackBase)
	{
		if (!(callbackBase is FireSkill fireSkill))
		{
			return false;
		}
		if (base.GlobalTimestamp == fireSkill.GlobalTimestamp && CreatureUid == fireSkill.CreatureUid && SkillId == fireSkill.SkillId && FiredSkillId == fireSkill.FiredSkillId && FirstTargetCreatureUid == fireSkill.FirstTargetCreatureUid && TargetPosition == fireSkill.TargetPosition && TickIndex == fireSkill.TickIndex)
		{
			return IsEquals(OperationInfos, fireSkill.OperationInfos);
		}
		return false;
	}

	private static bool IsEquals(OperationInfo[] lhs, OperationInfo[] rhs)
	{
		int num = ((lhs != null) ? lhs.Length : 0);
		int num2 = ((rhs != null) ? rhs.Length : 0);
		if (num != num2)
		{
			return false;
		}
		if (num == 0)
		{
			return true;
		}
		for (int i = 0; i < num; i++)
		{
			if (lhs[i].OperationUid != rhs[i].OperationUid)
			{
				return false;
			}
			if (lhs[i].OperationNo != rhs[i].OperationNo)
			{
				return false;
			}
			if (lhs[i].TargetCreatureUid != rhs[i].TargetCreatureUid)
			{
				return false;
			}
			if (lhs[i].TargetPosition != rhs[i].TargetPosition)
			{
				return false;
			}
			if (lhs[i].SoloTimeMs != rhs[i].SoloTimeMs)
			{
				return false;
			}
			if (lhs[i].IsIgnoreSoloTime != rhs[i].IsIgnoreSoloTime)
			{
				return false;
			}
		}
		return true;
	}
}
