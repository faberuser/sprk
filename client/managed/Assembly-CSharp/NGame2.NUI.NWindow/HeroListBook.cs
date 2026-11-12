using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class HeroListBook : BaseWindow, HeroContentsTabComponent.Listener, HeroPortraitComponent.Listener
{
	public interface Listener
	{
		void OnEnterHeroBook(CachedHeroInfo heroInfo);
	}

	[Serializable]
	public class ClassCategory
	{
		public HeroContentsTabComponent Component;

		public UIToggle Toggle;
	}

	public const string PrefabPath = "Resources/Prefab2/Window/IllustratedBook/HeroListBook";

	private Listener _listener;

	private List<HeroPortraitComponent> _heroPortraitList = new List<HeroPortraitComponent>();

	public List<ClassCategory> ClassCategoryList = new List<ClassCategory>();

	public UIScrollGrid TabGrid;

	public UIScrollGrid HeroGrid;

	public UISprite SpriteCollection;

	public UILabel LabelCollection;

	private CreatureTagType _currentTagType;

	public List<HeroPortraitComponent> HeroPortraitList => _heroPortraitList;

	public void Open(Listener listener)
	{
		_listener = listener;
		base.Open(delegate
		{
			Init();
		});
	}

	public void Init()
	{
		SetupClassComponent();
		SetupHeroPortrait();
	}

	public override void Reset()
	{
		FactoryBase.ReleaseAndClearList(_heroPortraitList);
		base.Reset();
	}

	private void SetupClassComponent()
	{
		foreach (ClassCategory classCategory in ClassCategoryList)
		{
			classCategory.Toggle.Set(state: false);
			classCategory.Component.Init(this);
		}
		ClassCategoryList[0].Toggle.Set(state: true);
	}

	private void SetupGauge(CreatureTagType creatureTagType)
	{
		int num = 0;
		foreach (CreatureData allDatum in GameRoot.Instance.CreatureDataContainer.GetAllData())
		{
			if (allDatum.OpenType != CreatureOpenType.None && allDatum.OpenType != CreatureOpenType.CommingSoon && (creatureTagType == CreatureTagType.None || allDatum.TagType == creatureTagType))
			{
				num++;
			}
		}
		int heroArchiveCount = GlobalAutoBehaviour<AccountManager>.instance.ArchiveManager.GetHeroArchiveCount(creatureTagType);
		SetupGauge(num, heroArchiveCount);
	}

	private void SetupGauge(int totalHeroCount, int curHeroCount)
	{
		if (curHeroCount >= totalHeroCount)
		{
			string text = $"{100:N2}% ";
			UIHelper.SetSpriteFillAmount(SpriteCollection, 1f);
			UIHelper.SetLabelText(LabelCollection, text);
		}
		else
		{
			float num = Mathf.Clamp((float)curHeroCount / (float)totalHeroCount, 0f, 100f);
			string text2 = $"{num * 100f:N2}%";
			UIHelper.SetLabelText(LabelCollection, text2);
			UIHelper.SetSpriteFillAmount(SpriteCollection, num);
		}
	}

	private void SetupHeroPortrait()
	{
		_currentTagType = CreatureTagType.None;
		int num = 0;
		int num2 = 0;
		foreach (CreatureData allDatum in GameRoot.Instance.CreatureDataContainer.GetAllData())
		{
			if (allDatum.OpenType != CreatureOpenType.None && allDatum.OpenType != CreatureOpenType.CommingSoon)
			{
				int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("HeroMaxStar");
				int maxLevel = GameRoot.Instance.CreatureLevelDataContainer.MaxLevel;
				int valueInt2 = GameRoot.Instance.ConstantDataContainer.GetValueInt("MaxSkillExtend");
				CachedHeroInfo cachedHeroInfo = new CachedHeroInfo(allDatum);
				cachedHeroInfo.Star = (sbyte)valueInt;
				cachedHeroInfo.Level = (short)maxLevel;
				for (int i = 1; i <= 5; i++)
				{
					cachedHeroInfo.SetSkillLevel(i, maxLevel);
					cachedHeroInfo.SetSkillExtend(i, valueInt2);
				}
				HeroPortraitComponent heroPortraitComponent = UIHelper.AddGridItem(HeroGrid, HeroPortraitComponent.PrefabAuto);
				heroPortraitComponent.Open(cachedHeroInfo, this);
				heroPortraitComponent.ShowStar(show: false);
				heroPortraitComponent.SetLevel(show: false);
				HeroProfileData data = GameRoot.Instance.HeroProfileDataContainer.GetData(cachedHeroInfo.HeroIndex);
				if (data != null && !String_Extension_Method.IsNullOrEmpty(data.HeroName))
				{
					heroPortraitComponent.ShowName(LocalizationManager.GetStringDirect(data.HeroName));
				}
				else
				{
					heroPortraitComponent.ShowName(allDatum.Name);
				}
				CachedHeroInfo heroInfo = GlobalAutoBehaviour<AccountManager>.instance.GetHeroInfo(allDatum.Index);
				bool flag = heroInfo != null;
				heroPortraitComponent.SetGrayScale(!flag);
				if (flag)
				{
					heroPortraitComponent.SetHeroImage(heroInfo.Star);
				}
				else
				{
					heroPortraitComponent.SetHeroImage(5);
				}
				_heroPortraitList.Add(heroPortraitComponent);
				num2++;
			}
		}
		UIHelper.RepositionGrid(HeroGrid);
		num = GlobalAutoBehaviour<AccountManager>.instance.ArchiveManager.GetHeroArchiveCount(_currentTagType);
		SetupGauge(num2, num);
	}

	public void OnArchiveTypeClick(CreatureTagType tagType)
	{
		if (tagType == _currentTagType)
		{
			return;
		}
		_currentTagType = tagType;
		foreach (HeroPortraitComponent heroPortrait in _heroPortraitList)
		{
			UIHelper.EnableGameObject(heroPortrait, tagType == CreatureTagType.None || heroPortrait.CreatureData.TagType == tagType);
		}
		UIHelper.RepositionGrid(HeroGrid);
		SetupGauge(_currentTagType);
	}

	public void OnClick(HeroPortraitComponent heroPortrait)
	{
		if (_listener != null)
		{
			_listener.OnEnterHeroBook(heroPortrait.HeroInfo);
		}
	}

	public void OnViewInfo(HeroPortraitComponent heroPortrait, bool isMine)
	{
	}

	public void OnClickHeroNotify(HeroNotifyComponent.ENotifyInfo notifyInfo, CachedHeroInfo heroInfo)
	{
	}

	public void OnLongClick(HeroPortraitComponent heroPortrait, CachedHeroInfo heroInfo)
	{
	}
}
