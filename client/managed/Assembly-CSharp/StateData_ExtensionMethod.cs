using NShared;

public static class StateData_ExtensionMethod
{
	public static bool CanMove(this StateData self, BattleCreature firedCreature)
	{
		if (self != null)
		{
			return firedCreature?.StateController.GetModifiedStateValue(self.StateType, StateFlag.CanMove, self.CanMove) ?? false;
		}
		return true;
	}

	public static bool CanPhysicalSkill(this StateData self, BattleCreature firedCreature)
	{
		if (self != null)
		{
			return firedCreature?.StateController.GetModifiedStateValue(self.StateType, StateFlag.CanPhysicalSkill, self.CanPhysicalSkill) ?? false;
		}
		return true;
	}

	public static bool CanMagicalSkill(this StateData self, BattleCreature firedCreature)
	{
		if (self != null)
		{
			return firedCreature?.StateController.GetModifiedStateValue(self.StateType, StateFlag.CanMagicalSkill, self.CanMagicalSkill) ?? false;
		}
		return true;
	}

	public static bool CanBaseAttack(this StateData self, BattleCreature firedCreature)
	{
		if (self != null)
		{
			return firedCreature?.StateController.GetModifiedStateValue(self.StateType, StateFlag.CanBaseAttack, self.CanBaseAttack) ?? false;
		}
		return true;
	}

	public static bool CanBeTargetedByAlly(this StateData self, BattleCreature firedCreature)
	{
		if (self != null)
		{
			return firedCreature?.StateController.GetModifiedStateValue(self.StateType, StateFlag.CanBeTargetedByAlly, self.CanBeTargetedByAlly) ?? false;
		}
		return true;
	}

	public static bool CanBeTargetedByEnemy(this StateData self, BattleCreature firedCreature)
	{
		if (self != null)
		{
			return firedCreature?.StateController.GetModifiedStateValue(self.StateType, StateFlag.CanBeTargetedByEnemy, self.CanBeTargetedByEnemy) ?? false;
		}
		return true;
	}

	public static bool CanBePhysicalCc(this StateData self, BattleCreature firedCreature)
	{
		if (self != null)
		{
			return firedCreature?.StateController.GetModifiedStateValue(self.StateType, StateFlag.CanBePhysicalCc, self.CanBePhysicalCc) ?? false;
		}
		return true;
	}

	public static bool CanBeMagicalCc(this StateData self, BattleCreature firedCreature)
	{
		if (self != null)
		{
			return firedCreature?.StateController.GetModifiedStateValue(self.StateType, StateFlag.CanBeMagicalCc, self.CanBeMagicalCc) ?? false;
		}
		return true;
	}

	public static bool CanBePhysicalDamaged(this StateData self, BattleCreature firedCreature)
	{
		if (self != null)
		{
			return firedCreature?.StateController.GetModifiedStateValue(self.StateType, StateFlag.CanBePhysicalDamaged, self.CanBePhysicalDamaged) ?? false;
		}
		return true;
	}

	public static bool CanBeMagicalDamaged(this StateData self, BattleCreature firedCreature)
	{
		if (self != null)
		{
			return firedCreature?.StateController.GetModifiedStateValue(self.StateType, StateFlag.CanBeMagicalDamaged, self.CanBeMagicalDamaged) ?? false;
		}
		return true;
	}

	public static bool CanHitPhysicalAttack(this StateData self, BattleCreature firedCreature)
	{
		if (self != null)
		{
			return firedCreature?.StateController.GetModifiedStateValue(self.StateType, StateFlag.CanHitPhysicalAttack, self.CanHitPhysicalAttack) ?? false;
		}
		return true;
	}

	public static bool CanHitMagicalAttack(this StateData self, BattleCreature firedCreature)
	{
		if (self != null)
		{
			return firedCreature?.StateController.GetModifiedStateValue(self.StateType, StateFlag.CanHitMagicalAttack, self.CanHitMagicalAttack) ?? false;
		}
		return true;
	}

	public static bool CanPhysicalBlockAll(this StateData self, BattleCreature firedCreature)
	{
		if (self != null)
		{
			return firedCreature?.StateController.GetModifiedStateValue(self.StateType, StateFlag.CanPhysicalBlockAll, self.CanPhysicalBlockAll) ?? false;
		}
		return true;
	}

	public static bool CanMagicalBlockAll(this StateData self, BattleCreature firedCreature)
	{
		if (self != null)
		{
			return firedCreature?.StateController.GetModifiedStateValue(self.StateType, StateFlag.CanMagicalBlockAll, self.CanMagicalBlockAll) ?? false;
		}
		return true;
	}

	public static bool CanPhysicalDodgeAll(this StateData self, BattleCreature firedCreature)
	{
		if (self != null)
		{
			return firedCreature?.StateController.GetModifiedStateValue(self.StateType, StateFlag.CanPhysicalDodgeAll, self.CanPhysicalDodgeAll) ?? false;
		}
		return true;
	}

	public static bool CanMagicalDodgeAll(this StateData self, BattleCreature firedCreature)
	{
		if (self != null)
		{
			return firedCreature?.StateController.GetModifiedStateValue(self.StateType, StateFlag.CanMagicalDodgeAll, self.CanMagicalDodgeAll) ?? false;
		}
		return true;
	}

	public static bool IsPositive(this StateData self, BattleCreature firedCreature)
	{
		if (self != null)
		{
			return firedCreature?.StateController.GetModifiedStateValue(self.StateType, StateFlag.IsPositive, self.IsPositive) ?? false;
		}
		return true;
	}

	public static bool IsNegative(this StateData self, BattleCreature firedCreature)
	{
		if (self != null)
		{
			return firedCreature?.StateController.GetModifiedStateValue(self.StateType, StateFlag.IsNegative, self.IsNegative) ?? false;
		}
		return true;
	}

	public static bool IsCc(this StateData self, BattleCreature firedCreature)
	{
		if (self != null)
		{
			return firedCreature?.StateController.GetModifiedStateValue(self.StateType, StateFlag.IsCc, self.IsCc) ?? false;
		}
		return true;
	}

	public static bool CanPassiveSkill(this StateData self, BattleCreature firedCreature)
	{
		if (self != null)
		{
			return firedCreature?.StateController.GetModifiedStateValue(self.StateType, StateFlag.CanPassiveSkill, self.CanPassiveSkill) ?? false;
		}
		return true;
	}

	public static int MaxStack(this StateData self, BattleCreature firedCreature)
	{
		return firedCreature?.StateController.GetModifiedStateValue(self.StateType, StateFlag.MaxStack, self.MaxStack) ?? 0;
	}

	public static bool RejectReservedSkill(this StateData self, BattleCreature firedCreature)
	{
		if (self != null)
		{
			return firedCreature?.StateController.GetModifiedStateValue(self.StateType, StateFlag.RejectReservedSkill, self.RejectReservedSkill) ?? false;
		}
		return true;
	}

	public static bool CanUpdateCooltime(this StateData self, BattleCreature firedCreature)
	{
		if (self != null)
		{
			return firedCreature?.StateController.GetModifiedStateValue(self.StateType, StateFlag.CanUpdateCooltime, self.CanUpdateCooltime) ?? false;
		}
		return true;
	}
}
