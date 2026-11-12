using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NGame2.NAccount;
using NGame2.NCutScene;
using NGame2.NUI.NComponent2.NMonthlyHero;
using NGame2.NUI.NWindow.NBattle;
using NGame2.NUI.NWindow.NMonthlyHero;
using NGameAsset;
using NNguiExtension;
using NShared;
using NShared.CompleteMonthlyHero;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class MonthlyHeroWindow : BaseWindow, CalendarComponent.IListener, MonthlyHeroComponent.IListener
{
	public interface IListener
	{
	}

	public const string PrefabPath = "Resources/Prefab2/Window/MonthlyHero/MonthlyHeroWindow";

	private const string ZodiacPath = "Portrait/MonthlyHero/Zodiac_";

	private const int CalendarLine5 = 35;

	private const int CalendarLine6 = 42;

	private const int ContentOpenYear = 2019;

	public UIScrollGrid GridCalendar;

	public UIScrollGrid GridHeroList;

	public UIScrollGrid GridRewards;

	private Dictionary<MonthlyDataMonthDayPair, List<MonthlyDataWrapper>> _monthDayPairDataListDict = new Dictionary<MonthlyDataMonthDayPair, List<MonthlyDataWrapper>>();

	private Dictionary<int, List<MonthlyDataWrapper>> _monthDataListDict = new Dictionary<int, List<MonthlyDataWrapper>>();

	private IListener _listener;

	private List<CalendarComponent> _calendarComponentList = new List<CalendarComponent>();

	private List<MonthlyHeroComponent> _heroPortraitList = new List<MonthlyHeroComponent>();

	private List<Component> _rewardComponentList = new List<Component>();

	public UISpriteSelector SpriteSelectorMonth;

	public Transform TransformUnrelease;

	public Transform TransformInvalidMonth;

	public UILabel LabelYear;

	public UILabel LabelMonth;

	public UILabel LabelMonthLocale;

	public UILabel LabelTitle;

	public UILabel LabelDesc;

	public UILabel LabelServerTime;

	public UIButton ButtonMonthLeft;

	public UIButton ButtonMonthRight;

	public UIButton ButtonPlayCutScene;

	public Transform TransformPlayableEffect;

	public Transform TransformPlayWaitEffect;

	public UIBundleLoader BundleTextureHero;

	public UIBundleLoader BundleGuardian;

	public UIBundleLoader BundleBackGround;

	public float OffsetHeroGrid;

	private const int PreMonth = -11;

	private const int NextMonth = 1;

	private CalendalDate _calendalDate;

	private MonthlyDataWrapper _selectedMonthlyData;

	private const float UpdateIntervalSec = 30f;

	private float _lastUpdateTimeSec;

	private DateTime _lastUpdateServerTime;

	private bool _needRefresh;

	private AssetTexture _backGroundPreload;

	private float _backGroundLoadTimeOut = 2f;

	private NGame2.NAccount.UserMonthlyHeroManager MonthlyHeroManager => GlobalAutoBehaviour<AccountManager>.instance.MonthlyHeroManager;

	private void OnChangeMonth(int creatureIndex)
	{
		UIHelper.EnableGameObject(TransformUnrelease, IsUnreleaseMonth());
		CreateCalendar();
		MonthlyDataWrapper selectedDefault = GetDefaultHeroOfMonth(creatureIndex);
		bool enable = !IsUnreleaseMonth() && selectedDefault.IsNull();
		UIHelper.EnableGameObject(TransformInvalidMonth, enable);
		MonthlyHeroComponent comp = _heroPortraitList.Find((MonthlyHeroComponent component) => component.MonthlyDataWrapper.Equals(selectedDefault));
		GridHeroList.ScrollTo(comp, OffsetHeroGrid, 0f);
		OnSelectHero(selectedDefault);
	}

	private bool IsUnreleaseMonth()
	{
		DateTime dateTime = _calendalDate.GetDateTime();
		_ = dateTime.Month;
		DateTime nowDateTime = GetNowDateTime();
		return new DateTime(nowDateTime.Year, nowDateTime.Month, 1) < new DateTime(dateTime.Year, dateTime.Month, 1);
	}

	public void Open(IListener listener, int creatureIndex)
	{
		_backGroundPreload = new AssetTexture();
		_backGroundPreload.Open(BundleBackGround.AssetPath);
		Func<bool> loadDone = () => _backGroundPreload.IsDone;
		Action onComplete = delegate
		{
			OnOpen(listener, creatureIndex);
		};
		VespaCoroutine.Start(WaitBackGroundLoad(loadDone, _backGroundLoadTimeOut), onComplete);
	}

	private IEnumerator WaitBackGroundLoad(Func<bool> loadDone, float timeOut)
	{
		float started = Time.time;
		while (!Vespa_System_Action_Extension_Method.fire(loadDone) && Time.time - started < timeOut)
		{
			yield return null;
		}
	}

	private void OnOpen(IListener listener, int creatureIndex)
	{
		base.Open();
		_listener = listener;
		_lastUpdateServerTime = GetNowDateTime();
		MonthlyHeroManager.MothlyHeroWindowVisited = true;
		CollectDataList();
		CreateCalendarDate(creatureIndex);
		UpdateServerTime();
		OnChangeMonth(creatureIndex);
		SetSelectCretureMonth(creatureIndex);
	}

	private void SetSelectCretureMonth(int creatureIndex)
	{
		if (creatureIndex > 0)
		{
			MonthlyDataWrapper defaultHeroOfMonth = GetDefaultHeroOfMonth(creatureIndex);
			if (!defaultHeroOfMonth.IsNull())
			{
				_calendalDate.SetDate(defaultHeroOfMonth.OpenDate);
			}
		}
	}

	private void CreateCalendarDate(int creatureIndex)
	{
		DateTime nowDateTime = GetNowDateTime();
		_calendalDate = new CalendalDate(nowDateTime, delegate
		{
			OnChangeMonth(creatureIndex);
		});
		DateTime minimum = nowDateTime.AddMonths(GetMinMonth(nowDateTime));
		_calendalDate.SetMinimum(minimum);
		DateTime maximum = nowDateTime.AddMonths(1);
		_calendalDate.SetMaximum(maximum);
	}

	private int GetMinMonth(DateTime current)
	{
		if (current.Year != GetContentsOpenYear())
		{
			return -11;
		}
		return 1 - current.Month;
	}

	private void SetCalendarBound(DateTime current)
	{
		DateTime minimum = current.AddMonths(GetMinMonth(current));
		_calendalDate.SetMinimum(minimum);
		DateTime maximum = current.AddMonths(1);
		_calendalDate.SetMaximum(maximum);
	}

	private int GetContentsOpenYear()
	{
		return GameRoot.Instance.ConstantDataContainer.GetValueInt("MothlyHeroOpenYear", 2019);
	}

	private DateTime GetNowDateTime()
	{
		return GlobalAutoBehaviour<AccountManager>.instance.MonthlyHeroManager.GetCurrentLocalTime();
	}

	private MonthlyDataWrapper GetDefaultHeroOfMonth(int creatureIndex)
	{
		if (creatureIndex > -1)
		{
			List<MonthlyDataWrapper> list = null;
			MonthlyDataWrapper monthlyDataWrapper = MonthlyDataWrapper.Null;
			for (int i = 0; i < _monthDataListDict.Values.Count; i++)
			{
				list = _monthDataListDict.Values.ToList()[i];
				if (__Extension_Method_ICollection__.IsNullOrEmpty(list))
				{
					continue;
				}
				for (int j = 0; j < list.Count; j++)
				{
					monthlyDataWrapper = list[j];
					if (CheckEqualHero(monthlyDataWrapper, creatureIndex))
					{
						return monthlyDataWrapper;
					}
				}
			}
		}
		List<MonthlyDataWrapper> heroListOfMonth = GetHeroListOfMonth();
		if (__Extension_Method_ICollection__.COUNT(heroListOfMonth) == 0)
		{
			return MonthlyDataWrapper.Null;
		}
		DateTime nowDateTime = GetNowDateTime();
		for (int k = 0; k < __Extension_Method_ICollection__.COUNT(heroListOfMonth); k++)
		{
			DateTime openDate = heroListOfMonth[k].OpenDate;
			if (CheckEqualHero(heroListOfMonth[k], creatureIndex))
			{
				return heroListOfMonth[k];
			}
			if (IsEqualDay(nowDateTime, openDate.Year, openDate.Month, openDate.Day))
			{
				return heroListOfMonth[k];
			}
		}
		return heroListOfMonth[0];
	}

	private List<MonthlyDataWrapper> GetHeroListOfMonth()
	{
		int month = _calendalDate.GetDateTime().Month;
		if (!_monthDataListDict.ContainsKey(month))
		{
			return null;
		}
		return CreateFilterDataList(_monthDataListDict[month]);
	}

	private List<MonthlyDataWrapper> CreateFilterDataList(List<MonthlyDataWrapper> dataList)
	{
		List<MonthlyDataWrapper> list = new List<MonthlyDataWrapper>(dataList);
		list.RemoveAll(Filter);
		return list;
	}

	private bool Filter(MonthlyDataWrapper data)
	{
		return CheckValid(data) | CheckUnreleased(data);
	}

	private bool CheckValid(MonthlyDataWrapper data)
	{
		DateTime dateTime = _calendalDate.GetDateTime();
		int year = dateTime.Year;
		_ = dateTime.Month;
		return data.OpenDate.Year > year;
	}

	private bool CheckUnreleased(MonthlyDataWrapper data)
	{
		return IsUnreleaseMonth();
	}

	private void CollectDataList()
	{
		foreach (MonthlyHeroData allDatum in GameRoot.Instance.MonthlyHeroDataContainer.GetAllData())
		{
			DateTime openDateTime = allDatum.GetOpenDateTime();
			int month = openDateTime.Month;
			int day = openDateTime.Day;
			MonthlyDataWrapper item = new MonthlyDataWrapper
			{
				Data = allDatum,
				OpenDate = openDateTime,
				CurrentTimeFunc = GetNowDateTime
			};
			if (!_monthDataListDict.ContainsKey(month))
			{
				_monthDataListDict[month] = new List<MonthlyDataWrapper>();
			}
			_monthDataListDict[month].Add(item);
			MonthlyDataMonthDayPair key = new MonthlyDataMonthDayPair(month, day);
			if (!_monthDayPairDataListDict.ContainsKey(key))
			{
				_monthDayPairDataListDict[key] = new List<MonthlyDataWrapper>();
			}
			_monthDayPairDataListDict[key].Add(item);
		}
	}

	void CalendarComponent.IListener.OnClick(CalendarComponent caller)
	{
		if (__Extension_Method_ICollection__.COUNT(caller.RelatedDataList) != 0 && !caller.DataKey.IsNull())
		{
			DateTime nowDateTime = GetNowDateTime();
			DateTime checkDate = new DateTime(_calendalDate.GetDateTime().Year, caller.DataKey.Month, caller.DataKey.Day);
			string remainTimeMessage = GetRemainTimeMessage(nowDateTime, checkDate);
			if (!String_Extension_Method.IsNullOrEmpty(remainTimeMessage))
			{
				CenterMessage.Open(remainTimeMessage);
			}
		}
	}

	bool CalendarComponent.IListener.IsRewardable(int monthlyHerodataIndex)
	{
		return !IsRewarded(monthlyHerodataIndex);
	}

	private string GetRemainTimeMessage(DateTime now, DateTime checkDate)
	{
		TimeSpan timeSpan = checkDate - now;
		if (timeSpan.TotalSeconds < 0.0)
		{
			return string.Empty;
		}
		int days = timeSpan.Days;
		int num = timeSpan.Hours;
		if (days == 0 && num == 0)
		{
			num = 1;
		}
		return LocalizationManager.GetStringDirect("MONTHLY_HERO_OPENDATE", days, num);
	}

	List<MonthlyDataWrapper> CalendarComponent.IListener.GetDataList(MonthlyDataMonthDayPair key)
	{
		if (!_monthDayPairDataListDict.ContainsKey(key))
		{
			return null;
		}
		return CreateFilterDataList(_monthDayPairDataListDict[key]);
	}

	private void ClearDataWrapperDict()
	{
		Dictionary<MonthlyDataMonthDayPair, List<MonthlyDataWrapper>>.Enumerator enumerator = _monthDayPairDataListDict.GetEnumerator();
		while (enumerator.MoveNext())
		{
			enumerator.Current.Value.Clear();
		}
		_monthDayPairDataListDict.Clear();
		Dictionary<int, List<MonthlyDataWrapper>>.Enumerator enumerator2 = _monthDataListDict.GetEnumerator();
		while (enumerator2.MoveNext())
		{
			enumerator2.Current.Value.Clear();
		}
		_monthDataListDict.Clear();
	}

	private void CreateHeroPortraits()
	{
		ClearHeroPortrait();
		List<MonthlyDataWrapper> heroListOfMonth = GetHeroListOfMonth();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(heroListOfMonth); i++)
		{
			MonthlyHeroComponent monthlyHeroComponent = UIHelper.AddGridItem(GridHeroList, MonthlyHeroComponent.PrefabAuto);
			monthlyHeroComponent.Open(this, heroListOfMonth[i]);
			_heroPortraitList.Add(monthlyHeroComponent);
		}
		GridHeroList.RepositionGrid();
	}

	private void ClearHeroPortrait()
	{
		FactoryBase.ReleaseAndClearList(_heroPortraitList);
	}

	private void SetLabelYearMonth()
	{
		int year = _calendalDate.GetDateTime().Year;
		int month = _calendalDate.GetDateTime().Month;
		UIHelper.SetLabelText(LabelYear, year.ToString());
		UIHelper.SetLabelText(LabelMonth, month.ToString());
		UIHelper.SetLabelText(LabelMonthLocale, _calendalDate.GetLocale());
		SpriteSelectorMonth.Select(month.ToString());
	}

	private void SetMonthGuardian()
	{
		string guardianPath = GetGuardianPath();
		UIHelper.SetBundleTexture(BundleGuardian, guardianPath);
	}

	private string GetGuardianPath()
	{
		return "Portrait/MonthlyHero/Zodiac_" + _calendalDate.GetDateTime().Month;
	}

	private void CreateCalendar()
	{
		int year = _calendalDate.GetDateTime().Year;
		int month = _calendalDate.GetDateTime().Month;
		SetLabelYearMonth();
		SetMonthGuardian();
		CreateCalender(year, month);
		CreateHeroPortraits();
	}

	private void CreateCalender(int year, int month)
	{
		GetCalenderInfo(year, month, out var daysInMonth, out var offset);
		int num = ((offset + daysInMonth > 35) ? 42 : 35);
		bool flag = __Extension_Method_ICollection__.COUNT(_calendarComponentList) != num;
		if (flag)
		{
			ClearCalendarComponents();
		}
		int num2 = 0;
		CalendarComponent comp = null;
		DateTime nowDateTime = GetNowDateTime();
		int num3 = 1;
		for (int i = 0; i < num; i++)
		{
			CalendarComponent calendarComponent = (flag ? GridCalendar.AddGridItem(CalendarComponent.PrefabAuto) : _calendarComponentList[i]);
			if (num3 <= daysInMonth && i >= offset)
			{
				bool flag2 = IsEqualDay(nowDateTime, year, month, num3);
				if (flag2)
				{
					comp = calendarComponent;
					num2 = i;
				}
				MonthlyDataMonthDayPair dataKey = new MonthlyDataMonthDayPair(month, num3);
				calendarComponent.Open(this, dataKey);
				calendarComponent.SetSelect(flag2);
				num3++;
			}
			else
			{
				calendarComponent.Open(this, MonthlyDataMonthDayPair.Null);
				calendarComponent.SetSelect(isSelect: false);
			}
			if (flag)
			{
				_calendarComponentList.Add(calendarComponent);
			}
		}
		GridCalendar.RepositionGrid();
		if (num2 >= 35)
		{
			GridCalendar.ScrollTo(comp, 0f, 0f);
		}
	}

	private bool IsEqualDay(DateTime l, int rYear, int rMonth, int rDay)
	{
		if (l.Year == rYear && l.Month == rMonth)
		{
			return l.Day == rDay;
		}
		return false;
	}

	private bool CheckEqualHero(MonthlyDataWrapper monthlyDataWrapper, int creatureIndex)
	{
		if (monthlyDataWrapper.IsNull())
		{
			return false;
		}
		if (monthlyDataWrapper.Data == null)
		{
			return false;
		}
		return monthlyDataWrapper.Data.HeroIndex == creatureIndex;
	}

	private void ClearCalendarComponents()
	{
		FactoryBase.ReleaseAndClearList(_calendarComponentList);
	}

	private void GetCalenderInfo(int year, int month, out int daysInMonth, out int offset)
	{
		daysInMonth = DateTime.DaysInMonth(year, month);
		offset = (int)new DateTime(year, month, 1).DayOfWeek;
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		if (!(Time.time - _lastUpdateTimeSec < 30f))
		{
			UpdateServerTime();
			CheckRefresh();
		}
	}

	private void CheckRefresh()
	{
		DateTime nowDateTime = GetNowDateTime();
		if (_lastUpdateServerTime.Day != nowDateTime.Day)
		{
			_lastUpdateServerTime = nowDateTime;
			_needRefresh = true;
		}
		if (_needRefresh)
		{
			SetCalendarBound(nowDateTime);
			CreateCalendar();
			Refresh();
		}
	}

	private void UpdateServerTime()
	{
		_lastUpdateTimeSec = Time.time;
		DateTime currentServerLocalTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerLocalTime();
		int serverUTCOffsetHour = GlobalAutoBehaviour<AccountManager>.instance.GetServerUTCOffsetHour();
		string text = ((serverUTCOffsetHour > 0) ? "+" : "-");
		string text2 = $"{currentServerLocalTime.Year}/{currentServerLocalTime.Month:00}/{currentServerLocalTime.Day:00} {currentServerLocalTime.Hour:00}:{currentServerLocalTime.Minute:00} UTC{text}{System.Math.Abs(serverUTCOffsetHour)}";
		UIHelper.SetLabelText(LabelServerTime, text2);
	}

	public override void Reset()
	{
		base.Reset();
		ReleseBackGroundPreload();
		ResetMemberVariables();
		ClearHeroPortrait();
		ClearCalendarComponents();
		ClearRewardComponents();
		ClearDataWrapperDict();
	}

	private void ReleseBackGroundPreload()
	{
		if (_backGroundPreload != null)
		{
			_backGroundPreload.Close();
			_backGroundPreload = null;
		}
	}

	private void ResetMemberVariables()
	{
		_calendalDate = null;
		_selectedMonthlyData = MonthlyDataWrapper.Null;
		UIHelper.EnableGameObject(BundleTextureHero, enable: false);
		UIHelper.SetLabelText(LabelTitle, string.Empty);
		UIHelper.SetLabelText(LabelDesc, string.Empty);
		SetCutScenePlayButton();
	}

	public void OnClickMonthInc()
	{
		_calendalDate.AddMonth(1);
	}

	public void OnClickMonthDec()
	{
		_calendalDate.AddMonth(-1);
	}

	public void OnClickPlayCutScene()
	{
		if (!_selectedMonthlyData.IsNull())
		{
			if (!_selectedMonthlyData.IsOpen())
			{
				CenterMessage.Open(GetRemainTimeMessage(GetNowDateTime(), _selectedMonthlyData.OpenDate));
			}
			else if (IsRewarded(_selectedMonthlyData.Data.Index))
			{
				SceneAutoBehaviour<CutSceneManager>.instance.Open(_selectedMonthlyData.Data.CutScenePath, canSkip: true, null);
			}
			else
			{
				RequestRewardAndPlayCutScene();
			}
		}
	}

	private void RequestRewardAndPlayCutScene()
	{
		int index = _selectedMonthlyData.Data.Index;
		string cutscenePath = _selectedMonthlyData.Data.CutScenePath;
		NShared.CompleteMonthlyHero.Request request = new NShared.CompleteMonthlyHero.Request();
		request.MonthlyHeroIndex = index;
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.CompleteMonthlyHero.Response response)
		{
			if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
			}
			else if (response.Result != ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				SceneAutoBehaviour<CutSceneManager>.instance.Open(cutscenePath, canSkip: true, delegate
				{
					ApplyResponse(response);
					ShowRewardPopup(response);
					Refresh();
					RefreshRewardableTag();
				}, OnCompleteLoad);
			}
		});
	}

	private void ApplyResponse(NShared.CompleteMonthlyHero.Response response)
	{
		MonthlyHeroManager.Apply(response.Info);
		GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.CurrencyResults);
		GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.ItemResults);
		GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Apply(response.EquipItemResults);
		GlobalAutoBehaviour<AccountManager>.instance.ApplyAddHero(response.HeroInfos, response.TeamExpResultInfos);
		GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.StaminaResultInfos);
		GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.Apply(response.DungeonInfos);
		GlobalAutoBehaviour<AccountManager>.instance.FreeEquipGachaManager.Set(response.FreeEquipGachaInfo);
		GlobalAutoBehaviour<AccountManager>.instance.EquipGachaManager.Set(response.EquipGachaInfo);
	}

	private void ShowRewardPopup(NShared.CompleteMonthlyHero.Response response)
	{
		BaseRewardPopup.OpenPopup(new BaseRewardPopup.PopupOpenData().SetTitle(LocalizationManager.GetStringDirect("MONTHLY_HERO_GET_REWARD")).SetCurrencyResultInfos3(response.CurrencyResults).SetItemResultInfos(response.ItemResults)
			.SetEquipResultInfos(response.EquipItemResults)
			.SetStaminaResultInfos(response.StaminaResultInfos), isGlobal: false);
	}

	private void StartCutSceneAndRequest()
	{
		string cutScenePath = _selectedMonthlyData.Data.CutScenePath;
		int dataIndex = _selectedMonthlyData.Data.Index;
		SceneAutoBehaviour<CutSceneManager>.instance.Open(cutScenePath, canSkip: true, delegate
		{
			RequestReward(dataIndex);
		}, OnCompleteLoad);
	}

	private void RequestReward(int dataIndex)
	{
		if (!IsRewarded(dataIndex))
		{
			RequestCutSceneReward(dataIndex);
		}
	}

	private void RequestCutSceneReward(int dataIndex)
	{
		NShared.CompleteMonthlyHero.Request request = new NShared.CompleteMonthlyHero.Request();
		request.MonthlyHeroIndex = dataIndex;
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.CompleteMonthlyHero.Response response)
		{
			if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
			}
			else if (response.Result != ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				ApplyResponse(response);
				ShowRewardPopup(response);
				Refresh();
				RefreshRewardableTag();
			}
		});
	}

	private void OnCompleteLoad()
	{
		VespaDebug.LogError("OnCompleteLoad");
	}

	private void OnSelectHero(MonthlyDataWrapper selectedData)
	{
		if (!_selectedMonthlyData.Equals(selectedData))
		{
			_selectedMonthlyData = selectedData;
			Refresh();
		}
	}

	private void Refresh()
	{
		_needRefresh = false;
		SetHeroTexture();
		SetDesc();
		SetRewardList();
		SetCutScenePlayButton();
	}

	private void RefreshRewardableTag()
	{
		foreach (CalendarComponent calendarComponent in _calendarComponentList)
		{
			calendarComponent.SetRewardableNew();
		}
		foreach (MonthlyHeroComponent heroPortrait in _heroPortraitList)
		{
			heroPortrait.SetRewardableTag();
		}
	}

	private void SetHeroTexture()
	{
		UIHelper.EnableGameObject(BundleTextureHero, !_selectedMonthlyData.IsNull());
		if (!_selectedMonthlyData.IsNull())
		{
			string illustPath = _selectedMonthlyData.Data.IllustPath;
			UIHelper.SetBundleTexture(BundleTextureHero, illustPath);
			UIHelper.SetBundleTextureGrayscale(BundleTextureHero, !_selectedMonthlyData.IsOpen());
		}
	}

	private void SetDesc()
	{
		if (_selectedMonthlyData.IsNull())
		{
			UIHelper.SetLabelText(LabelTitle, string.Empty);
			UIHelper.SetLabelText(LabelDesc, string.Empty);
			return;
		}
		string empty = string.Empty;
		string empty2 = string.Empty;
		if (!_selectedMonthlyData.IsOpen())
		{
			empty = LocalizationManager.GetStringDirect("MONTHLY_HERO_NOT_ACTIVATE_TITLE");
			empty2 = GetRemainTimeMessage(GetNowDateTime(), _selectedMonthlyData.OpenDate);
		}
		else
		{
			empty = LocalizationManager.GetStringDirect(_selectedMonthlyData.Data.Title);
			empty2 = LocalizationManager.GetStringDirect(_selectedMonthlyData.Data.Desc);
		}
		UIHelper.SetLabelText(LabelTitle, empty);
		UIHelper.SetLabelText(LabelDesc, empty2);
	}

	private void SetRewardList()
	{
		ClearRewardComponents();
		if (_selectedMonthlyData.IsNull())
		{
			return;
		}
		bool alreadyRewarded = MonthlyHeroManager.Get(_selectedMonthlyData.Data.Index) != null;
		foreach (RewardElementData reward in GetRewardList())
		{
			if (reward != null)
			{
				RewardComponent rewardComponent = UIHelper.AddGridItem(GridRewards, RewardComponent.PrefabAuto);
				rewardComponent.Open(reward, alreadyRewarded);
				_rewardComponentList.Add(rewardComponent);
			}
		}
		GridRewards.RepositionGrid();
	}

	private List<RewardElementData> GetRewardList()
	{
		int rewardIndex = _selectedMonthlyData.Data.RewardIndex;
		int GroupCount;
		return GameRoot.Instance.RewardDataContainer.GetData(rewardIndex)?.GetRewardItemListWithGroupCount(out GroupCount);
	}

	private void ClearRewardComponents()
	{
		FactoryBase.ReleaseAndClearList(_rewardComponentList);
	}

	private void SetCutScenePlayButton()
	{
		UIHelper.EnableButton(ButtonPlayCutScene, !_selectedMonthlyData.IsNull(), grayscaleIfDisable: true);
		if (_selectedMonthlyData.IsNull())
		{
			UIHelper.EnableGameObject(TransformPlayableEffect, enable: false);
			UIHelper.EnableGameObject(TransformPlayWaitEffect, enable: false);
		}
		else
		{
			bool flag = _selectedMonthlyData.IsOpen();
			UIHelper.EnableGameObject(TransformPlayableEffect, flag && !IsRewarded(_selectedMonthlyData.Data.Index));
			UIHelper.EnableGameObject(TransformPlayWaitEffect, !flag);
		}
	}

	private bool IsRewarded(int index)
	{
		return MonthlyHeroManager.Get(index) != null;
	}

	void MonthlyHeroComponent.IListener.OnClick(MonthlyHeroComponent caller)
	{
		OnSelectHero(caller.MonthlyDataWrapper);
	}

	bool MonthlyHeroComponent.IListener.IsRewardable(int monthlyHeroDataIndex)
	{
		return !IsRewarded(monthlyHeroDataIndex);
	}
}
