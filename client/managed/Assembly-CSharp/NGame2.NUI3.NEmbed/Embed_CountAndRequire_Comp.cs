using NGame2.NUI;
using UnityEngine;

namespace NGame2.NUI3.NEmbed;

public class Embed_CountAndRequire_Comp : MonoBehaviour
{
	[Header("현재 갯수")]
	public UILabel Label_Value_Count;

	[Header("필요 갯수")]
	public UILabel Label_Value_Require;

	public UILabel Label_Static_Slash;

	public Color Color_Insufficient = Color.red;

	public Color Color_Enough = Color.green;

	public void InitUi(int count, int require)
	{
		SetEnableLabels(enable: true);
		UIHelper.SetLabelText(Label_Value_Count, Vespa_Int_Extension_Method.ToCommaString(count));
		UIHelper.SetLabelText(Label_Value_Require, Vespa_Int_Extension_Method.ToCommaString(require));
		if (count < require)
		{
			SetLabelColor(Color_Insufficient);
		}
		else
		{
			SetLabelColor(Color_Enough);
		}
	}

	public void Reset()
	{
		SetEnableLabels(enable: false);
	}

	private void SetEnableLabels(bool enable)
	{
		UIHelper.EnableGameObject(Label_Value_Count, enable);
		UIHelper.EnableGameObject(Label_Value_Require, enable);
		UIHelper.EnableGameObject(Label_Static_Slash, enable);
	}

	private void SetLabelColor(Color color)
	{
		UIHelper.SetLabelColor(Label_Value_Count, color);
		UIHelper.SetLabelColor(Label_Value_Require, color);
		UIHelper.SetLabelColor(Label_Static_Slash, color);
	}
}
