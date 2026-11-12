using System.Collections;
using NGame2.NUI.NHelper.NPortal;
using NGameAsset;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class PortalPunishmentRaidComponent : PortalContentsComponent
{
	public const string PrefabPath = "Resources/Prefab2/Window/Portal/PortalRenewalComponents/PortalPunishmentRaidComponent";

	public static PrefabAuto<PortalPunishmentRaidComponent> PrefabAuto = new PrefabAuto<PortalPunishmentRaidComponent>("Resources/Prefab2/Window/Portal/PortalRenewalComponents/PortalPunishmentRaidComponent");

	[Header("[PortalPunishmentRaidComponent]")]
	public UIScrollView DescScrollView;

	public string BackgroundCode;

	public UILabel OpenTimeLabel;

	protected override void Init()
	{
		base.Init();
		InitBackGround();
		InitOpenLabel();
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

	private void InitOpenLabel()
	{
		if (!(OpenTimeLabel == null))
		{
			UIHelper.EnableGameObject(OpenTimeLabel, enable: false);
		}
	}

	protected override void UpdateResetTime()
	{
	}
}
