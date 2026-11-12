using NGame2.NUI;
using UnityEngine;

namespace NGame2.NUI3.NCommon.NChangingStatList;

public class GroupStatUi : ChangingStatListComponent.GroupAbstract
{
	public UILabel Label_Name;

	public UILabel Label_ValueBefore;

	public UILabel Label_ValueAfter;

	public UISprite Sprite_Arrow;

	public GroupStatUi CreateFromTemplate(string typeName, string beforeValue, string afterValue, ChangingStatListComponent.ColorInfo statColor, ChangingStatListComponent.ColorInfo beforeValueColor, ChangingStatListComponent.ColorInfo afterValueColor, bool isPlus)
	{
		GameObject gameObject = Object.Instantiate(base.gameObject);
		if (gameObject == null)
		{
			return null;
		}
		statColor = statColor ?? ChangingStatListComponent.DefaultStatColor;
		beforeValueColor = beforeValueColor ?? ChangingStatListComponent.DefaultValueColor;
		afterValueColor = afterValueColor ?? ChangingStatListComponent.DefaultValueColor;
		GroupStatUi component = gameObject.GetComponent<GroupStatUi>();
		UIHelper.SetLabelText(component.Label_Name, NGUIText.EncodeColor(typeName, statColor.LabelColor));
		UIHelper.SetLabelText(component.Label_ValueBefore, NGUIText.EncodeColor(beforeValue, beforeValueColor.LabelColor));
		UIHelper.SetLabelText(component.Label_ValueAfter, NGUIText.EncodeColor(afterValue, afterValueColor.LabelColor));
		ChangingStatListComponent.SetLabelGradient(component.Label_Name, statColor.Gradient);
		ChangingStatListComponent.SetLabelGradient(component.Label_ValueBefore, beforeValueColor.Gradient);
		ChangingStatListComponent.SetLabelGradient(component.Label_ValueAfter, afterValueColor.Gradient);
		UIHelper.SetGrayscaleSprite(component.Sprite_Arrow, !isPlus);
		return component;
	}
}
