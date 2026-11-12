using System.Collections;
using NGame2.NContentsInfo;
using NGame2.NUI.NHelper.NPortal;
using NGameAsset;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class PortalWorldBossComponent : PortalContentsComponent
{
	public const string PrefabPath = "Resources/Prefab2/Window/Portal/PortalRenewalComponents/PortalWorldBossComponent";

	public static PrefabAuto<PortalWorldBossComponent> PrefabAuto = new PrefabAuto<PortalWorldBossComponent>("Resources/Prefab2/Window/Portal/PortalRenewalComponents/PortalWorldBossComponent");

	[Header("[PortalWorldBossComponent]")]
	public UILabel WorldBossNameLabel;

	public UIScrollView DescScrollView;

	private string BackgroundCode
	{
		get
		{
			WorldBossData worldBossData = WorldBossData;
			if (worldBossData == null)
			{
				return string.Empty;
			}
			return worldBossData.PortalImagePath;
		}
	}

	private WorldBossData WorldBossData => SingletonUpdater<WorldBoss>.instance.GetCurrentWorldBossData();

	protected override void Init()
	{
		base.Init();
		InitBackGround();
		SetWorldBossNameLabel();
	}

	public override void ResolutionComp(float contentsWidth)
	{
		base.ResolutionComp(contentsWidth);
		VespaCoroutine.Start(coDescLabelBugFix());
	}

	private IEnumerator coDescLabelBugFix()
	{
		UIHelper.EnableGameObject(_basicData.ContentsDescriptionLabel, enable: false);
		yield return null;
		if (!String_Extension_Method.IsNullOrEmpty(base._portalRenewalData.DescLocalKey))
		{
			UIHelper.EnableGameObject(_basicData.ContentsDescriptionLabel, enable: true);
			DescScrollView.ResetPosition();
		}
	}

	private void InitBackGround()
	{
		if (_basicData != null && !(_basicData.ContentsTexture == null))
		{
			AssetTexture newPortalBackground = PortalHelper.GetNewPortalBackground(BackgroundCode);
			UIHelper.ResetPlaneAssetTextureAsync(this, _basicData.ContentsTexture, newPortalBackground);
		}
	}

	private void SetWorldBossNameLabel()
	{
		UIHelper.SetLabelText(WorldBossNameLabel, "-");
		WorldBossData worldBossData = WorldBossData;
		if (worldBossData != null)
		{
			CampaignWaveData bossWaveData;
			CreatureData bossCreatureData = GameRoot.Instance.CampaignDungeonDataContainer.GetBossCreatureData(worldBossData.ChapterIndex, worldBossData.DungeonIndex, DungeonDifficultyType.Easy, scenario: false, out bossWaveData);
			if (bossCreatureData != null)
			{
				string visualInfo = bossWaveData?.BossVisualInfo;
				string creatureName = string.Empty;
				string creatureNamePrefix = string.Empty;
				GameRoot.Instance.CreatureVisualDataContainer.GetCreatureName(bossCreatureData, visualInfo, LocalizationManager.GetStringDirect, out creatureName, out creatureNamePrefix);
				UIHelper.SetLabelText(WorldBossNameLabel, creatureName);
			}
		}
	}

	protected override void UpdateResetTime()
	{
	}
}
