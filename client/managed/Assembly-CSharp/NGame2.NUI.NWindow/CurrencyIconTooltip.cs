using NShared;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class CurrencyIconTooltip : SimpleTooltip<CurrencyTooltipTextInfo>
{
	public const string PrefabPath = "Resources/Prefab2/Window/Tooltip/CurrencyIconTooltip";

	private static WindowPrefab<CurrencyIconTooltip> _instance = new WindowPrefab<CurrencyIconTooltip>("Resources/Prefab2/Window/Tooltip/CurrencyIconTooltip", GameUIRoot.ELayerType.UI);

	public UILabel Desc1Label;

	public UILabel MaxCapTitleLabel;

	public UILabel MaxCapCountLabel;

	public static CurrencyIconTooltip instance => _instance.instance;

	public static bool IsVisible(UIWidget ownerWidget)
	{
		return SimpleTooltip<CurrencyTooltipTextInfo>.IsVisible(_instance, ownerWidget);
	}

	public static void Open(UIWidget ownerWidget, CurrencyTooltipTextInfo textInfo)
	{
		SimpleTooltip<CurrencyTooltipTextInfo>.Open(_instance, ownerWidget, textInfo);
	}

	public static void Hide(UIWidget ownerWidget)
	{
		SimpleTooltip<CurrencyTooltipTextInfo>.Hide(_instance, ownerWidget);
	}

	public override void Reset()
	{
		base.Reset();
	}

	public static void SetScale(float fPer)
	{
		_instance.instance.transform.localScale = new Vector3(fPer, fPer, 1f);
	}

	protected override void SetupText(CurrencyTooltipTextInfo textInfo)
	{
		base.SetupText(textInfo);
		UIHelper.SetLabelText(Desc1Label, textInfo.Desc1.Value);
		UIHelper.EnableGameObject(Desc1Label, !String_Extension_Method.IsNullOrEmpty(textInfo.Desc1.Value));
		SetupMaxCap(textInfo);
	}

	private void SetupMaxCap(CurrencyTooltipTextInfo textInfo)
	{
		long vALUE = Vespa_Generic_IDictionary_Extension_Method.GETVALUE(ContentsDefine.CurrencyMaxCap.MaxCap, textInfo.CurrencyType, 0L);
		if (vALUE <= 0)
		{
			UIHelper.EnableGameObject(MaxCapTitleLabel, enable: false);
			UIHelper.EnableGameObject(MaxCapCountLabel, enable: false);
		}
		else
		{
			UIHelper.EnableGameObject(MaxCapTitleLabel, enable: true);
			UIHelper.EnableGameObject(MaxCapCountLabel, enable: true);
			UIHelper.SetLabelText(MaxCapCountLabel, Vespa_Long_Extension_Method.ToCommaString(vALUE));
		}
	}
}
