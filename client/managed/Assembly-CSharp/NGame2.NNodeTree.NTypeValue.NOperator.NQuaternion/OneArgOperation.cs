using NGame2.NNodeTree.NRuntime;
using NGame2.NRender;
using UnityEngine;

namespace NGame2.NNodeTree.NTypeValue.NOperator.NQuaternion;

public static class OneArgOperation
{
	public static Quaternion GetRotationReal(IVariable variable, long uid)
	{
		RenderFacade rENDERFACADE = variable.RUNTIMEINSTANCE().SCENECONTAINER().GETRENDERFACADE((int)uid);
		if (rENDERFACADE == null)
		{
			return Quaternion.identity;
		}
		return rENDERFACADE.DirectionReal.transform.rotation;
	}

	public static Quaternion GetRotationView(IVariable variable, long uid)
	{
		RenderFacade rENDERFACADE = variable.RUNTIMEINSTANCE().SCENECONTAINER().GETRENDERFACADE((int)uid);
		if (rENDERFACADE == null)
		{
			return Quaternion.identity;
		}
		return rENDERFACADE.DirectionView.transform.rotation;
	}

	public static Quaternion CreateFromDirection(Vector3 dir)
	{
		return Quaternion.FromToRotation(Vector3.forward, dir);
	}

	public static Quaternion Inverse(Quaternion quat)
	{
		return Quaternion.Inverse(quat);
	}
}
