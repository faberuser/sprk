using NGame2.NBattleView;
using NGame2.NNodeTree.NRuntime;
using NShared;

namespace NGame2.NNodeTree.NTypeValue.NOperator.NInteger;

public static class CreatureOperation
{
	public static long GetSoulWeaponGrade(IVariable variable, long uid)
	{
		Creature bATTLECREATURE = variable.RUNTIMEINSTANCE().SCENECONTAINER().GETBATTLECREATURE((int)uid);
		if (bATTLECREATURE == null)
		{
			return -1L;
		}
		SoulWeaponInfo soulWeaponInfo = bATTLECREATURE.CreateInfo.HeroInfo.GetSoulWeaponInfo();
		if (soulWeaponInfo == null)
		{
			return -1L;
		}
		return soulWeaponInfo.Grade;
	}

	public static long GetCostumeIndex(IVariable variable, long uid)
	{
		Creature bATTLECREATURE = variable.RUNTIMEINSTANCE().SCENECONTAINER().GETBATTLECREATURE((int)uid);
		if (bATTLECREATURE == null)
		{
			return 0L;
		}
		return bATTLECREATURE.CreateInfo.HeroInfo.CostumeIndex;
	}
}
