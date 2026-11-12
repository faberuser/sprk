using System;
using System.Collections.Generic;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class ClearPartyListComponent : LinkFactoryComponent, InitOnCreate, HeroPortraitComponent.Listener
{
	public interface IListener
	{
		void OnClickComponent(RecommendDeckInfo info, CachedHeroInfo[] heroInfos);
	}

	public const string PrefabPath = "Prefab2/Component/RecommendClearParty/ClearPartyListComponent";

	public static PrefabAuto<ClearPartyListComponent> PrefabAuto = new PrefabAuto<ClearPartyListComponent>("Prefab2/Component/RecommendClearParty/ClearPartyListComponent");

	public UISimpleGrid Grid_HeroIcon;

	public UILabel Label_Time;

	public GameObject EmptySlotTemplete;

	public UIResizeByGrid Resize_BG;

	public UIWidget Widget_BG;

	public int ExtraWidth;

	private IListener Listener;

	public List<CachedHeroInfo> CachedHeroInfos = new List<CachedHeroInfo>();

	private List<HeroPortraitComponent> PortraitComponents = new List<HeroPortraitComponent>();

	private List<UIWidget> EmptySlots = new List<UIWidget>();

	private List<UIWidget> UnusedEmptySlotPool = new List<UIWidget>();

	public int Width => ((Resize_BG != null) ? ((int)Resize_BG.GetSize().x) : 0) + ExtraWidth;

	public RecommendDeckInfo RecommendDeckInfo { get; private set; }

	public void Init(RecommendDeckInfo info, int deckCount, IListener listener)
	{
		RecommendDeckInfo = info;
		Listener = listener;
		CreateHeroIcons(info, deckCount);
		if (info != null)
		{
			SetTime(info.PlayTime);
		}
	}

	private void CreateHeroIcons(RecommendDeckInfo info, int deckCount)
	{
		ClearHeorIcons();
		for (int i = 0; i < deckCount; i++)
		{
			if (info != null && i < __Extension_Method_ICollection__.COUNT(info.Heroes))
			{
				ClearPartyHeroInfo clearPartyHeroInfo = info.Heroes[i];
				if (clearPartyHeroInfo != null && clearPartyHeroInfo.CreatureData != null)
				{
					HeroPortraitComponent heroPortraitComponent = UIHelper.AddGridItem(Grid_HeroIcon, HeroPortraitComponent.SmallPrefabAuto);
					heroPortraitComponent.Open(clearPartyHeroInfo.CreatureData, this, HeroPortraitComponent.EPortraitMode.Icon, clearPartyHeroInfo.Star, clearPartyHeroInfo.Transcended, clearPartyHeroInfo.CostumeIndex, clearPartyHeroInfo.Level);
					heroPortraitComponent.gameObject.SetActive(value: true);
					PortraitComponents.Add(heroPortraitComponent);
				}
				continue;
			}
			UIWidget uIWidget = null;
			if (__Extension_Method_ICollection__.COUNT(UnusedEmptySlotPool) == 0)
			{
				uIWidget = NGUITools.AddChild<UIWidget>(Grid_HeroIcon.Grid.gameObject, EmptySlotTemplete);
			}
			else
			{
				uIWidget = UnusedEmptySlotPool[0];
				uIWidget.transform.parent = Grid_HeroIcon.Grid.transform;
				UnusedEmptySlotPool.Remove(uIWidget);
			}
			UIHelper.EnableGameObject(uIWidget, enable: true);
			EmptySlots.Add(uIWidget);
		}
		UIHelper.RepositionGrid(Grid_HeroIcon);
		if ((bool)Resize_BG)
		{
			Resize_BG.Refresh();
		}
	}

	private void AddEmptySlot()
	{
	}

	private void SetTime(int time)
	{
		TimeSpan timeSpan = TimeSpan.FromSeconds(time);
		UIHelper.SetLabelText(Label_Time, $"{timeSpan.Minutes:00} : {timeSpan.Seconds:00}");
	}

	private void ClearHeorIcons()
	{
		FactoryBase.ReleaseAndClearList(PortraitComponents);
		ClearEmptySlot();
		CachedHeroInfos.Clear();
	}

	private void ClearEmptySlot()
	{
		foreach (UIWidget emptySlot in EmptySlots)
		{
			UIHelper.EnableGameObject(emptySlot, enable: false);
			UnusedEmptySlotPool.Add(emptySlot);
			emptySlot.transform.parent = Grid_HeroIcon.transform;
		}
		EmptySlots.Clear();
	}

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		ClearHeorIcons();
	}

	public void OnClickComponent()
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(CachedHeroInfos))
		{
			CreateCachedHeroInfos(RecommendDeckInfo);
		}
		if (Listener != null && RecommendDeckInfo != null)
		{
			Listener.OnClickComponent(RecommendDeckInfo, CachedHeroInfos.ToArray());
		}
	}

	private void CreateCachedHeroInfos(RecommendDeckInfo info)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(info.Heroes); i++)
		{
			CachedHeroInfo item = RecommendDeckInfo.CreateCachedHeroInfo(info.Heroes[i], info.EquipItems);
			CachedHeroInfos.Add(item);
		}
	}

	void HeroPortraitComponent.Listener.OnClick(HeroPortraitComponent heroPortrait)
	{
		OnClickComponent();
	}

	void HeroPortraitComponent.Listener.OnViewInfo(HeroPortraitComponent heroPortrait, bool isMine)
	{
	}

	void HeroPortraitComponent.Listener.OnLongClick(HeroPortraitComponent heroPortrait, CachedHeroInfo heroInfo)
	{
	}

	void HeroPortraitComponent.Listener.OnClickHeroNotify(HeroNotifyComponent.ENotifyInfo notifyInfo, CachedHeroInfo heroInfo)
	{
	}
}
