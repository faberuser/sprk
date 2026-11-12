using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NGame2.NAccount;
using NGame2.NBattleField;
using NGame2.NUI.NManager.NBattle;
using NGame2.NUI.NWindow.NBattle;
using NGameAsset;
using NShared;
using NShared.NJit;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SprkArenaLatencyProbe
{
	private sealed class Raiser : BattleFacade.IBattleLogRaiser
	{
		public int Count;

		public int Slot;

		public int Uid;

		public bool Auto;

		public void ReserveSkill(int uid, int slot, int target, bool auto)
		{
			Count++;
			Uid = uid;
			Slot = slot;
			Auto = auto;
		}

		public void FireNextSkill(int uid)
		{
		}

		public void SetManualTarget(int uid, int target)
		{
		}

		public void SetManualMove(int uid, Vector3 position)
		{
		}

		public void ReserveDungeonSkill(TeamType team, int slot)
		{
		}
	}

	private static void Check(bool ok, string message)
	{
		if (!ok)
		{
			throw new Exception(message);
		}
	}

	public static string Run()
	{
		string environmentVariable = Environment.GetEnvironmentVariable("SPRK_AUTOMATION_PROFILE");
		Check(SceneManager.GetActiveScene().name == "Login" && environmentVariable != null && environmentVariable.StartsWith("arena-latency-"), "Probe requires an isolated arena-latency Login profile");
		TableRoot.Instance.SetPatchDownLoadComplete(isComplete: true);
		GameRoot.Instance.SetJsonLoader(new JsonAssetAsyncLoader());
		GameRoot.Instance.SetAiScriptLoader(new TextAssetSyncLoader());
		GameRoot.Instance.InitForClient();
		GameRoot.Instance.OnDataContainerLoaded(editorVerify: false);
		BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
		AccountManager instance = GlobalAutoBehaviour<AccountManager>.instance;
		object value = instance.GetType().GetProperty("AccountManagers", bindingAttr).GetValue(instance, null);
		value.GetType().GetField("UserManager").SetValue(value, new UserManager(instance, new UserInfo
		{
			AccountId = 123456L
		}, new PlayerMiscInfo(), new PlayerGuildPointInfo()));
		BattleFacade instance2 = SingletonAuto<BattleFacade>.instance;
		object value2 = typeof(BattleFacade).GetField("_threadContext", bindingAttr).GetValue(instance2);
		FieldInfo field = value2.GetType().GetField("BattleCache");
		object value3 = field.GetValue(value2);
		FieldInfo field2 = typeof(BattleFacade).GetField("_battleLogRaiser", bindingAttr);
		object value4 = field2.GetValue(instance2);
		GameObject gameObject = new GameObject("ArenaAutoFixture");
		gameObject.SetActive(value: false);
		try
		{
			SprkConquest.Login(new Dictionary<string, object>
			{
				{ "MatchUid", "0" },
				{ "MasterId", "123" }
			});
			long time = 1000L;
			Check(SprkConquest.FilterClock(ref time) && time == 1000, "Co-op clock must filter without adding a buffer");
			time = 1000L;
			Check(!SprkConquest.FilterClock(ref time), "Duplicate co-op heartbeat restarted extrapolation");
			BattleInstance battleInstance = new BattleInstance();
			battleInstance.SetCampaignDungeon(1000, 1, 0, isScenario: false);
			battleInstance.GlobalTimer.Set(1000L);
			battleInstance.SetWaveIndex(1, 3);
			battleInstance.OnWaveReadyBattle();
			Check(battleInstance.GlobalTimer.GetCurrentTimeMs() == 1000, "Native WaveReady unexpectedly resets the global clock");
			battleInstance.GlobalTimer.Set(6000L);
			battleInstance.SetWaveIndex(2, 3);
			battleInstance.OnWaveReadyBattle();
			Check(battleInstance.GlobalTimer.GetCurrentTimeMs() == 6000, "Native wave transition resets the global clock");
			time = 6000L;
			Check(SprkConquest.FilterClock(ref time) && time == 6000, "Next wave cannot advance the co-op clock");
			time = 3000L;
			Check(!SprkConquest.FilterClock(ref time), "Old-wave heartbeat rewound the next wave");
			SprkConquest.Login(new Dictionary<string, object> { { "MasterId", "123" } });
			time = 0L;
			Check(SprkConquest.FilterClock(ref time) && time == 0, "Co-op reconnect cannot reset its history clock");
			SprkConquest.Login(new Dictionary<string, object>());
			time = 20L;
			Check(SprkConquest.FilterClock(ref time) && time == 20, "Offline clock behavior changed");
			SprkConquest.Login(new Dictionary<string, object> { { "MatchUid", "123" } });
			long time2 = 1000L;
			Check(SprkConquest.FilterClock(ref time2) && time2 == 900, "Arena login did not enable replay headroom");
			time2 = 900L;
			Check(!SprkConquest.FilterClock(ref time2), "Older heartbeat replaced a combat clock");
			SprkConquest.Login(new Dictionary<string, object> { { "MatchUid", "124" } });
			time2 = 0L;
			Check(SprkConquest.FilterClock(ref time2) && time2 == 0, "Next Arena match cannot reset its clock");
			SystemButtonAutoSimple systemButtonAutoSimple = gameObject.AddComponent<SystemButtonAutoSimple>();
			SystemButtonAuto systemButtonAuto = gameObject.AddComponent<SystemButtonAuto>();
			BattleType[] array = new BattleType[2]
			{
				BattleType.Arena,
				BattleType.ArenaBanPick
			};
			for (int i = 0; i < array.Length; i++)
			{
				BattleType battleType = array[i];
				field.SetValue(value2, new BattleFacade.BattleCacheData
				{
					ChapterIndex = 1000,
					BattleType = battleType
				});
				typeof(SystemButtonAutoSimple).GetField("_autoPlayerable", bindingAttr).SetValue(systemButtonAutoSimple, false);
				typeof(SystemButtonAuto).GetField("_autoPlayerable", bindingAttr).SetValue(systemButtonAuto, false);
				systemButtonAutoSimple.ChangeAutoUI(auto: true);
				systemButtonAuto.ChangeAutoUI(auto: true);
				Check(systemButtonAutoSimple.SkillReserveAuto && systemButtonAuto.SkillReserveAuto, "Lit Arena Auto did not enable skill selection: " + battleType);
				systemButtonAutoSimple.ChangeAutoUI(auto: false);
				systemButtonAuto.ChangeAutoUI(auto: false);
				Check(!systemButtonAutoSimple.SkillReserveAuto && !systemButtonAuto.SkillReserveAuto, "Arena Auto cannot be disabled: " + battleType);
				bool auto = false;
				bool fast = true;
				Check(SprkConquest.ArenaAutoPlayable(battleType, ref auto, ref fast) && auto && !fast, "Arena Auto must retain x1 speed");
			}
			field.SetValue(value2, new BattleFacade.BattleCacheData
			{
				ChapterIndex = 1,
				BattleType = BattleType.Arena
			});
			systemButtonAutoSimple.ChangeAutoUI(auto: true);
			Check(!systemButtonAutoSimple.SkillReserveAuto, "Arena override leaked into another chapter");
			field.SetValue(value2, new BattleFacade.BattleCacheData
			{
				ChapterIndex = 1000,
				BattleType = BattleType.Arena
			});
			int index = GameRoot.Instance.CreatureDataContainer.GetAllData().First((CreatureData c) => c.Index > 0 && c.Index < 10000).Index;
			CachedHeroInfo heroInfo = new CachedHeroInfo(new HeroInfo
			{
				HeroIndex = index,
				Level = 90,
				Star = 5,
				SkillLevel1 = 20,
				SkillLevel2 = 20,
				SkillLevel3 = 20,
				SkillLevel4 = 20
			});
			BattleFacade.CreatureCacheData creatureCacheData = new BattleFacade.CreatureCacheData(17)
			{
				HeroInfo = heroInfo,
				CreatureData = GameRoot.Instance.CreatureDataContainer.GetData(index),
				CreateInfo = BattleCreatureCreateInfo.CreateMyCreatureCreateInfo(heroInfo, isLeader: true, isAi: false, null, null),
				Player = new BattleFacade.PlayerCacheData
				{
					Id = 123456L
				}
			};
			BattleManagement instance3 = SceneAutoBehaviour<BattleManagement>.instance;
			instance3.GetType().GetField("_playerId", bindingAttr).SetValue(instance3, 123456L);
			MethodInfo methodInfo = instance3.GetType().GetMethods(bindingAttr).Single((MethodInfo m) => m.Name == "UpdateSkillAutoReserve" && m.GetParameters().Length == 1);
			Raiser raiser = new Raiser();
			field2.SetValue(instance2, raiser);
			methodInfo.Invoke(instance3, new object[1] { creatureCacheData });
			object value5 = instance3.GetType().GetField("_skillAutoReserveDic", bindingAttr).GetValue(instance3);
			object obj = value5.GetType().GetMethod("GetValueByIndex").Invoke(value5, new object[1] { 0 });
			obj.GetType().GetField("ElapsedTime").SetValue(obj, 1f);
			if (systemButtonAutoSimple.SkillReserveAuto)
			{
				methodInfo.Invoke(instance3, new object[1] { creatureCacheData });
			}
			Check(raiser.Count > 0 && raiser.Uid == 17 && raiser.Slot >= 1 && raiser.Slot <= 5 && raiser.Auto, "Native Auto selector did not reserve an owned hero skill");
			int count = raiser.Count;
			creatureCacheData.Player.Id = 999999L;
			methodInfo.Invoke(instance3, new object[1] { creatureCacheData });
			Check(raiser.Count == count, "Auto selected another player's hero");
			return "PASS: co-op zero-buffer/stale clocks, native wave continuity and reconnect reset; Victory/Honor Auto on/off, campaign isolation, x1 eligibility, native skill reservation and ownership.";
		}
		finally
		{
			SprkConquest.Login(new Dictionary<string, object>());
			field.SetValue(value2, value3);
			field2.SetValue(instance2, value4);
			UnityEngine.Object.Destroy(gameObject);
		}
	}
}
