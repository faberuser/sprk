using System;
using System.Collections.Generic;
using NNguiExtension;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NEventCalendar;

public class EventCalendarDayComponent : MonoBehaviour, InitOnCreate
{
	public const string PrefabPath = "Resources/Prefab2/Component/EventCalendar/EventCalendarDayComponent";

	public static PrefabAuto<EventCalendarDayComponent> PrefabAuto = new PrefabAuto<EventCalendarDayComponent>("Resources/Prefab2/Component/EventCalendar/EventCalendarDayComponent");

	public UIWidget WidgetBackground;

	public UIWidget WidgetGrayBackground;

	public UILabel DayLabel;

	public UISpriteSelector DaySelector1;

	public UISpriteSelector DaySelector2;

	public Transform SpriteDayUnderbar1;

	public Transform SpriteDayUnderbar2;

	public Transform SelectedRoot;

	public UIBaseGrid RegularEventIconGrid;

	public UIBaseGrid OftenEventIconGrid;

	public Transform OccasionalEventIconRoot;

	public int OccasionalEventSpacing;

	public IList<EventCalendarData> _datas;

	private Action<EventCalendarDayComponent> _onClick;

	private List<RegularEventIconComponent> _regularIconComponentList = new List<RegularEventIconComponent>();

	private RegularEventIconComponent _emptyIconComponent;

	private List<OftenEventIconComponent> _oftenIconComponentList = new List<OftenEventIconComponent>();

	private List<OccasionalEventIconComponent> _occasionalIconComponentList = new List<OccasionalEventIconComponent>();

	public int Year { get; set; }

	public int Month { get; set; }

	public int Day { get; set; }

