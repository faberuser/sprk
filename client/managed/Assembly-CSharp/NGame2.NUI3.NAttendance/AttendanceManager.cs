using System;
using System.Collections.Generic;
using System.Linq;
using NGame2.NAccount;
using NGame2.NContentsInfo;
using NGame2.NNotification;
using NGame2.NUI.NComponent2.NGuildAttendance;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NAttendance;
using NGame2.NUI.NWindow.NBattle;
using NShared;
using NShared.GetAttendanceReward;
using NVespa;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI3.NAttendance;

public class AttendanceManager : SingletonAuto<AttendanceManager>
{
	private abstract class WindowWrapperAbstract
	{
		public abstract Type GetWindowType();

		public abstract BaseWindow CreateWindow(Transform parentTm);

		public abstract BaseWindow GetInstance_from_LobbyManagement();
	}

	private class WindowWrapper<TWindow> : WindowWrapperAbstract where TWindow : BaseWindow
	{
		private PrefabAuto<TWindow> _prefabAuto;

		public string PrefabPath { get; private set; }

		public string Keyword { get; private set; }

		public WindowWrapper(string prefabPath, string keyword)
		{
			PrefabPath = prefabPath;
			Keyword = keyword;
			_prefabAuto = new PrefabAuto<TWindow>(PrefabPath);
		}

		public override Type GetWindowType()
		{
			return typeof(TWindow);
		}

		public override BaseWindow CreateWindow(Transform parentTm)
		{
			return _prefabAuto.Factory.Create(parentTm);
		}

		public override BaseWindow GetInstance_from_LobbyManagement()
		{
			if (!SceneAutoBehaviour<LobbyManagement>.isValidInstance)
			{
				return null;
			}
			if (SceneAutoBehaviour<LobbyManagement>.instance.GetPrefab<TWindow>() == null)
			{
				SceneAutoBehaviour<LobbyManagement>.instance.AddInstance<TWindow>(PrefabPath);
			}
			return SceneAutoBehaviour<LobbyManagement>.instance.GetInstance<TWindow>();
		}
	}

	private Dictionary<string, WindowWrapperAbstract> _windowWrappers = new Dictionary<string, WindowWrapperAbstract>();

	private List<Type> _registeredWindowTypes;

	public AttendanceManager()
	{
		RegisterAttendanceAll();
	}

	public AccumulateLogin CreateAccumlateLoginWindow(Transform parentTm)
	{
		return __Extension_Method_CSharp_Class_Common__.Downcasting(CreateWindow(AccumulateLogin.AccumlateLoginTypeName, parentTm)).To<AccumulateLogin>();
	}

	public AttendanceWindow CreateAttendanceWindow(AttendanceKeywordGroup attendanceGroup, Transform parentTm)
	{
		return __Extension_Method_CSharp_Class_Common__.Downcasting(CreateWindow(attendanceGroup.KeywordMain.ToString(), parentTm)).To<AttendanceWindow>();
	}

	private BaseWindow CreateWindow(string keyword, Transform parentTm)
	{
		return Vespa_Generic_IDictionary_Extension_Method.GETVALUE(_windowWrappers, keyword, null)?.CreateWindow(parentTm);
	}

	public void CreateGuildAttendance(ref GuildAttendanceComponent component, Transform rootTransform)
	{
		SceneAutoBehaviour<GuildManagement2>.instance.CreateGuildAttendanceComponent(ref component, isPopup: false, rootTransform, isReposition: true);
	}

	public bool IsOpened_Any()
	{
		if (!SceneAutoBehaviour<LobbyManagement>.isValidInstance)
		{
			return false;
		}
		return SceneAutoBehaviour<LobbyManagement>.instance.IsOpenedAny(_registeredWindowTypes);
	}

	public AttendanceWindow GetAttendanceWindow(AttendanceKeywordGroup attendanceGroup)
	{
		return __Extension_Method_CSharp_Class_Common__.Downcasting(GetBaseWindow(attendanceGroup.KeywordMain.ToString())).To<AttendanceWindow>();
	}

	private BaseWindow GetBaseWindow(string keyword)
	{
		return Vespa_Generic_IDictionary_Extension_Method.GETVALUE(_windowWrappers, keyword, null)?.GetInstance_from_LobbyManagement();
	}

