using NGame2.NUI.NWindow;
using NGame2.NUtil.NStringProxy;
using NShared;
using NShared.NLocalization;
using NVespa.NGlobal;
using UnityEngine;

namespace NGame2.NUI;

public class ItemTooltipComponent : MonoBehaviour
{
	private class TooltipTextInfoProxy
	{
		private ItemTooltipTextInfo _tooltipTextInfo;

		public ItemTooltipTextInfo GetInfo(string itemCode, bool forceSetup)
		{
			if (_tooltipTextInfo == null || forceSetup)
			{
				if (String_Extension_Method.IsNullOrEmpty(itemCode))
				{
					return null;
				}
				ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(itemCode);
				if (dataByCode == null)
				{
					return null;
				}
				SetupTooltipText(dataByCode);
			}
			return _tooltipTextInfo;
		}

		public ItemTooltipTextInfo GetInfo(int index, bool forceSetup)
		{
			if (_tooltipTextInfo == null || forceSetup)
			{
				ItemData data = GameRoot.Instance.ItemDataContainer.GetData(index);
				if (data == null)
				{
					return null;
				}
				SetupTooltipText(data);
			}
			return _tooltipTextInfo;
		}

		private void SetupTooltipText(ItemData itemData)
		{
			if (_tooltipTextInfo == null)
			{
				_tooltipTextInfo = new ItemTooltipTextInfo();
			}
			_tooltipTextInfo.Reset();
			_tooltipTextInfo.Name.Value = itemData.Name;
			if (itemData.Type == ItemType.Equip)
			{
				SetupEquipItemTooltipText(itemData, null, null);
			}
			else
			{
				SetupItemTooltipText(itemData);
			}
		}

		private void SetupEquipItemTooltipText(ItemData itemData, EquipItemInfo equipItemInfo, CachedHeroInfo cachedHeroInfo)
		{
			EquipItemData subData = itemData.GetSubData<EquipItemData>();
			if (subData == null)
			{
				return;
			}
			_tooltipTextInfo.Name.Assign(new ItemNameBuilder(itemData, equipItemInfo, appendLine: false));
			_tooltipTextInfo.Desc1.Assign(new EquipItemDataBuilder(itemData, subData, equipItemInfo, cachedHeroInfo, EquipItemDataBuilder.EMethod.Desc1, appendLine: false));
			_tooltipTextInfo.Desc2.Assign(new EquipItemDataBuilder(itemData, subData, equipItemInfo, cachedHeroInfo, EquipItemDataBuilder.EMethod.ReqLevelAndPartType, appendLine: false));
			string creatureLimitString = subData.GetCreatureLimitString();
			if (!string.IsNullOrEmpty(creatureLimitString))
			{
				if (subData.SubType == EquipItemSubType.SpecialTreasure)
				{
					_tooltipTextInfo.CreatureLimit = LocalizationManager.GetString(Equip.EQUIP_CREATURE_LIMIT_TREASURE, creatureLimitString);
				}
				else
				{
					_tooltipTextInfo.CreatureLimit = LocalizationManager.GetString(Equip.EQUIP_CREATURE_LIMIT_WEAPON, creatureLimitString);
				}
			}
			else
			{
				_tooltipTextInfo.CreatureLimit = string.Empty;
			}
			if (GameRoot.Instance.EquipItemDataContainer.ContainsAllClassTag(subData.EquipCode))
			{
				_tooltipTextInfo.EquipAllClassTag = true;
			}
			else
			{
				for (int i = 0; i < subData.TagType.Length; i++)
				{
					_tooltipTextInfo.EquipClassTags.Add(subData.TagType[i]);
				}
			}
			if (!String_Extension_Method.IsNullOrEmpty(itemData.Description))
			{
				_tooltipTextInfo.Desc.Value = itemData.Description;
			}
		}

		private void SetupItemTooltipText(ItemData itemData)
		{
			_tooltipTextInfo.Name.Assign(new ItemNameBuilder(itemData, null, appendLine: false));
			switch (itemData.Type)
			{
			case ItemType.Recipe:
			case ItemType.Material:
				_tooltipTextInfo.TypeText.Assign(new EnumBuilder().Key(Item.ITEM_MATERIAL));
				break;
			case ItemType.UpgradeBook:
				_tooltipTextInfo.TypeText.Assign(new EnumBuilder().Key(Item.ITEM_UPGRADEBOOK));
				break;
			default:
				_tooltipTextInfo.TypeText.Reset();
				break;
			}
			if (!String_Extension_Method.IsNullOrEmpty(itemData.Description))
			{
				_tooltipTextInfo.Desc.Value = itemData.Description;
			}
			if (itemData.Type == ItemType.Potion)
			{
				_tooltipTextInfo.Desc1.Assign(new PotionItemDataBuilder(itemData, appendLine: false));
			}
			else if (itemData.Type == ItemType.Rune)
			{
				_tooltipTextInfo.Desc1.Assign(new RuneItemDataBuilder(itemData, null, RuneItemDataBuilder.EMethod.Desc1, appendLine: false));
			}
			else if (itemData.Type == ItemType.Flask)
			{
				_tooltipTextInfo.Desc1.Assign(new FlaskItemDataBuilder(itemData, appendLine: false));
			}
			else
			{
				_tooltipTextInfo.Desc1.Value = itemData.Description2;
			}
		}
	}

	public string ItemCode;

	public int ItemIndex;

	public UIWidget WidgetThis;

	private TooltipTextInfoProxy _tooltipTextProxy = new TooltipTextInfoProxy();

	public void Open(UIWidget widget, string itemCode, int itemIndex)
	{
		ItemCode = itemCode;
		ItemIndex = itemIndex;
		WidgetThis = widget;
	}

	public void OnShowTooltip(bool forceSetup = false)
	{
		ItemTooltipTextInfo info = _tooltipTextProxy.GetInfo(ItemCode, forceSetup);
		ItemIconTooltip.Open(WidgetThis, info);
	}

	public void OnShowTooltipByItemIndex(bool forceSetup = false)
	{
		ItemTooltipTextInfo info = _tooltipTextProxy.GetInfo(ItemIndex, forceSetup);
		ItemIconTooltip.Open(WidgetThis, info);
	}

	public void OnHideTooltip()
	{
		ItemIconTooltip.Hide(WidgetThis);
	}
}
