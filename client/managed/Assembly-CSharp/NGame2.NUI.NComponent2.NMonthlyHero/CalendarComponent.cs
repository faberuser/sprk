using System.Collections.Generic;
using NGame2.NUI.NWindow.NMonthlyHero;
using NNguiExtension;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NMonthlyHero;

public class CalendarComponent : LinkFactoryComponent, InitOnCreate
{
	public interface IListener
	{
		bool IsRewardable(int monthlyHerodataIndex);

		void OnClick(CalendarComponent caller);

		List<MonthlyDataWrapper> GetDataList(MonthlyDataMonthDayPair key);
	}

	public const string PrefabPath = "Prefab2/Component/MonthlyHero/CalendarComponent";

	public static PrefabAuto<CalendarComponent> PrefabAuto = new PrefabAuto<CalendarComponent>("Prefab2/Component/MonthlyHero/CalendarComponent");

	public UISpriteSelector SpriteSelectorBirthday;

	public UISpriteSelector SpriteSelectorAnniversary;

	public UISimpleGrid HeroPortraitGrid;

	public UILabel LabelDate;

	public UISprite SpriteBg;

	public UISprite SpriteSelect;

	private List<HeroPortraitTextureName> _heroPortraitList = new List<HeroPortraitTextureName>();

	private IListener _listener;

	public Transform TransformNew;

	public Color ActiveColor;

	public Color DeactiveColor;

	public MonthlyDataMonthDayPair DataKey { get; private set; }

	public List<MonthlyDataWrapper> RelatedDataList { get; private set; }

	public void Open(IListener listener, MonthlyDataMonthDayPair dataKey)
	{
		_listener = listener;
		DataKey = dataKey;
		Reset();
		Init();
	}

	private void Reset()
	{
		if (RelatedDataList != null)
		{
			RelatedDataList = null;
		}
		FactoryBase.ReleaseAndClearList(_heroPortraitList);
	}

	private void Init()
	{
		SetDataList();
		SetDate();
		SetType();
		SetHeroPortrait();
		SetRewardableNew();
	}

	public void SetSelect(bool isSelect)
	{
		UIHelper.EnableGameObject(SpriteSelect, isSelect);
	}

	private void SetDataList()
	{
		RelatedDataList = _listener.GetDataList(DataKey);
	}

	private void SetDate()
	{
		if (DataKey.Equals(MonthlyDataMonthDayPair.Null))
		{
			UIHelper.SetWidgetColor(SpriteBg, DeactiveColor);
			UIHelper.SetLabelText(LabelDate, string.Empty);
		}
		else
		{
			UIHelper.SetWidgetColor(SpriteBg, ActiveColor);
			UIHelper.SetLabelText(LabelDate, DataKey.Day.ToString());
		}
	}

	private void SetType()
	{
		UIHelper.EnableGameObject(SpriteSelectorBirthday, IsExistType(CalendarCutSceneType.BirthDay));
		UIHelper.EnableGameObject(SpriteSelectorAnniversary, IsExistType(CalendarCutSceneType.Anniversary));
	}

	private bool IsExistType(CalendarCutSceneType checkType)
	{
		if (__Extension_Method_ICollection__.COUNT(RelatedDataList) == 0)
		{
			return false;
		}
		return RelatedDataList.FindIndex((MonthlyDataWrapper d) => d.Data.Type == checkType) != -1;
	}

	private void SetHeroPortrait()
	{
		if (__Extension_Method_ICollection__.COUNT(RelatedDataList) != 0)
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(RelatedDataList); i++)
			{
				int heroIndex = RelatedDataList[i].Data.HeroIndex;
				HeroPortraitTextureName heroPortraitTextureName = UIHelper.AddGridItem(HeroPortraitGrid, HeroPortraitTextureName.PrefabAuto);
				heroPortraitTextureName.Open(heroIndex, 5, 0, printName: false);
				_heroPortraitList.Add(heroPortraitTextureName);
			}
			HeroPortraitGrid.RepositionGrid();
		}
	}

	public void SetRewardableNew()
	{
		UIHelper.EnableGameObject(TransformNew, IsRewardable());
	}

	public bool IsRewardable()
	{
		if (__Extension_Method_ICollection__.COUNT(RelatedDataList) == 0)
		{
			return false;
		}
		foreach (MonthlyDataWrapper relatedData in RelatedDataList)
		{
			bool num = relatedData.IsOpen();
			bool flag = _listener.IsRewardable(relatedData.Data.Index);
			if (num && flag)
			{
				return true;
			}
		}
		return false;
	}

	public void OnClick()
	{
		_listener.OnClick(this);
	}

	public void OnCreate()
	{
	}

	public void OnRelease()
	{
		Reset();
	}
}
