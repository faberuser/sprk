using System;
using System.Collections.Generic;
using System.Linq;
using NGame2.NBattleContext;
using NGame2.NScene;
using NGame2.NScene.NLobby;
using NGame2.NUI;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow;
using NGameAsset;
using NShared;
using NShared.NLocalization;
using NVespa;
using NVespa.NNGUIExtension;
using NVespa.NSingleton;
using UnityEngine;

public static class SprkArenaPractice
{
	private sealed class PartyListener : BattlePartySetting.Listener
	{
		public void StartBattle(EnterDungeonData enter, BattlePartySetting.BattlePartyInfo party, BattlePartySetting.ExtraOptionData option)
		{
			try
			{
				PlayerEnterBattleLog enterPlayerBattleLog = CreateParty(party);
				if (GameRoot.Instance.CampaignDungeonDataContainer.GetData(Chapter, Dungeon) == null)
				{
					throw new Exception("Skill-test dummy dungeon is unavailable.");
				}
				ArenaDungeon();
				SingletonAuto<BattleContext>.instance.SetCustomCampaign(Chapter, Dungeon, NShared.DungeonDifficultyType.Easy);
				SingletonAuto<CustomCampaignContext>.instance.SetEnterPlayerBattleLog(enterPlayerBattleLog);
				SingletonAuto<CustomCampaignContext>.instance.SetBattleEndCallback(delegate
				{
					Finish();
				});
				LobbyScene2.ListReturnState.Clear();
				LobbyScene2.ListReturnState.Add(new ReturnState());
				Active = true;
				SingletonAuto<BattleContext>.instance.MoveToBattleScene();
			}
			catch (Exception ex)
			{
				Clear();
				MessagePopup.Open(SprkText.Get("Arena Practice"), ex.Message);
			}
		}

		public bool OnCloseRequest(BattlePartySetting window)
		{
			Clear();
			return true;
		}

		public void OpenHeroDetailView(CachedHeroInfo hero)
		{
		}

		public bool OpenAutoNextBattleSelect()
		{
			return false;
		}
	}

	private sealed class ReturnState : BaseEntryMenuReturnState
	{
		public override void Execute(LobbyScene2.ReturnStateInfo info, Action onEnd)
		{
			Clear();
			SceneAutoBehaviour<MatchManagement>.instance.OpenMatchSelect(immediately: true);
			onEnd?.Invoke();
		}
	}

	public const string CardName = "SPRK Arena Practice";

	public static bool Active { get; private set; }

	private static int Chapter => GameRoot.Instance.ConstantDataContainer.GetValueInt("HeroTryUseChapterIndex");

	private static int Dungeon => GameRoot.Instance.ConstantDataContainer.GetValueInt("HeroTryUseDungeonIndex");

	public static void Clear()
	{
		Active = false;
	}

	private static CampaignDungeonData ArenaDungeon()
	{
		return GameRoot.Instance.CampaignDungeonDataContainer.GetAllDataOfBattleType(BattleType.Arena).FirstOrDefault((CampaignDungeonData x) => (string)x.BattleDefineCode == "Arena") ?? throw new InvalidOperationException("League of Victory dungeon is unavailable.");
	}

	public static CampaignDungeonData CombatDungeon(CampaignDungeonData original)
	{
		if (!Active || original == null || original.ChapterIndex != Chapter || original.DungeonIndex != Dungeon)
		{
			return original;
		}
		CampaignDungeonData campaignDungeonData = JsonMarshaler.Clone(ArenaDungeon());
		campaignDungeonData.BattleField = original.BattleField;
		campaignDungeonData.BgmCode = original.BgmCode;
		campaignDungeonData.Constant = (campaignDungeonData.Constant ?? new string[0]).Concat(new string[1] { "InfiniteTime=true" }).ToArray();
		return campaignDungeonData;
	}

	public static EBattleMenu Menu(EBattleMenu original)
	{
		if (!Active)
		{
			return original;
		}
		return EBattleMenu.TimeStatus | EBattleMenu.SystemButton | EBattleMenu.DamageMeter | EBattleMenu.HeroParty | EBattleMenu.Hud | EBattleMenu.SystemButtonAutoSimple;
	}

	public static float Speed(float original)
	{
		if (!Active)
		{
			return original;
		}
		return 1f;
	}

	public static ERenderDamageText DamageText()
	{
		if (!Active)
		{
			return SystemOptionData.Render_DamageText;
		}
		return ERenderDamageText.ShowAll;
	}

	public static bool ShowDamage(bool original)
	{
		return Active || original;
	}

	public static BattleDefineData DisplayDefine(BattleDefineData original)
	{
		if (!Active)
		{
			return original;
		}
		return ArenaDungeon().GetBattleDefine(isScenario: false);
	}

	public static CreatureData CombatTarget(CreatureData original)
	{
		if (!Active || original == null || original.Index != 999999)
		{
			return original;
		}
		CreatureData creatureData = JsonMarshaler.Clone(original);
		creatureData.Race = CreatureRaceType.Hero;
		return creatureData;
	}

	public static BattleCreatureCreateInfo ProtectTarget(BattleCreatureCreateInfo info)
	{
		if (Active && info != null && info.HeroInfo != null && info.HeroInfo.HeroIndex == 999999)
		{
			info.ForceNoDie = true;
			info.ForceMinHp = 1L;
		}
		return info;
	}

