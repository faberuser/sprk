using System;
using NGame2.NAccount;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow.NPunish;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NScene.NLobby;

public class PunishmentRaidReturnState : LobbyScene2.IReturnState
{
	public RaidData RaidData;

	public PunishmentGroupData PunishmentGroupData;

	public int[] AffixIndices { get; set; }

	public string BgmCode => string.Empty;

	public int OpenLevel { get; set; }

	void LobbyScene2.IReturnState.Execute(LobbyScene2.ReturnStateInfo info, Action onEnd)
	{
		if ((info.OpenFlag & LobbyScene2.ReturnStateOpenFlag.OpenNewbieShop) != LobbyScene2.ReturnStateOpenFlag.None)
		{
			SceneAutoBehaviour<PayShopManagement>.instance.OpenNewPayShop(NewPayShopCategoryType.Beginner.ToString());
		}
		if ((info.OpenFlag & LobbyScene2.ReturnStateOpenFlag.OpenEventShop) != LobbyScene2.ReturnStateOpenFlag.None)
		{
			SceneAutoBehaviour<PayShopManagement>.instance.OpenNewPayShop(NewPayShopCategoryType.Event.ToString());
		}
		else if (RaidData != null && PunishmentGroupData != null)
		{
			OpenPunishmentRaidInfo openPunishmentRaidInfo = GlobalAutoBehaviour<AccountManager>.instance.OpenPunishmentRaidManager.Get(PunishmentGroupData.GroupIndex, PunishmentGroupData.PunishmentDungeonType);
			if (openPunishmentRaidInfo == null || !NGame2.NUI.NWindow.NPunish.Helper.IsOpenPunishRaid(openPunishmentRaidInfo))
			{
				SceneAutoBehaviour<PunishManagment>.instance.OpenPunishMainWindow();
				Vespa_System_Action_Extension_Method.fire(onEnd);
				return;
			}
			if (PunishmentGroupData.PunishmentDungeonType == EPunishmentDungeonType.Karma)
			{
				SceneAutoBehaviour<PunishManagment>.instance.OpenByKarmaFailReturnState(PunishmentGroupData, RaidData);
				Vespa_System_Action_Extension_Method.fire(onEnd);
				return;
			}
			int index = RaidData.Index;
			int level = RaidData.Level;
			PunishmentRaidInfo punishmentRaidInfo = GlobalAutoBehaviour<AccountManager>.instance.OpenPunishmentRaidManager.GetPunishmentRaidInfo(PunishmentGroupData.GroupIndex, index, level);
			if (punishmentRaidInfo != null && punishmentRaidInfo.ClearCheckIndex != 0 && !String_Extension_Method.IsNullOrEmpty(punishmentRaidInfo.ClearedTime))
			{
				if (GameRoot.Instance.PunishmentRaidDataContainer.GetData(index, level) == null)
				{
					Vespa_System_Action_Extension_Method.fire(onEnd);
					return;
				}
				SceneAutoBehaviour<PunishManagment>.instance.OpenByBossClearReturnState(PunishmentGroupData, RaidData, OpenLevel);
				Vespa_System_Action_Extension_Method.fire(onEnd);
				return;
			}
			SceneAutoBehaviour<PunishManagment>.instance.OpenByBossFailReturnState(PunishmentGroupData, RaidData, OpenLevel, AffixIndices);
		}
		else
		{
			SceneAutoBehaviour<PunishManagment>.instance.OpenByScenarioReturnState(PunishmentGroupData);
		}
		Vespa_System_Action_Extension_Method.fire(onEnd);
	}
}
