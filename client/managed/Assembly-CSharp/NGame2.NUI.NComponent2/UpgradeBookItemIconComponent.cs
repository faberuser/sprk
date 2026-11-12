using NGame2.NUI.NWindow;
using NGame2.NUtil.NStringProxy;
using NGameAsset;
using NShared;
using NShared.NLocalization;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class UpgradeBookItemIconComponent : LinkFactoryComponent
{
	public const string PrefabPath = "Resources/Prefab2/Component/Icon/UpgradeBookItemIconComponent";

	public static PrefabAuto<UpgradeBookItemIconComponent> PrefabAuto = new PrefabAuto<UpgradeBookItemIconComponent>("Resources/Prefab2/Component/Icon/UpgradeBookItemIconComponent");

	public UIWidget WidgetThis;

	public UITexture Texture_ItemIcon;

	public UILabel Label_Count;

	public UILabel Label_UpdateCount;

	public TweenPosition UpdateCountTweenPos;

	public TweenAlpha UpdateCountTweenAlpha;

	public ItemTooltipTextInfo _TooltipTextInfo;

	private int _itemCount;

	public ItemData ItemData { get; private set; }

	public void Open(ItemData itemData, int itemCount, string TagType, int skillSlotIndex)
	{
		ItemData = itemData;
		AssetTexture plainTexture = UIHelper.GetPlainTexture("UpgradeBook/" + TagType + "_" + skillSlotIndex);
		UIHelper.ResetPlaneAssetTextureAsync(this, Texture_ItemIcon, plainTexture);
		SetItemCount(itemCount);
		SetupTooltipText();
	}

	public void SetItemCount(int itemCount, bool CountLabelTween = false)
	{
		itemCount = SprkSkillBookTickets.Display(this, itemCount);
		int itemCount2 = _itemCount;
		UIHelper.SetLabelText(Label_Count, itemCount);
		UIHelper.EnableGameObject(Label_UpdateCount, CountLabelTween);
		if (CountLabelTween && itemCount2 != itemCount)
		{
			int num = itemCount - itemCount2;
			UIHelper.SetLabelText(Label_UpdateCount, num);
			UpdateCountTweenPos.PlayReset(forward: true);
			UpdateCountTweenAlpha.PlayReset(forward: true);
		}
		_itemCount = itemCount;
	}

	public void OnShowTooltip()
	{
		ItemIconTooltip.Open(WidgetThis, _TooltipTextInfo);
	}

	public void OnHideTooltip()
	{
		ItemIconTooltip.Hide(WidgetThis);
	}

	private void SetupTooltipText()
	{
		if (ItemData != null && ItemData.Type == ItemType.UpgradeBook)
		{
			if (_TooltipTextInfo == null)
			{
				_TooltipTextInfo = new ItemTooltipTextInfo();
			}
			_TooltipTextInfo.Reset();
			_TooltipTextInfo.Name.Assign(new ItemNameBuilder(ItemData, null, appendLine: false));
			_TooltipTextInfo.TypeText.Assign(new EnumBuilder().Key(Item.ITEM_UPGRADEBOOK));
			if (!String_Extension_Method.IsNullOrEmpty(ItemData.Description))
			{
				_TooltipTextInfo.Desc.Value = ItemData.Description;
			}
			_TooltipTextInfo.Desc1.Value = ItemData.Description2;
		}
	}
}
