using NShared;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class ShopItemGroupComponent : UIScrollItemBehaviour
{
	public interface Listener
	{
		void OnClickComponent(ShopItemGroupComponent This);
	}

	public const string PrefabPath = "Resources/Prefab2/Component/ShopItem/ShopItemGroupComponent";

	public static PrefabAuto<ShopItemGroupComponent> PrefabAuto = new PrefabAuto<ShopItemGroupComponent>("Resources/Prefab2/Component/ShopItem/ShopItemGroupComponent");

	public UISprite Sprite_Selected;

	public UILabel Label_GroupName;

	private Listener _listener;

	public ShopItemGroupData ShopItemGroupData { get; private set; }

	public void Open(ShopItemGroupData GroupData, Listener listener)
	{
		if (GroupData != null)
		{
			ShopItemGroupData = GroupData;
			_listener = listener;
			SetSelect(Select: false);
			string stringDirect = LocalizationManager.GetStringDirect(GroupData.GroupNameKey);
			UIHelper.SetLabelText(Label_GroupName, stringDirect);
		}
	}

	public void SetSelect(bool Select)
	{
		UIHelper.EnableGameObject(Sprite_Selected, Select);
	}

	public void OnClickComponent()
	{
		if (_listener != null)
		{
			_listener.OnClickComponent(this);
		}
	}
}
