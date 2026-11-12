using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class StateData : IDataKeySelector<string>
{
	string IDataKeySelector<string>.Key => StateType;

	[Key(0)]
	public string StateType { get; set; }

	[Key(1)]
	public string Group { get; set; }

	[Key(2)]
	public int MaxStack { get; set; }

	[Key(3)]
	public StateOverflowRuleType OverflowRule { get; set; }

	[Key(4)]
	public StateStackDurationRuleType StackDurationRuleType { get; set; }

	[Key(5)]
	public bool CanMove { get; set; }

	[Key(6)]
	public bool CanPhysicalSkill { get; set; }

	[Key(7)]
	public bool CanMagicalSkill { get; set; }

	[Key(8)]
	public bool CanBaseAttack { get; set; }

	[Key(9)]
	public bool CanBeTargetedByAlly { get; set; }

	[Key(10)]
	public bool CanBeTargetedByEnemy { get; set; }

	[Key(11)]
	public bool CanBePhysicalCc { get; set; }

	[Key(12)]
	public bool CanBeMagicalCc { get; set; }

	[Key(13)]
	public bool CanBePhysicalDamaged { get; set; }

	[Key(14)]
	public bool CanBeMagicalDamaged { get; set; }

	[Key(15)]
	public bool CanHitPhysicalAttack { get; set; }

	[Key(16)]
	public bool CanHitMagicalAttack { get; set; }

	[Key(17)]
	public bool CanPhysicalBlockAll { get; set; }

	[Key(18)]
	public bool CanMagicalBlockAll { get; set; }

	[Key(19)]
	public bool CanPhysicalDodgeAll { get; set; }

	[Key(20)]
	public bool CanMagicalDodgeAll { get; set; }

	[Key(21)]
	public bool IsPositive { get; set; }

	[Key(22)]
	public bool IsNegative { get; set; }

	[Key(23)]
	public bool IsSticky { get; set; }

	[Key(24)]
	public bool CanUpdateCooltime { get; set; }

	[Key(25)]
	public bool RejectReservedSkill { get; set; }

	[Key(26)]
	public bool IsCc { get; set; }

	[Key(27)]
	public bool CanPassiveSkill { get; set; }

	[Key(28)]
	public CcMotionType CcMotionType { get; set; }

	[Key(29)]
	public int CcPower { get; set; }

	[Key(30)]
	public string IconPath { get; set; }

	[Key(31)]
	public StateFlag StateFlag { get; set; }

	public StateFlag GetStateFlag()
	{
		return StateFlag;
	}

	public bool HasFlag(StateFlag flag, BattleCreature firedCreature)
	{
		if (flag == StateFlag.None)
		{
			return false;
		}
		bool flag2 = true;
		if ((flag & StateFlag.CanMove) != StateFlag.None)
		{
			flag2 &= this.CanMove(firedCreature);
		}
		if ((flag & StateFlag.CanPhysicalSkill) != StateFlag.None)
		{
			flag2 &= this.CanPhysicalSkill(firedCreature);
		}
		if ((flag & StateFlag.CanMagicalSkill) != StateFlag.None)
		{
			flag2 &= this.CanMagicalSkill(firedCreature);
		}
		if ((flag & StateFlag.CanBaseAttack) != StateFlag.None)
		{
			flag2 &= this.CanBaseAttack(firedCreature);
		}
		if ((flag & StateFlag.CanBeTargetedByAlly) != StateFlag.None)
		{
			flag2 &= this.CanBeTargetedByAlly(firedCreature);
		}
		if ((flag & StateFlag.CanBeTargetedByEnemy) != StateFlag.None)
		{
			flag2 &= this.CanBeTargetedByEnemy(firedCreature);
		}
		if ((flag & StateFlag.CanBePhysicalCc) != StateFlag.None)
		{
			flag2 &= this.CanBePhysicalCc(firedCreature);
		}
		if ((flag & StateFlag.CanBeMagicalCc) != StateFlag.None)
		{
			flag2 &= this.CanBeMagicalCc(firedCreature);
		}
		if ((flag & StateFlag.CanBePhysicalDamaged) != StateFlag.None)
		{
			flag2 &= this.CanBePhysicalDamaged(firedCreature);
		}
		if ((flag & StateFlag.CanBeMagicalDamaged) != StateFlag.None)
		{
			flag2 &= this.CanBeMagicalDamaged(firedCreature);
		}
		if ((flag & StateFlag.CanHitPhysicalAttack) != StateFlag.None)
		{
			flag2 &= this.CanHitPhysicalAttack(firedCreature);
		}
		if ((flag & StateFlag.CanHitMagicalAttack) != StateFlag.None)
		{
			flag2 &= this.CanHitMagicalAttack(firedCreature);
		}
		if ((flag & StateFlag.CanPhysicalBlockAll) != StateFlag.None)
		{
			flag2 &= this.CanPhysicalBlockAll(firedCreature);
		}
		if ((flag & StateFlag.CanMagicalBlockAll) != StateFlag.None)
		{
			flag2 &= this.CanMagicalBlockAll(firedCreature);
		}
		if ((flag & StateFlag.CanPhysicalDodgeAll) != StateFlag.None)
		{
			flag2 &= this.CanPhysicalDodgeAll(firedCreature);
		}
		if ((flag & StateFlag.CanMagicalDodgeAll) != StateFlag.None)
		{
			flag2 &= this.CanMagicalDodgeAll(firedCreature);
		}
		if ((flag & StateFlag.IsSticky) != StateFlag.None)
		{
			flag2 &= IsSticky;
		}
		if ((flag & StateFlag.IsPositive) != StateFlag.None)
		{
			flag2 &= this.IsPositive(firedCreature);
		}
		if ((flag & StateFlag.IsNegative) != StateFlag.None)
		{
			flag2 &= this.IsNegative(firedCreature);
		}
		if ((flag & StateFlag.IsCc) != StateFlag.None)
		{
			flag2 &= this.IsCc(firedCreature);
		}
		if ((flag & StateFlag.CanPassiveSkill) != StateFlag.None)
		{
			flag2 &= this.CanPassiveSkill(firedCreature);
		}
		if ((flag & StateFlag.RejectReservedSkill) != StateFlag.None)
		{
			flag2 &= this.RejectReservedSkill(firedCreature);
		}
		if ((flag & StateFlag.CanUpdateCooltime) != StateFlag.None)
		{
			flag2 &= this.CanUpdateCooltime(firedCreature);
		}
		return flag2;
	}
}
