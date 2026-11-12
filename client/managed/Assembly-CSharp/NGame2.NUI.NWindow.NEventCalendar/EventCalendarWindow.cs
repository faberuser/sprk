using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NComponent2.NEventCalendar;
using NShared;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow.NEventCalendar;

public class EventCalendarWindow : BaseWindow
{
	public interface Listener
	{
	}

	public const string PrefabPath = "Resources/Prefab2/Window/EventCalendar/EventCalendarWindow";

	public UIBaseGrid CalendarDayGrid;

	public UIBaseGrid EventRegularGrid;

	public UIScrollGrid EventOftenGrid;

	public UILabel LabelTitle;

	public UILabel LabelRegularTitle;

	public UILabel LabelOftenTitle;

	public int OftenEventSpacing;

	public UIButton Button_Prev;

	public UIButton Button_Next;

	public UILabel Lable_ServerTime;

	private int _year;

	private int _month;

	private List<EventCalendarData> _eventDatas;

	private Listener _listener;

	private List<EventCalendarDayComponent> _dayComponentList = new List<EventCalendarDayComponent>();

	private List<EventCalendarRegularComponent> _regularComponentList = new List<EventCalendarRegularComponent>();

	private List<EventCalendarOftenComponent> _oftenComponentList = new List<EventCalendarOftenComponent>();

