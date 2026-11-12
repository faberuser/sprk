using System;
using NGame2.NAccount;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NUI.NManager.NTower;

public class GatewayImpl : TowerManagement.IImplement
{
	private readonly TowerManagement _management;

	public GatewayImpl(TowerManagement management)
	{
		_management = management;
	}

	public void Open(TowerData towerData, TowerInfo towerInfo, int showFloor, bool showClearEffect, bool returnFromBattle, Action onOpen)
	{
		int maxStep = GameRoot.Instance.GatewayToTrialDataContainer.MaxStep;
		GatewayToTrialData dataByStep = GameRoot.Instance.GatewayToTrialDataContainer.GetDataByStep(maxStep);
		if (dataByStep == null)
		{
			_management.OpenGatewayUI(onOpen);
		}
		else if (GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.IsCompleted(dataByStep.ChapterIndex, dataByStep.DungeonIndex))
		{
			OpenEntryMenu();
		}
		else
		{
			_management.OpenGatewayUI(onOpen);
		}
	}

	private void OpenEntryMenu()
	{
		int chapterIndex = NGame2.NAccount.UserCampaignManager.ChapterIndex;
		int dungeonIndex = NGame2.NAccount.UserCampaignManager.DungeonIndex;
		SceneAutoBehaviour<EntryMenuManagement>.instance.OpenEntryMenu(chapterIndex, dungeonIndex, skipAni: false, playNPCSound: true, isIgnoreTrigger: false, null);
	}
}
