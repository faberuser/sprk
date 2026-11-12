using System;
using System.Collections.Generic;
using System.Linq;
using NGame2.NUI.NWindow.NEntryMenu.NAction;
using NVespa;

namespace NGame2.NUI.NWindow.NEntryMenu;

public class ActionContainer : SingletonAuto<ActionContainer>
{
	private readonly Dictionary<string, IAction> _actionDictionary = new Dictionary<string, IAction>();

	public ActionContainer()
	{
		Register(new EnterClassTrialSelect());
		Register(new EnterPrison());
		Register(new EnterPurifyRoom());
		Register(new OpenShop());
		Register(new OpenTower());
		Register(new OpenMazeTowerList());
		Register(new SelectArena());
		Register(new ViewArenaRewardInfo());
		Register(new OpenGuild());
		Register(new OpenEquipAwaken());
		Register(new OpenEquipOptionUpgrade());
		Register(new OpenEquipOptionRenew());
		Register(new OpenScoutHero());
		Register(new CreateGuild());
		Register(new FindGuild());
		Register(new GuildInform());
		Register(new GuildRaid());
		Register(new NGame2.NUI.NWindow.NEntryMenu.NAction.GuildConstruction());
		Register(new GuildAttendance());
		Register(new OpenRestoreArtifact());
		Register(new EnterClassTrialSelectTranscend());
		Register(new EnterPurifyRoomTranscend());
		Register(new EnterEventDungeon());
		Register(new OpenNPCRecruiteView());
		Register(new OpenTierUpgrade());
		Register(new OpenSubMenu());
		Register(new OpenEquipEnchantOption());
		Register(new OpenEquipLimitBreak());
		Register(new OpenEventDungeon());
		Register(new OpenEventDungeonSelect());
		Register(new OpenMainStory());
		Register(new OpenHelp());
		Register(new OpenSubStory());
		Register(new OpenGuildSuppress());
		Register(new OpenGuildRankingBoard());
		Register(new OpenGuildTrainingCenter());
		Register(new OpenGuildSuppressBanList());
		Register(new OpenGuildArena());
		Register(new OpenWorldBoss());
		Register(new OpenArena());
		Register(new OpenArenaBanPick());
		Register(new OpenDungeonInfo());
		Register(new OpenNPCGiftView());
		Register(new NGame2.NUI.NWindow.NEntryMenu.NAction.OpenHelpList());
		Register(new OpenMonthlyHero());
		Register(new OpenGatewaytoTrial());
		Register(new OpenGodkingTrial());
		Register(new OpenEclipse());
		Register(new OpenSoulWeapon());
		Register(new OpenEventStepRewardView());
		Register(new OpenPhaseStepRewardInfoPopup());
		Register(new OpenShakmehDungeon());
		Register(new OpenMemory());
		Register(new OpenParttimeDungeonList());
		Register(new OpenValanceMainLobby());
		Register(new NGame2.NUI.NWindow.NEntryMenu.NAction.OpenValanceCraft());
		Register(new OpenValanceIndentified());
		Register(new OpenValanceManage());
		Register(new NGame2.NUI.NWindow.NEntryMenu.NAction.OpenValanceManageInven());
		Register(new NGame2.NUI.NWindow.NEntryMenu.NAction.OpenValanceCraftInven());
		Register(new NGame2.NUI.NWindow.NEntryMenu.NAction.OpenValanceIdentifiedInven());
		Register(new NGame2.NUI.NWindow.NEntryMenu.NAction.OpenValanceAwakenInven());
		Register(new NGame2.NUI.NWindow.NEntryMenu.NAction.OpenValanceTierUpInven());
		Register(new NGame2.NUI.NWindow.NEntryMenu.NAction.OpenValanceRenewInven());
		Register(new NGame2.NUI.NWindow.NEntryMenu.NAction.OpenValanceEnchantInven());
		Register(new OpenOrdealArena());
		Register(new OpenGuildSuppressStatusBoard());
		Register(new OpenEventRoulette());
		Register(new OpenEventCraft());
		Register(new OpenTreasureHouse());
		Register(new OpenEventForge());
		Register(new OpenEventGrowWorldTree());
		Register(new OpenPunishmentRuneCraft());
	}

	public void Register<T>(T action) where T : IAction
	{
		string name = action.GetType().Name;
		_actionDictionary.Remove(name);
		_actionDictionary.Add(name, action);
	}

	public IAction GetAction(string name)
	{
		_actionDictionary.TryGetValue(name, out var value);
		return value;
	}

	public IAction GetAction(Type type)
	{
		string name = type.Name;
		return GetAction(name);
	}

	public string[] GetKeys()
	{
		return _actionDictionary.Keys.ToArray();
	}
}
