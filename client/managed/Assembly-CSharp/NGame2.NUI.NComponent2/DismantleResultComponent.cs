using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class DismantleResultComponent : LinkFactoryComponent, InitOnCreate
{
	public const string PrefabPath = "Prefab2/Component/OptionSell/DismantleResultComponent";

	public static PrefabAuto<DismantleResultComponent> PrefabAuto = new PrefabAuto<DismantleResultComponent>("Prefab2/Component/OptionSell/DismantleResultComponent");

	public UISimpleGrid ItemIconRoot;

	public UILabel CountLabel;

	private ItemIconComponent ItemIcon;

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		FactoryBase.ReleaseComponent(ItemIcon);
		ItemIcon = null;
	}

	public void Open(ItemData data, int min, int max)
	{
		if (data != null)
		{
			ItemIconComponent itemIconComponent = UIHelper.AddGridItem(ItemIconRoot, ItemIconComponent.PrefabAuto);
			itemIconComponent.Open(data);
			ItemIcon = itemIconComponent;
			string text = "";
			UIHelper.SetLabelText(text: (min != max) ? $"{min} ~ {max}" : min.ToString(), label: CountLabel);
		}
	}

	public void ShowEventBonus(bool isShow)
	{
		if (!(ItemIcon == null))
		{
			ItemIcon.ShowEventBonus(isShow);
		}
	}
}
