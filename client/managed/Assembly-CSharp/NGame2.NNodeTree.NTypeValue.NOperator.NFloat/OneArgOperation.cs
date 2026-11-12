using NGame2.NNodeTree.NRuntime;
using NGame2.NRender;
using UnityEngine;

namespace NGame2.NNodeTree.NTypeValue.NOperator.NFloat;

public static class OneArgOperation
{
	public static float GetVector3X(Vector3 v3)
	{
		return v3.x;
	}

	public static float GetVector3Y(Vector3 v3)
	{
		return v3.y;
	}

	public static float GetVector3Z(Vector3 v3)
	{
		return v3.z;
	}

	public static float GetCreatureRadius(IVariable variable, long uid)
	{
		RenderFacade rENDERFACADE = variable.RUNTIMEINSTANCE().SCENECONTAINER().GETRENDERFACADE((int)uid);
		if (rENDERFACADE == null)
		{
			return 1f;
		}
		return rENDERFACADE.ShapeBounds.extents.x;
	}

	public static float GetCreatureHeight(IVariable variable, long uid)
	{
		RenderFacade rENDERFACADE = variable.RUNTIMEINSTANCE().SCENECONTAINER().GETRENDERFACADE((int)uid);
		if (rENDERFACADE == null)
		{
			return 1f;
		}
		return rENDERFACADE.ShapeBounds.extents.y * 2f;
	}

	public static float Length(Vector3 a)
	{
		return a.magnitude;
	}
}
