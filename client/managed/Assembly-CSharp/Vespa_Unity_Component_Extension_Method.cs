using UnityEngine;

public static class Vespa_Unity_Component_Extension_Method
{
	public static T GetComponentAuto<T>(this Component This) where T : Component
	{
		if (This == null)
		{
			return null;
		}
		return Vespa_Unity_GameObject_Extension_Method.GetComponentAuto<T>(This.gameObject);
	}
}