	public static void AddCard(MatchSelect view)
	{
		if (view == null || view.Grid == null)
		{
			return;
		}
		Transform transform = view.Grid.transform.Find("SPRK Arena Practice");
		if (transform != null)
		{
			transform.gameObject.SetActive(value: true);
			return;
		}
		MatchSelect.ButtonGroup buttonGroup = view.PvpButtonGroups.FirstOrDefault((MatchSelect.ButtonGroup x) => x.ArenaType == MatchArenaType.Normal);
		MatchSelect.ButtonGroup buttonGroup2 = view.PvpButtonGroups.FirstOrDefault((MatchSelect.ButtonGroup x) => x.ArenaType == MatchArenaType.Ordeal);
		if (buttonGroup == null || buttonGroup.Button == null || buttonGroup2 == null || buttonGroup2.Button == null)
		{
			throw new InvalidOperationException("League of Victory or Ordeals card is unavailable.");
		}
		Transform transform2 = buttonGroup.Button.transform;
		while (transform2.parent != view.Grid.transform && transform2.parent != null)
		{
			transform2 = transform2.parent;
		}
		Transform transform3 = buttonGroup2.Button.transform;
		while (transform3.parent != view.Grid.transform && transform3.parent != null)
		{
			transform3 = transform3.parent;
		}
		if (transform2.parent == null || transform3.parent == null)
		{
			throw new InvalidOperationException("Arena card is outside the Arena grid.");
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(transform2.gameObject, view.Grid.transform, worldPositionStays: false);
		gameObject.name = "SPRK Arena Practice";
		gameObject.transform.SetSiblingIndex(transform3.GetSiblingIndex() + 1);
		string n = RelativePath(transform2, buttonGroup.Button.transform);
		UIButton component = gameObject.transform.Find(n).GetComponent<UIButton>();
		component.onClick.Clear();
		EventDelegate.Add(component.onClick, delegate
		{
			OpenParty();
		});
		component.gameObject.SetActive(value: true);
		component.isEnabled = true;
		if (buttonGroup.WidgetLocked != null)
		{
			gameObject.transform.Find(RelativePath(transform2, buttonGroup.WidgetLocked.transform)).gameObject.SetActive(value: false);
		}
		if (buttonGroup.Enable != null)
		{
			gameObject.transform.Find(RelativePath(transform2, buttonGroup.Enable.transform)).gameObject.SetActive(value: true);
		}
		if (buttonGroup.WidgetLockedBg != null)
		{
			UIHelper.SetWidgetGrayScale(gameObject.transform.Find(RelativePath(transform2, buttonGroup.WidgetLockedBg.transform)).GetComponent<UIWidget>(), grayScale: false);
		}
		UILabel[] componentsInChildren = gameObject.GetComponentsInChildren<UILabel>(includeInactive: true);
		foreach (UILabel uILabel in componentsInChildren)
		{
			UILocalizeEx component2 = uILabel.GetComponent<UILocalizeEx>();
			if (component2 != null)
			{
				component2.enabled = false;
				UnityEngine.Object.Destroy(component2);
			}
			string text = RelativePath(gameObject.transform, uILabel.transform).ToLowerInvariant();
			if (uILabel.transform.IsChildOf(component.transform))
			{
				uILabel.text = SprkText.Get("Practice");
			}
			else if (text.Contains("desc") || uILabel.name == "Label_Info")
			{
				uILabel.text = SprkText.Get("Test Arena damage with 4 heroes and equipped gear.");
			}
			else if (!text.Contains("lock"))
			{
				uILabel.text = SprkText.Get("Arena Practice");
			}
		}
		gameObject.SetActive(value: true);
		view.Grid.Reposition();
		UIHelper.RepositionGrid(view.ScrollView);
	}

	public static void SetSlots(MatchSelect view, int[] slots)
	{
		HashSet<int> hashSet = new HashSet<int>(slots ?? new int[0]);
		for (int i = 0; i < view.Sprite_Slot.Count; i++)
		{
			if (view.Sprite_Slot[i] != null)
			{
				view.Sprite_Slot[i].gameObject.SetActive(hashSet.Contains(i + 1));
			}
		}
		AddCard(view);
		view.Grid.Reposition();
	}

	private static string RelativePath(Transform root, Transform child)
	{
		List<string> list = new List<string>();
		while (child != root)
		{
			list.Insert(0, child.name);
			child = child.parent;
			if (child == null)
			{
				throw new Exception("Invalid card binding");
			}
		}
		return string.Join("/", list.ToArray());
	}

	public static void OpenParty()
	{
		Clear();
		MatchManagement instance = SceneAutoBehaviour<MatchManagement>.instance;
		DefaultBattlePartySettingDataProdiver dataProvider = new DefaultBattlePartySettingDataProdiver("SprkArenaPractice", StaminaType.None, 0, Battle.MATCH_START_BUTTON_LABEL, new EnterDungeonData());
		instance.GetInstance<BattlePartySetting>().Open(new PartyListener(), dataProvider);
	}

	public static PlayerEnterBattleLog CreateParty(BattlePartySetting.BattlePartyInfo party)
	{
		if (party == null || party.heroIndices == null || party.heroIndices.Length != 4 || party.heroIndices.Distinct().Count() != 4)
		{
			throw new InvalidOperationException(SprkText.Get("Select four different heroes for Arena Practice."));
		}
		PlayerEnterBattleLog playerEnterBattleLog = CampaignContext.CreateMyEnterPlayerBattleLog(party.heroIndices, null, party.leaderHeroIndex, null, null, null);
		if (playerEnterBattleLog == null || playerEnterBattleLog.CreatureCreateInfos == null || playerEnterBattleLog.CreatureCreateInfos.Length != 4)
		{
			throw new InvalidOperationException(SprkText.Get("The selected heroes are no longer available."));
		}
		return playerEnterBattleLog;
	}

	private static void Finish()
	{
		Clear();
		GlobalAutoBehaviour<LoadingManager>.instance.MoveScene(LoadingManager.ESceneType.Lobby);
	}
}
