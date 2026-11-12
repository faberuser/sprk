using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NComponent2;

public class AwakeChallengeItemComponent : LinkFactoryComponent, NVespa.NPrefab2.InitOnCreate, ItemIconComponent.Listener
{
	public const string PrefabPath = "Prefab2/Component/HeroTrialDungeon/AwakeChallengeItemComponent";

	public static PrefabAuto<AwakeChallengeItemComponent> PrefabAuto = new PrefabAuto<AwakeChallengeItemComponent>("Prefab2/Component/HeroTrialDungeon/AwakeChallengeItemComponent");

	public UISimpleGrid ItemGrid;

	public UILabel Label_Count;

	public UISprite Sprite_New;

	private ItemIconComponent _ItemIcon;

	private Action<ItemIconComponent> _onClick;

	public ItemIconComponent ItemIconComponent => _ItemIcon;

	void NVespa.NPrefab2.InitOnCreate.OnCreate()
	{
	}

	void NVespa.NPrefab2.InitOnCreate.OnRelease()
	{
		FactoryBase.SafeReleaseComponent(ref _ItemIcon);
	}

	public void SetListener(Action<ItemIconComponent> onClick)
	{
		_onClick = onClick;
	}

	public void Open(ItemData itemData, long ReqCount, int CurCount, Action<ItemIconComponent> onClick = null)
	{
		Setup(itemData, ReqCount, CurCount);
		UIHelper.EnableGameObject(Sprite_New, enable: false);
		if (itemData != null)
		{
			bool enable = IsCraft(itemData.Code, ReqCount, CurCount);
			UIHelper.EnableGameObject(Sprite_New, enable);
		}
		SetListener(onClick);
	}

	protected virtual void SetItemCountText(long ReqCount, int CurCount)
	{
		string text = ((ReqCount > CurCount) ? "[96866f]" : "[17a0a0]") + $"{CurCount}/{ReqCount}" + "[-]";
		UIHelper.SetLabelText(Label_Count, text);
	}

	private void Setup(ItemData itemData, long ReqCount, int CurCount)
	{
		FactoryBase.SafeReleaseComponent(ref _ItemIcon);
		_ItemIcon = UIHelper.AddGridItem(ItemGrid, ItemIconComponent.PrefabAuto);
		_ItemIcon.Open(itemData, this);
		UIHelper.RepositionGrid(ItemGrid);
		SetItemCountText(ReqCount, CurCount);
	}

	void ItemIconComponent.Listener.OnClick(ItemIconComponent This)
	{
		if (_onClick != null)
		{
			Vespa_System_Action_Extension_Method.fire(_onClick, This);
		}
	}

	private bool IsCraft(string itemCode, long ReqCount, int CurCount)
	{
		int num = SprkEssenceCraftTickets.Indicator(itemCode, ReqCount, CurCount);
		if (num != -1)
		{
			return num == 1;
		}
		if (CurCount >= ReqCount)
		{
			return false;
		}
		IList<CraftItemData> categoryData = GameRoot.Instance.CraftItemDataContainer.GetCategoryData(itemCode);
		if (categoryData == null)
		{
			return false;
		}
		if (__Extension_Method_ICollection__.IsNullOrEmpty(categoryData))
		{
			return false;
		}
		foreach (CraftItemData item in categoryData)
		{
			foreach (CraftItemData.MaterialItemInfo materialItem in item.GetMaterialItemList())
			{
				if (materialItem != null && GlobalAutoBehaviour<AccountManager>.instance.ItemManager.GetItemCount(materialItem.ItemCode) < materialItem.ItemCount)
				{
					return false;
				}
			}
		}
		return true;
	}

	public void UseGraySacle(bool use)
	{
		_ItemIcon.SetIconGrayScale(use);
	}

	public int GetCurrentItemIndex()
	{
		if (!(_ItemIcon != null))
		{
			return 0;
		}
		return _ItemIcon.ItemIndex;
	}

	public string GetCurrentItemCode()
	{
		if (!(_ItemIcon != null))
		{
			return null;
		}
		return _ItemIcon.ItemData.Code;
	}

	public void RefreshItemCount(int ReqCount, int CurCount)
	{
		if (_ItemIcon != null)
		{
			bool enable = IsCraft(_ItemIcon.ItemData.Code, ReqCount, CurCount);
			UIHelper.EnableGameObject(Sprite_New, enable);
		}
		SetItemCountText(ReqCount, CurCount);
	}
}
