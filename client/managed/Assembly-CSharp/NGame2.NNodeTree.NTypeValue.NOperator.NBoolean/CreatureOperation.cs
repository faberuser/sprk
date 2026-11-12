using NGame2.NBattleField;
using NGame2.NBattleView;
using NGame2.NNodeTree.NRuntime;
using NShared;
using NVespa;

namespace NGame2.NNodeTree.NTypeValue.NOperator.NBoolean;

public static class CreatureOperation
{
	public static bool HasSubCreature(IVariable variable, long uid)
	{
		Creature bATTLECREATURE = variable.RUNTIMEINSTANCE().SCENECONTAINER().GETBATTLECREATURE((int)uid);
		if (bATTLECREATURE == null)
		{
			return false;
		}
		return bATTLECREATURE.SubCreature != null;
	}

	public static bool IsAlive(IVariable variable, long uid)
	{
		return !IsDead(variable, uid);
	}

	public static bool IsDead(IVariable variable, long uid)
	{
		Creature bATTLECREATURE = variable.RUNTIMEINSTANCE().SCENECONTAINER().GETBATTLECREATURE((int)uid);
		if (bATTLECREATURE == null)
		{
			return false;
		}
		return bATTLECREATURE.IsDead;
	}

	public static bool IsLeftTeam(IVariable variable, long uid)
	{
		Creature bATTLECREATURE = variable.RUNTIMEINSTANCE().SCENECONTAINER().GETBATTLECREATURE((int)uid);
		if (bATTLECREATURE == null)
		{
			return false;
		}
		return bATTLECREATURE.CreatureCache.TeamType == TeamType.Left;
	}

	public static bool IsRightTeam(IVariable variable, long uid)
	{
		Creature bATTLECREATURE = variable.RUNTIMEINSTANCE().SCENECONTAINER().GETBATTLECREATURE((int)uid);
		if (bATTLECREATURE == null)
		{
			return false;
		}
		return bATTLECREATURE.CreatureCache.TeamType == TeamType.Right;
	}

	public static bool IsFlip(IVariable variable, long uid)
	{
		Creature bATTLECREATURE = variable.RUNTIMEINSTANCE().SCENECONTAINER().GETBATTLECREATURE((int)uid);
		if (bATTLECREATURE == null)
		{
			return false;
		}
		if (bATTLECREATURE.RenderFacade == null)
		{
			return false;
		}
		return bATTLECREATURE.RenderFacade.IsFlip;
	}

	public static bool IsUsingSkillIndex(long uid, long skillIndex)
	{
		if (!SingletonAuto<BattleFacade>.isValidInstance)
		{
			return false;
		}
		BattleFacade.CreatureCacheData creatureCache = SingletonAuto<BattleFacade>.instance.GetCreatureCache((int)uid);
		if (creatureCache == null)
		{
			return false;
		}
		return creatureCache.UsingSkillIndex == skillIndex;
	}
}
