using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class LabelFieldResizedComponent : LabelFieldComponent
{
	public const string PerfabPathResized = "Prefab2/Component/LabelFieldResizedComponent";

	public static PrefabAuto<LabelFieldResizedComponent> PrefabAutoResized = new PrefabAuto<LabelFieldResizedComponent>("Prefab2/Component/LabelFieldResizedComponent");

	public UIWidgetResizer Resizer;

	public void Open(string field, string value)
	{
		UIHelper.SetLabelText(FieldLabel, field);
		UIHelper.SetLabelText(ValueLabel, value);
		UIHelper.SetLabelColor(ValueLabel, ValueNormalColor);
		UIHelper.EnableGameObject(ExtraLabel, enable: false);
		SetFieldButtonInfo();
		Resize();
	}

	public void Open(string field, string value, FieldButtonInfo fieldButtonInfo)
	{
		Open(field, value, null, fieldButtonInfo);
		Resize();
	}

	public void Resize()
	{
		if (Resizer != null)
		{
			Resizer.Resize();
		}
	}
}
