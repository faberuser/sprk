using NGameAsset;
using NShared;
using NVespa;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class ChallengeReqItemComponent : LinkFactoryComponent
{
	public interface Listener
	{
		void OnClickOpen(ChallengeReqItemComponent comp);
	}

	public const string PrefabPath = "Prefab2/Component/HeroTrialDungeon/ChallengeReqItemComponent";

	public static PrefabAuto<ChallengeReqItemComponent> PrefabAuto = new PrefabAuto<ChallengeReqItemComponent>("Prefab2/Component/HeroTrialDungeon/ChallengeReqItemComponent");

	public UISprite Sprite_ItemBorder;

	public UITexture Texture_Item;

	public UISprite Sprite_Plus;

	public UILabel Label_ItemCount;

	private Listener _listener;

	public ItemData ItemData { get; private set; }

	public int reqItemCount { get; private set; }

	public bool Used { get; private set; }

	public int CurItemCount { get; private set; }

	public void Open(ItemData itemData, int CurItem, int ReqItem, Listener listener)
	{
		if (itemData == null)
		{
			VespaDebug.LogError("itemdata is null");
			return;
		}
		Used = false;
		_listener = listener;
		ItemData = itemData;
		reqItemCount = ReqItem;
		CurItemCount = CurItem;
		if (Sprite_ItemBorder != null)
		{
			Sprite_ItemBorder.isGrayscale = true;
		}
		AssetTexture itemIcon = UIHelper.GetItemIcon2(itemData.Index);
		UIHelper.ResetPlaneAssetTextureAsync(this, Texture_Item, itemIcon);
		Texture_Item.isGrayscale = true;
		UIHelper.EnableGameObject(Sprite_Plus, enable: true);
		UIHelper.EnableGameObject(Label_ItemCount, enable: true);
		string text = ((reqItemCount <= CurItemCount) ? "[00FF00]" : "[FF0000]");
		UIHelper.SetLabelText(Label_ItemCount, text + $"{CurItemCount}/{reqItemCount}" + "[-]");
	}

	public void OnClickOpen()
	{
		if (_listener != null)
		{
			_listener.OnClickOpen(this);
		}
	}

	public bool OpenButtonClicked()
	{
		return false;
	}

	public void SetUsed()
	{
		if (!Used)
		{
			Used = true;
			Texture_Item.isGrayscale = false;
			UIHelper.EnableGameObject(Sprite_Plus, enable: false);
			UIHelper.EnableGameObject(Label_ItemCount, enable: false);
			UIHelper.SetWidgetGrayScale(Sprite_ItemBorder, grayScale: false);
		}
	}
}
