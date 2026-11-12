using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NGame2.NUI.NHelper.NPortal;
using NGame2.NUI.NWindow;
using NGameAsset;
using NShared;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class PortalPethouseComponent : PortalContentsComponent
{
	public const string PrefabPath = "Resources/Prefab2/Window/Portal/PortalRenewalComponents/PortalPethouseComponent";

	public static PrefabAuto<PortalPethouseComponent> PrefabAuto = new PrefabAuto<PortalPethouseComponent>("Resources/Prefab2/Window/Portal/PortalRenewalComponents/PortalPethouseComponent");

	[Header("[PortalPethouseComponent]")]
	public UIScrollView DescScrollView;

	public string BackgroundCode;

	[Header("[Control Buttons]")]
	public List<PortalShakmehComponent.ButtonData> ButtonDatas;

	protected override void Init()
	{
		base.Init();
		InitBackGround();
		InitButtonDatas();
		InitButtonStatus();
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
		if (_basicData != null && !(_basicData.ContentsTexture == null) && !String_Extension_Method.IsNullOrEmpty(BackgroundCode))
		{
			AssetTexture newPortalBackground = PortalHelper.GetNewPortalBackground(BackgroundCode);
			UIHelper.ResetPlaneAssetTextureAsync(this, _basicData.ContentsTexture, newPortalBackground);
		}
	}

	private void InitButtonDatas()
	{
		IList<PortalRenewalData> data = GameRoot.Instance.PortalRenewalDataContainer.GetData(PortalRenewalCategory.Pet);
		if (__Extension_Method_ICollection__.IsNullOrEmpty(data))
		{
			return;
		}
		foreach (PortalShakmehComponent.ButtonData button in ButtonDatas)
		{
			PortalRenewalData portalRenewalData = data.FirstOrDefault((PortalRenewalData x) => x.SubCategory == button.subCategory);
			if (portalRenewalData != null)
			{
				button.portalData = portalRenewalData;
				button.LockedState = PortalHelper.GetLockedReason(portalRenewalData);
				if (button.LockedState == PortalRenewal.LockedState.NONE)
				{
					button.NoticeState = PortalHelper.GetContentsNoticeState(portalRenewalData);
				}
				else
				{
					button.NoticeState = PortalRenewal.NoticeState.NONE;
				}
			}
		}
	}

	private void InitButtonStatus()
	{
		foreach (PortalShakmehComponent.ButtonData buttonData in ButtonDatas)
		{
			_ = buttonData.portalData;
			UIHelper.EnableGameObject(buttonData.LockedRoot, buttonData.LockedState != PortalRenewal.LockedState.NONE);
			buttonData.StateIconSelector.SelectEnum(buttonData.NoticeState);
		}
	}

	public void OnClickPetContentsButton(UIButton button)
	{
		if (_listener == null || _listener.IsRunningEditMode())
		{
			return;
		}
		PortalShakmehComponent.ButtonData buttonData = ButtonDatas.Find((PortalShakmehComponent.ButtonData x) => x.ContentsButton == button);
		if (buttonData == null || buttonData.portalData == null)
		{
			return;
		}
		if (buttonData.LockedState == PortalRenewal.LockedState.CONDITION_LOCKED)
		{
			string openConditionLocalKey = buttonData.portalData.OpenConditionLocalKey;
			if (!String_Extension_Method.IsNullOrEmpty(openConditionLocalKey))
			{
				CenterMessage.Open(LocalizationManager.GetStringDirect(openConditionLocalKey));
			}
		}
		else if (buttonData.LockedState == PortalRenewal.LockedState.TUTORIAL_LOCKED)
		{
			CenterMessage.Open(LocalizationManager.GetStringDirect("TUTORIAL_RAIDPARTYSETTING_3"));
		}
		else
		{
			ForceContentsUpdate(buttonData.portalData);
		}
	}

	protected override void UpdateResetTime()
	{
	}
}
