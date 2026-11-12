using System.Collections.Generic;
using UnityEngine;

public static class ExtensionMethod_Ngui_UIGrid
{
	public static void SORT(this UIGrid.IGridSort This, List<Transform> list)
	{
		This?.Sort(list);
	}
}