	public void Open(int year, int month, int day, IList<EventCalendarData> datas, Action<EventCalendarDayComponent> onClick)
	{
		Year = year;
		Month = month;
		Day = day;
		_datas = datas;
		_onClick = onClick;
		SetSelected(selected: false);
		SetDay(day);
		SetupEventList();
	}

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		_onClick = null;
		ClearIconComponent();
	}

	private void ClearIconComponent()
	{
		FactoryBase.ReleaseAndClearList(_regularIconComponentList);
		FactoryBase.ReleaseAndClearList(_oftenIconComponentList);
		FactoryBase.ReleaseAndClearList(_occasionalIconComponentList);
	}

	public void OnClickComponent()
	{
		if (_onClick != null)
		{
			Vespa_System_Action_Extension_Method.fire(_onClick, this);
		}
	}

	public void SetSelected(bool selected)
	{
		UIHelper.EnableGameObject(SelectedRoot, selected);
		if (Day < 10)
		{
			UIHelper.EnableGameObject(SpriteDayUnderbar1, selected);
			UIHelper.EnableGameObject(SpriteDayUnderbar2, enable: false);
		}
		else
		{
			UIHelper.EnableGameObject(SpriteDayUnderbar1, enable: false);
			UIHelper.EnableGameObject(SpriteDayUnderbar2, selected);
		}
	}

	public void SetDay(int day)
	{
		if (day < 10)
		{
			DaySelector1.SelectInt(day);
			UIHelper.EnableGameObject(DaySelector1, enable: true);
			UIHelper.EnableGameObject(DaySelector2, enable: false);
		}
		else
		{
			DaySelector1.SelectInt(day / 10);
			DaySelector2.SelectInt(day % 10);
			UIHelper.EnableGameObject(DaySelector1, enable: true);
			UIHelper.EnableGameObject(DaySelector2, enable: true);
		}
	}

	public void SetGrayScale(bool grayScale)
	{
		UIHelper.SetWidgetGrayScale(DayLabel, grayScale);
		UIHelper.EnableGameObject(WidgetBackground, !grayScale);
		UIHelper.EnableGameObject(WidgetGrayBackground, grayScale);
	}

	public void SetHighlight(int eventId = 0)
	{
		foreach (RegularEventIconComponent regularIconComponent in _regularIconComponentList)
		{
			regularIconComponent.SetHighlight(eventId);
		}
		foreach (OftenEventIconComponent oftenIconComponent in _oftenIconComponentList)
		{
			oftenIconComponent.SetHighlight(eventId);
		}
	}

	private void UpdateRegularIconListPosition()
	{
		RegularEventIconGrid.RepositionGrid();
	}

	private void UpdateOftenIconListPosition()
	{
		OftenEventIconGrid.RepositionGrid();
	}

	private void UpdateOccasionalIconListPosition()
	{
		int num = 0;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_occasionalIconComponentList); i++)
		{
			OccasionalEventIconComponent occasionalEventIconComponent = _occasionalIconComponentList[i];
			Vector3 localPosition = occasionalEventIconComponent.transform.localPosition;
			if (!Mathf.Approximately(localPosition.y, num))
			{
				Vector3 localPosition2 = new Vector3(localPosition.x, num, localPosition.z);
				occasionalEventIconComponent.transform.localPosition = localPosition2;
			}
			num += occasionalEventIconComponent.GetHeight();
			num += OccasionalEventSpacing;
		}
	}

	private void SetupEventList(int highlightEventId = 0)
	{
		ClearIconComponent();
		List<EventCalendarData> list = new List<EventCalendarData>();
		List<EventCalendarData> list2 = new List<EventCalendarData>();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_datas); i++)
		{
			EventCalendarData eventCalendarData = _datas[i];
			if (eventCalendarData.Regular != 0)
			{
				list.Add(eventCalendarData);
			}
			else
			{
				list2.Add(eventCalendarData);
			}
		}
		for (int j = 0; j < __Extension_Method_ICollection__.COUNT(list); j++)
		{
			EventCalendarData eventCalendarData2 = list[j];
			if (j >= 5)
			{
				if (__Extension_Method_ICollection__.COUNT(list) > 6)
				{
					if (j == 5)
					{
						AddRegularEventIcon(EventBonusType.None, 0, __Extension_Method_ICollection__.COUNT(list) - 5);
					}
					AddBundleRegularEventIcon(eventCalendarData2.BonusType, eventCalendarData2.Id);
				}
				else
				{
					AddRegularEventIcon(eventCalendarData2.BonusType, eventCalendarData2.Id);
				}
			}
			else
			{
				AddRegularEventIcon(eventCalendarData2.BonusType, eventCalendarData2.Id);
			}
		}
		for (int num = ((__Extension_Method_ICollection__.COUNT(list2) > 4) ? 4 : __Extension_Method_ICollection__.COUNT(list2)) - 1; num >= 0; num--)
		{
			EventCalendarData eventCalendarData3 = list2[num];
			AddOftenEventIcon(eventCalendarData3.BonusType, eventCalendarData3.Color, eventCalendarData3.Id);
		}
		UpdateRegularIconListPosition();
		UpdateOftenIconListPosition();
	}

	private void AddRegularEventIcon(EventBonusType bonusType, int eventId = 0, int count = 0)
	{
		if (RegularEventIconGrid == null)
		{
			return;
		}
		RegularEventIconComponent regularEventIconComponent = RegularEventIconGrid.AddGridItem(RegularEventIconComponent.PrefabAuto);
		if (!(regularEventIconComponent == null))
		{
			regularEventIconComponent.Open(bonusType, eventId, count);
			_regularIconComponentList.Add(regularEventIconComponent);
			if (bonusType == EventBonusType.None)
			{
				_emptyIconComponent = regularEventIconComponent;
			}
		}
	}

	private void AddBundleRegularEventIcon(EventBonusType bonusType, int eventId = 0)
	{
		if (!(_emptyIconComponent == null))
		{
			_emptyIconComponent.AddEvent(bonusType, eventId);
		}
	}

	private void AddOftenEventIcon(EventBonusType bonusType, string color, int eventId = 0)
	{
		if (!(OftenEventIconGrid == null))
		{
			OftenEventIconComponent oftenEventIconComponent = OftenEventIconGrid.AddGridItem(OftenEventIconComponent.PrefabAuto);
			if (!(oftenEventIconComponent == null))
			{
				oftenEventIconComponent.Open(bonusType, color, eventId);
				_oftenIconComponentList.Add(oftenEventIconComponent);
			}
		}
	}

	private void AddOccasionalEventIcon(EventCalendarData data)
	{
		OccasionalEventIconComponent occasionalEventIconComponent = OccasionalEventIconComponent.PrefabAuto.Factory.Create(OccasionalEventIconRoot);
		if (!(occasionalEventIconComponent == null))
		{
			EventCalendarLocalInfo localInfo = data.LocalInfo;
			bool isTodayStarted = Year == data.BeginYear && Month == data.BeginMonth && Day == data.BeginDay;
			occasionalEventIconComponent.Open(data, localInfo, isTodayStarted);
			_occasionalIconComponentList.Add(occasionalEventIconComponent);
		}
	}
}
