using NGame2.NUI;
using UnityEngine;

namespace NGame2.NUI3.NCommon.NChangingStatList;

public class GroupOptionUi : ChangingStatListComponent.GroupAbstract
{
	public UILabel Label_ValueBefore;

	public UILabel Label_ValueAfter;

	public UISprite Sprite_Arrow;

	public GroupOptionUi CreateFromTemplate(string beforeValue, string afterValue, ChangingStatListComponent.ColorInfo beforeValueColor, ChangingStatListComponent.ColorInfo afterValueColor, bool isPlus)
	{
		beforeValueColor = beforeValueColor ?? ChangingStatListComponent.DefaultValueColor;
		afterValueColor = afterValueColor ?? ChangingStatListComponent.DefaultValueColor;
		GameObject gameObject = Object.Instantiate(base.gameObject);
		if (gameObject == null)
		{
			return null;
		}
		GroupOptionUi component = gameObject.GetComponent<GroupOptionUi>();
		UIHelper.SetLabelText(component.Label_ValueBefore, NGUIText.EncodeColor(beforeValue, beforeValueColor.LabelColor));
		UIHelper.SetLabelText(component.Label_ValueAfter, NGUIText.EncodeColor(afterValue, afterValueColor.LabelColor));
		ChangingStatListComponent.SetLabelGradient(component.Label_ValueBefore, beforeValueColor.Gradient);
		ChangingStatListComponent.SetLabelGradient(component.Label_ValueAfter, afterValueColor.Gradient);
		UIHelper.SetGrayscaleSprite(Sprite_Arrow, !isPlus);
		return component;
	}
}
