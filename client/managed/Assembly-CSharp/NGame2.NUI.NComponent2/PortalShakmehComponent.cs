using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NGame2.NAccount;
using NGame2.NUI.NHelper.NPortal;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow;
using NGame2.NUtil;
using NGameAsset;
using NNguiExtension;
using NShader;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class PortalShakmehComponent : PortalContentsComponent
{
	[Serializable]
	public class ButtonData
	{
		public PortalRenewalSubCategory subCategory;

		public UIButton ContentsButton;

		public PortalRenewalData portalData;

		[HideInInspector]
		public PortalRenewal.LockedState LockedState;

		[HideInInspector]
		public PortalRenewal.NoticeState NoticeState;

		public Transform LockedRoot;

		public UISpriteSelector StateIconSelector;

		public Transform EventImageSprite;
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Portal/PortalRenewalComponents/PortalShakmehComponent";

	public static PrefabAuto<PortalShakmehComponent> PrefabAuto = new PrefabAuto<PortalShakmehComponent>("Resources/Prefab2/Window/Portal/PortalRenewalComponents/PortalShakmehComponent");

	[Header("[PortalShakmehComponent]")]
	public List<UIScrollView> DescScrollViews;

	public List<UILabel> DescLabels;

	public Transform ShakmehMiddleTitleRoot;

	public Transform ShakmehFinalTitleRoot;

	public Transform ShakmehGaugeRootTranform;

	[Header("[Shakmeh Spine Group]")]
	public Transform SpineGroupRoot;

	public UITexture SpineTexture;

	public Camera SpineCamera;

	[Space(10f)]
	public UIGame3DWidget ForwardEffectWidget;

	public string ForwardEffectCode;

	public Vector3 ForwardEffectOffset;

	public float ForwardEffectScale;

	[Header("[Control Buttons]")]
	public List<ButtonData> ButtonDatas;

	private ShakmehGaugeComponent _shakmehGaugeComponent;

	private bool IsOpenShakmehDungeon
	{
		get
		{
			NGame2.NUI.NManager.ShakmehDungeonManagement instance = SceneAutoBehaviour<NGame2.NUI.NManager.ShakmehDungeonManagement>.instance;
			if (instance == null)
			{
				return false;
			}
			return instance.IsOpenShakmehDungeon;
		}
	}

	private bool IsOpenShakmehFinalBoss
	{
		get
		{
			NGame2.NUI.NManager.ShakmehDungeonManagement instance = SceneAutoBehaviour<NGame2.NUI.NManager.ShakmehDungeonManagement>.instance;
			if (instance == null)
			{
				return false;
			}
			return instance.IsEnoughShakmehBossGauge;
		}
	}

	private string BackgroundCode
	{
		get
		{
			if (!IsOpenShakmehFinalBoss)
			{
				return GetBackgrounCode("_MiddleBossShakmeh");
			}
			return GetBackgrounCode("_FinalBossShakmeh");
		}
	}

	protected override void Init()
	{
		base.Init();
		InitBackGround();
		InitShakmehTitle();
		InitShakmeGaugeComponent();
		InitButtonDatas();
		InitButtonStatus();
	}

	protected override void Close()
	{
		base.Close();
	}

	public override void ResolutionComp(float contentsWidth)
	{
		base.ResolutionComp(contentsWidth);
		VespaCoroutine.Start(coDescLabelBugFix());
	}

	private IEnumerator coDescLabelBugFix()
	{
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(DescScrollViews) && !__Extension_Method_ICollection__.IsNullOrEmpty(DescLabels))
		{
			DescLabels.ForEach(delegate(UILabel x)
			{
				UIHelper.EnableGameObject(x, enable: false);
			});
			yield return null;
			DescLabels.ForEach(delegate(UILabel x)
			{
				UIHelper.EnableGameObject(x, enable: true);
			});
			DescScrollViews.ForEach(delegate(UIScrollView x)
			{
				x.ResetPosition();
			});
		}
	}

	private void InitShakmehTitle()
	{
		_listener.ChangeContentsTitle(string.Empty);
		UIHelper.EnableGameObject(ShakmehMiddleTitleRoot, !IsOpenShakmehFinalBoss);
		UIHelper.EnableGameObject(ShakmehFinalTitleRoot, IsOpenShakmehFinalBoss);
	}

	private void InitBackGround()
	{
		if (_basicData != null && !(_basicData.ContentsTexture == null))
		{
			AssetTexture newPortalBackground = PortalHelper.GetNewPortalBackground(BackgroundCode);
			UIHelper.ResetPlaneAssetTextureAsync(this, _basicData.ContentsTexture, newPortalBackground);
			EnableShakmehSpine(IsOpenShakmehFinalBoss);
		}
	}

	private void InitShakmeGaugeComponent()
	{
		FactoryBase.ReleaseComponent(_shakmehGaugeComponent);
		_shakmehGaugeComponent = ShakmehGaugeComponent.PrefabAuto.Factory.Create(ShakmehGaugeRootTranform);
		if (!(_shakmehGaugeComponent == null))
		{
			long userStat = GlobalAutoBehaviour<AccountManager>.instance.UserManager.GetUserStat(UserManager.UserStatType.ShakmehMiddleBossPoint);
			_shakmehGaugeComponent.Open(0f, userStat, 0f, autoStart: true);
		}
	}

	private void InitButtonDatas()
	{
		IList<PortalRenewalData> data = GameRoot.Instance.PortalRenewalDataContainer.GetData(PortalRenewalCategory.Shakmeh);
		if (__Extension_Method_ICollection__.IsNullOrEmpty(data))
		{
			return;
		}
		foreach (ButtonData button in ButtonDatas)
		{
			PortalRenewalData portalRenewalData = data.FirstOrDefault((PortalRenewalData x) => x.SubCategory == button.subCategory);
			if (portalRenewalData == null)
			{
				continue;
			}
			button.portalData = portalRenewalData;
			if (SingletonAuto<ConditionChecker>.instance.Check(portalRenewalData.ShowCondition))
			{
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
		foreach (ButtonData buttonData in ButtonDatas)
		{
			PortalRenewalData portalData = buttonData.portalData;
			bool flag = SingletonAuto<ConditionChecker>.instance.Check(portalData.ShowCondition);
			UIHelper.EnableGameObject(buttonData.ContentsButton, flag);
			if (flag)
			{
				UIHelper.EnableGameObject(buttonData.LockedRoot, buttonData.LockedState != PortalRenewal.LockedState.NONE);
				buttonData.StateIconSelector.SelectEnum(buttonData.NoticeState);
			}
		}
	}

	protected override void UpdateResetTime()
	{
	}

	public void OnClickShakmehContentsButton(UIButton button)
	{
		if (_listener == null || _listener.IsRunningEditMode())
		{
			return;
		}
		ButtonData buttonData = ButtonDatas.Find((ButtonData x) => x.ContentsButton == button);
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

	public void EnableShakmehSpine(bool enable)
	{
		UIHelper.EnableGameObject(SpineGroupRoot, enable);
		if (enable)
		{
			SpineCamera.orthographicSize = PortalHelper.GetOrthographicSize();
			RenderTexture renderTexture = FilterBase.CreateRenderTarget(SpineTexture.width, SpineTexture.height);
			SpineCamera.targetTexture = renderTexture;
			SpineTexture.mainTexture = renderTexture;
			TransformData transformData = new TransformData();
			transformData.Position = ForwardEffectOffset;
			transformData.Scale = ForwardEffectScale;
			ForwardEffectWidget.PlayInstantEffect(ForwardEffectCode, transformData, null);
		}
	}
}
