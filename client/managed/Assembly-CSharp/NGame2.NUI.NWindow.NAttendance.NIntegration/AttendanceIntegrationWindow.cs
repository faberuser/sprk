using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NContentsDefine;
using NGame2.NContentsInfo;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NComponent2.NGuildAttendance;
using NGame2.NUI3.NAttendance;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow.NAttendance.NIntegration;

public class AttendanceIntegrationWindow : BaseWindow, AttendanceIntegrationTabComponent.IListener, IUserObserver<AttendanceInfo>, IUserObserver
{
	[Serializable]
	public class TabViewUi
	{
		public AttendanceIntegrationWindow Owner;

		public UIScrollGrid TabGrid;

		private List<AttendanceIntegrationTabComponent> _tabList = new List<AttendanceIntegrationTabComponent>();

		public void InitUi(List<AttendanceKeywordGroup> attendanceGroups)
		{
			AddAttendanceRewardWindows(attendanceGroups);
			AddAccumlateLoginWindow();
			AddGuildAttendanceTab();
			SelectDefaultTab();
			UIHelper.RepositionGrid(TabGrid);
		}

		public void DestroyAll()
		{
			FactoryBase.ReleaseAndClearList(_tabList);
		}

		private void AddAttendanceRewardWindows(List<AttendanceKeywordGroup> attendanceGroups)
		{
			foreach (AttendanceKeywordGroup attendanceGroup in attendanceGroups)
			{
				if (!Attendance.TryGetRemainTime(attendanceGroup.AttendanceInfo, out var remainTimeSpan) || !(remainTimeSpan <= TimeSpan.Zero))
				{
					bool flag = attendanceGroup.IsPurchasable_v1();
					AttendanceIntegrationTabComponent attendanceIntegrationTabComponent = UIHelper.AddGridItem(TabGrid, AttendanceIntegrationTabComponent.PrefabAuto);
					attendanceIntegrationTabComponent.OpenWithAttendanceData(Owner, attendanceGroup.KeywordMain.ToString(), attendanceGroup);
					attendanceIntegrationTabComponent.SetMenuTag(flag ? AttendanceIntegrationTabComponent.MenuTagType.Purchaseable : AttendanceIntegrationTabComponent.MenuTagType.Default);
					_tabList.Add(attendanceIntegrationTabComponent);
				}
			}
		}

		private void AddAccumlateLoginWindow()
		{
			AttendanceIntegrationTabComponent attendanceIntegrationTabComponent = UIHelper.AddGridItem(TabGrid, AttendanceIntegrationTabComponent.PrefabAuto);
			attendanceIntegrationTabComponent.Open(Owner, AccumulateLogin.AccumlateLoginTypeName, AccumulateLogin.AccumlateLoginTabName);
			attendanceIntegrationTabComponent.SetMenuTag(AttendanceIntegrationTabComponent.MenuTagType.Default);
			_tabList.Add(attendanceIntegrationTabComponent);
		}

		private void AddGuildAttendanceTab()
		{
			if (!(TabGrid == null))
			{
				AttendanceIntegrationTabComponent attendanceIntegrationTabComponent = UIHelper.AddGridItem(TabGrid, AttendanceIntegrationTabComponent.PrefabAuto);
				if (!(attendanceIntegrationTabComponent == null))
				{
					attendanceIntegrationTabComponent.Open(Owner, EAttendanceKeyword.GuildAttendance.ToString(), LocalizationManager.GetStringDirect("GUILD_COLLECTIVE_ATTENDANCE_MENUNAME"));
					attendanceIntegrationTabComponent.SetMenuTag(AttendanceIntegrationTabComponent.MenuTagType.Default);
					_tabList.Add(attendanceIntegrationTabComponent);
				}
			}
		}

		private void SelectDefaultTab()
		{
			if (__Extension_Method_ICollection__.COUNT(_tabList) != 0)
			{
				_tabList[0].SelectToggle(select: true);
			}
		}

		public void ForEach(Action<AttendanceIntegrationTabComponent> onLoop)
		{
			if (onLoop == null || __Extension_Method_ICollection__.IsNullOrEmpty(_tabList))
			{
				return;
			}
			foreach (AttendanceIntegrationTabComponent tab in _tabList)
			{
				onLoop(tab);
			}
		}
	}

	[Serializable]
	public class WindowViewUi
	{
		public Transform AttendanceRoot;

		private Dictionary<string, BaseWindow> _cache = new Dictionary<string, BaseWindow>();

		private GuildAttendanceComponent _guildAttendanceComponent;

