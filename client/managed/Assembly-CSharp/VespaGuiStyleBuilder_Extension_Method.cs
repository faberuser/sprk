using NVespa.NGui;
using UnityEngine;

public static class VespaGuiStyleBuilder_Extension_Method
{
	public static StyleBuilder Builder(this GUIStyle i_this)
	{
		return new StyleBuilder(i_this);
	}
}
