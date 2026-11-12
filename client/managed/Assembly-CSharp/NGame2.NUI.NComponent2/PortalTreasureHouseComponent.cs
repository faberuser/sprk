using System.Collections;
using NGame2.NAccount;
using NGame2.NUI.NHelper.NPortal;
using NGame2.NUI.NManager;
using NGameAsset;
using NShared;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class PortalTreasureHouseComponent : PortalContentsComponent
{
	public const string PrefabPath = "Resources/Prefab2/Window/Portal/PortalRenewalComponents/PortalTreasureHouseComponent";

	public static PrefabAuto<PortalTreasureHouseComponent> PrefabAuto = new PrefabAuto<PortalTreasureHouseComponent>("Resources/Prefab2/Window/Portal/PortalRenewalComponents/PortalTreasureHouseComponent");

	[Header("[PortalTreasureHouseComponent]")]
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
		if (OpenTimeLabel == null)
		{
			return;
		}
		UIHelper.EnableGameObject(OpenTimeLabel, enable: false);
		SceneAutoBehaviour<TreasureHouseManagement>.instance.RequestTreasureHouseInfo(delegate
		{
			CurrencyTypeData currencyData = GameRoot.Instance.CurrencyTypeDataContainer.GetCurrencyData(CurrencyType.TreasureHousePoint);
			if (currencyData != null)
			{
				UIHelper.EnableGameObject(OpenTimeLabel, enable: true);
				if (TreasureHouseManagement.CheckTreasureHouseDungeonCleard())
				{
					UIHelper.SetLabelText(OpenTimeLabel, LocalizationManager.GetStringDirect("PORTAL_TREASUREHOUSE_TIME_NOTICE2"));
				}
				else
				{
					int maxValue = currencyData.MaxValue;
					long userStat = GlobalAutoBehaviour<AccountManager>.instance.UserManager.GetUserStat(UserManager.UserStatType.TreasureHousePoint);
					_ = (float)userStat / (float)maxValue;
					long remainTime = checked(maxValue - userStat);
					if (maxValue <= userStat)
					{
						UIHelper.SetLabelText(OpenTimeLabel, LocalizationManager.GetStringDirect("PORTAL_TREASUREHOUSE_ENTER_NOTICE"));
					}
					else
					{
						string stringDirect = LocalizationManager.GetStringDirect("PORTAL_TREASUREHOUSE_TIME_NOTICE", UIHelper.GetRemainTimeLabelText(remainTime));
						UIHelper.SetLabelText(OpenTimeLabel, stringDirect);
					}
				}
			}
		});
	}

	protected override void UpdateResetTime()
	{
	}
}
