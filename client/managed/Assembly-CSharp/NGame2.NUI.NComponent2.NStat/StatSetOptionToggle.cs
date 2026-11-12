using UnityEngine;

namespace NGame2.NUI.NComponent2.NStat;

public class StatSetOptionToggle : MonoBehaviour
{
	public UISprite spriteOn;

	public void On(bool isOn)
	{
		if (isOn)
		{
			UIHelper.SetWidgetAlpha(spriteOn, 1f);
			UIHelper.SetWidgetGrayScale(spriteOn, grayScale: false);
		}
		else
		{
			UIHelper.SetWidgetAlpha(spriteOn, 0.4f);
			UIHelper.SetWidgetGrayScale(spriteOn, grayScale: true);
		}
	}
}