		public void DestroyAll()
		{
			FactoryBase.SafeReleaseComponent(ref _guildAttendanceComponent);
			Dictionary<string, BaseWindow>.Enumerator enumerator = _cache.GetEnumerator();
			while (enumerator.MoveNext())
			{
				BaseWindow value = enumerator.Current.Value;
				if (!Vespa_Common_Class_Extension_Method.IsUnityNull(value))
				{
					value.transform.parent = null;
					VespaObject.DestroyGo(value);
				}
			}
			_cache.Clear();
		}

		public void CloseAllBut(Component exclude = null)
		{
			Dictionary<string, BaseWindow>.Enumerator enumerator = _cache.GetEnumerator();
			while (enumerator.MoveNext())
			{
				BaseWindow value = enumerator.Current.Value;
				if (!Vespa_Common_Class_Extension_Method.IsUnityNull(value) && (!(exclude != null) || !(exclude == value)))
				{
					enumerator.Current.Value.Close();
				}
			}
		}

		public void CloseGuildAttendance()
		{
			if (_guildAttendanceComponent != null)
			{
				_guildAttendanceComponent.Close();
			}
		}

		public void CreateGuildAttendance()
		{
			SingletonAuto<AttendanceManager>.instance.CreateGuildAttendance(ref _guildAttendanceComponent, AttendanceRoot);
		}

		public AttendanceWindow GetAttendanceWindow(AttendanceKeywordGroup attendanceGroup, bool createIfNotExist)
		{
			string key = attendanceGroup.KeywordMain.ToString();
			if (_cache.ContainsKey(key))
			{
				return __Extension_Method_CSharp_Class_Common__.Downcasting(_cache[key]).To<AttendanceWindow>();
			}
			if (!createIfNotExist)
			{
				return null;
			}
			AttendanceWindow attendanceWindow = SingletonAuto<AttendanceManager>.instance.CreateAttendanceWindow(attendanceGroup, AttendanceRoot);
			if (attendanceWindow == null)
			{
				return null;
			}
			_cache[key] = attendanceWindow;
			return attendanceWindow;
		}

		public AccumulateLogin GetAccumlateLoginWindow()
		{
			if (_cache.ContainsKey(AccumulateLogin.AccumlateLoginTypeName))
			{
				return __Extension_Method_CSharp_Class_Common__.Downcasting(_cache[AccumulateLogin.AccumlateLoginTypeName]).To<AccumulateLogin>();
			}
			AccumulateLogin accumulateLogin = SingletonAuto<AttendanceManager>.instance.CreateAccumlateLoginWindow(AttendanceRoot);
			if (accumulateLogin == null)
			{
				return null;
			}
			_cache[AccumulateLogin.AccumlateLoginTypeName] = accumulateLogin;
			return accumulateLogin;
		}
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Attendance/Integration/AttendanceIntegrationWindow";

	[Header("-- [[왼쪽]] [ 출석탭 ]")]
	public TabViewUi TabView;

	[Header("-- [[오른쪽]] [ 선택된 출석을 띄워주는 UI ]")]
	public WindowViewUi WindowView;

	private EAttendanceKeyword[] KeywordList = new EAttendanceKeyword[8]
	{
		EAttendanceKeyword.GrowthPackage,
		EAttendanceKeyword.NoviceAttendance,
		EAttendanceKeyword.NoviceAttendance2,
		EAttendanceKeyword.Welcome2020,
		EAttendanceKeyword.Welcom2020June7,
		EAttendanceKeyword.FallenHero7,
		EAttendanceKeyword.MagicKingdomPart1,
		EAttendanceKeyword.MagicKingdomPart2
	};

	public override void Reset()
	{
		base.Reset();
		TabView.DestroyAll();
		WindowView.DestroyAll();
	}

	public void OpenWindow()
	{
		base.Open();
		TabView.InitUi(GetAttendanceGroups());
	}

	private List<AttendanceKeywordGroup> GetAttendanceGroups()
	{
		List<AttendanceKeywordGroup> list = new List<AttendanceKeywordGroup>();
		foreach (int attendanceIndex in GetAttendanceIndices())
		{
			AttendanceKeywordGroup item = new AttendanceKeywordGroup(attendanceIndex);
			if (item.IsValid_AttendanceData())
			{
				list.Add(item);
			}
		}
		list.Sort((AttendanceKeywordGroup l, AttendanceKeywordGroup r) => l.ShowIndex.CompareTo(r.ShowIndex));
		return list;
	}

	private HashSet<int> GetAttendanceIndices()
	{
		HashSet<int> hashSet = new HashSet<int>();
		hashSet.UnionWith(GetDefaultAttendance());
		hashSet.UnionWith(GetInProgressAttendance());
		hashSet.UnionWith(GetPurchaseableAttendanceList());
		return hashSet;
	}

	private HashSet<int> GetDefaultAttendance()
	{
		HashSet<int> hashSet = new HashSet<int>();
		AttendanceKeywordGroup attendance_DefaultDailyAttendance = AttendanceFacade.GetAttendance_DefaultDailyAttendance();
		if (attendance_DefaultDailyAttendance.IsValid_AttendanceData())
		{
			hashSet.Add(attendance_DefaultDailyAttendance.IndexMain);
		}
		return hashSet;
	}

