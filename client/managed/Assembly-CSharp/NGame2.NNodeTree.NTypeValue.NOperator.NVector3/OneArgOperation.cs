using NGame2.NBattleView;
using NGame2.NNodeTree.NRuntime;
using NGame2.NRender;
using NShared;
using UnityEngine;

namespace NGame2.NNodeTree.NTypeValue.NOperator.NVector3;

public static class OneArgOperation
{
	public static Vector3 GetDirection(IVariable variable, long uid)
	{
		RenderFacade rENDERFACADE = variable.RUNTIMEINSTANCE().SCENECONTAINER().GETRENDERFACADE((int)uid);
		if (rENDERFACADE == null)
		{
			return Vector3.forward;
		}
		return rENDERFACADE.Action.GetDirection().ToVec3().normalized;
	}

	public static Vector3 GetTeamDirection(IVariable variable, long uid)
	{
		Creature bATTLECREATURE = variable.RUNTIMEINSTANCE().SCENECONTAINER().GETBATTLECREATURE((int)uid);
		if (bATTLECREATURE == null)
		{
			return Vector3.right;
		}
		if (bATTLECREATURE.CreatureCache.TeamType == TeamType.Left)
		{
			return Vector3.right;
		}
		return Vector3.left;
	}
}
