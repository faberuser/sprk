using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class CutSceneForegroundTextComponent : MonoBehaviour
{
	[SerializeField]
	private UIWidget TextPivotWidget;

	[SerializeField]
	private UILabel TextLabel;

	public string Text
	{
		get
		{
			return TextLabel.text;
		}
		set
		{
			TextLabel.text = value;
		}
	}

	public int TextSize
	{
		get
		{
			return TextLabel.fontSize;
		}
		set
		{
			TextLabel.fontSize = value;
		}
	}

	public UIWidget.Pivot TextPivot
	{
		get
		{
			return TextLabel.pivot;
		}
		set
		{
			TextPivotWidget.pivot = value;
			TextLabel.pivot = value;
		}
	}

	public Vector2 TextOffset
	{
		get
		{
			return Vespa_Unity_Vector3_Extension_Method.XY(TextLabel.transform.localPosition);
		}
		set
		{
			TextLabel.transform.localPosition = Vespa_Unity_Vector3_Extension_Method.ReplaceXY(TextLabel.transform.localPosition, value.x, value.y);
		}
	}

	public Color TextColor
	{
		get
		{
			return TextLabel.color;
		}
		set
		{
			TextLabel.color = value;
		}
	}

	public Color OutlineColor
	{
		get
		{
			return TextLabel.effectColor;
		}
		set
		{
			TextLabel.effectColor = value;
		}
	}

	public void Open()
	{
		TextLabel.text = string.Empty;
		TextLabel.color = Color.white;
		TextLabel.effectColor = Color.black;
		TextLabel.pivot = UIWidget.Pivot.Center;
	}
}
