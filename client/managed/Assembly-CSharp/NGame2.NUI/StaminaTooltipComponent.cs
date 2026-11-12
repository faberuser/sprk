using NGame2.NUI.NWindow;
using NShared;
using UnityEngine;

namespace NGame2.NUI;

public class StaminaTooltipComponent : MonoBehaviour
{
	private class TooltipTextInfoProxy
	{
		private StaminaTooltipTextInfo _tooltipTextInfo;

		private float _lastUpdateStaminaTooltipTime;

		private StaminaType StaminaType;

		public StaminaTooltipTextInfo GetInfo(StaminaType staminaType)
		{
			StaminaType = staminaType;
			SetupTooltipText(staminaType);
			return _tooltipTextInfo;
		}

		private void SetupTooltipText(StaminaType staminaType)
		{
			if (_tooltipTextInfo == null)
			{
				_tooltipTextInfo = new StaminaTooltipTextInfo();
			}
			_tooltipTextInfo.Reset();
		}
	}

	public StaminaType StaminaType;

	public float ScalePercent = 1f;

	private TooltipTextInfoProxy _tooltipTextProxy = new TooltipTextInfoProxy();

	public void OnShowTooltip(StaminaType _staminaType)
	{
		StaminaType = _staminaType;
		StaminaTooltipTextInfo info = _tooltipTextProxy.GetInfo(_staminaType);
		StaminaTooltip.SetScale(ScalePercent);
		StaminaTooltip.Open(GetComponent<UIWidget>(), info, _staminaType);
	}

	public void OnHideTooltip()
	{
		StaminaTooltip.Hide(GetComponent<UIWidget>());
	}
}
