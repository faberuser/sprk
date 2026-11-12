using NShared;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class HelpLabelComponent : BaseHelpComponent
{
	public const string PrefabPathHorizontal = "Prefab2/Component/Help/HelpLabelHorizontalComponent";

	public static PrefabAuto<HelpLabelComponent> PrefabAutoHorizontal = new PrefabAuto<HelpLabelComponent>("Prefab2/Component/Help/HelpLabelHorizontalComponent");

	public const string PrefabPathVertical = "Prefab2/Component/Help/HelpLabelVerticalComponent";

	public static PrefabAuto<HelpLabelComponent> PrefabAutoVertical = new PrefabAuto<HelpLabelComponent>("Prefab2/Component/Help/HelpLabelVerticalComponent");

	public override void Open(HelpListPageElement elementData)
	{
		SetLabelSize(elementData.TextureWidth);
		base.Open(elementData);
	}

	private void SetLabelSize(int width)
	{
		if (Movement == UIScrollView.Movement.Horizontal && width != 0)
		{
			LabelDesc.width = width;
		}
	}
}
