using NGame2.NUtil.NStringProxy;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class EnchantOptionItemComponent : UIScrollItemBehaviour, InitOnCreate, ItemIconComponent.Listener
{
	public interface Listener
	{
		void OnClickComponent(EnchantOptionItemComponent component);
	}

	public const string PrefabPath = "Prefab2/Component/Icon/EnchantOptionItemComponent";

	public static PrefabAuto<EnchantOptionItemComponent> PrefabAuto = new PrefabAuto<EnchantOptionItemComponent>("Prefab2/Component/Icon/EnchantOptionItemComponent");

	public UISimpleGrid ItemGrid;

	public UILabel Label_ItemName;

	public UILabel Label_ItemDesc;

	public UISprite Sprite_Selected;

	private ItemIconComponent _EnchantOptionItemIconComponent;

	private Listener _listener;

	public ItemData ItemDataToEnchantOption { get; private set; }

	void InitOnCreate.OnCreate()
	{
		SetSelected(Selected: false);
	}

	void InitOnCreate.OnRelease()
	{
		FactoryBase.SafeReleaseComponent(ref _EnchantOptionItemIconComponent);
	}

	public void Open(ItemData itemData, int itemCount, Listener listener)
	{
		_listener = listener;
		ItemDataToEnchantOption = itemData;
		_EnchantOptionItemIconComponent = UIHelper.AddGridItem(ItemGrid, ItemIconComponent.PrefabAuto);
		_EnchantOptionItemIconComponent.Open(ItemDataToEnchantOption, this);
		_EnchantOptionItemIconComponent.SetItemCount(itemCount);
		UIHelper.SetLabelText(Label_ItemName, ItemNameBuilder.GetItemName(ItemDataToEnchantOption));
		UIHelper.SetLabelText(Label_ItemDesc, ItemDataToEnchantOption.Description2);
	}

	void ItemIconComponent.Listener.OnClick(ItemIconComponent itemIconComponent)
	{
		if (_listener != null)
		{
			_listener.OnClickComponent(this);
		}
	}

	public void OnClickComponent()
	{
		if (_listener != null)
		{
			_listener.OnClickComponent(this);
		}
	}

	public void SetSelected(bool Selected)
	{
		UIHelper.EnableGameObject(Sprite_Selected, Selected);
	}
}
