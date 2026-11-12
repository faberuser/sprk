using NVespa.NNGUIExtension;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class Item3dModelExtraDescComponent : UIScrollItemBehaviour
{
	public const string PrefabPath = "Prefab2/Component/Item3dModelExtraDescComponent";

	public static PrefabAuto<Item3dModelExtraDescComponent> PrefabAuto = new PrefabAuto<Item3dModelExtraDescComponent>("Prefab2/Component/Item3dModelExtraDescComponent");

	public UILabel LabelTitle;

	public UILabel LabelValue;

	public UIWidgetResizer Resizer;

	public void Open(string title, string value)
	{
		UIHelper.SetLabelText(LabelTitle, title);
		UIHelper.SetLabelText(LabelValue, value);
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
