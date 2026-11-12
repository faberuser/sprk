using NGame2.NUI.NManager.NBooster;
using NGame2.NUI.NWindow;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class BoosterComponent : LinkFactoryComponent, InitOnCreate
{
	public const string PrefabPath = "Prefab2/Component/Icon/BoosterComponent";

	public static PrefabAuto<BoosterComponent> PrefabAuto = new PrefabAuto<BoosterComponent>("Prefab2/Component/Icon/BoosterComponent");

	public UITexture Texture_Booster;

	public UIWidget WidgetIcon;

	public UILabel LabelRemainTime;

	public UIGame3DWidget UiEffect_Border;

	private TooltipTextInfo _TooltipTextInfo;

	private float _lastUpdateTooltipTime;

	private BoosterInfo _info;

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		UiEffect_Border.Stop(immediatelyClear: true);
		_info = null;
	}

	public void Open(BoosterInfo info)
	{
		if (info != null)
		{
			_info = info;
			SetupIcon();
			SetupRemainTimeLabel();
			SetupTooltipText();
		}
	}

	private void OnEnable()
	{
		if (UiEffect_Border != null && !UiEffect_Border.IsPlaying)
		{
			UiEffect_Border.Play(null);
		}
	}

	private void SetupIcon()
	{
		if (_info != null)
		{
			UIHelper.ResetPlaneAssetTextureAsync(this, Texture_Booster, UIHelper.GetBoosterIcon(_info.ItemCode));
		}
		if (base.gameObject.activeInHierarchy && UiEffect_Border != null && !UiEffect_Border.IsPlaying)
		{
			UiEffect_Border.Play(null);
		}
	}

	private void SetupRemainTimeLabel()
	{
		if (_info == null || !_info.DisplayRemainTime())
		{
			UIHelper.EnableGameObject(LabelRemainTime, enable: false);
			return;
		}
		UpdateRemainTimeLabel();
		UIHelper.EnableGameObject(LabelRemainTime, enable: true);
	}

	private void UpdateRemainTimeLabel()
	{
		if (_info != null && _info.DisplayRemainTime())
		{
			int remainSeconds = _info.GetRemainSeconds();
			if (remainSeconds > 0)
			{
				UIHelper.SetLabelText(LabelRemainTime, UIHelper.GetRemainTimeTextSingle(remainSeconds));
			}
			else
			{
				UIHelper.SetLabelText(LabelRemainTime, string.Empty);
			}
		}
	}

	public BoosterInfo GetBoosterInfo()
	{
		return _info;
	}

	public void OnShowTooltip()
	{
		SetupTooltipText();
		BoosterTooltip.Open(WidgetIcon, _TooltipTextInfo);
	}

	public void OnHideTooltip()
	{
		BoosterTooltip.Hide(WidgetIcon);
	}

	private void Update()
	{
		UpdateTooltipText();
		UpdateRemainTimeLabel();
	}

	public void UpdateTooltipText()
	{
		if (BoosterTooltip.IsVisible(WidgetIcon) && !((double)(Time.time - _lastUpdateTooltipTime) < 0.5))
		{
			_lastUpdateTooltipTime = Time.time;
			SetupTooltipText();
			BoosterTooltip.Refresh(WidgetIcon, _TooltipTextInfo);
		}
	}

	private void SetupTooltipText()
	{
		if (_TooltipTextInfo == null)
		{
			_TooltipTextInfo = new TooltipTextInfo();
		}
		if (_info != null)
		{
			_info.SetupBoosterTooltip(_TooltipTextInfo);
		}
	}
}
