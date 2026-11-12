using NVespa.NNGUIExtension;
using UnityEngine;

public static class Vespa_Ngui_UIWidget_Extension_Method
{
	public static void RearrangeWidgetDepth(this UIWidget This)
	{
		Tools.RearrangeWidgetDepth(This);
	}

	public static int RearrangeWidgetDepth(this UIWidget This, int beginDepth)
	{
		return Tools.RearrangeWidgetDepth(This.transform, beginDepth);
	}

	public static Vector2 VespaLocalSize(this UIButton This)
	{
		UIWidget component = This.GetComponent<UIWidget>();
		if (component != null)
		{
			return component.localSize;
		}
		return Vector2.zero;
	}
}
