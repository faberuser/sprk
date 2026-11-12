using System;
using System.Collections;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NBattleContext;
using NGame2.NCutScene;
using NGame2.NCutScene.NScript;
using NGame2.NDispatchBattle;
using NGame2.NGuild;
using NGame2.NScene;
using NGame2.NTutorial;
using NGame2.NUI;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NComponent2.NHeroPortraitSub;
using NGame2.NUI.NComponent2.NStat;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NPunish;
using NShader;
using NShared;
using NShared.BeginCampaign;
using NShared.EndCampaign;
using NShared.NJit;
using NShared.NLocalization;
using NShared.NUtil;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class RestoredPortal
{
	private static readonly ExponentialTable PunishmentDefenseRatios = new ExponentialTable(1000000L);

	private static Dictionary<int, GuildSuppressSessionData> serverConquestSessions;

	private static Dictionary<int, List<GuildSuppressSessionData>> serverConquestSeasons;

	public static void OpenValanceCraft(CampaignNodeData node)
	{
		ValanceManagement instance = SceneAutoBehaviour<ValanceManagement>.instance;
		if (node == null)
		{
			instance.OpenCraftInven();
		}
		else
		{
			instance.OpenCraft();
		}
	}

	public static void OpenValanceIdentified(CampaignNodeData node)
	{
		ValanceManagement instance = SceneAutoBehaviour<ValanceManagement>.instance;
		if (node == null)
		{
			instance.OpenIdentifiedInven(null);
		}
		else
		{
			instance.OpenIdentified();
		}
	}

	public static void OpenValanceManage(CampaignNodeData node)
	{
		ValanceManagement instance = SceneAutoBehaviour<ValanceManagement>.instance;
		if (node == null)
		{
			instance.OpenManageInven();
		}
		else
		{
			instance.OpenManage();
		}
	}

	public static NShared.EntryMenu ArenaMenu(NShared.EntryMenu menu)
	{
		if (menu != null && menu.ChildIndices != null && menu.ChildIndices.Length != 0)
		{
			return menu;
		}
		int[] array = new int[3];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = i + 1;
		}
		return new NShared.EntryMenu(menu?.Index ?? 0, (menu == null) ? new string[0] : menu.EntryMenuButton, (menu == null) ? "" : menu.EntryMenuCondition, (menu == null) ? new string[0] : menu.EntryMenuEvent, array, menu?.ParentIndex ?? 0);
	}

	public static bool RequiredBattleStoryEnabled(bool enabled)
	{
		if (enabled)
		{
			return true;
		}
		if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Battle2")
		{
			return false;
		}
		EnterDungeonData enterDungeon = SingletonAuto<CampaignContext>.instance.EnterDungeon;
		if (enterDungeon != null && enterDungeon.IsScenario)
		{
			if (enterDungeon.Chapter != 2 || enterDungeon.OriginalDungeon != 18)
			{
				if (enterDungeon.Chapter == 4)
				{
					return enterDungeon.OriginalDungeon == 21;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	public static bool ShowMailCapacityError(NShared.Response response)
	{
		if (response == null || response.InternalErrorMessage != "Equipment bag is full. Free space and try claiming the mail again.")
		{
			return false;
		}
		MessagePopup.Open("Equipment bag full", response.InternalErrorMessage);
		return true;
	}

	public static bool ReserveContentsUnlockNotice(ContentsOpenConditionData data)
	{
		if (data == null || !AccountManager.IsLogined())
		{
			return false;
		}
		LocalAccountBool localAccountBool = new LocalAccountBool(i_default: false, "Sprk.ContentsUnlockNotice." + data.Index);
		if (localAccountBool.Value)
		{
			return false;
		}
		localAccountBool.Value = true;
		return true;
	}

	public static bool GodkingAccess()
	{
		if (SoulWeaponManagement.IsOpenGodKingTemple())
		{
			return true;
		}
		MessagePopup.Open("Content locked", "Clear 6-21 and complete the God King's Temple story to unlock God King's Trial and Eclipse.");
		return false;
	}

	public static bool ClosePortalForActivity(bool original, ClientActivityType activity)
	{
		if (!original && activity != ClientActivityType.OpenEclipse)
		{
			return activity == ClientActivityType.OpenPunishmentRaid;
		}
		return true;
	}

	public static void TempleMissingScene(CutSceneManager scene)
	{
		if (scene.ScriptPath != "CutScene/CH65_Main_4")
		{
			return;
		}
		foreach (ScriptGroup group in scene.GroupList)
		{
			foreach (ScriptHolder script in group.ScriptList)
			{
				if (script.Value is DisplayText displayText)
				{
					displayText.TitleLink.Key = "Temple of the God King";
					displayText.TextLink.Key = "This story scene is unavailable in the recovered game files. Tap to continue to the next stage.";
				}
			}
		}
	}

	public static bool ShakmehAccess()
	{
		NGame2.NAccount.UserCampaignManager campaignManager = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager;
		if (!campaignManager.IsCompleted(97, 2))
		{
			MessagePopup.Open("Content locked", "Complete the World Tree story in Chapter 9.5 to access The Shadow of Aegina.");
			return false;
		}
		if (campaignManager.IsCompleted(50000, 210))
		{
			return true;
		}
		MessagePopup.Open("Content locked", "Continue The Shadow of Aegina story and clear its Devourer Shakmeh battle first.");
		return false;
	}

	private static string UnlockMessage(PortalRenewalCategory category)
	{
		return category switch
		{
			PortalRenewalCategory.TechnoRaid => "Clear 10-9 to unlock Technomagic Raid. Siegfried and Ascalon require 10-30.", 
			PortalRenewalCategory.PunishmentRaid => "Clear X-10 to unlock Apocalypsion Raid.", 
			PortalRenewalCategory.Shakmeh => "Complete the World Tree story in Chapter 9.5 to access The Shadow of Aegina.", 
			_ => "Complete the God King's Temple story after 6-21.", 
		};
	}

	public static void SetupCardCurve(PortalContentsComponent component, PortalRenewalData data)
	{
		if (data == null || data.SubComponentShape != PortalRenewalCompType.Small)
		{
			return;
		}
		string mainImagePath = data.MainImagePath;
		if (!(mainImagePath != "Portal_WorldBoss_NotProgress_Image") || !(mainImagePath != "Portal_ChallengeRaid_NotProgress_Image") || !(mainImagePath != "Portal_PetHouse_Image") || !(mainImagePath != "Portal_TreasureHouse_Image") || !(mainImagePath != "Portal_Punishment_Image") || !(mainImagePath != "Portal_MiddleBossShakmeh_Image"))
		{
			UITexture contentsTexture = component._basicData.ContentsTexture;
			if (contentsTexture != null)
			{
				contentsTexture.onPostFill = CurveCardMesh;
			}
		}
	}

	public static void CurveCardMesh(UIWidget widget, int offset, RenderingList<UnityEngine.Vector3> verts, RenderingList<Vector2> uvs, RenderingList<Color32> colors)
	{
		if (offset == 0 && verts.size == 4)
		{
			UnityEngine.Vector3 vector = verts[0];
			UnityEngine.Vector3 vector2 = verts[2];
			Vector2 vector3 = uvs[0];
			Vector2 vector4 = uvs[2];
			Color32 item = colors[0];
			verts.Clear();
			uvs.Clear();
			colors.Clear();
			verts.ReserveCount(256);
			uvs.ReserveCount(256);
			for (int i = 0; i < 64; i++)
			{
				float num = (float)i / 64f;
				float num2 = (float)(i + 1) / 64f;
				float t = 0.12f + 0.88f * Mathf.Sqrt(1f - num * num);
				float t2 = 0.12f + 0.88f * Mathf.Sqrt(1f - num2 * num2);
				verts.AddUnsafe(new UnityEngine.Vector3(vector.x, Mathf.Lerp(vector.y, vector2.y, num), vector.z));
				verts.AddUnsafe(new UnityEngine.Vector3(vector.x, Mathf.Lerp(vector.y, vector2.y, num2), vector.z));
				verts.AddUnsafe(new UnityEngine.Vector3(Mathf.Lerp(vector.x, vector2.x, t2), Mathf.Lerp(vector.y, vector2.y, num2), vector.z));
				verts.AddUnsafe(new UnityEngine.Vector3(Mathf.Lerp(vector.x, vector2.x, t), Mathf.Lerp(vector.y, vector2.y, num), vector.z));
				uvs.AddUnsafe(new Vector2(vector3.x, Mathf.Lerp(vector3.y, vector4.y, num)));
				uvs.AddUnsafe(new Vector2(vector3.x, Mathf.Lerp(vector3.y, vector4.y, num2)));
				uvs.AddUnsafe(new Vector2(Mathf.Lerp(vector3.x, vector4.x, t2), Mathf.Lerp(vector3.y, vector4.y, num2)));
				uvs.AddUnsafe(new Vector2(Mathf.Lerp(vector3.x, vector4.x, t), Mathf.Lerp(vector3.y, vector4.y, num)));
				colors.AddCount(item, 4);
			}
		}
	}

	public static bool ApocalypsionAccess()
	{
		if (GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.IsCompleted(11, 10))
		{
			return true;
		}
		MessagePopup.Open("Content locked", UnlockMessage(PortalRenewalCategory.PunishmentRaid));
		return false;
	}

	public static void GodkingWindowBackground(ContentUIGodkingTrialDungeonList window)
	{
		Transform transform = window.transform.Find("Panel/Root/Bg");
		if (transform != null)
		{
			UITexture component = transform.GetComponent<UITexture>();
			if (component != null)
			{
				transform.gameObject.SetActive(value: true);
				component.color = Color.white;
				UIHelper.ResetPlaneAssetTextureAsync(component, component, UIHelper.GetPlainTexture("UI/Tower/GodKing_Bg"));
			}
		}
		PortalRenewal portalRenewal = UnityEngine.Object.FindObjectOfType<PortalRenewal>();
		if (portalRenewal != null && portalRenewal.IsOpen())
		{
			portalRenewal.SetEventHide(0f);
		}
	}

	public static void GodkingReturnToPortal()
	{
		PortalRenewal portalRenewal = UnityEngine.Object.FindObjectOfType<PortalRenewal>();
		if (portalRenewal != null && portalRenewal.IsOpen())
		{
			portalRenewal.SetEventHide(1f);
		}
	}

	public static bool DispatchShortcutVisible(bool hasDispatch)
	{
		return hasDispatch;
	}

	public static bool DispatchListHasEntries()
	{
		UserDispatchBattleInfoManager dispatchBattleInfoManager = GlobalAutoBehaviour<AccountManager>.instance.DispatchBattleInfoManager;
		if (dispatchBattleInfoManager != null && dispatchBattleInfoManager.Count() > 0)
		{
			return true;
		}
		MessagePopup.Open("Dispatch", "No dispatches in progress.");
		return false;
	}

	public static void RefreshHeroDeckDispatch(HeroDeck2 window)
	{
		HeroPortraitComponent[] componentsInChildren = window.GetComponentsInChildren<HeroPortraitComponent>(includeInactive: true);
		foreach (HeroPortraitComponent heroPortraitComponent in componentsInChildren)
		{
			if (heroPortraitComponent.HeroInfo != null)
			{
				heroPortraitComponent.SetDispatched(DispatchHelper.IsDispatchedHero(heroPortraitComponent.HeroInfo), heroPortraitComponent.PortraitMode == HeroPortraitComponent.EPortraitMode.Icon);
				FitHeroDeckDispatchLabel(heroPortraitComponent);
			}
		}
	}

	public static void FitHeroDeckDispatchLabel(HeroPortraitComponent portrait)
	{
		if (!portrait.IsDispatched || portrait.PortraitMode == HeroPortraitComponent.EPortraitMode.Icon)
		{
			return;
		}
		CustomLabelSubComponent subComponent = portrait.GetSubComponent<CustomLabelSubComponent>();
		if (!(subComponent == null) && !(subComponent.SpriteDiabled == null) && !(portrait.WidgetThis == null))
		{
			UIWidget spriteDiabled = subComponent.SpriteDiabled;
			UIWidget uIWidget = ((portrait.HeroBundleTex != null) ? portrait.HeroBundleTex.UiTex : null);
			if (uIWidget == null)
			{
				uIWidget = ((portrait.FrameWidget != null) ? portrait.FrameWidget : portrait.WidgetThis);
			}
			spriteDiabled.SetAnchor(uIWidget.gameObject, 0, 0, 0, 0);
			spriteDiabled.UpdateAnchors();
		}
	}

	public static ExpResultInfo ParseStaminaExp(IDictionary data)
	{
		IDictionary dictionary = ((data == null) ? null : (data["RaiderExpResult"] as IDictionary));
		if (dictionary != null)
		{
			return JM_NShared_ExpResultInfo.Parse(dictionary);
		}
		return null;
	}

	public static bool NeedsOrvelTransition(int tutorialIndex)
	{
		if (tutorialIndex != 95009 || NGame2.NAccount.UserCampaignManager.ChapterIndex != 96)
		{
			return false;
		}
		AccountManager instance = GlobalAutoBehaviour<AccountManager>.instance;
		if (instance.IsTutorialSkip)
		{
			return instance.CampaignManager.IsCompleted(96, 2);
		}
		return false;
	}

	public static bool OrvelTutorialAvailable(bool available, EventTriggerData data)
	{
		if (!available)
		{
			if (data != null && data.Index == 95009)
			{
				return NeedsOrvelTransition(95009);
			}
			return false;
		}
		return true;
	}

	public static bool TrySkippedOrvelTransition(TutorialManager manager, int tutorialIndex, Action onComplete, Action onCompleteLoad)
	{
		if (!NeedsOrvelTransition(tutorialIndex) || manager.CurrentTutorialRuntime != null)
		{
			return false;
		}
		onCompleteLoad?.Invoke();
		SceneAutoBehaviour<LobbyManagement>.instance.MoveMap(95, 2, onComplete);
		return true;
	}

	public static void UpdateGuildBuilding(MyGuildInfo guild, GuildBuildingInfo update)
	{
		if (guild == null || update == null)
		{
			return;
		}
		List<GuildBuildingInfo> list = new List<GuildBuildingInfo>(guild.GuildBuildingInfos ?? new GuildBuildingInfo[0]);
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].BuildingIndex == update.BuildingIndex)
			{
				list[i] = update;
				guild.GuildBuildingInfos = list.ToArray();
				return;
			}
		}
		list.Add(update);
		guild.GuildBuildingInfos = list.ToArray();
	}

	public static void RefreshGuildConstruction()
	{
		GuildConstruction[] array = UnityEngine.Object.FindObjectsOfType<GuildConstruction>();
		foreach (GuildConstruction guildConstruction in array)
		{
			if (guildConstruction.gameObject.activeInHierarchy)
			{
				guildConstruction.RefreshUI();
			}
		}
	}

	public static void OpenPortalGuildRankings()
	{
		SceneAutoBehaviour<GuildManagement2>.instance.OpenGuildRankingBoard(GuildRankingBoardType.SuppressRanking);
	}

	public static bool OpenShakmehBranch(string[] values)
	{
		if (values == null || values.Length == 0 || (values[0] != "Middle" && values[0] != "Final"))
		{
			return false;
		}
		if (!ShakmehAccess())
		{
			return true;
		}
		if (!GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.IsCompleted(50000, 601))
		{
			MessagePopup.Open("Content locked", "Complete The Shadow of Aegina story to unlock the Shakmeh boss shortcuts.");
			return true;
		}
		bool flag = values[0] == "Final";
		if (!ShakmehGaugeAccess(flag))
		{
			return true;
		}
		Type type = typeof(GameRoot).Assembly.GetType("NGame2.NUI.NManager.ShakmehDungeonManagement", throwOnError: true);
		object value = typeof(SceneAutoBehaviour<>).MakeGenericType(type).GetProperty("instance").GetValue(null, null);
		NGame2.NAccount.UserCampaignManager.ChapterIndex = 50000;
		NGame2.NAccount.UserCampaignManager.DungeonIndex = (flag ? 11 : 10);
		type.GetMethod("OpenShakmehDungeonWindow", new Type[2]
		{
			typeof(int),
			typeof(int)
		}).Invoke(value, new object[2]
		{
			50000,
			flag ? 11 : 10
		});
		return true;
	}

	public static bool UseShakmehWindow(PortalRenewalData menu)
	{
		if (menu.Category == PortalRenewalCategory.Shakmeh)
		{
			return menu.SubComponentShape == PortalRenewalCompType.Window;
		}
		return false;
	}

	public static bool ShakmehBossAccess(int chapter, int dungeon)
	{
		if (chapter != 50000 || (dungeon != 10 && dungeon != 11))
		{
			return true;
		}
		NGame2.NAccount.UserCampaignManager campaignManager = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager;
		if (dungeon == 10 && !campaignManager.IsCompleted(50000, 601))
		{
			MessagePopup.Open("Content locked", "Finish The Shadow of Aegina story by defeating Otherworldly Darkness Shakmeh first.");
			return false;
		}
		return ShakmehGaugeAccess(dungeon == 11);
	}

	public static bool ShakmehGaugeAccess(bool final)
	{
		UserManager userManager = GlobalAutoBehaviour<AccountManager>.instance.UserManager;
		CurrencyTypeData data = GameRoot.Instance.CurrencyTypeDataContainer.GetData(CurrencyType.ShakmehMiddleBossPoint);
		long userStat = userManager.GetUserStat(UserManager.UserStatType.ShakmehMiddleBossPoint);
		bool flag = data != null && userStat >= data.MaxValue;
		if (final == flag)
		{
			return true;
		}
		MessagePopup.Open("Shakmeh", final ? ("Shakmeh's Wrath is " + userStat + "/" + ((data == null) ? "600" : data.MaxValue.ToString()) + ". Defeat Devourer Shakmeh to fill it before challenging Otherworldly Darkness Shakmeh.") : "Otherworldly Darkness Shakmeh has appeared. Challenge him before returning to Devourer Shakmeh.");
		return false;
	}

	public static bool MeetsRaidHeroLevel(RaidData raid, int heroLevel)
	{
		if (raid != null && raid.ReqHeroLevel > 0)
		{
			return heroLevel >= raid.ReqHeroLevel;
		}
		return true;
	}

	public static bool CheckRaidHeroLevel(BattlePartySetting setting, HeroPortraitComponent portrait)
	{
		RaidPartySetting raidPartySetting = setting as RaidPartySetting;
		if (raidPartySetting == null || portrait == null || portrait.HeroInfo == null)
		{
			return true;
		}
		if (MeetsRaidHeroLevel(raidPartySetting.RaidData, portrait.HeroInfo.Level))
		{
			return true;
		}
		CenterMessage.Open("Requires a hero of level " + raidPartySetting.RaidData.ReqHeroLevel + " or higher.");
		return false;
	}

	public static List<CachedHeroInfo> EligibleRaidSavedHeroes(List<CachedHeroInfo> heroes, BattlePartySetting setting)
	{
		RaidPartySetting raidPartySetting = setting as RaidPartySetting;
		if (raidPartySetting == null || heroes == null)
		{
			return heroes;
		}
		List<CachedHeroInfo> list = new List<CachedHeroInfo>();
		foreach (CachedHeroInfo hero in heroes)
		{
			if (hero != null && MeetsRaidHeroLevel(raidPartySetting.RaidData, hero.Level))
			{
				list.Add(hero);
			}
		}
		return list;
	}

	public static void RestorePunishmentPartyFrames(PunishmentRaidPartySetting setting)
	{
		if (setting == null || setting.RaidData == null)
		{
			return;
		}
		Transform transform = setting.transform.Find("Panel/Root/Widget_SelectHeroList");
		if (transform == null)
		{
			return;
		}
		bool flag = setting.RaidData.Type == RaidType.PunishmentRaid && setting.RaidData.SubPartyCount > 0;
		Transform transform2 = transform.Find(flag ? "Deco/MainSubDeco/Panel_BG/RaidDeco" : "Deco/OnlyMainDeco/Panel_BG/RaidDeco");
		if (transform2 != null)
		{
			setting.RaidDeco = transform2;
			transform2.gameObject.SetActive(value: true);
		}
		if (!flag)
		{
			Transform transform3 = transform.Find("Deco/OnlyMainDeco/Sprite_Bg");
			Transform transform4 = transform.Find("Squard/MainSubSquard/MainSquard/Sprite_Bg/Sprite_Frame_Right");
			if (transform3 == null || transform4 == null || transform3.Find("SprkPartyFrame") != null)
			{
				return;
			}
			UISprite component = transform4.GetComponent<UISprite>();
			UISprite component2 = transform3.GetComponent<UISprite>();
			if (!(component == null) && !(component2 == null))
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(transform4.gameObject);
				gameObject.name = "SprkPartyFrame";
				gameObject.transform.SetParent(transform3, worldPositionStays: false);
				gameObject.transform.localPosition = UnityEngine.Vector3.zero;
				gameObject.transform.localRotation = Quaternion.identity;
				gameObject.transform.localScale = UnityEngine.Vector3.one;
				UISprite component3 = gameObject.GetComponent<UISprite>();
				component3.width = component2.width;
				component3.height = component2.height;
				component3.depth = component2.depth + 1;
				Collider[] componentsInChildren = gameObject.GetComponentsInChildren<Collider>(includeInactive: true);
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					componentsInChildren[i].enabled = false;
				}
				gameObject.SetActive(value: true);
			}
			return;
		}
		string[] array = new string[2] { "MainSquard", "SubSquard" };
		foreach (string text in array)
		{
			Transform transform5 = transform.Find("Squard/MainSubSquard/" + text + "/Sprite_Bg/Sprite_Frame_Right");
			if (transform5 != null)
			{
				transform5.gameObject.SetActive(value: true);
			}
		}
	}

	public static bool MenuShortcutKeyUp(KeyCode key)
	{
		UIInput selection = UIInput.selection;
		if (UICamera.inputHasFocus || (selection != null && selection.isActiveAndEnabled))
		{
			return false;
		}
		return Input.GetKeyUp(key);
	}

	public static bool AllowAutoGuildAttendance()
	{
		GuildManager instance = GlobalAutoBehaviour<GuildManager>.instance;
		if (instance == null || !instance.CheckGuildAttendanceRewardable)
		{
			return false;
		}
		AccountManager instance2 = GlobalAutoBehaviour<AccountManager>.instance;
		string text = instance2.GetCurrentServerLocalTime().ToString("yyyy-MM-dd");
		string key = "SprkGuildAttendanceShown/" + instance2.UserInfo.AccountId + "/" + instance.JoinedGuildId;
		if (SprkTestPrefs.GetString(key, "") == text)
		{
			return false;
		}
		SprkTestPrefs.SetString(key, text);
		SprkTestPrefs.Save();
		return true;
	}

	public static void SeedGuildRaidScore(NShared.EndCampaign.Response response)
	{
		if (response == null || response.GuildRaidMemberTotalScoreInfo == null)
		{
			return;
		}
		GuildManager instance = GlobalAutoBehaviour<GuildManager>.instance;
		AccountManager instance2 = GlobalAutoBehaviour<AccountManager>.instance;
		if (!(instance == null) && instance.MyGuildManager != null && !(instance2 == null))
		{
			GuildRaidManager guildRaidManager = instance.MyGuildManager.GuildRaidManager;
			GuildRaidMemberTotalScoreInfo guildRaidMemberTotalScoreInfo = response.GuildRaidMemberTotalScoreInfo;
			if (guildRaidMemberTotalScoreInfo.AccountId == instance2.UserInfo.AccountId && guildRaidManager.GetGuildRaidScoreInfo(guildRaidMemberTotalScoreInfo.AccountId) == null)
			{
				guildRaidManager.Apply(new GuildRaidMemberTotalScoreInfo
				{
					AccountId = guildRaidMemberTotalScoreInfo.AccountId,
					GuildId = guildRaidMemberTotalScoreInfo.GuildId,
					Score = 0L
				});
			}
		}
	}

	public static GuildSuppressSessionData ConquestSession(GuildSuppressSessionDataContainer container, int index)
	{
		if (index < 10000)
		{
			return container.GetData(index);
		}
		if (serverConquestSessions == null || !serverConquestSessions.TryGetValue(index, out var value))
		{
			return null;
		}
		return value;
	}

	public static List<GuildSuppressSessionData> ConquestSessions(List<GuildSuppressSessionData> original, int season)
	{
		if (serverConquestSeasons == null || !serverConquestSeasons.TryGetValue(season, out var value))
		{
			return original;
		}
		return new List<GuildSuppressSessionData>(value);
	}

	public static void WorkshopOptionDrag(StatGaugeInfo info)
	{
		if (info == null || info.GetComponentInParent<ValanceOptionRenewWindow>() == null)
		{
			return;
		}
		UIScrollView uIScrollView = NGUITools.FindInParents<UIScrollView>(info.gameObject);
		if (uIScrollView == null)
		{
			return;
		}
		Collider[] componentsInChildren = info.GetComponentsInChildren<Collider>(includeInactive: true);
		foreach (Collider collider in componentsInChildren)
		{
			UIDragScrollView uIDragScrollView = collider.GetComponent<UIDragScrollView>();
			if (uIDragScrollView == null)
			{
				uIDragScrollView = collider.gameObject.AddComponent<UIDragScrollView>();
			}
			uIDragScrollView.scrollView = uIScrollView;
		}
	}

	public static void ReadablePerkDescription(LearnTranscendSkillMessagePopup popup)
	{
		if (popup == null || popup.Label_SkillDesc == null)
		{
			return;
		}
		Transform transform = popup.transform.Find("Panel/Root");
		if (transform == null)
		{
			return;
		}
		UILabel label_SkillDesc = popup.Label_SkillDesc;
		label_SkillDesc.leftAnchor.target = null;
		label_SkillDesc.rightAnchor.target = null;
		label_SkillDesc.topAnchor.target = null;
		label_SkillDesc.bottomAnchor.target = null;
		label_SkillDesc.pivot = UIWidget.Pivot.TopLeft;
		label_SkillDesc.width = 594;
		label_SkillDesc.fontSize = 30;
		label_SkillDesc.useFloatSpacing = false;
		label_SkillDesc.spacingY = 4;
		label_SkillDesc.maxLineCount = 0;
		label_SkillDesc.overflowMethod = UILabel.Overflow.ResizeHeight;
		label_SkillDesc.ProcessText();
		float num = Mathf.Clamp(label_SkillDesc.height + 8, 130, 320);
		int num2 = (int)num + 400 + (popup.Label_Warning.gameObject.activeInHierarchy ? 100 : 0);
		float num3 = (float)num2 * 0.5f;
		float y = num3 - 115f - (num + 40f) * 0.5f;
		float num4 = num3 - num - 195f;
		string[] array = new string[2] { "Sprite_Frame", "Sprite_Frame/Sprite_Bg" };
		foreach (string n in array)
		{
			Transform transform2 = transform.Find(n);
			if (transform2 != null)
			{
				transform2.GetComponent<UIWidget>().height = num2;
			}
		}
		Transform transform3 = transform.Find("Sprite_Title");
		if (transform3 != null)
		{
			transform3.localPosition = new UnityEngine.Vector3(0f, num3 - 55f, 0f);
		}
		Transform transform4 = transform.Find("WidgetSkillIconName");
		if (transform4 != null)
		{
			transform4.localPosition = new UnityEngine.Vector3(-337.96f, num3 - 195f, 0f);
		}
		popup.Label_NeedSkillPoint.transform.localPosition = new UnityEngine.Vector3(-7f, num4, 0f);
		array = new string[2] { "Button_Ok", "Button_Cancel" };
		foreach (string n2 in array)
		{
			Transform transform5 = transform.Find(n2);
			if (transform5 != null)
			{
				transform5.localPosition = new UnityEngine.Vector3(transform5.localPosition.x, 0f - num3 + 80f, 0f);
			}
		}
		Transform transform6 = transform.Find("LearnSkillLabelBG");
		if (transform6 != null)
		{
			transform6.localPosition = new UnityEngine.Vector3(-1f, num4 - 90f, 0f);
		}
		Transform transform7 = transform.Find("WidgetSkillDesc");
		if (!(transform7 == null))
		{
			transform7.localPosition = new UnityEngine.Vector3(139f, 0f, 0f);
			Transform transform8 = transform7.Find("Sprite_Back");
			if (transform8 != null)
			{
				transform8.localPosition = new UnityEngine.Vector3(-52.8f, y, 0f);
				transform8.GetComponent<UIWidget>().height = (int)num + 40;
			}
			Transform transform9 = transform7.Find("ReadablePerkScroll");
			if (transform9 == null)
			{
				GameObject obj = new GameObject("ReadablePerkScroll")
				{
					layer = transform7.gameObject.layer
				};
				transform9 = obj.transform;
				transform9.SetParent(transform7, worldPositionStays: false);
				UIPanel uIPanel = obj.AddComponent<UIPanel>();
				uIPanel.depth = popup.GetComponentInChildren<UIPanel>().depth + 1;
				uIPanel.clipping = UIDrawCall.Clipping.SoftClip;
				uIPanel.clipSoftness = new Vector2(2f, 2f);
				UIScrollView uIScrollView = obj.AddComponent<UIScrollView>();
				uIScrollView.movement = UIScrollView.Movement.Vertical;
				uIScrollView.contentPivot = UIWidget.Pivot.TopLeft;
				uIScrollView.restrictWithinPanel = true;
				uIScrollView.disableDragIfFits = true;
			}
			transform9.localPosition = new UnityEngine.Vector3(-52.8f, y, 0f);
			transform9.GetComponent<UIPanel>().baseClipRegion = new Vector4(0f, 0f, 594f, num);
			label_SkillDesc.transform.SetParent(transform9, worldPositionStays: false);
			label_SkillDesc.leftAnchor.target = null;
			label_SkillDesc.rightAnchor.target = null;
			label_SkillDesc.topAnchor.target = null;
			label_SkillDesc.bottomAnchor.target = null;
			label_SkillDesc.pivot = UIWidget.Pivot.TopLeft;
			label_SkillDesc.transform.localPosition = new UnityEngine.Vector3(-297f, num * 0.5f, 0f);
			UIScrollView component = transform9.GetComponent<UIScrollView>();
			component.DisableSpring();
			component.currentMomentum = UnityEngine.Vector3.zero;
			component.InvalidateBounds();
			component.ResetPosition();
			NGUITools.AddWidgetCollider(label_SkillDesc.gameObject);
			UIDragScrollView uIDragScrollView = label_SkillDesc.GetComponent<UIDragScrollView>();
			if (uIDragScrollView == null)
			{
				uIDragScrollView = label_SkillDesc.gameObject.AddComponent<UIDragScrollView>();
			}
			uIDragScrollView.scrollView = component;
		}
	}

	public static bool HasEnhanceableSkill(NGame2.NAccount.Helper.SkillDetailInfo info)
	{
		if (info.BasicInfo.TableIndex > 0)
		{
			return info.BasicInfo.TableData != null;
		}
		return false;
	}

	public static long LegacyPunishmentDamage(long attack, long defense, int attackerLevel, int damagedLevel)
	{
		attackerLevel = System.Math.Min(attackerLevel, 80);
		damagedLevel = System.Math.Min(damagedLevel, 80);
		long accRatio = PunishmentDefenseRatios.GetAccRatio(1047294, attackerLevel * 3 - damagedLevel * 2 - 1);
		long val = defense * 2 * 1000000 / System.Math.Max(accRatio, 1L) + 1000;
		return attack * (20 + 980000 / System.Math.Max(val, 1L)) / 1000;
	}

	public static long PunishmentDamage(long attack, long defense, int attackerLevel, int damagedLevel, StatController receiver)
	{
		return LegacyPunishmentDamage(attack, defense, attackerLevel, damagedLevel);
	}

	public static SkillData PunishmentSkill(JitDataContainerDefault<int, SkillData> container, int index)
	{
		return container.GetData(index);
	}

	public static CreatureData CombatCreature(JitDataContainerDefault<int, CreatureData> container, int index)
	{
		return container.GetData(index);
	}

	public static SkillProjectileData CombatProjectile(JitDataContainerDefault<int, SkillProjectileData> container, int index)
	{
		return container.GetData(index);
	}

	public static StateData CombatState(JitDataContainerDefault<string, StateData> container, string key)
	{
		return container.GetData(key);
	}

	public static void AcceptServerConquestSessions(IDictionary response)
	{
		IList list = ((response == null) ? null : (response["SPRKConquestSessionDefinitions"] as IList));
		if (list == null)
		{
			return;
		}
		if (serverConquestSessions == null)
		{
			serverConquestSessions = new Dictionary<int, GuildSuppressSessionData>();
		}
		if (serverConquestSeasons == null)
		{
			serverConquestSeasons = new Dictionary<int, List<GuildSuppressSessionData>>();
		}
		foreach (IDictionary item in list)
		{
			GuildSuppressSessionData guildSuppressSessionData = JM_NShared_GuildSuppressSessionData.Parse(item);
			if (guildSuppressSessionData.SessionIndex < 10000)
			{
				continue;
			}
			serverConquestSessions[guildSuppressSessionData.SessionIndex] = guildSuppressSessionData;
			if (!serverConquestSeasons.TryGetValue(guildSuppressSessionData.SeasonIndex, out var value))
			{
				value = new List<GuildSuppressSessionData>();
				serverConquestSeasons.Add(guildSuppressSessionData.SeasonIndex, value);
			}
			bool flag = false;
			for (int i = 0; i < value.Count; i++)
			{
				if (value[i].SessionIndex == guildSuppressSessionData.SessionIndex)
				{
					value[i] = guildSuppressSessionData;
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				value.Add(guildSuppressSessionData);
			}
			value.Sort(CompareConquestSessions);
		}
	}

	public static bool CheckNativeStoryCondition(string[] condition)
	{
		if (condition == null || condition.Length != 5 || condition[0] != "CompleteTutorialOrSkippedDungeon" || condition[2] != "True" || !int.TryParse(condition[1], out var result) || !int.TryParse(condition[3], out var result2) || !int.TryParse(condition[4], out var result3))
		{
			return false;
		}
		AccountManager instance = GlobalAutoBehaviour<AccountManager>.instance;
		if (!instance.TutorialManager.IsComplete(result))
		{
			if (instance.IsTutorialSkip)
			{
				return instance.CampaignManager.IsCompleted(result2, result3);
			}
			return false;
		}
		return true;
	}

	public static void EnsureNativeShaderVariables()
	{
		if (GlobalAutoBehaviour<ShaderVariableUpdater>.instance != null)
		{
			ShaderVariableUpdater.UpdateGlobalVariable();
		}
	}

	public static void FinishStartupLogo()
	{
		if (!LogoScene.IsLogoPlayed)
		{
			return;
		}
		LogoScene[] array = UnityEngine.Object.FindObjectsOfType<LogoScene>();
		foreach (LogoScene logoScene in array)
		{
			if (logoScene.LogoWidget != null)
			{
				logoScene.LogoWidget.alpha = 0f;
			}
		}
	}

	public static bool IsOpenedDragonHard(RaidData raid)
	{
		if (raid == null || (raid.Type != RaidType.DragonHard && raid.Type != RaidType.DragonHardSingle) || raid.OpenCondition == null || raid.OpenCondition.Length < 2)
		{
			return false;
		}
		int num = raid.OpenCondition[0];
		int num2 = raid.OpenCondition[1];
		if (num != raid.Index)
		{
			num2 = System.Math.Max(num2, 10);
		}
		if (num <= 0 || num2 <= 0)
		{
			return false;
		}
		if (RaidHelper.GetRaidMaxLevel(num) >= num2)
		{
			return true;
		}
		foreach (RaidData allDatum in GameRoot.Instance.RaidDataContainer.GetAllData())
		{
			if (allDatum.Index == num && allDatum.SingleRaidIndex > 0 && RaidHelper.GetRaidMaxLevel(allDatum.SingleRaidIndex) >= num2)
			{
				return true;
			}
		}
		return false;
	}

	public static string DragonHardRequirement(RaidData raid)
	{
		if (raid == null || (raid.Type != RaidType.DragonHard && raid.Type != RaidType.DragonHardSingle) || IsOpenedDragonHard(raid))
		{
			return null;
		}
		int num = ((raid.OpenCondition != null && raid.OpenCondition.Length >= 2) ? raid.OpenCondition[1] : 0);
		if (num > 0 && raid.OpenCondition[0] != raid.Index)
		{
			num = System.Math.Max(num, 10);
		}
		RaidData raidData = null;
		if (num > 0)
		{
			foreach (RaidData allDatum in GameRoot.Instance.RaidDataContainer.GetAllData())
			{
				if (allDatum.Index == raid.OpenCondition[0] && allDatum.Level < num && (raidData == null || allDatum.Level > raidData.Level))
				{
					raidData = allDatum;
				}
			}
		}
		if (raidData == null)
		{
			return "This Hard raid is locked. Its normal raid prerequisite is unavailable.";
		}
		string stringDirect = LocalizationManager.GetStringDirect(raidData.Name);
		string stringDirect2 = LocalizationManager.GetStringDirect(raidData.Step_Name);
		return "Clear " + stringDirect + " (Normal), " + stringDirect2 + " once to unlock this Hard raid.";
	}

	public static bool CheckDragonHardEntry(RaidData raid)
	{
		string text = DragonHardRequirement(raid);
		if (text == null)
		{
			return true;
		}
		MessagePopup.Open(LocalizationManager.GetString(NShared.NLocalization.Common.INFORM), text);
		return false;
	}

	public static bool CheckDragonHardEntryByIndex(int raidIndex, int raidLevel)
	{
		return CheckDragonHardEntry(GameRoot.Instance.RaidDataContainer.GetData(raidIndex, raidLevel));
	}

	public static void LogRaidEntryFailure(NShared.BeginCampaign.Request request, NShared.BeginCampaign.Response response)
	{
		if (request == null || request.RaidIndex <= 0)
		{
			return;
		}
		try
		{
			RaidData data = GameRoot.Instance.RaidDataContainer.GetData(request.RaidIndex, request.RaidLevel);
			string text = "";
			if (request.HeroIndices != null)
			{
				int[] heroIndices = request.HeroIndices;
				foreach (int num in heroIndices)
				{
					text = text + ((text.Length == 0) ? "" : ",") + num;
				}
			}
			Debug.LogWarning("SPRK raid entry rejected: chapter=" + request.ChapterIndex + " dungeon=" + request.DungeonIndex + " linkedDungeon=" + request.LinkedDungeonIndex + " difficulty=" + request.DungeonDifficulty + " raid=" + request.RaidIndex + " level=" + request.RaidLevel + " heroes=" + text + " baseResult=" + ((response == null) ? "null" : response.BaseResult.ToString()) + " result=" + ((response == null) ? "null" : response.Result.ToString()) + " clientRaidType=" + ((data == null) ? "missing" : data.Type.ToString()) + " clientChapter=" + (data?.ChapterIndex ?? 0) + " clientDungeon=" + (data?.DungeonIndex ?? 0));
		}
		catch (Exception)
		{
		}
	}

	private static int CompareConquestSessions(GuildSuppressSessionData a, GuildSuppressSessionData b)
	{
		return a.SessionIndex.CompareTo(b.SessionIndex);
	}
}