	public void Open(IList<EventCalendarData> datas, Listener listener)
	{
		base.Open();
		bool flag = GlobalAutoBehaviour<AccountManager>.instance.AdminLevel >= 1;
		_eventDatas = new List<EventCalendarData>();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(datas); i++)
		{
			EventCalendarData eventCalendarData = datas[i];
			if (eventCalendarData.Show != 0 || flag)
			{
				_eventDatas.Add(eventCalendarData);
			}
		}
		_listener = listener;
		SortEventCalendar();
		SetupTodayCalendar();
	}

	public override void Reset()
	{
		base.Reset();
		_eventDatas = null;
		_listener = null;
		ClearDayComponentList();
		ClearRegularComponentList();
		ClearOftenComponentList();
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		UpdateOftenEventListPosition();
		UpdateServerTime();
	}

	private void ClearDayComponentList()
	{
		FactoryBase.ReleaseAndClearList(_dayComponentList);
	}

	private void ClearRegularComponentList()
	{
		FactoryBase.ReleaseAndClearList(_regularComponentList);
	}

	private void ClearOftenComponentList()
	{
		FactoryBase.ReleaseAndClearList(_oftenComponentList);
	}

	private void SetupTodayCalendar()
	{
		DateTime currentServerLocalTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerLocalTime();
		_year = currentServerLocalTime.Year;
		_month = currentServerLocalTime.Month;
		SetupCalendar();
	}

	private string GetCalendarMonth(int month)
	{
		string text = LocalizationManager.GetString((CalendarMonth)month);
		string stringDirect = LocalizationManager.GetStringDirect("CONTENT_EVENT");
		return text + " " + stringDirect;
	}

	private void SortEventCalendar()
	{
		_eventDatas.Sort(delegate(EventCalendarData l, EventCalendarData r)
		{
			int num = r.BeginYear.CompareTo(l.BeginYear);
			if (num != 0)
			{
				return num;
			}
			num = r.BeginMonth.CompareTo(l.BeginMonth);
			return (num != 0) ? num : r.BeginDay.CompareTo(l.BeginDay);
		});
	}

	private void SetupCalendar()
	{
		string calendarMonth = GetCalendarMonth(_month);
		UIHelper.SetLabelText(LabelTitle, calendarMonth);
		DateTime dateTime = new DateTime(_year, _month, 1);
		int num = 0 - dateTime.DayOfWeek;
		DateTime dateTime2 = dateTime.AddDays(num);
		DateTime currentServerLocalTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerLocalTime();
		bool flag = _year == currentServerLocalTime.Year && _month == currentServerLocalTime.Month;
		EventCalendarDayComponent eventCalendarDayComponent = null;
		ClearDayComponentList();
		int num2 = 0;
		while (true)
		{
			DateTime dateTime3 = dateTime2.AddDays(num2);
			if (dateTime3.DayOfWeek == DayOfWeek.Sunday && dateTime3.Month != _month && num2 > 0)
			{
				break;
			}
			EventCalendarDayComponent eventCalendarDayComponent2 = CalendarDayGrid.AddGridItem(EventCalendarDayComponent.PrefabAuto);
			if (eventCalendarDayComponent2 == null)
			{
				VespaDebug.LogError("Failed to create component of EventCalendarDayComponent");
				break;
			}
			List<EventCalendarData> list = new List<EventCalendarData>();
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_eventDatas); i++)
			{
				EventCalendarData eventCalendarData = _eventDatas[i];
				if (eventCalendarData.BeginYear > dateTime3.Year || (eventCalendarData.BeginYear == dateTime3.Year && eventCalendarData.BeginMonth > dateTime3.Month) || (eventCalendarData.BeginYear == dateTime3.Year && eventCalendarData.BeginMonth == dateTime3.Month && eventCalendarData.BeginDay > dateTime3.Day) || eventCalendarData.EndYear < dateTime3.Year || (eventCalendarData.EndYear == dateTime3.Year && eventCalendarData.EndMonth < dateTime3.Month) || (eventCalendarData.EndYear == dateTime3.Year && eventCalendarData.EndMonth == dateTime3.Month && eventCalendarData.EndDay < dateTime3.Day))
				{
					continue;
				}
				if (eventCalendarData.Regular == 1)
				{
					if (eventCalendarData.DayOfWeek == null || eventCalendarData.DayOfWeek.Length <= 0)
					{
						continue;
					}
					string[] array = eventCalendarData.DayOfWeek.Split(',');
					List<int> list2 = new List<int>();
					string[] array2 = array;
					foreach (string text in array2)
					{
						if (text.Length > 0)
						{
							int num3 = Convert.ToInt32(text);
							num3++;
							list2.Add(num3);
						}
					}
					int calendarDayOfWeek = (int)(dateTime3.DayOfWeek + 1);
					if (list2.Find((int day) => day == calendarDayOfWeek) == 0)
					{
						continue;
					}
				}
				list.Add(eventCalendarData);
			}
			bool flag2 = dateTime3.Month == _month;
			eventCalendarDayComponent2.SetGrayScale(!flag2);
			eventCalendarDayComponent2.Open(dateTime3.Year, dateTime3.Month, dateTime3.Day, list, OnClickCalendarDay);
			if (flag)
			{
				if (dateTime3.Year == currentServerLocalTime.Year && dateTime3.Month == currentServerLocalTime.Month && dateTime3.Day == currentServerLocalTime.Day)
				{
					eventCalendarDayComponent = eventCalendarDayComponent2;
					SetupEventInfo(eventCalendarDayComponent);
				}
			}
			else if (dateTime3.Day == 1 && flag2 && dateTime3.Month == _month)
			{
				eventCalendarDayComponent = eventCalendarDayComponent2;
				SetupEventInfo(eventCalendarDayComponent2);
			}
			_dayComponentList.Add(eventCalendarDayComponent2);
			num2++;
		}
		CalendarDayGrid.RepositionGrid();
		if (eventCalendarDayComponent != null)
		{
			eventCalendarDayComponent.SetSelected(selected: true);
			CalendarDayGrid.ScrollTo(eventCalendarDayComponent, 0f, 0f);
		}
		SetMoveMonthButton(_year, _month);
	}

	private void SetupCalendarHighlight(int eventId = 0)
	{
		foreach (EventCalendarDayComponent dayComponent in _dayComponentList)
		{
			dayComponent.SetHighlight(eventId);
		}
	}

	private void SetupEventInfo(EventCalendarDayComponent dayComponent)
	{
		if (dayComponent == null)
		{
			return;
		}
		foreach (EventCalendarDayComponent dayComponent2 in _dayComponentList)
		{
			if (!(dayComponent2 == null))
			{
				bool selected = dayComponent2 == dayComponent;
				dayComponent2.SetSelected(selected);
			}
		}
		string text = LocalizationManager.GetString(EventCalendar.EVENT_REGULAR_TITLE);
		string text2 = LocalizationManager.GetString(EventCalendar.EVENT_OFTEN_TITLE);
		if (dayComponent == null)
		{
			UIHelper.SetLabelText(LabelRegularTitle, text);
			UIHelper.SetLabelText(LabelOftenTitle, text2);
			return;
		}
		IList<EventCalendarData> datas = dayComponent._datas;
		int num = 0;
		int num2 = 0;
		ClearRegularComponentList();
		ClearOftenComponentList();
		foreach (EventCalendarData item in datas)
		{
			if (item.Regular == 1)
			{
				EventCalendarRegularComponent eventCalendarRegularComponent = EventRegularGrid.AddGridItem(EventCalendarRegularComponent.PrefabAuto);
				if (eventCalendarRegularComponent == null)
				{
					VespaDebug.LogError("Failed to create component of EventCalendarRegularComponent");
				}
				eventCalendarRegularComponent.Open(item, OnPressRegularEvent, OnReleaseRegularEvent);
				_regularComponentList.Add(eventCalendarRegularComponent);
				num++;
			}
			else
			{
				EventCalendarOftenComponent eventCalendarOftenComponent = EventOftenGrid.AddGridItem(EventCalendarOftenComponent.PrefabAuto);
				if (eventCalendarOftenComponent == null)
				{
					VespaDebug.LogError("Failed to create component of EventCalendarOftenComponent");
				}
				eventCalendarOftenComponent.Open(item, OnPressOftenEvent, OnReleaseOftenEvent, OnClickEventOftenDetail);
				_oftenComponentList.Add(eventCalendarOftenComponent);
				num2++;
			}
		}
		EventRegularGrid.RepositionGrid();
		UpdateOftenEventListPosition();
		EventOftenGrid.ScrollView.InvalidateBounds();
		EventOftenGrid.RepositionGrid();
		if (num > 0)
		{
			text = text + " " + LocalizationManager.GetString(NShared.NLocalization.Common.PARENTHESES_NUMBER, num);
		}
		if (num2 > 0)
		{
			text2 = text2 + " " + LocalizationManager.GetString(NShared.NLocalization.Common.PARENTHESES_NUMBER, num2);
		}
		UIHelper.SetLabelText(LabelRegularTitle, text);
		UIHelper.SetLabelText(LabelOftenTitle, text2);
	}

	private void UpdateOftenEventListPosition()
	{
		int num = 0;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_oftenComponentList); i++)
		{
			EventCalendarOftenComponent eventCalendarOftenComponent = _oftenComponentList[i];
			Vector3 localPosition = eventCalendarOftenComponent.transform.localPosition;
			if (!Mathf.Approximately(localPosition.y, num))
			{
				Vector3 localPosition2 = new Vector3(localPosition.x, num, localPosition.z);
				eventCalendarOftenComponent.transform.localPosition = localPosition2;
			}
			num -= eventCalendarOftenComponent.GetHeight();
			num -= OftenEventSpacing;
		}
	}

	private void SetMoveMonthButton(int year, int month)
	{
		DateTime dateTime = new DateTime(year, month, 1);
		DateTime dateTime2 = dateTime.AddMonths(-1);
		DateTime dateTime3 = dateTime.AddMonths(1);
		bool enable = false;
		bool enable2 = false;
		if (dateTime2.Year != DateTime.Now.Year)
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_eventDatas); i++)
			{
				EventCalendarData eventCalendarData = _eventDatas[i];
				if (eventCalendarData.BeginYear <= dateTime2.Year && (eventCalendarData.BeginYear != dateTime2.Year || eventCalendarData.BeginMonth <= dateTime2.Month) && eventCalendarData.EndYear >= dateTime2.Year && (eventCalendarData.EndYear != dateTime2.Year || eventCalendarData.EndMonth >= dateTime2.Month))
				{
					enable = true;
					break;
				}
			}
		}
		else
		{
			enable = true;
		}
		if (year == DateTime.Now.Year && month < DateTime.Now.Month)
		{
			enable2 = true;
		}
		else
		{
			for (int j = 0; j < __Extension_Method_ICollection__.COUNT(_eventDatas); j++)
			{
				EventCalendarData eventCalendarData2 = _eventDatas[j];
				if (eventCalendarData2.BeginYear <= dateTime3.Year && (eventCalendarData2.BeginYear != dateTime3.Year || eventCalendarData2.BeginMonth <= dateTime3.Month) && eventCalendarData2.EndYear >= dateTime3.Year && (eventCalendarData2.EndYear != dateTime3.Year || eventCalendarData2.EndMonth >= dateTime3.Month))
				{
					enable2 = true;
					break;
				}
			}
		}
		UIHelper.EnableButton(Button_Prev, enable, grayscaleIfDisable: true);
		UIHelper.EnableButton(Button_Next, enable2, grayscaleIfDisable: true);
	}

	public void OnClickCloseButton()
	{
		Reset();
		Close();
	}

	public void OnClickPrevButton()
	{
		DateTime dateTime = new DateTime(_year, _month, 1).AddMonths(-1);
		_year = dateTime.Year;
		_month = dateTime.Month;
		SetupCalendar();
	}

	public void OnClickNextButton()
	{
		DateTime dateTime = new DateTime(_year, _month, 1).AddMonths(1);
		_year = dateTime.Year;
		_month = dateTime.Month;
		SetupCalendar();
	}

	private void OnClickCalendarDay(EventCalendarDayComponent dayComponent)
	{
		if (!(dayComponent == null))
		{
			SetupEventInfo(dayComponent);
		}
	}

	private void OnPressRegularEvent(EventCalendarRegularComponent regularComponent)
	{
		if (!(regularComponent == null))
		{
			SetupCalendarHighlight(regularComponent.EventId);
		}
	}

	private void OnReleaseRegularEvent(EventCalendarRegularComponent regularComponent)
	{
		if (!(regularComponent == null))
		{
			SetupCalendarHighlight();
		}
	}

	private void OnPressOftenEvent(EventCalendarOftenComponent oftenComponent)
	{
		if (!(oftenComponent == null))
		{
			SetupCalendarHighlight(oftenComponent.EventId);
		}
	}

	private void OnReleaseOftenEvent(EventCalendarOftenComponent oftenComponent)
	{
		if (!(oftenComponent == null))
		{
			SetupCalendarHighlight();
		}
	}

	private void OnClickEventOftenDetail(string desc, EventCalendarOftenComponent oftenComponent)
	{
		if (!(oftenComponent == null))
		{
			oftenComponent.SetDesc(desc);
			UpdateOftenEventListPosition();
			EventOftenGrid.ScrollView.InvalidateBounds();
			EventOftenGrid.RepositionGrid();
		}
	}

	private void UpdateServerTime()
	{
		DateTime currentServerLocalTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerLocalTime();
		string text = $"{currentServerLocalTime.Year}/{currentServerLocalTime.Month:00}/{currentServerLocalTime.Day:00} {currentServerLocalTime.Hour:00}:{currentServerLocalTime.Minute:00}";
		UIHelper.SetLabelText(Lable_ServerTime, text);
	}
}
