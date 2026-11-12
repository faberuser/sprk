using System.Collections.Generic;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class MailInfoAttached : MailInfo
{
	public new const string PrefabPath = "Resources/Prefab2/Window/Mail/MailInfoAttached";

	public UIScrollGrid AttachmentGrid;

	private List<Component> _itemIconList = new List<Component>();

	public override void Init(NShared.MailInfo mailInfo, bool isNew, Listener listener)
	{
		base.Init(mailInfo, isNew, listener);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(mailInfo.ItemInfos); i++)
		{
			MailItemInfo mailItemInfo = mailInfo.ItemInfos[i];
			ItemData data = GameRoot.Instance.ItemDataContainer.GetData(mailItemInfo.ItemIndex);
			if (data != null)
			{
				ItemIconComponent itemIconComponent = UIHelper.AddGridItem(AttachmentGrid, ItemIconComponent.PrefabAuto);
				if (!(itemIconComponent == null))
				{
					itemIconComponent.Open(data);
					int itemCount = mailItemInfo.ItemCount;
					Color? color = null;
					int num = itemCount;
					itemIconComponent.SetItemCount(num, color);
					_itemIconList.Add(itemIconComponent);
				}
			}
		}
		for (int j = 0; j < __Extension_Method_ICollection__.COUNT(mailInfo.EquipItemInfos); j++)
		{
			EquipItemInfo equipItemInfo = mailInfo.EquipItemInfos[j];
			ItemIconComponent itemIconComponent2 = UIHelper.AddGridItem(AttachmentGrid, ItemIconComponent.PrefabAuto);
			if (!(itemIconComponent2 == null))
			{
				if (!itemIconComponent2.Open(equipItemInfo, null))
				{
					FactoryBase.ReleaseComponent(itemIconComponent2);
				}
				else
				{
					_itemIconList.Add(itemIconComponent2);
				}
			}
		}
		for (int k = 0; k < __Extension_Method_ICollection__.COUNT(mailInfo.CurrencyInfos); k++)
		{
			MailCurrencyInfo mailCurrencyInfo = mailInfo.CurrencyInfos[k];
			CurrencyIconComponent currencyIconComponent = UIHelper.AddGridItem(AttachmentGrid, CurrencyIconComponent.PrefabAuto);
			if (!(currencyIconComponent == null) && mailCurrencyInfo != null && mailCurrencyInfo.Amount > 0)
			{
				CurrencyType currencyType = mailCurrencyInfo.CurrencyType;
				int amount = mailCurrencyInfo.Amount;
				CurrencyIconComponent.Listener listener2 = null;
				int num2 = amount;
				currencyIconComponent.Open(currencyType, num2, listener2);
				_itemIconList.Add(currencyIconComponent);
			}
		}
		for (int l = 0; l < __Extension_Method_ICollection__.COUNT(mailInfo.StaminaInfos); l++)
		{
			MailStaminaInfo mailStaminaInfo = mailInfo.StaminaInfos[l];
			CurrencyIconComponent currencyIconComponent2 = UIHelper.AddGridItem(AttachmentGrid, CurrencyIconComponent.PrefabAuto);
			if (!(currencyIconComponent2 == null) && mailStaminaInfo != null && mailStaminaInfo.Amount > 0)
			{
				CurrencyType staminaType = mailStaminaInfo.StaminaType;
				int amount2 = mailStaminaInfo.Amount;
				CurrencyIconComponent.Listener listener3 = null;
				int num3 = amount2;
				currencyIconComponent2.Open(staminaType, num3, listener3);
				_itemIconList.Add(currencyIconComponent2);
			}
		}
		if (mailInfo.SendGold > 0)
		{
			CurrencyIconComponent currencyIconComponent3 = UIHelper.AddGridItem(AttachmentGrid, CurrencyIconComponent.PrefabAuto);
			if (currencyIconComponent3 != null)
			{
				int sendGold = mailInfo.SendGold;
				CurrencyIconComponent.Listener listener4 = null;
				int num4 = sendGold;
				currencyIconComponent3.Open(CurrencyType.Gold, num4, listener4);
				_itemIconList.Add(currencyIconComponent3);
			}
		}
		if (mailInfo.SendGem > 0)
		{
			CurrencyIconComponent currencyIconComponent4 = UIHelper.AddGridItem(AttachmentGrid, CurrencyIconComponent.PrefabAuto);
			if (currencyIconComponent4 != null)
			{
				int sendGem = mailInfo.SendGem;
				CurrencyIconComponent.Listener listener5 = null;
				int num5 = sendGem;
				currencyIconComponent4.Open(CurrencyType.Gem, num5, listener5);
				_itemIconList.Add(currencyIconComponent4);
			}
		}
		if (mailInfo.PvpCoin > 0)
		{
			CurrencyIconComponent currencyIconComponent5 = UIHelper.AddGridItem(AttachmentGrid, CurrencyIconComponent.PrefabAuto);
			if (currencyIconComponent5 != null)
			{
				int pvpCoin = mailInfo.PvpCoin;
				CurrencyIconComponent.Listener listener6 = null;
				int num6 = pvpCoin;
				currencyIconComponent5.Open(CurrencyType.PvpCoin, num6, listener6);
				_itemIconList.Add(currencyIconComponent5);
			}
		}
		if (mailInfo.Mileage > 0)
		{
			CurrencyIconComponent currencyIconComponent6 = UIHelper.AddGridItem(AttachmentGrid, CurrencyIconComponent.PrefabAuto);
			if (currencyIconComponent6 != null)
			{
				int mileage = mailInfo.Mileage;
				CurrencyIconComponent.Listener listener7 = null;
				int num7 = mileage;
				currencyIconComponent6.Open(CurrencyType.Mileage, num7, listener7);
				_itemIconList.Add(currencyIconComponent6);
			}
		}
		if (mailInfo.LuaPoint > 0)
		{
			CurrencyIconComponent currencyIconComponent7 = UIHelper.AddGridItem(AttachmentGrid, CurrencyIconComponent.PrefabAuto);
			if (currencyIconComponent7 != null)
			{
				int luaPoint = mailInfo.LuaPoint;
				CurrencyIconComponent.Listener listener8 = null;
				int num8 = luaPoint;
				currencyIconComponent7.Open(CurrencyType.LuaPoint, num8, listener8);
				_itemIconList.Add(currencyIconComponent7);
			}
		}
		if (mailInfo.GuildPoint > 0)
		{
			CurrencyIconComponent currencyIconComponent8 = UIHelper.AddGridItem(AttachmentGrid, CurrencyIconComponent.PrefabAuto);
			if (currencyIconComponent8 != null)
			{
				int guildPoint = mailInfo.GuildPoint;
				CurrencyIconComponent.Listener listener9 = null;
				int num9 = guildPoint;
				currencyIconComponent8.Open(CurrencyType.GuildPoint, num9, listener9);
				_itemIconList.Add(currencyIconComponent8);
			}
		}
		if (mailInfo.WorldBossPoint > 0)
		{
			CurrencyIconComponent currencyIconComponent9 = UIHelper.AddGridItem(AttachmentGrid, CurrencyIconComponent.PrefabAuto);
			if (currencyIconComponent9 != null)
			{
				int worldBossPoint = mailInfo.WorldBossPoint;
				CurrencyIconComponent.Listener listener10 = null;
				int num10 = worldBossPoint;
				currencyIconComponent9.Open(CurrencyType.WorldBossPoint, num10, listener10);
				_itemIconList.Add(currencyIconComponent9);
			}
		}
		if (mailInfo.EventDungeonPoint > 0)
		{
			CurrencyIconComponent currencyIconComponent10 = UIHelper.AddGridItem(AttachmentGrid, CurrencyIconComponent.PrefabAuto);
			if (currencyIconComponent10 != null)
			{
				int eventDungeonPoint = mailInfo.EventDungeonPoint;
				CurrencyIconComponent.Listener listener11 = null;
				int num11 = eventDungeonPoint;
				currencyIconComponent10.Open(CurrencyType.EventDungeonPoint, num11, listener11);
				_itemIconList.Add(currencyIconComponent10);
			}
		}
		if (mailInfo.EventDungeonPoint2 > 0)
		{
			CurrencyIconComponent currencyIconComponent11 = UIHelper.AddGridItem(AttachmentGrid, CurrencyIconComponent.PrefabAuto);
			if (currencyIconComponent11 != null)
			{
				int eventDungeonPoint2 = mailInfo.EventDungeonPoint2;
				CurrencyIconComponent.Listener listener12 = null;
				int num12 = eventDungeonPoint2;
				currencyIconComponent11.Open(CurrencyType.EventDungeonPoint2, num12, listener12);
				_itemIconList.Add(currencyIconComponent11);
			}
		}
		if (mailInfo.GloryPoint > 0)
		{
			CurrencyIconComponent currencyIconComponent12 = UIHelper.AddGridItem(AttachmentGrid, CurrencyIconComponent.PrefabAuto);
			if (currencyIconComponent12 != null)
			{
				int gloryPoint = mailInfo.GloryPoint;
				CurrencyIconComponent.Listener listener13 = null;
				int num13 = gloryPoint;
				currencyIconComponent12.Open(CurrencyType.GloryPoint, num13, listener13);
				_itemIconList.Add(currencyIconComponent12);
			}
		}
		if (mailInfo.EventGiftPoint > 0)
		{
			CurrencyIconComponent currencyIconComponent13 = UIHelper.AddGridItem(AttachmentGrid, CurrencyIconComponent.PrefabAuto);
			if (currencyIconComponent13 != null)
			{
				int eventGiftPoint = mailInfo.EventGiftPoint;
				CurrencyIconComponent.Listener listener14 = null;
				int num14 = eventGiftPoint;
				currencyIconComponent13.Open(CurrencyType.EventGiftPoint, num14, listener14);
				_itemIconList.Add(currencyIconComponent13);
			}
		}
		if (mailInfo.SendChicken > 0)
		{
			CurrencyIconComponent currencyIconComponent14 = UIHelper.AddGridItem(AttachmentGrid, CurrencyIconComponent.PrefabAuto);
			if (currencyIconComponent14 != null)
			{
				int sendChicken = mailInfo.SendChicken;
				CurrencyIconComponent.Listener listener15 = null;
				int num15 = sendChicken;
				currencyIconComponent14.Open(CurrencyType.Stamina, num15, listener15);
				_itemIconList.Add(currencyIconComponent14);
			}
		}
		if (mailInfo.SendSword > 0)
		{
			CurrencyIconComponent currencyIconComponent15 = UIHelper.AddGridItem(AttachmentGrid, CurrencyIconComponent.PrefabAuto);
			if (currencyIconComponent15 != null)
			{
				int sendSword = mailInfo.SendSword;
				CurrencyIconComponent.Listener listener16 = null;
				int num16 = sendSword;
				currencyIconComponent15.Open(CurrencyType.Sword, num16, listener16);
				_itemIconList.Add(currencyIconComponent15);
			}
		}
		if (mailInfo.SendSword2 > 0)
		{
			CurrencyIconComponent currencyIconComponent16 = UIHelper.AddGridItem(AttachmentGrid, CurrencyIconComponent.PrefabAuto);
			if (currencyIconComponent16 != null)
			{
				int sendSword2 = mailInfo.SendSword2;
				CurrencyIconComponent.Listener listener17 = null;
				int num17 = sendSword2;
				currencyIconComponent16.Open(CurrencyType.Sword2, num17, listener17);
				_itemIconList.Add(currencyIconComponent16);
			}
		}
		if (mailInfo.SendUndergroundLabyrinthKey > 0)
		{
			CurrencyIconComponent currencyIconComponent17 = UIHelper.AddGridItem(AttachmentGrid, CurrencyIconComponent.PrefabAuto);
			if (currencyIconComponent17 != null)
			{
				int sendUndergroundLabyrinthKey = mailInfo.SendUndergroundLabyrinthKey;
				CurrencyIconComponent.Listener listener18 = null;
				int num18 = sendUndergroundLabyrinthKey;
				currencyIconComponent17.Open(CurrencyType.UndergroundLabyrinthKey, num18, listener18);
				_itemIconList.Add(currencyIconComponent17);
			}
		}
		if (mailInfo.SendHideoutKey > 0)
		{
			CurrencyIconComponent currencyIconComponent18 = UIHelper.AddGridItem(AttachmentGrid, CurrencyIconComponent.PrefabAuto);
			if (currencyIconComponent18 != null)
			{
				int sendHideoutKey = mailInfo.SendHideoutKey;
				CurrencyIconComponent.Listener listener19 = null;
				int num19 = sendHideoutKey;
				currencyIconComponent18.Open(CurrencyType.HideoutKey, num19, listener19);
				_itemIconList.Add(currencyIconComponent18);
			}
		}
		if (mailInfo.SendChallengeTowerKey > 0)
		{
			CurrencyIconComponent currencyIconComponent19 = UIHelper.AddGridItem(AttachmentGrid, CurrencyIconComponent.PrefabAuto);
			if (currencyIconComponent19 != null)
			{
				int sendChallengeTowerKey = mailInfo.SendChallengeTowerKey;
				CurrencyIconComponent.Listener listener20 = null;
				int num20 = sendChallengeTowerKey;
				currencyIconComponent19.Open(CurrencyType.ChallengeTowerKey, num20, listener20);
				_itemIconList.Add(currencyIconComponent19);
			}
		}
		if (mailInfo.SendMazeKey > 0)
		{
			CurrencyIconComponent currencyIconComponent20 = UIHelper.AddGridItem(AttachmentGrid, CurrencyIconComponent.PrefabAuto);
			if (currencyIconComponent20 != null)
			{
				int sendMazeKey = mailInfo.SendMazeKey;
				CurrencyIconComponent.Listener listener21 = null;
				int num21 = sendMazeKey;
				currencyIconComponent20.Open(CurrencyType.MazeKey, num21, listener21);
				_itemIconList.Add(currencyIconComponent20);
			}
		}
		AttachmentGrid.Grid.sorting = UIGrid.Sorting.Custom;
		AttachmentGrid.Grid.onCustomSort = SortItem;
		UIHelper.RepositionGrid(AttachmentGrid);
	}

	public override void Reset()
	{
		base.Reset();
		ClearIconList();
	}

	private void ClearIconList()
	{
		FactoryBase.ReleaseAndClearList(_itemIconList);
	}

	private int SortItem(ItemIconComponent a, ItemIconComponent b)
	{
		if (a.ItemData == null || b.ItemData == null)
		{
			if (a.ItemData != null)
			{
				return -1;
			}
			if (b.ItemData != null)
			{
				return 1;
			}
		}
		int num = 0;
		num = -a.ItemData.Grade.CompareTo(b.ItemData.Grade);
		if (num != 0)
		{
			return num;
		}
		int num2 = ((a.EquipItemInfo != null) ? a.EquipItemInfo.Level : 0);
		int num3 = ((b.EquipItemInfo != null) ? b.EquipItemInfo.Level : 0);
		num = -num2.CompareTo(num3);
		if (num != 0)
		{
			return num;
		}
		if (num2 > num3)
		{
			return -1;
		}
		int num4 = ((a.EquipItemInfo != null) ? a.EquipItemInfo.ItemIndex : a.ItemIndex);
		int value = ((b.EquipItemInfo != null) ? b.EquipItemInfo.ItemIndex : b.ItemIndex);
		return num4.CompareTo(value);
	}

	private int SortItem(Transform a, Transform b)
	{
		ItemIconComponent component = a.GetComponent<ItemIconComponent>();
		ItemIconComponent component2 = b.GetComponent<ItemIconComponent>();
		if (component == null || component2 == null)
		{
			if (component != null)
			{
				return -1;
			}
			if (component2 != null)
			{
				return 1;
			}
			return 0;
		}
		return SortItem(component, component2);
	}
}
