using NGame2.NUI.NWindow;
using NVespa.NGlobal;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class KarmaBuffComponent : LinkFactoryComponent, InitOnCreate
{
	public static string PrefabPath = "Resources/Prefab2/Component/Icon/KarmaBuffComponent";

	public static PrefabAuto<KarmaBuffComponent> PrefabAuto = new PrefabAuto<KarmaBuffComponent>(PrefabPath);

	public UIGame3DWidget Effect;

	public UIBundleLoader IconBundleLoader;

	public UILabel LabelStackCount;

	public UIWidget WidgetRoot;

	public UIGame3DWidget EffectHit;

	public UILabel LabelSumupValue;

	private KarmaCardSelectWindow.BuffInfo _info;

	private TooltipTextInfo _tooltipText;

	private int _groupIndex;

	public int GroupIndex => _groupIndex;

	public int Priority
	{
		get
		{
			if (_info != null)
			{
				return _info.Priority;
			}
			return 0;
		}
	}

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		_info = null;
		Effect.Stop(immediatelyClear: false);
	}

	public void Open(int groupIndex, KarmaCardSelectWindow.BuffInfo buffInfo)
	{
		_groupIndex = groupIndex;
		_info = buffInfo;
		SetBuffInfo();
	}

	public void PlayEffectCardHit()
	{
		if (!(EffectHit == null))
		{
			EffectHit.Play(null);
		}
	}

	public void Refresh()
	{
		SetBuffInfo();
	}

	public void RefreshTooltip()
	{
		SetTooltipText();
		if (KarmaBuffTooltip.IsVisible(WidgetRoot))
		{
			KarmaBuffTooltip.Refresh(WidgetRoot, _tooltipText);
		}
	}

	private void SetTooltipText()
	{
		if (_info != null)
		{
			string stringDirect = LocalizationManager.GetStringDirect(_info.DescKey, _info.BuffValue);
			if (_tooltipText == null)
			{
				_tooltipText = new TooltipTextInfo();
			}
			_tooltipText.Desc.Value = stringDirect;
		}
	}

	private void SetBuffInfo()
	{
		if (_info != null)
		{
			UIHelper.SetBundleTexture(IconBundleLoader, _info.IconPath);
			bool enable = _info.BuffValue > 0;
			UIHelper.EnableGameObject(LabelSumupValue, enable);
			UIHelper.SetLabelText(LabelSumupValue, $"{_info.BuffValue}{_info.BuffValueUnit}");
			Effect.Stop(immediatelyClear: false);
			Effect.Play(null);
		}
	}

	public void OnShowTooltip()
	{
		SetTooltipText();
		KarmaBuffTooltip.Open(WidgetRoot, _tooltipText);
	}

	public void OnHideTooltip()
	{
		KarmaBuffTooltip.Hide(WidgetRoot);
	}
}
