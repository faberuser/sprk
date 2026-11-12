using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NGame2.NUtil.NStringProxy;
using NShared;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class SkillBookHeroSelectPopup : BaseWindow, HeroPortraitComponent.Listener
{
	public interface Listener
	{
		void SelectHero(CachedHeroInfo heroInfo, int grade);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/MultipleDungeon/SkillBookHeroSelectPopup";

	public UILabel TitleLabel;

	public UILabel BookDescLabel;

	public UISimpleGrid ItemGrid;

	public UIScrollGrid HeroGrid;

	public UILabel EmptyHeroLabel;

	private List<Component> HeroPortraitList = new List<Component>();

	private List<Component> SkillBookHeroPortraitCompList = new List<Component>();

	private Component BookIcon;

	private Listener _Listener;

	public UpgradeBookItemData BookItemData { get; private set; }

	public void Open(UpgradeBookItemData bookItemData, Listener listener)
	{
		base.Open();
		HeroGrid.Grid.onCustomSort = Sort;
		if (bookItemData == null)
		{
			return;
		}
		_Listener = listener;
		BookItemData = bookItemData;
		ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(bookItemData.ItemCode);
		if (dataByCode == null)
		{
			return;
		}
		int itemCount = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.GetItemCount(dataByCode.Index);
		long num = itemCount;
		ItemIconComponent itemIconComponent = UIHelper.AddGridItem(ItemGrid, ItemIconComponent.PrefabAuto);
		if (itemIconComponent == null)
		{
			return;
		}
		UIHelper.RepositionGrid(ItemGrid);
		itemIconComponent.Open(dataByCode);
		itemIconComponent.SetItemCount(num);
		BookIcon = itemIconComponent;
		string itemName = ItemNameBuilder.GetItemName(dataByCode);
		UIHelper.SetLabelText(TitleLabel, itemName);
		string stringDirect = LocalizationManager.GetStringDirect("SELECT_HERO_USING_SKILLBOOK", dataByCode.Name);
		UIHelper.SetLabelText(BookDescLabel, stringDirect);
		CachedHeroInfo[] array = Array.FindAll(GlobalAutoBehaviour<AccountManager>.instance.GetHeroInfos(), (CachedHeroInfo info) => info.CreatureData.TagType == bookItemData.TagType);
		bool flag = __Extension_Method_ICollection__.COUNT(array) > 0;
		UIHelper.EnableGameObject(EmptyHeroLabel, !flag);
		if (flag)
		{
			CachedHeroInfo[] array2 = array;
			foreach (CachedHeroInfo heroInfo in array2)
			{
				HeroPortraitComponent heroPortraitComponent = UIHelper.AddGridItem(HeroGrid, HeroPortraitComponent.PrefabAuto);
				heroPortraitComponent.Open(heroInfo, this);
				heroPortraitComponent.ShowCreatureName();
				HeroPortraitList.Add(heroPortraitComponent);
				SkillBookHeroPortraitComponent skillBookHeroPortraitComponent = SkillBookHeroPortraitComponent.PrefabAuto.Factory.Create(heroPortraitComponent.transform);
				skillBookHeroPortraitComponent.Open(heroInfo, bookItemData.Grade, num);
				SkillBookHeroPortraitCompList.Add(skillBookHeroPortraitComponent);
			}
			UIHelper.RepositionGrid(HeroGrid);
		}
	}

	public override void Reset()
	{
		base.Reset();
		FactoryBase.ReleaseAndClearList(SkillBookHeroPortraitCompList);
		FactoryBase.ReleaseAndClearList(HeroPortraitList);
		FactoryBase.ReleaseComponent(BookIcon);
		BookIcon = null;
	}

	void HeroPortraitComponent.Listener.OnClick(HeroPortraitComponent heroPortrait)
	{
		if (_Listener != null)
		{
			_Listener.SelectHero(heroPortrait.HeroInfo, BookItemData.Grade);
		}
	}

	void HeroPortraitComponent.Listener.OnViewInfo(HeroPortraitComponent comp, bool IsMine)
	{
	}

	void HeroPortraitComponent.Listener.OnClickHeroNotify(HeroNotifyComponent.ENotifyInfo notifyInfo, CachedHeroInfo cachedHeroInfo)
	{
	}

	void HeroPortraitComponent.Listener.OnLongClick(HeroPortraitComponent heroPortrait, CachedHeroInfo heroInfo)
	{
	}

	private int Sort(Transform a, Transform b)
	{
		SkillBookHeroPortraitComponent componentInChildren = a.GetComponentInChildren<SkillBookHeroPortraitComponent>();
		SkillBookHeroPortraitComponent componentInChildren2 = b.GetComponentInChildren<SkillBookHeroPortraitComponent>();
		if (componentInChildren.Status != componentInChildren2.Status)
		{
			return componentInChildren.Status.CompareTo(componentInChildren2.Status);
		}
		HeroPortraitComponent componentInChildren3 = a.GetComponentInChildren<HeroPortraitComponent>();
		HeroPortraitComponent componentInChildren4 = b.GetComponentInChildren<HeroPortraitComponent>();
		if (componentInChildren3.HeroInfo.Level != componentInChildren4.HeroInfo.Level)
		{
			return -componentInChildren3.HeroInfo.Level.CompareTo(componentInChildren4.HeroInfo.Level);
		}
		if (componentInChildren3.HeroInfo.Star != componentInChildren4.HeroInfo.Star)
		{
			return -componentInChildren3.HeroInfo.Star.CompareTo(componentInChildren4.HeroInfo.Star);
		}
		if (componentInChildren3.HeroInfo.Transcended != componentInChildren4.HeroInfo.Transcended)
		{
			return -componentInChildren3.HeroInfo.Transcended.CompareTo(componentInChildren4.HeroInfo.Transcended);
		}
		return componentInChildren3.HeroInfo.HeroIndex.CompareTo(componentInChildren4.HeroInfo.HeroIndex);
	}
}
