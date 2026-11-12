using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NGame2.NAccount;
using NGame2.NBattleContext;
using NGame2.NBattleField.NManager.NPlay;
using NGame2.NBattleView.NUIOn3D;
using NGame2.NUI.NAtlasAssignComponent;
using NGame2.NUI.NManager.NBattle;
using NGame2.NUI.NWindow;
using NGameAsset;
using NShared;
using NShared.NCampaign;
using NShared.NJit;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SprkArenaPracticeProbe
{
	private static Transform visibleCard;

	private static string[] victoryArtwork;

	private static string[] Artwork(Transform root)
	{
		return (from x in root.GetComponentsInChildren<UISpriteAtlasAssigner>(includeInactive: true)
			select ((x.transform == root) ? "<root>" : x.name) + ":" + x.AtlasName + ":" + x.SpriteName into x
			orderby x
			select x).ToArray();
	}

	public static string VerifyLabels()
	{
		Check(visibleCard != null, "Run the native card fixture first");
		Check(visibleCard.GetComponentsInChildren<UILabel>(includeInactive: true).Single((UILabel l) => l.name == "Label_SlotTitle").text == "Arena Practice", "Localized Ordeals title replaced the practice title after Start/OnEnable");
		Check(visibleCard.GetComponentsInChildren<UILabel>(includeInactive: true).Any((UILabel l) => l.text == "Practice"), "Practice button label was replaced");
		Check(visibleCard.GetComponentsInChildren<UILabel>(includeInactive: true).Single((UILabel l) => l.name == "Label_Info").text.Contains("4 heroes"), "Practice description was replaced");
		Check(Artwork(visibleCard).SequenceEqual(victoryArtwork), "Practice artwork differs from League of Victory after OnEnable");
		return "PASS: practice title, description, button and League of Victory artwork survive native Start/OnEnable.";
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
		Check(SceneManager.GetActiveScene().name == "Login", "Probe requires Login");
		TableRoot.Instance.SetPatchDownLoadComplete(isComplete: true);
		GameRoot.Instance.SetJsonLoader(new JsonAssetAsyncLoader());
		GameRoot.Instance.SetAiScriptLoader(new TextAssetSyncLoader());
		GameRoot.Instance.InitForClient();
		GameRoot.Instance.OnDataContainerLoaded(editorVerify: false);
		GameObject gameObject = VespaResource.Load<GameObject>("Resources/Prefab2/Window/Match/MatchSelect");
		Check(gameObject != null, "Arena selection prefab missing");
		GameObject gameObject2 = new GameObject("ArenaPracticeFixture");
		gameObject2.SetActive(value: false);
		UIRoot uIRoot = UnityEngine.Object.FindObjectOfType<UIRoot>();
		if (uIRoot != null)
		{
			gameObject2.transform.SetParent(uIRoot.transform, worldPositionStays: false);
		}
		GameObject gameObject3 = UnityEngine.Object.Instantiate(gameObject, gameObject2.transform, worldPositionStays: false);
		MatchSelect component = gameObject3.GetComponent<MatchSelect>();
		try
		{
			component.SetArena(new int[3] { 1, 2, 5 });
			SprkArenaPractice.AddCard(component);
			component.SetArena(new int[3] { 1, 2, 5 });
			Check(component.Grid.transform.Cast<Transform>().Count((Transform x) => x.name == "SPRK Arena Practice") == 1, "Duplicate practice card after reopen");
			Transform transform = component.Grid.transform.Find("SPRK Arena Practice");
			Transform transform2 = component.PvpButtonGroups.First((MatchSelect.ButtonGroup x) => x.ArenaType == MatchArenaType.Normal).Button.transform;
			while (transform2.parent != component.Grid.transform)
			{
				transform2 = transform2.parent;
			}
			victoryArtwork = Artwork(transform2);
			File.WriteAllLines(Path.Combine(Environment.GetEnvironmentVariable("SPRK_AUTOMATION_DIR"), "card-components.txt"), (from t in transform2.GetComponentsInChildren<Transform>(includeInactive: true)
				select t.name + ": " + string.Join(", ", (from c in t.GetComponents<Component>()
					where c != null
					select c.GetType().FullName).ToArray())).ToArray());
			Check(victoryArtwork.Any((string x) => x.StartsWith("Sprite_Image:") && !x.EndsWith(":")), "League of Victory background binding unavailable");
			Check(Artwork(transform).SequenceEqual(victoryArtwork), "Practice card must reuse League of Victory artwork");
			Transform transform3 = component.PvpButtonGroups.First((MatchSelect.ButtonGroup x) => x.ArenaType == MatchArenaType.Ordeal).Button.transform;
			while (transform3.parent != component.Grid.transform)
			{
				transform3 = transform3.parent;
			}
			Check(transform.GetSiblingIndex() == transform3.GetSiblingIndex() + 1, "Practice card must follow Ordeals");
			Check(transform.GetComponentsInChildren<UIButton>(includeInactive: true).Any((UIButton b) => b.gameObject.activeSelf && b.isEnabled && b.onClick.Count == 1), "Practice entry button unavailable");
			string text = string.Join(" | ", (from l in transform.GetComponentsInChildren<UILabel>(includeInactive: true)
				select l.name + "=" + l.text).ToArray());
			File.WriteAllText(Path.Combine(Environment.GetEnvironmentVariable("SPRK_AUTOMATION_DIR"), "labels.txt"), text);
			int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("HeroTryUseChapterIndex");
			int valueInt2 = GameRoot.Instance.ConstantDataContainer.GetValueInt("HeroTryUseDungeonIndex");
			CampaignDungeonData data = GameRoot.Instance.CampaignDungeonDataContainer.GetData(valueInt, valueInt2);
			Check(data != null, "Native dummy dungeon missing");
			Check(SprkArenaPractice.CombatDungeon(data) == data, "Regular See Motions was modified");
			ERenderDamageText render_DamageText = SystemOptionData.Render_DamageText;
			Check(SprkArenaPractice.Menu(EBattleMenu.SystemButtonSpeed) == EBattleMenu.SystemButtonSpeed && SprkArenaPractice.Speed(1.5f) == 1.5f, "Nonpractice speed/menu settings were modified");
			SingletonAuto<BattleContext>.instance.SetCustomCampaign(valueInt, valueInt2, DungeonDifficultyType.Easy);
			typeof(SprkArenaPractice).GetProperty("Active").GetSetMethod(nonPublic: true).Invoke(null, new object[1] { true });
			CustomCampaignContext instance = SingletonAuto<CustomCampaignContext>.instance;
			EBattleMenu battleMenuType = ((IDataProvider)instance).GetBattleMenuType();
			Check((battleMenuType & EBattleMenu.Hud) != EBattleMenu.None && (battleMenuType & EBattleMenu.SystemButtonAutoSimple) != 0, "HUD or Auto-only widget missing from the actual provider menu");
			Check((battleMenuType & (EBattleMenu.SystemButtonAuto | EBattleMenu.SystemButtonSpeed | EBattleMenu.SystemButtonAutoFastContinue)) == 0, "Practice menu still exposes a speed control");
			Check(SprkArenaPractice.Speed(1.5f) == 1f && SprkArenaPractice.DamageText() == ERenderDamageText.ShowAll && SprkArenaPractice.ShowDamage(original: false), "Practice speed or damage-display policy missing");
			Check(SystemOptionData.Render_DamageText == render_DamageText, "Practice changed the saved damage-text preference");
			Check((string)((IDataProvider)instance).GetBattleDefine().Code == "Arena", "Presentation must use the Arena battle definition");
			BattleManagement instance2 = SceneAutoBehaviour<BattleManagement>.instance;
			BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.NonPublic;
			CampaignDungeonData campaignDungeonData = (CampaignDungeonData)instance2.GetType().GetMethod("GetCampaignDungeonData", bindingAttr).Invoke(instance2, new object[2] { valueInt, valueInt2 });
			Check(campaignDungeonData.BattleType == BattleType.Arena && (string)campaignDungeonData.BattleDefineCode == "Arena", "Battle UI still uses preview auto/speed rules");
			FieldInfo field = instance2.GetType().GetField("_basicGameSpeedRatio", bindingAttr);
			field.SetValue(instance2, 1.5f);
			Check((float)instance2.GetType().GetMethod("GetGameSpeedRatio", bindingAttr).Invoke(instance2, null) == 1f, "Native practice battle speed is not fixed to x1");
			field.SetValue(instance2, 1f);
			HUD component2 = VespaResource.Load<GameObject>("Resources/Prefab2/UIOn3D/HUD").GetComponent<HUD>();
			Check(component2 != null && component2.GetType().GetField("_spriteHp", bindingAttr).GetValue(component2) != null && component2.GetType().GetField("_buffGrid", bindingAttr).GetValue(component2) != null, "Native health/status HUD bindings are missing");
			GameObject gameObject4 = VespaResource.Load<GameObject>("Resources/Prefab2/Window/Battle/SystemButtonAutoSimple");
			Check(gameObject4 != null && !gameObject4.GetComponentsInChildren<UIButton>(includeInactive: true).Any((UIButton b) => b.name.ToLowerInvariant().Contains("speed")), "Auto-only prefab unexpectedly exposes a speed button");
			CampaignDungeonData campaignDungeonData2 = SprkArenaPractice.CombatDungeon(data);
			Check(campaignDungeonData2.BattleType == BattleType.Arena && (string)campaignDungeonData2.BattleDefineCode == "Arena", "Practice must use League of Victory combat rules");
			Check(campaignDungeonData2.BattleField == data.BattleField && campaignDungeonData2 != data, "Practice must preserve the dummy field on a private copy");
			BattleInstance battleInstance = new BattleInstance();
			battleInstance.SetCampaignDungeon(valueInt, valueInt2, 0, isScenario: false);
			Check(battleInstance.DungeonData.BattleType == BattleType.Arena && (string)battleInstance.BattleDefine.Code == "Arena", "Actual battle initialization bypassed Arena rules");
			Check(battleInstance.InfiniteBattleTime && battleInstance.InstanceConstant.MpRegenM == 0, "Practice must not inherit preview mana regeneration");
			GameRoot.Instance.BattleLogFactory.SetSerializer(JsonMarshaler.EncodeJsonString);
			GameRoot.Instance.BattleLogFactory.SetDeserializer(JsonMarshaler.Parse);
			CachedHeroInfo[] heroes = (from c in (from c in GameRoot.Instance.CreatureDataContainer.GetAllData()
					where c.Index < 10000 && c.Index > 0
					select c).Take(4)
				select new CachedHeroInfo(new HeroInfo
				{
					HeroIndex = c.Index,
					Star = 5,
					Level = 90,
					SkillLevel1 = 20,
					SkillLevel2 = 20,
					SkillLevel3 = 20,
					SkillLevel4 = 20
				})).ToArray();
			Check(heroes.Length == 4, "Four hero fixture unavailable");
			long stat = StatCalculator.GetEquippedHeroStat(heroes[0]).GetStat(StatType.PhysicalAttack);
			ItemData<EquipItemData> itemData = (from i in GameRoot.Instance.ItemDataContainer.GetAllData()
				select i as ItemData<EquipItemData>).First((ItemData<EquipItemData> i) => i != null && i.SubData != null && i.SubData.PartType == EquipPartType.Weapon);
			heroes[0].EquipItemInfo1 = new EquipItemInfo
			{
				SlotIndex = 1,
				ItemIndex = itemData.Index,
				Star = 0,
				Level = 0
			};
			BattleCreatureCreateInfo[] array = heroes.Select((CachedHeroInfo h) => BattleCreatureCreateInfo.CreateMyCreatureCreateInfo(h, h == heroes[0], isAi: false, null, null)).ToArray();
			Check(array.All((BattleCreatureCreateInfo i) => i != null), "Equipped hero stats missing");
			Check(array[0].HeroInfo.EquipItemInfo1.ItemIndex == itemData.Index, "Equipped weapon was stripped");
			Check(array[0].StatData.GetStat(StatType.PhysicalAttack) > stat, "Equipped weapon did not affect combat stats");
			AccountManager instance3 = GlobalAutoBehaviour<AccountManager>.instance;
			BindingFlags bindingAttr2 = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
			object value = instance3.GetType().GetProperty("AccountManagers", bindingAttr2).GetValue(instance3, null);
			value.GetType().GetField("UserManager").SetValue(value, new UserManager(instance3, new UserInfo
			{
				AccountId = 123456L
			}, new PlayerMiscInfo(), new PlayerGuildPointInfo()));
			heroes[0].EquipItemSlotIndex1 = 1;
			instance3.InitEquipItemInfos(new EquipItemInfo[1] { heroes[0].EquipItemInfo1 });
			instance3.InitHeroInfos(heroes.Cast<HeroInfo>().ToArray());
			PlayerEnterBattleLog playerEnterBattleLog = SprkArenaPractice.CreateParty(new BattlePartySetting.BattlePartyInfo
			{
				heroIndices = heroes.Select((CachedHeroInfo h) => h.HeroIndex).ToArray(),
				leaderHeroIndex = heroes[0].HeroIndex
			});
			Check(playerEnterBattleLog.CreatureCreateInfos.Length == 4 && playerEnterBattleLog.CreatureCreateInfos[0].HeroInfo.EquipItemInfo1.ItemIndex == itemData.Index, "Native Arena party factory lost gear");
			array = playerEnterBattleLog.CreatureCreateInfos;
			BattleCreatureCreateInfo[] array2 = array;
			foreach (BattleCreatureCreateInfo obj in array2)
			{
				obj.IsAi = true;
				obj.IsSmartAi = true;
			}
			BattleInstanceBuilder battleInstanceBuilder = new BattleInstanceBuilder();
			battleInstanceBuilder.SetCampaignDungeon(valueInt, valueInt2, 0);
			battleInstanceBuilder.SetEnterPlayerBattleLogList(new List<IBattleLogRuntime>
			{
				new PlayerEnterBattleLog
				{
					PlayerCreateInfo = new BattlePlayerCreateInfo
					{
						Id = 123456L,
						TeamType = TeamType.Left
					},
					CreatureCreateInfos = array
				},
				new PlayerEnterBattleLog
				{
					PlayerCreateInfo = new BattlePlayerCreateInfo
					{
						Id = 0L,
						TeamType = TeamType.Right
					}
				}
			});
			CampaignUpdater campaignUpdater = battleInstanceBuilder.BuildUpdater<BattleInstance>();
			campaignUpdater.StartBattle();
			for (int num2 = 0; num2 < 80; num2++)
			{
				campaignUpdater.Update();
				campaignUpdater.BattleInstance.UpdateTick(snapshotEnabled: false);
			}
			List<BattleCreature> allCreatures = campaignUpdater.BattleInstance.CreatureContainer.GetAllCreatures();
			Check(allCreatures.Count((BattleCreature c) => c.PlayerId == 123456) >= 4, "Four selected heroes did not spawn: " + string.Join(",", allCreatures.Select((BattleCreature c) => c.PlayerId + ":" + c.CreatureData.Index).ToArray()));
			Check(allCreatures.Any((BattleCreature c) => c.PlayerId == 0), "Native dummy did not spawn");
			BattleCreature battleCreature = allCreatures.First((BattleCreature c) => c.PlayerId == 0);
			Check(battleCreature.CreatureData.Race == CreatureRaceType.Hero && battleCreature.CreateInfo.ForceNoDie, "Dummy must activate PvP hero-target bonuses and survive testing");
			Check(GameRoot.Instance.CreatureDataContainer.GetData(999999).Race != CreatureRaceType.Hero, "Shared dummy table was changed");
			long num3 = campaignUpdater.BattleInstance.Snapshot.StatisticsManager.TotalDamagedMeter(battleCreature.Uid);
			Check(num3 > 0, "Four-hero combat did not record damage on the dummy");
			string text2 = "Dummy=" + battleCreature.CreatureData.Index + ", race=" + battleCreature.CreatureData.Race.ToString() + ", noDie=" + battleCreature.CreateInfo.ForceNoDie + ", damage=" + num3;
			SprkArenaPractice.Clear();
			Check(SprkArenaPractice.Speed(1.5f) == 1.5f && !SprkArenaPractice.ShowDamage(original: false) && SprkArenaPractice.DamageText() == render_DamageText, "Practice display overrides leaked after exit");
			BattleInstance battleInstance2 = new BattleInstance();
			battleInstance2.SetCampaignDungeon(valueInt, valueInt2, 0, isScenario: false);
			Check(battleInstance2.DungeonData.BattleType == data.BattleType && battleInstance2.BattleDefine.Code == data.BattleDefineCode, "PvP rules leaked into regular skill testing");
			component.enabled = false;
			gameObject3.SetActive(value: true);
			gameObject2.SetActive(value: true);
			UIPanel[] componentsInChildren = gameObject3.GetComponentsInChildren<UIPanel>(includeInactive: true);
			for (int num = 0; num < componentsInChildren.Length; num++)
			{
				componentsInChildren[num].alpha = 1f;
			}
			File.WriteAllText(Path.Combine(Environment.GetEnvironmentVariable("SPRK_AUTOMATION_DIR"), "layout.txt"), "Ordeals=" + transform3.localPosition.ToString() + ", practice=" + transform.localPosition.ToString() + ", sorting=" + component.Grid.sorting);
			gameObject3.SetActive(value: false);
			GameObject gameObject5 = new GameObject("ArenaCardPreview");
			gameObject5.AddComponent<UIPanel>().depth = 1000;
			transform3.SetParent(gameObject5.transform, worldPositionStays: false);
			transform3.localPosition = new Vector3(-290f, 0f, 0f);
			transform.SetParent(gameObject5.transform, worldPositionStays: false);
			transform.localPosition = new Vector3(290f, 0f, 0f);
			visibleCard = transform;
			transform3.gameObject.SetActive(value: true);
			transform.gameObject.SetActive(value: true);
			Transform[] componentsInChildren2 = gameObject5.GetComponentsInChildren<Transform>(includeInactive: true);
			for (int num = 0; num < componentsInChildren2.Length; num++)
			{
				componentsInChildren2[num].gameObject.layer = 29;
			}
			UITweener[] componentsInChildren3 = gameObject5.GetComponentsInChildren<UITweener>(includeInactive: true);
			for (int num = 0; num < componentsInChildren3.Length; num++)
			{
				componentsInChildren3[num].enabled = false;
			}
			UIWidget[] componentsInChildren4 = gameObject5.GetComponentsInChildren<UIWidget>(includeInactive: true);
			for (int num = 0; num < componentsInChildren4.Length; num++)
			{
				componentsInChildren4[num].alpha = 1f;
			}
			GameObject gameObject6 = new GameObject("ArenaCardPreviewCamera");
			Camera camera = gameObject6.AddComponent<Camera>();
			camera.orthographic = true;
			camera.orthographicSize = 540f;
			camera.transform.position = new Vector3(0f, 0f, -1000f);
			camera.nearClipPlane = 0.1f;
			camera.farClipPlane = 2000f;
			camera.cullingMask = 536870912;
			camera.clearFlags = CameraClearFlags.Color;
			camera.backgroundColor = new Color(0.07f, 0.08f, 0.1f, 1f);
			camera.depth = 1000f;
			gameObject6.AddComponent<UICamera>();
			return "PASS: native Arena prefab, Victory artwork, card ordering/reuse, HUD/status bindings, Auto-only controls, native x1 speed, practice damage text, four-hero equipped combat and preview isolation. " + text2 + ". Labels: " + text;
		}
		finally
		{
			SprkArenaPractice.Clear();
		}
	}
}
