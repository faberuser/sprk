using NGame2.NAccount;
using NShared;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class StaminaTooltip : SimpleTooltip<StaminaTooltipTextInfo>
{
	public const string PrefabPath = "Resources/Prefab2/Window/Tooltip/StaminaTooltip";

	private static WindowPrefab<StaminaTooltip> _instance = new WindowPrefab<StaminaTooltip>("Resources/Prefab2/Window/Tooltip/StaminaTooltip", GameUIRoot.ELayerType.UI);

	[HideInInspector]
	public StaminaType StaminaType;

	public UILabel Desc1Label;

	private float _lastUpdateStaminaTooltipTime;

	public static StaminaTooltip instance => _instance.instance;

	public static void Open(UIWidget ownerWidget, StaminaTooltipTextInfo textInfo, StaminaType staminaType = StaminaType.None)
	{
		SimpleTooltip<StaminaTooltipTextInfo>.Open(_instance, ownerWidget, textInfo);
		instance.StaminaType = staminaType;
		instance.SetStaminaLabel();
	}

	public static void Hide(UIWidget ownerWidget)
	{
		SimpleTooltip<StaminaTooltipTextInfo>.Hide(_instance, ownerWidget);
	}

	public override void Reset()
	{
		base.Reset();
	}

	public static void SetScale(float fPer)
	{
		_instance.instance.transform.localScale = new Vector3(fPer, fPer, 1f);
	}

	protected override void SetupText(StaminaTooltipTextInfo textInfo)
	{
		base.SetupText(textInfo);
		UIHelper.SetLabelText(Desc1Label, textInfo.Desc1.Value);
		UIHelper.EnableGameObject(Desc1Label, !String_Extension_Method.IsNullOrEmpty(textInfo.Desc1.Value));
	}

	public void SetStaminaLabel()
	{
		if (!(Time.time - _lastUpdateStaminaTooltipTime < 1f))
		{
			_lastUpdateStaminaTooltipTime = Time.time;
			if (StaminaType == StaminaType.GuildSuppressKey)
			{
				string stringDirect = LocalizationManager.GetStringDirect("DISABLE_RUBY_RECHARGE");
				UIHelper.SetLabelText(NameLabel, stringDirect);
				return;
			}
			string FullStaminaRechargeRemainTime = string.Empty;
			string OneStaminaRechargeRemainTime = string.Empty;
			GlobalAutoBehaviour<AccountManager>.instance.UserManager.StaminaManager.GetCurStaminaTimeInfo(StaminaType, out FullStaminaRechargeRemainTime, out OneStaminaRechargeRemainTime);
			UIHelper.SetLabelText(DescLabel, FullStaminaRechargeRemainTime);
			UIHelper.SetLabelText(NameLabel, OneStaminaRechargeRemainTime);
			UIHelper.EnableGameObject(DescLabel, enable: true);
			UIHelper.EnableGameObject(NameLabel, enable: true);
		}
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		if (UIHelper.IsEnabledGameObjet(instance))
		{
			SetStaminaLabel();
		}
	}
}
