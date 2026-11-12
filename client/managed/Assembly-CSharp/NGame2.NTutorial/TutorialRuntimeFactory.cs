using System;
using System.Collections.Generic;
using NGame2.NTutorial.NRuntime;
using NVespa.NSingleton;

namespace NGame2.NTutorial;

public class TutorialRuntimeFactory : SceneAutoBehaviour<TutorialRuntimeFactory>
{
	private readonly Dictionary<string, Func<ITutorialRuntime>> _creatorDictionary = new Dictionary<string, Func<ITutorialRuntime>>();

	public TutorialRuntimeFactory()
	{
		Register<EnterBattle>();
		Register<InitBattleHero>();
		Register<InitBattleHero2>();
		Register<InitMyBattleHero>();
		Register<OpenBattlePartySetting>();
		Register<ClearBattlePartySettingHero>();
		Register<HideDifficultySelect>();
		Register<SelectBattlePartySettingHero>();
		Register<PauseBattle>();
		Register<ResumeBattle>();
		Register<CloseBattleMenu>();
		Register<UseSkill>();
		Register<UseEventSkill>();
		Register<WaitCanUseSkill>();
		Register<SetMana>();
		Register<SetStack>();
		Register<EventMoveHero>();
		Register<SetBattleCamera>();
		Register<RemoveFromBattle>();
		Register<ShowBattleMenu>();
		Register<EndBattleWin>();
		Register<EndBattleReward>();
		Register<UseTeamSkill>();
		Register<CancelTeamSkill>();
		Register<WaitDefeatAllEnemy>();
		Register<ResetCooltime>();
		Register<SetHeroEquip>();
		Register<SetHeroEquipAuto>();
		Register<SetHeroSkillLevel>();
		Register<SetHeroSkillExtend>();
		Register<UpgradeHeroEquip>();
		Register<OpenHeroWindow>();
		Register<AddEquipItem>();
		Register<AddItem>();
		Register<RemoveEquipItem>();
		Register<GetNewHero>();
		Register<AddQuest>();
		Register<AcceptQuest>();
		Register<LeaveCity>();
		Register<ZoomMapNode>();
		Register<CompleteTutorial>();
		Register<NextTutorial>();
		Register<WorldMapPlayerMove>();
		Register<ResetStoredPlayerPos>();
		Register<PlayCutScene>();
		Register<WaitCutScene>();
		Register<PlayNodeTree>();
		Register<StopNodeTree>();
		Register<WaitNodeTree>();
		Register<NodeTreeNextFlow>();
		Register<NodeTreeSetFlow>();
		Register<WaitEvent>();
		Register<WaitTime>();
		Register<EnterLobby>();
		Register<ReserveLobbyPopup>();
		Register<OpenMenuOpenPopup>();
		Register<OpenLeftMenu>();
		Register<OpenCurrentNode>();
		Register<PlaySound>();
		Register<PlayBgm>();
		Register<SetFunctionality>();
		Register<ResetFunctionality>();
		Register<PlayVideo>();
		Register<WaitVideo>();
		Register<AnimatorSetBool>();
		Register<OpenImageWindow>();
		Register<CloseImageWindow>();
		Register<ClearFocus>();
		Register<ClearTooltip>();
		Register<CloseAllPopup>();
		Register<CloseAllWindow>();
		Register<WaitWindowOpen>();
		Register<CloseWindow>();
		Register<EnableGameUi>();
		Register<EnableUiByPosition>();
		Register<EnableUiByPath>();
		Register<FocusArea>();
		Register<FocusAreaEffect>();
		Register<FocusWidget>();
		Register<FocusWidgetEffect>();
		Register<ShowIconTooltip>();
		Register<ShowIconTooltip2>();
		Register<ShowTooltip>();
		Register<ShowTooltip2>();
		Register<ShowTooltipArrow>();
		Register<ShowUiByPath>();
		Register<WaitObject>();
		Register<WaitHideObject>();
		Register<WaitTouch>();
		Register<WaitWidgetTouch>();
		Register<WaitWidgetLongTouch>();
		Register<OpenEntryMenu>();
		Register<SceneChangeEffect>();
		Register<FocusWidgetMove>();
		Register<OpenTrialHeroSelect>();
		Register<OpenTrialEnterDungeon>();
		Register<OpenPurifyHeroSelect>();
		Register<OpenPurifyHeroRoom>();
		Register<FillPurifyHeroGauge>();
		Register<AwakePurifiedHero>();
		Register<OpenShopCategory>();
		Register<OpenEquipAwaken>();
		Register<AwakeHeroEquip>();
		Register<RemoveMaterialEquipItem>();
		Register<EquipRuneItem>();
		Register<CloseEquipAwakenScene>();
		Register<AddCustomEquipItem>();
		Register<OpenRecommendOptionPopup>();
		Register<OpenRenewProcessItem>();
		Register<OpenRenewProcessPopup>();
		Register<OpenRenewPricePopup>();
		Register<ChangeItemOption>();
		Register<ChangeItemOptionResult>();
		Register<OpenRaidPartySetting>();
		Register<OpenRaidLevelSelect>();
		Register<WaitBattleHero>();
		Register<AddBattleHero>();
		Register<InitRaidBattleHero>();
		Register<EndRaidBattleReward>();
		Register<EnableGetMultiRewardButton>();
		Register<GetMultiplayReward>();
		Register<OpenEntryMenuShop>();
		Register<OpenMaze>();
		Register<OpenSuppressBoss>();
		Register<ChangeSuppressButton>();
		Register<OpenSuppressList>();
		Register<OpenSuppressPartySetting>();
		Register<SkipStart>();
		Register<SkipEnd>();
		Register<SkipTime>();
		Register<OpenCraftItemPopup>();
		Register<OpenGameGuide>();
		Register<OpenHelpList>();
		Register<MovePortalChapter>();
		Register<OpenEclipsePartySetting>();
		Register<OpenSoulWeaponLiberation>();
		Register<OpenSanctuaryOfGodking>();
		Register<OpenUnderPrisonList>();
		Register<MoveScrollFocus>();
		Register<MoveScrollFocus2>();
		Register<OpenPetIncubator>();
		Register<ResetScrollView>();
		Register<NGame2.NTutorial.NRuntime.PunishmentRuneCraft_GoToNext>();
	}

	private void Register<T>() where T : ITutorialRuntime, new()
	{
		string key = typeof(T).Name;
		_creatorDictionary.Remove(key);
		_creatorDictionary.Add(key, () => new T());
	}

	public ITutorialRuntime Create(string commandType)
	{
		if (String_Extension_Method.IsNullOrEmpty(commandType))
		{
			return null;
		}
		Func<ITutorialRuntime> value = null;
		_creatorDictionary.TryGetValue(commandType, out value);
		if (value == null)
		{
			return null;
		}
		return Vespa_System_Action_Extension_Method.fire(value);
	}
}
