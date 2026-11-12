using NGame2.NBattleView;
using NGame2.NNodeTree.NRuntime;
using UnityEngine;

namespace NGame2.NNodeTree.NTypeValue.NOperator.NTransform;

public static class SubCreature
{
	public static Transform GetTransfrom(IVariable variable, long uid)
	{
		Creature bATTLECREATURE = variable.RUNTIMEINSTANCE().SCENECONTAINER().GETBATTLECREATURE((int)uid);
		if (bATTLECREATURE == null || bATTLECREATURE.SubCreature == null)
		{
			return null;
		}
		return bATTLECREATURE.SubCreature.SubCreatureTm;
	}
}