	public void OnReceive_AttendanceReward(NGame2.NUI.NWindow.NAttendance.AttendanceReward caller)
	{
		if (caller == null)
		{
			return;
		}
		AttendanceData attendanceData = caller.AttendanceData;
		if (attendanceData == null)
		{
			return;
		}
		int index = attendanceData.Index;
		NShared.GetAttendanceReward.Request request = new NShared.GetAttendanceReward.Request();
		request.AttendanceIndex = index;
		caller.OnReceivedReward(start: true);
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.GetAttendanceReward.Response response)
		{
			if (response == null)
			{
				MessagePopup.OpenError(BaseResultType.Fail);
			}
			else if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
			}
			else if (response.Result != ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.AttendanceManager.Apply(response.AttendanceInfos);
				DbRewardResult rewardResult = response.AttendanceRewardResult;
				if (rewardResult != null)
				{
					GlobalAutoBehaviour<AccountManager>.instance.ApplyDbRewardResult(rewardResult);
					caller.StartReceiveEffect(delegate
					{
						BaseRewardPopup.PopupOpenData popupOpen = new BaseRewardPopup.PopupOpenData().SetTitle(caller.Title).SetItemResultInfos(rewardResult.ItemResults).SetEquipResultInfos(rewardResult.EquipItemResults)
							.SetCurrencyResultInfos3(rewardResult.CurrencyResults)
							.AddPetInfos(rewardResult.PetInfos);
						if (rewardResult.HeroInfos != null && rewardResult.HeroInfos.Length != 0)
						{
							GlobalAutoBehaviour<AccountManager>.instance.ApplyAddHero(rewardResult.HeroInfos, rewardResult.ExpResultInfos, reserveWindow: false, delegate
							{
								BaseRewardPopup.OpenPopup(popupOpen, isGlobal: false, delegate
								{
									caller.OnReceivedReward(start: false);
								});
							});
						}
						else
						{
							BaseRewardPopup.OpenPopup(popupOpen, isGlobal: false, delegate
							{
								caller.OnReceivedReward(start: false);
								caller.OpenNextPageNoticePopup();
							});
						}
					});
				}
				SingletonAuto<NotificationManager>.instance.ResetLocalNotification();
			}
		});
	}

	private void RegisterWindow<TWindow>(string prefabPath, string keyword) where TWindow : BaseWindow
	{
		WindowWrapper<TWindow> value = new WindowWrapper<TWindow>(prefabPath, keyword);
		_windowWrappers.Add(keyword, value);
	}

	private void RegisterAttendance<TWindow>(string prefabPath, EAttendanceKeyword keyword) where TWindow : BaseWindow
	{
		RegisterWindow<TWindow>(prefabPath, keyword.ToString());
	}

	private void RegisterAttendanceAll()
	{
		RegisterAttendance<NGame2.NUI.NWindow.NAttendance.AttendanceReward>("Resources/Prefab2/Window/Attendance/AttendanceReward", EAttendanceKeyword.Common);
		RegisterAttendance<AttendanceRewardNew7Days>("Resources/Prefab2/Window/Attendance/AttendanceRewardNew7Days", EAttendanceKeyword.ReturnUser);
		RegisterAttendance<AttendanceRewardSupply14Days>("Resources/Prefab2/Window/Attendance/AttendanceRewardSupply14Days", EAttendanceKeyword.GrowthPackage);
		RegisterAttendance<AttendanceRewardNovice14Days>("Resources/Prefab2/Window/Attendance/AttendanceRewardNovice14Days", EAttendanceKeyword.NoviceAttendance);
		RegisterAttendance<AttendanceRewardSoulWeapon14Days>("Resources/Prefab2/Window/Attendance/AttendanceRewardSoulWeapon14Days", EAttendanceKeyword.SoulWeapon14);
		RegisterAttendance<AttendanceRewardMay14Days>("Resources/Prefab2/Window/Attendance/AttendanceRewardMay14Days", EAttendanceKeyword.NoviceAttendance2);
		RegisterAttendance<AttendanceRewardMay14Days>("Resources/Prefab2/Window/Attendance/AttendanceRewardMay14Days", EAttendanceKeyword.MayNewbieAttendance);
		RegisterAttendance<AttendanceRewardMay14Days>("Resources/Prefab2/Window/Attendance/AttendanceRewardMay14Days", EAttendanceKeyword.NewMayNewbieAttendance);
		RegisterAttendance<AttendanceRewardNew14Days>("Resources/Prefab2/Window/Attendance/AttendanceRewardNew14Days", EAttendanceKeyword.BeginUser);
		RegisterAttendance<AttendanceRewardOrvelFounding>("Resources/Prefab2/Window/Attendance/AttendanceRewardOrvelFounding", EAttendanceKeyword.OrvelFounding);
		RegisterAttendance<AttendanceRewardNewbieMarch2020>("Resources/Prefab2/Window/Attendance/AttendanceRewardNewbieMarch2020", EAttendanceKeyword.NewbieMarch2020);
		RegisterAttendance<AttendanceRewardReturnMarch2020>("Resources/Prefab2/Window/Attendance/AttendanceRewardReturnMarch2020", EAttendanceKeyword.ReturnMarch2020);
		RegisterAttendance<AttendanceReward2020NewSemester14Days>("Resources/Prefab2/Window/Attendance/AttendanceReward2020NewSemester14Days", EAttendanceKeyword.KingsSchool14);
		RegisterAttendance<AttendanceRewardWelcome2020June7Days>("Resources/Prefab2/Window/Attendance/AttendanceRewardWelcome2020June7Days", EAttendanceKeyword.Welcom2020June7);
		RegisterAttendance<AttendanceRewardFallenHero7Days>("Resources/Prefab2/Window/Attendance/AttendanceRewardFallenHero7Days", EAttendanceKeyword.FallenHero7);
		RegisterAttendance<AttendanceReward14DaysMagicKingdomPart1>("Resources/Prefab2/Window/Attendance/AttendanceReward14DaysMagicKingdomPart1", EAttendanceKeyword.MagicKingdomPart1);
		RegisterAttendance<AttendanceReward14DaysMagicKingdomPart2>("Resources/Prefab2/Window/Attendance/AttendanceReward14DaysMagicKingdomPart2", EAttendanceKeyword.MagicKingdomPart2);
		RegisterAttendance<AttendanceRewardRebelClause7Days>("Resources/Prefab2/Window/Attendance/AttendanceRewardRebelClause7Days", EAttendanceKeyword.RebelClause7);
		RegisterAttendance<AttendanceRewardNew14Days>("Resources/Prefab2/Window/Attendance/AttendanceRewardNew14Days", EAttendanceKeyword.BeginUser201110);
		RegisterAttendance<AttendanceReward14DaysAdieu2020partone14Days>("Resources/Prefab2/Window/Attendance/AttendanceReward14DaysAdieu2020partone14Days", EAttendanceKeyword.Adieu2020partone14);
		RegisterAttendance<AttendanceRewardPetLaunch7Days>("Resources/Prefab2/Window/Attendance/AttendanceRewardPetLaunch7Days", EAttendanceKeyword.PetLaunch);
		RegisterAttendance<AttendanceRewardAdieu2020PartTwo14Days>("Resources/Prefab2/Window/Attendance/AttendanceRewardAdieu2020PartTwo14Days", EAttendanceKeyword.Adieu2020PartTwo14);
		RegisterAttendance<AttendanceRewardNewYear2021Attendance14Days>("Resources/Prefab2/Window/Attendance/AttendanceRewardNewYear2021Attendance14Days", EAttendanceKeyword.NewYear2021Attendance14);
		RegisterAttendance<AttendanceRewardOrvelFounding2021PartTwo14Days>("Resources/Prefab2/Window/Attendance/AttendanceRewardOrvelFounding2021PartTwo14Days", EAttendanceKeyword.OrvelFounding2021PartTwo);
		RegisterAttendance<AttendanceRewardOrvelFounding2021PartOne14Days>("Resources/Prefab2/Window/Attendance/AttendanceRewardOrvelFounding2021PartOne14Days", EAttendanceKeyword.OrvelFounding2021PartOne);
		RegisterAttendance<AttendanceRewardOrvelFounding2021Free14Days>("Resources/Prefab2/Window/Attendance/AttendanceRewardOrvelFounding2021Free14Days", EAttendanceKeyword.OrvelFoudning2021Free);
		RegisterAttendance<AttendanceRewardSeriaGrowth2021>("Resources/Prefab2/Window/Attendance/AttendanceRewardSeriaGrowth20217Days", EAttendanceKeyword.SeriaGrowth2021);
		RegisterAttendance<AttendanceRewardSpecialJune20217Days>("Resources/Prefab2/Window/Attendance/AttendanceRewardSpecialJune20217Days", EAttendanceKeyword.SpecialJune2021);
		RegisterAttendance<AttendanceRewardTechnomagic14Days>("Resources/Prefab2/Window/Attendance/AttendanceRewardTechnomagic14Days", EAttendanceKeyword.TechnoMagic2021);
		RegisterAttendance<AttendanceRewardCommon14Days>("Resources/Prefab2/Window/Attendance/AttendanceRewardCommon14Days", EAttendanceKeyword.ChangeStartSoulWeapon);
		RegisterAttendance<AttendanceRewardCommon7Days>("Resources/Prefab2/Window/Attendance/AttendanceRewardCommon7Days", EAttendanceKeyword.ChangeStartUniqueGear);
		RegisterAttendance<AttendanceRewardEvent28Days>("Resources/Prefab2/Window/Attendance/AttendanceRewardEvent28Days", EAttendanceKeyword.EtherEvent2021);
		RegisterAttendance<AttendanceRewardCommon14Days>("Resources/Prefab2/Window/Attendance/AttendanceRewardCommon14Days", EAttendanceKeyword.BlackFridaySoulweapon2021);
		RegisterAttendance<AttendanceRewardCommon7Days>("Resources/Prefab2/Window/Attendance/AttendanceRewardCommon7Days", EAttendanceKeyword.BlackFridayBoost2021);
		RegisterAttendance<AttendanceRewardCommon14Days>("Resources/Prefab2/Window/Attendance/AttendanceRewardCommon14Days", EAttendanceKeyword.HappyNewYear2022);
		RegisterAttendance<BonusAttendance20221001_AttendanceWindow>("Resources/Prefab3/Attendance/BonusAttendance20221001_AttendanceWindow", EAttendanceKeyword.BonusAttendance20221001_1);
		RegisterWindow<AccumulateLogin>("Resources/Prefab2/Window/AccumulateLogin/AccumulateLogin", typeof(AccumulateLogin).Name);
		_registeredWindowTypes = _windowWrappers.Select((KeyValuePair<string, WindowWrapperAbstract> pair) => pair.Value.GetWindowType()).ToList();
	}
}
