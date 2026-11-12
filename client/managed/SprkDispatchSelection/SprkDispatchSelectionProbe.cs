using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using NGame2.NAccount;
using NGame2.NMatch;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NComponent2.NHeroPortraitSub;
using NGame2.NUI.NWindow;
using NGame2.NUtil.NWrappedInfo;
using NShared;
using NShared.RegisterMatch;
using NVespa.NSingleton;
using UnityEngine;

public static class SprkDispatchSelectionProbe
{
	private static UserHeroListPanel roster;

	private static HeroPortraitComponent busy;

	private static HeroPortraitComponent free;

	private static UserDispatchBattleInfoManager manager;

	private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

	private static void Check(bool value, string reason)
	{
		if (!value)
		{
			throw new Exception("Dispatch selection: " + reason);
		}
	}

	private static WCL_DispatchBattleInfo Job(DispatchState state)
	{
		return new WCL_DispatchBattleInfo().Copy(new DispatchBattleInfo
		{
			SlotIndex = 1,
			ChapterIndex = 1,
			DungeonIndex = 1,
			HeroIndices = "[1]",
			State = state,
			BeginTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
			CompleteTime = DateTime.UtcNow.AddHours(1.0).ToString("yyyy-MM-dd HH:mm:ss")
		});
	}

	public static string Run(string action)
	{
		GameObject host;
		GameObject prefab;
		if (action == "init")
		{
			Assembly.Load("SprkSkillDescriptionFormula").GetType("SprkSkillDescriptionFormulaProbe").GetMethod("Init")
				.Invoke(null, null);
			AccountManager instance = GlobalAutoBehaviour<AccountManager>.instance;
			object value = instance.GetType().GetProperty("AccountManagers", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).GetValue(instance, null);
			manager = new UserDispatchBattleInfoManager();
			value.GetType().GetField("DispatchBattleInfoManager").SetValue(value, manager);
			UIPanel uIPanel = NGUITools.CreateUI(advanced3D: false, 30);
			host = new GameObject("Native dispatch selection fixture");
			host.transform.SetParent(uIPanel.transform, worldPositionStays: false);
			BattlePartySetting battlePartySetting = host.AddComponent<BattlePartySetting>();
			battlePartySetting.enabled = false;
			roster = new UserHeroListPanel();
			battlePartySetting.UserHeroListPanel = roster;
			prefab = VespaResource.Load<GameObject>("Resources/" + HeroPortraitComponent.PrefabPath);
			Check(prefab != null, "native portrait prefab");
			busy = Card(1, -200f);
			free = Card(2, 200f);
			manager.Set(new WCL_DispatchBattleInfo[1] { Job(DispatchState.Battle) });
			return "Initialized native portraits and dispatch manager; let textures finish loading.";
		}
		Check(roster != null, "initialize fixture first");
		switch (action)
		{
		case "clear":
			manager.Set(new WCL_DispatchBattleInfo[0]);
			break;
		case "restore":
			manager.Set(new WCL_DispatchBattleInfo[1] { Job(DispatchState.Battle) });
			break;
		case "verify":
		{
			Check(busy.IsDispatched && !free.IsDispatched, "only the dispatched Hero is marked");
			Check(!SprkDispatchSelection.CanClick(busy) && SprkDispatchSelection.CanClick(free), "native roster clicks locked");
			busy.Owner = new HeroPortraitComponent.OwnerInfo
			{
				AccountId = 99999L
			};
			roster.SetDispatch();
			Check(!busy.IsDispatched, "another player's borrowed Hero is not locked by my dispatch");
			busy.Owner = null;
			roster.SetDispatch();
			Check(busy.IsDispatched, "owned Hero reservation restored");
			Check(SprkDispatchSelection.AvailableHeroes(new int[2] { 1, 2 }).SequenceEqual(new int[1] { 2 }), "Honor collection excludes dispatched Heroes");
			Check(SprkDispatchSelection.CanUseHeroes(new int[1] { 2 }), "available Heroes can start");
			roster.SetDispatch(isSet: false);
			Check(busy.IsDispatched, "non-dispatchable stages cannot turn the lock off");
			manager.Remove(1);
			Check(!busy.IsDispatched, "cancel/removal updates an open roster");
			manager.Add(Job(DispatchState.Prepare));
			Check(busy.IsDispatched, "new dispatch updates an open roster");
			manager.Set(new WCL_DispatchBattleInfo[1] { Job(DispatchState.ReadyToComplete) });
			Check(busy.IsDispatched, "unclaimed dispatch remains reserved");
			CustomLabelSubComponent subComponent = busy.GetSubComponent<CustomLabelSubComponent>();
			Check(subComponent != null && subComponent.gameObject.activeInHierarchy, "native dispatched label visible");
			Check(subComponent.SpriteDiabled != null && subComponent.SpriteDiabled.width > 0 && subComponent.SpriteDiabled.height > 0, "native dimmer anchored to card");
			Check(subComponent.GetComponentsInChildren<UILabel>(includeInactive: true).Any((UILabel l) => l.text.IndexOf("Dispatch", StringComparison.OrdinalIgnoreCase) >= 0), "native localized Dispatched text");
			return "PASS: native dispatched label/dimmer, blocked/available clicks, stage-independent locking, Honor filtering, live add/remove and ready-to-collect reservation.";
		}
		case "guard":
		{
			Check(!SprkDispatchSelection.CanUseHeroes(new int[2] { 1, 2 }), "saved dispatched party refused before submit");
			((DuelManager)FormatterServices.GetUninitializedObject(typeof(DuelManager))).CompletePartySetting(1L, new int[1] { 1 }, 1);
			Type typeFromHandle = typeof(MatchManager);
			object uninitializedObject = FormatterServices.GetUninitializedObject(typeFromHandle);
			object obj = Activator.CreateInstance(typeFromHandle.GetNestedType("RequestWrapper_Pvp", BindingFlags.Public | BindingFlags.NonPublic), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new object[5]
			{
				MatchPvpType.Normal,
				new int[1] { 1 },
				1,
				0L,
				0
			}, null);
			MethodInfo method = typeFromHandle.GetMethod("RequestMatchInternal_Pvp", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			method.Invoke(uninitializedObject, new object[2] { obj, null });
			method.Invoke(uninitializedObject, new object[2]);
			NShared.RegisterMatch.Request request = new NShared.RegisterMatch.Request();
			request.DuelType = MatchDuelType.Friendly;
			request.HeroIndices = new int[1] { 1 };
			Check(SprkDispatchSelection.CanRegister(request), "late friendly rejection must reach server to release the partner");
			return "PASS: saved dispatched parties refused with an explanatory popup; actual duel-ready and registration entry points return before networking; native null request remains safe.";
		}
		}
		return "busy=" + busy.IsDispatched + ", free=" + free.IsDispatched;
		HeroPortraitComponent Card(int id, float x)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(prefab, host.transform, worldPositionStays: false);
			gameObject.name = "Hero " + id;
			gameObject.transform.localPosition = new Vector3(x, 0f, 0f);
			NGUITools.SetLayer(gameObject, 30);
			HeroPortraitComponent component = gameObject.GetComponent<HeroPortraitComponent>();
			component.Open(new CachedHeroInfo(new HeroInfo
			{
				HeroIndex = id,
				Level = 100,
				Star = 5,
				Transcended = 5
			}), null);
			object value2 = roster.GetType().GetField("_dicUserHeroPortarit", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).GetValue(roster);
			UserHeroListPanel.HeroInfoKey heroInfoKey = new UserHeroListPanel.HeroInfoKey(0L, id);
			value2.GetType().GetMethod("Add").Invoke(value2, new object[2] { heroInfoKey, component });
			return component;
		}
	}
}
