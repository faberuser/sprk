using System.Collections.Generic;
using NGame2.NUI.NWindow;
using NGame2.NUtil;
using NNguiExtension;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class PunishmentRaidStateComponent : LinkFactoryComponent
{
	public interface Listener
	{
		void OnClick(PunishmentRaidStateComponent caller);
	}

	public const string PrefabPath = "Prefab2/Component/Raid/PunishmentRaidStateComponent";

	public static PrefabAuto<PunishmentRaidStateComponent> PrefabAuto = new PrefabAuto<PunishmentRaidStateComponent>("Prefab2/Component/Raid/PunishmentRaidStateComponent");

	public UIWidget WidgetThis;

	public UISpriteSelector UISpriteSelector;

	public List<UISprite> ColorBgList;

	public UIWidget WidgetComp;

	private Listener _listener;

	private PunishmentRaidTriggerData _triggerData;

	private string _tooltipTitleLocalKey = string.Empty;

	private string _tooltipDescLocalKey = string.Empty;

	public void Open(PunishmentRaidTriggerData triggerData, Listener listener)
	{
		_listener = listener;
		_triggerData = triggerData;
		InitIconImage();
		InitIconColor();
		SetStatus();
		SetTooltipLocalKey();
	}

	private void InitIconImage()
	{
		UISpriteSelector.Select(_triggerData.IconName);
	}

	private void InitIconColor()
	{
		if (!ColorUtility.TryParseHtmlString(_triggerData.IconColor, out var color))
		{
			return;
		}
		foreach (UISprite colorBg in ColorBgList)
		{
			if (!(colorBg == null))
			{
				colorBg.color = color;
			}
		}
	}

	public void SetStatus()
	{
		bool flag = SingletonAuto<ConditionChecker>.instance.Check(_triggerData.Condition);
		foreach (UISprite colorBg in ColorBgList)
		{
			if (!(colorBg == null))
			{
				UIHelper.EnableGameObject(colorBg, flag);
			}
		}
		if (flag)
		{
			WidgetComp.alpha = 1f;
		}
		else
		{
			WidgetComp.alpha = 0.7f;
		}
	}

	private void SetTooltipLocalKey()
	{
		if (_triggerData != null)
		{
			_tooltipTitleLocalKey = "PUNISHMENTRAID_AMBIENCE_SKILL_INFO_0" + _triggerData.TriggerIndex + "_TITLE";
			_tooltipDescLocalKey = "PUNISHMENTRAID_AMBIENCE_SKILL_INFO_0" + _triggerData.TriggerIndex + "_DESC2";
		}
	}

	public void OnClickIcon()
	{
		if (_listener != null)
		{
			_listener.OnClick(this);
		}
	}

	public void OnShowToolTip()
	{
		DescTooltip.Open(WidgetThis, new TooltipTextInfo
		{
			Desc = new StringProxy
			{
				Value = LocalizationManager.GetStringDirect(_tooltipTitleLocalKey) + "\n" + LocalizationManager.GetStringDirect(_tooltipDescLocalKey)
			}
		});
	}

	public void OnHideToolTip()
	{
		DescTooltip.Hide(WidgetThis);
	}
}