	private HashSet<int> GetInProgressAttendance()
	{
		HashSet<int> hashSet = new HashSet<int>();
		foreach (AttendanceKeywordGroup item in AttendanceFacade.GetGroupList_InProgress())
		{
			hashSet.Add(item.IndexMain);
		}
		return hashSet;
	}

	private HashSet<int> GetPurchaseableAttendanceList()
	{
		HashSet<int> hashSet = new HashSet<int>();
		EAttendanceKeyword[] keywordList = KeywordList;
		for (int i = 0; i < keywordList.Length; i++)
		{
			AttendanceKeywordGroup attendanceKeywordGroup = AttendanceKeywordGroup.Helper.CreateByAnyKeyword(keywordList[i]);
			if (attendanceKeywordGroup.IsPurchasable_v1())
			{
				hashSet.Add(attendanceKeywordGroup.IndexMain);
			}
		}
		return hashSet;
	}

	public override void OnSystemBackButton()
	{
		base.OnSystemBackButton();
		WindowView.CloseAllBut();
		Close();
	}

	void AttendanceIntegrationTabComponent.IListener.OnClickTab(AttendanceIntegrationTabComponent caller)
	{
		OpenWindowByTabComponentData(caller);
	}

	private void OpenWindowByTabComponentData(AttendanceIntegrationTabComponent caller)
	{
		WindowView.CloseGuildAttendance();
		if (caller.Keyword == AccumulateLogin.AccumlateLoginTypeName)
		{
			OpenAccumWindow();
		}
		else if (caller.Keyword == EAttendanceKeyword.GuildAttendance.ToString())
		{
			OpenGuildAttendance();
		}
		else
		{
			OpenAttendanceWindow(caller.AttendanceGroup);
		}
	}

	private void RefreshWindowByTabComponentData(AttendanceIntegrationTabComponent caller)
	{
		WindowView.CloseGuildAttendance();
		if (caller.Keyword == AccumulateLogin.AccumlateLoginTypeName)
		{
			OpenAccumWindow();
		}
		else if (caller.Keyword == EAttendanceKeyword.GuildAttendance.ToString())
		{
			OpenGuildAttendance();
		}
		else
		{
			RefreshAttendanceWindow(caller.AttendanceGroup);
		}
	}

	private void OpenAccumWindow()
	{
		AccumulateLogin window = WindowView.GetAccumlateLoginWindow();
		if (!(window == null))
		{
			UIHelper.ReleaseWidgetAnchor(window.WindowRoot);
			window.BackButtonType = EBackButtonType.Pass;
			window.Open(GlobalAutoBehaviour<AccountManager>.instance.UserManager.MiscInfo.LoginDailyCount, is_in_AttendanceIntegration: true, delegate
			{
				MakeDirty();
				WindowView.CloseAllBut(window);
			});
		}
	}

	private void OpenGuildAttendance()
	{
		WindowView.CreateGuildAttendance();
		WindowView.CloseAllBut();
	}

	private void OpenAttendanceWindow(AttendanceKeywordGroup attendanceGroup)
	{
		if (attendanceGroup.AttendanceData == null)
		{
			return;
		}
		AttendanceWindow window = WindowView.GetAttendanceWindow(attendanceGroup, createIfNotExist: true);
		if (!(window == null))
		{
			UIHelper.ReleaseWidgetAnchor(window.WindowRoot);
			window.BackButtonType = EBackButtonType.Pass;
			window.OpenAttendance(attendanceGroup, openedFromReservePopup: false, delegate
			{
				window.Setup_for_AttendanceIntegration();
				MakeDirty();
				WindowView.CloseAllBut(window);
			});
		}
	}

	private void RefreshAttendanceWindow(AttendanceKeywordGroup attendanceGroup)
	{
		if (attendanceGroup.AttendanceData != null)
		{
			AttendanceWindow attendanceWindow = WindowView.GetAttendanceWindow(attendanceGroup, createIfNotExist: false);
			if (!(attendanceWindow == null))
			{
				attendanceWindow.RefreshAttendance(attendanceGroup);
			}
		}
	}

	void IUserObserver<AttendanceInfo>.OnChanged(AttendanceInfo changedInfo)
	{
		TabView.ForEach(delegate(AttendanceIntegrationTabComponent tab)
		{
			if (tab.AttendanceGroup.IsAny_AttendanceIndex(changedInfo.AttendanceIndex))
			{
				tab.RefreshUi();
				RefreshWindowByTabComponentData(tab);
			}
		});
	}

	bool IUserObserver.IsGlobal()
	{
		return false;
	}
}
