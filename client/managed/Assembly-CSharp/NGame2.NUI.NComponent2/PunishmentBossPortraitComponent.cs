using NShared;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class PunishmentBossPortraitComponent : LinkFactoryComponent, InitOnCreate
{
	public static string PrefabPath = "Resources/Prefab2/Component/Raid/PunishmentRaidBossPortraitComponent";

	public static PrefabAuto<PunishmentBossPortraitComponent> BossPrefabAuto = new PrefabAuto<PunishmentBossPortraitComponent>(PrefabPath);

	public static string PrefabPath2 = "Resources/Prefab2/Component/Raid/PunishmentRaidMonsterPortraitComponent";

	public static PrefabAuto<PunishmentBossPortraitComponent> MonsterPrefabAuto = new PrefabAuto<PunishmentBossPortraitComponent>(PrefabPath2);

	public UIWidget RootWidget;

	public UIBundleLoader BossBundleLoader;

	private Transform _transform;

	public GameObject LeftLineRoot;

	public GameObject RightLineRoot;

	void InitOnCreate.OnCreate()
	{
		_transform = base.transform;
	}

	void InitOnCreate.OnRelease()
	{
		_transform = null;
	}

	public void Open(PunishmentRaidData data)
	{
		if (data != null)
		{
			int portraitPositionX = data.PortraitPositionX;
			int portraitPositionY = data.PortraitPositionY;
			_transform.localPosition = new Vector3(portraitPositionX, portraitPositionY, 0f);
			string bossMiniPortraitPath = data.BossMiniPortraitPath;
			UIHelper.SetBundleTexture(BossBundleLoader, bossMiniPortraitPath);
			UIHelper.EnableGameObject(LeftLineRoot, data.PortraitLineType == EPunishmentPortraitLineType.Left);
			UIHelper.EnableGameObject(RightLineRoot, data.PortraitLineType == EPunishmentPortraitLineType.Right);
		}
	}

	public void SetAlreadyClear(bool alreadyClear)
	{
		UIHelper.SetWidgetGrayScale(RootWidget, alreadyClear);
	}
}
