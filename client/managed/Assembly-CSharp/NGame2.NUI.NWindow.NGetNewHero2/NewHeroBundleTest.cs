using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow.NAttendance;
using NShared;
using NShared.GetAttendanceReward;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow.NGetNewHero2;

public class NewHeroBundleTest
{
	private static bool OnInput()
	{
		if (Input.GetKey(KeyCode.LeftControl))
		{
			return Input.GetKeyDown(KeyCode.F12);
		}
		return false;
	}

	public static void TestResponse(NGame2.NUI.NWindow.NAttendance.AttendanceReward attendanceWindow)
	{
		if (!OnInput())
		{
			return;
		}
		NShared.GetAttendanceReward.Response response = new NShared.GetAttendanceReward.Response();
		response.BaseResult = BaseResultType.Success;
		response.Result = ResultType.Success;
		response.AttendanceRewardResult = new DbRewardResult();
		List<HeroInfo> list = new List<HeroInfo>();
		HeroInfo heroInfo = new HeroInfo();
		int key = (heroInfo.HeroIndex = 30);
		heroInfo.Star = GameRoot.Instance.CreatureDataContainer.GetData(key).StartHeroStar;
		heroInfo.Level = 1;
		HeroInfo heroInfo2 = new HeroInfo();
		int key2 = (heroInfo2.HeroIndex = 56);
		heroInfo2.Star = GameRoot.Instance.CreatureDataContainer.GetData(key2).StartHeroStar;
		heroInfo2.Level = 1;
		HeroInfo heroInfo3 = new HeroInfo();
		int key3 = (heroInfo3.HeroIndex = 87);
		heroInfo3.Star = GameRoot.Instance.CreatureDataContainer.GetData(key3).StartHeroStar;
		heroInfo3.Level = 1;
		list.Add(heroInfo);
		list.Add(heroInfo2);
		list.Add(heroInfo3);
		foreach (HeroInfo item in list)
		{
			GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Remove(item.HeroIndex);
		}
		response.AttendanceRewardResult.HeroInfos = list.ToArray();
		CurrencyResultInfo3 currencyResultInfo = new CurrencyResultInfo3();
		currencyResultInfo.CurrencyType = CurrencyType.Gold;
		currencyResultInfo.AddValue = 10000L;
		List<CurrencyResultInfo3> list2 = new List<CurrencyResultInfo3>();
		list2.Add(currencyResultInfo);
		response.AttendanceRewardResult.CurrencyResults = list2.ToArray();
		SceneAutoBehaviour<LobbyManagement>.instance.OnResponse(attendanceWindow, response);
	}

	public static void TestNewHeroBundle()
	{
		if (!OnInput())
		{
			return;
		}
		GetNewHeroSceneBundleData firstData = GameRoot.Instance.GetNewHeroSceneBundleDataContainer.GetFirstData();
		if (firstData == null)
		{
			VespaDebug.LogError("Test NewHeroBundle data is null");
			return;
		}
		List<HeroInfo> list = new List<HeroInfo>();
		int[] bundleHeroIndices = firstData.BundleHeroIndices;
		foreach (int num in bundleHeroIndices)
		{
			list.Add(new HeroInfo
			{
				HeroIndex = num,
				Star = GameRoot.Instance.CreatureDataContainer.GetData(num).StartHeroStar
			});
		}
		if (__Extension_Method_ICollection__.IsNullOrEmpty(list))
		{
			VespaDebug.LogError("BundleHeroIndices is null");
			return;
		}
		foreach (HeroInfo item in list)
		{
			GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Remove(item.HeroIndex);
		}
		GlobalAutoBehaviour<AccountManager>.instance.HeroManager.AddHerosAndAnnounceRecreateChangeHeroInfo(list.ToArray());
	}
}
