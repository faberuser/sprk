using System.Collections;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NHelper.NPortal;
using NGame2.NUI.NManager.NLobby;
using NShader;
using NShared;
using NVespa;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class PortalEclipseComponent : PortalContentsComponent
{
	public const string PrefabPath = "Resources/Prefab2/Window/Portal/PortalRenewalComponents/PortalEclipseComponent";

	public static PrefabAuto<PortalEclipseComponent> PrefabAuto = new PrefabAuto<PortalEclipseComponent>("Resources/Prefab2/Window/Portal/PortalRenewalComponents/PortalEclipseComponent");

	[Header("[PortalEclipseComponent]")]
	public List<UIScrollView> DescScrollViews;

	public List<UILabel> DescLabels;

	public Transform EclipseNormalRoot;

	public Transform EclipseFullRoot;

	public Transform EclipseKeyRootTransform;

	public UILabel EclipseStartWaveLabel;

	public UILabel EclipseBestWaveLabel;

	[Header("[Effect Group]")]
	public Camera EffectCamera;

	public UITexture EffectTexture;

	public UIGame3DWidget ForwardEffectWidget;

	public string BlueMoonEffectCode;

	public string PopRedMoonEffectCode;

	public string RedMoonEffectCode;

	public Vector3 ForwardEffectOffset;

	public float ForwardEffectScale;

	private EclipseKeyGaugeComponent _eclipseKeyGaugeComponent;

	protected override void Init()
	{
		base.Init();
		InitEclipseKeyGauge();
		InItEclipseEffect();
		SetEclipseWaveLabel();
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

	private void InItEclipseEffect()
	{
		if (EffectTexture == null || ForwardEffectWidget == null)
		{
			return;
		}
		long stamina = GlobalAutoBehaviour<AccountManager>.instance.UserManager.GetStamina(StaminaType.EclipseKey);
		UIHelper.EnableGameObject(ForwardEffectWidget, enable: true);
		EffectCamera.orthographicSize = PortalHelper.GetOrthographicSize();
		RenderTexture renderTexture = FilterBase.CreateRenderTarget(EffectTexture.width, EffectTexture.height);
		EffectCamera.targetTexture = renderTexture;
		EffectTexture.mainTexture = renderTexture;
		TransformData transformData = new TransformData();
		transformData.Position = ForwardEffectOffset;
		transformData.Scale = ForwardEffectScale;
		if (GlobalAutoBehaviour<AccountManager>.instance.EclipseManager.GetEclipseInfo() != null && GlobalAutoBehaviour<AccountManager>.instance.EclipseManager.GetEclipseInfo().IsPlayEclipse)
		{
			ForwardEffectWidget.PlayInstantEffect(RedMoonEffectCode, transformData, null);
			return;
		}
		int num = 0;
		if (stamina > num)
		{
			ForwardEffectWidget.PlayInstantEffect(PopRedMoonEffectCode, transformData, null);
		}
		else
		{
			ForwardEffectWidget.PlayInstantEffect(BlueMoonEffectCode, transformData, null);
		}
	}

	private void InitEclipseKeyGauge()
	{
		if (EclipseKeyGaugeComponent.ShowEclipseKeyGauge())
		{
			if (_eclipseKeyGaugeComponent != null)
			{
				FactoryBase.SafeReleaseComponent(ref _eclipseKeyGaugeComponent);
			}
			_eclipseKeyGaugeComponent = EclipseKeyGaugeComponent.PrefabAuto.Factory.Create(EclipseKeyRootTransform);
			bool flag = GlobalAutoBehaviour<AccountManager>.instance.EclipseManager.GetEclipseInfo() != null && GlobalAutoBehaviour<AccountManager>.instance.EclipseManager.GetEclipseInfo().IsPlayEclipse;
			GlobalAutoBehaviour<AccountManager>.instance.UserManager.StaminaManager.GetCurrentStamina(StaminaType.EclipseKey, out var currentValue, out var maxValue);
			long currencyValue = GlobalAutoBehaviour<AccountManager>.instance.PlayerCurrencyManager.GetCurrencyValue(UserManager.UserStatType.EclipsePoint);
			CurrencyResultInfo3 currencyResultInfo = new CurrencyResultInfo3();
			currencyResultInfo.AddValue = 0L;
			currencyResultInfo.NewValue = currencyValue;
			currencyResultInfo.CurrencyType = CurrencyType.EclipsePoint;
			_eclipseKeyGaugeComponent.Init(currentValue, currencyResultInfo, playFillAnimation: false);
			if (currentValue >= maxValue)
			{
				_eclipseKeyGaugeComponent.ChangeFullTweenState(isActive: true);
			}
			Transform eclipseNormalRoot = EclipseNormalRoot;
			long num = currentValue;
			int num2 = 0;
			UIHelper.EnableGameObject(eclipseNormalRoot, num <= num2 && !flag);
			Transform eclipseFullRoot = EclipseFullRoot;
			long num3 = currentValue;
			int num4 = 0;
			UIHelper.EnableGameObject(eclipseFullRoot, num3 > num4 || flag);
		}
	}

	private void SetEclipseWaveLabel()
	{
		UIHelper.SetLabelText(EclipseStartWaveLabel, "-");
		UIHelper.SetLabelText(EclipseBestWaveLabel, "-");
		EclipseInfo eclipseInfo = GlobalAutoBehaviour<AccountManager>.instance.EclipseManager.GetEclipseInfo();
		if (eclipseInfo != null)
		{
			UIHelper.SetLabelText(EclipseBestWaveLabel, (eclipseInfo.MaxWaveIndex == 0) ? "-" : eclipseInfo.MaxWaveIndex.ToString());
			EclipseStartData data = GameRoot.Instance.EclipseStartDataContainer.GetData(eclipseInfo.MaxWaveIndex);
			if (data != null)
			{
				UIHelper.SetLabelText(EclipseStartWaveLabel, data.StartWave);
			}
		}
	}

	protected override void UpdateResetTime()
	{
		SoulWeaponManagement instance = SceneAutoBehaviour<SoulWeaponManagement>.instance;
		if (!(instance == null))
		{
			instance.RequestGetEclipseInfo(showLoading: false, delegate
			{
				InitEclipseKeyGauge();
				InItEclipseEffect();
				SetEclipseWaveLabel();
			}, null);
		}
	}
}
