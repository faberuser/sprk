using System.Collections.Generic;
using NGame2.NAccount;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NHero;

public class HeroPortraitListPanel : MonoBehaviour, IHeroPanel
{
	public UIScrollGrid HeroListGrid;

	public UIButton TotalViewButton;

	private List<HeroPortraitComponent> _heroCompList = new List<HeroPortraitComponent>();

	private IDataProvider _dataProvider;

	private HeroPortraitComponent.Listener _listener;

	public int HeroStar { get; private set; }

	public int HeroTranscended { get; private set; }

	public int HeroLevel { get; private set; }

	public void Open(IDataProvider dataProvider, HeroPortraitComponent.Listener listener, int heroStar, int heroLevel, int heroTrans)
	{
		Reset();
		base.gameObject.SetActive(value: true);
		_dataProvider = dataProvider;
		_listener = listener;
		HeroStar = heroStar;
		HeroLevel = heroLevel;
		HeroTranscended = heroTrans;
		Init();
	}

	public void Close()
	{
		SetTotalViewButton(onOff: true);
		Reset();
	}

	private void Init()
	{
		SetHeroList();
	}

	public void Reset()
	{
		_listener = null;
		_dataProvider = null;
		ClearHeroList();
	}

	private void SetHeroList()
	{
		ClearHeroList();
		List<CreatureData> heroDataList = _dataProvider.GetHeroDataList();
		HeroPortraitComponent heroPortraitComponent = null;
		bool level = HeroLevel > 1;
		SkillAttrType recommandedType = _dataProvider.GetRecommandedType();
		foreach (CreatureData item in heroDataList)
		{
			if (item != null)
			{
				HeroPortraitComponent heroPortraitComponent2 = UIHelper.AddGridItem(HeroListGrid, HeroPortraitComponent.SmallPrefabAuto);
				bool isRecommanded = recommandedType == item.HeroAttrType;
				heroPortraitComponent2.Open(item, _listener, HeroPortraitComponent.EPortraitMode.Icon, HeroStar, 0, 0, HeroLevel, isRecommanded);
				heroPortraitComponent2.SetLevel(level);
				if (_dataProvider.GetSelectedHeroData() == item)
				{
					heroPortraitComponent = heroPortraitComponent2;
					heroPortraitComponent2.SetSelectSmall(select: true);
				}
				if (GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(item.Index) != null)
				{
					heroPortraitComponent2.SetOwned(owned: true);
				}
				_heroCompList.Add(heroPortraitComponent2);
			}
		}
		UIHelper.RepositionGrid(HeroListGrid);
		if (heroPortraitComponent != null)
		{
			MoveList(heroPortraitComponent);
		}
	}

	public void SetSelectionComp(CreatureData oldHero, CreatureData seletHero)
	{
		List<CreatureData> heroDataList = _dataProvider.GetHeroDataList();
		if (oldHero != seletHero && heroDataList.Contains(oldHero))
		{
			int index = _heroCompList.FindIndex((HeroPortraitComponent value) => value.CreatureData == oldHero);
			if (Vespa_Generic_IList_Extension_Method.IsValidIndex(_heroCompList, index))
			{
				_heroCompList[index].SetSelectSmall(select: false);
			}
		}
		if (heroDataList.Contains(seletHero))
		{
			int index2 = _heroCompList.FindIndex((HeroPortraitComponent value) => value.CreatureData == seletHero);
			if (Vespa_Generic_IList_Extension_Method.IsValidIndex(_heroCompList, index2))
			{
				_heroCompList[index2].SetSelectSmall(select: true);
				_heroCompList[index2].SetNew(show: false);
				MoveList(_heroCompList[index2]);
			}
		}
	}

	public void SetTotalViewButton(bool onOff)
	{
		UIHelper.EnableGameObject(TotalViewButton, onOff);
	}

	private void MoveList(HeroPortraitComponent heroComp, float scrollStrength = 13f)
	{
		int num = __Extension_Method_ICollection__.COUNT(_heroCompList);
		if (num <= 5)
		{
			return;
		}
		int num2 = _heroCompList.IndexOf(heroComp);
		if (num2 != -1)
		{
			Transform cachedTransform = HeroListGrid.ScrollView.panel.cachedTransform;
			Vector3[] worldCorners = HeroListGrid.ScrollView.panel.worldCorners;
			Vector3 vector2;
			if (num2 <= 1)
			{
				Vector3 vector = cachedTransform.InverseTransformPoint(worldCorners[2]);
				vector2 = cachedTransform.InverseTransformPoint(_heroCompList[0].transform.position) - vector;
				vector2.y += HeroListGrid.Grid.cellHeight / 2f;
			}
			else if (num2 >= num - 2)
			{
				Vector3 vector3 = cachedTransform.InverseTransformPoint(worldCorners[0]);
				vector2 = cachedTransform.InverseTransformPoint(_heroCompList[num - 1].transform.position) - vector3;
				vector2.y -= HeroListGrid.Grid.cellHeight / 2f;
			}
			else
			{
				Vector3 position = (worldCorners[2] + worldCorners[0]) * 0.5f;
				Vector3 vector4 = cachedTransform.InverseTransformPoint(heroComp.transform.position);
				Vector3 vector5 = cachedTransform.InverseTransformPoint(position);
				vector2 = vector4 - vector5;
			}
			if (!HeroListGrid.ScrollView.canMoveHorizontally)
			{
				vector2.x = 0f;
			}
			if (!HeroListGrid.ScrollView.canMoveVertically)
			{
				vector2.y = 0f;
			}
			vector2.z = 0f;
			Vector3 pos = cachedTransform.localPosition - vector2;
			SpringPanel.Begin(HeroListGrid.ScrollView.panel.cachedGameObject, pos, scrollStrength).onFinished = delegate
			{
				HeroListGrid.ScrollView.RestrictWithinBounds(instant: true);
			};
		}
	}

	private void ClearHeroList()
	{
		FactoryBase.ReleaseAndClearList(_heroCompList);
	}
}
