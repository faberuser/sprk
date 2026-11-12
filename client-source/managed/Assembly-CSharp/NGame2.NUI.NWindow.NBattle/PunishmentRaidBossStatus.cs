using UnityEngine;

namespace NGame2.NUI.NWindow.NBattle;

public class PunishmentRaidBossStatus : BossStatus
{
	public new const string PrefabPath = "Resources/Prefab2/Window/Battle/PunishmentRaidBossStatus";

	public GameObject EmptyMpRoot;

	public GameObject MpGagueRoot;

	protected override void SetupRoot()
	{
		base.SetupRoot();
		bool showBossManaGauge = _battleDefineData.ShowBossManaGauge;
		UIHelper.EnableGameObject(EmptyMpRoot, !showBossManaGauge);
		UIHelper.EnableGameObject(MpGagueRoot, showBossManaGauge);
	}

	public void TempClose()
	{
		Close2(immediately: true);
	}
}
