using System;
using System.Collections.Generic;
using NGame2.NLobby.NActivity;
using NShared;
using NVespa;

namespace NGame2.NLobby;

public class ActivityFactory : SingletonAuto<ActivityFactory>
{
	private Dictionary<string, Func<string[], IActivity>> _createDic = new Dictionary<string, Func<string[], IActivity>>();

	public ActivityFactory()
	{
		Register(ClientActivityType.MoveToNode, MoveToNode.Create);
		Register(ClientActivityType.MoveToLastDungeon, MoveToLastDungeon.Create);
		Register(ClientActivityType.MoveToLastHighLevelDungeon, MoveToLastHighLevelDungeon.Create);
		Register(ClientActivityType.OpenEntryMenu, OpenEntryMenu.Create);
		Register(ClientActivityType.OpenHero, OpenHero.Create);
		Register(ClientActivityType.OpenTopHeroInfo, OpenTopHeroInfo.Create);
		Register(ClientActivityType.OpenDeck, OpenDeck.Create);
		Register(ClientActivityType.OpenInventory, OpenInventory.Create);
		Register(ClientActivityType.OpenPayShop, OpenPayShop.Create);
		Register(ClientActivityType.OpenPortalSelect, OpenPortalSelect.Create);
		Register(ClientActivityType.OpenFriend, OpenFriend.Create);
		Register(ClientActivityType.OpenRaid, OpenRaid.Create);
		Register(ClientActivityType.OpenPunishmentRaid, OpenPunishmentRaid.Create);
		Register(ClientActivityType.OpenWorldBoss, OpenWorldBoss.Create);
		Register(ClientActivityType.CloseAllPopup, CloseAllPopup.Create);
		Register(ClientActivityType.CloseAllWindow, CloseAllWindow.Create);
		Register(ClientActivityType.OpenUrl, OpenUrl.Create);
		Register(ClientActivityType.MoveToLastConquestDungeon, MoveToLastConquestDungeon.Create);
		Register(ClientActivityType.OpenAccumulateLogin, OpenAccumulateLogin.Create);
		Register(ClientActivityType.OpenClassBuff, OpenClassBuff.Create);
		Register(ClientActivityType.OpenDungeonListPopup, OpenDungeonListPopup.Create);
		Register(ClientActivityType.MoveToNpc, MoveToNpc.Create);
		Register(ClientActivityType.OpenEclipse, OpenEclipse.Create);
		Register(ClientActivityType.OpenGodkingTrial, OpenGodkingTrial.Create);
		Register(ClientActivityType.OpenWorldmap, OpenWorldmap.Create);
		Register(ClientActivityType.OpenTower, OpenTower.Create);
		Register(ClientActivityType.OpenDungeonInfo, OpenDungeonInfo.Create);
		Register(ClientActivityType.OpenArena, OpenArena.Create);
		Register(ClientActivityType.OpenShakmehDungeon, OpenShakmehDungeon.Create);
		Register(ClientActivityType.OpenPrison, OpenPrison.Create);
		Register(ClientActivityType.OpenGuildArena, OpenGuildArena.Create);
		Register(ClientActivityType.OpenGuildRaid, OpenGuildRaid.Create);
		Register(ClientActivityType.OpenGuildSuppress, OpenGuildSuppress.Create);
		Register(ClientActivityType.OpenMazeTowerList, OpenMazeTowerList.Create);
		Register(ClientActivityType.OpenClearMissionPopup, OpenClearMissionPopup.Create);
		Register(ClientActivityType.OpenPurchaseMarketingGroupPopup, OpenPurchaseMarketingGroupPopup.Create);
		Register(ClientActivityType.OpenChallengeRaid, OpenChallengeRaid.Create);
		Register(ClientActivityType.UsingActionContainer, UsingActionContainer.Create);
		Register(ClientActivityType.OpenPetHouse, OpenPetHouse.Create);
		Register(ClientActivityType.OpenPetIncubator, OpenPetIncubator.Create);
		Register(ClientActivityType.OpenPetTotalBuffPopup, OpenPetTotalBuffPopup.Create);
		Register(ClientActivityType.OpenKingsPassWindow, OpenKingsPassWindow.Create);
		Register(ClientActivityType.OpenExternalURL, OpenExternalURL.Create);
		Register(ClientActivityType.OpenLegendCostumeTotalBuffPopup, OpenLegendCostumeTotalBuffPopup.Create);
	}

	private void Register(ClientActivityType type, Func<string[], IActivity> createFunc)
	{
		_createDic.Add(type.ToString(), createFunc);
	}

	public IActivity Create(string taskType, string[] taskValues)
	{
		return Vespa_Generic_IDictionary_Extension_Method.GETVALUE(_createDic, taskType, null)?.Invoke(taskValues);
	}
}
