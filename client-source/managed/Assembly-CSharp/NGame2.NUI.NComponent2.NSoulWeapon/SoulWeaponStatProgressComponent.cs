using System.Collections.Generic;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NTooltip;
using NNguiExtension;
using NShared;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NSoulWeapon;

public class SoulWeaponStatProgressComponent : LinkFactoryComponent
{
	public enum BgColorType
	{
		Brown,
		Black
	}

	public static string PrefabPath = "Resources/Prefab2/Component/SoulWeapon/SoulWeaponStatProgressComponent";

	public static PrefabAuto<SoulWeaponStatProgressComponent> PrefabAuto = new PrefabAuto<SoulWeaponStatProgressComponent>(PrefabPath);

	public List<SoulWeaponStatProgressInfo> SoulWeaponStatProgressInfoList = new List<SoulWeaponStatProgressInfo>();

	public BoxCollider BoxCollider;

	public UIWidget TooltipWidget;

	public List<UISpriteSelector> BgSpriteSelectorList = new List<UISpriteSelector>();

	private SoulWeaponInfo _soulWeaponInfo;

	public void Open(SoulWeaponInfo soulWeapon, bool isRenew, BgColorType bgColorType = BgColorType.Brown)
	{
		Init(soulWeapon, isRenew, bgColorType);
	}

	private void Init(SoulWeaponInfo soulWeaponInfo, bool isRenew, BgColorType bgColorType)
	{
		_soulWeaponInfo = soulWeaponInfo;
		SetStatProgressInfoList(isRenew);
		SetBgColor(bgColorType);
		EnableTouch(enable: true);
	}

	private void SetStatProgressInfoList(bool isRenew)
	{
		if (_soulWeaponInfo != null && !__Extension_Method_ICollection__.IsNullOrEmpty(SoulWeaponStatProgressInfoList))
		{
			SoulWeaponStatProgressInfo soulWeaponStatProgressInfo = null;
			for (int i = 0; i < SoulWeaponStatProgressInfoList.Count; i++)
			{
				SoulWeaponStatProgressInfoList[i]?.Open(_soulWeaponInfo, isRenew);
			}
		}
	}

	private void SetBgColor(BgColorType bgColorType)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(BgSpriteSelectorList))
		{
			return;
		}
		UISpriteSelector uISpriteSelector = null;
		for (int i = 0; i < BgSpriteSelectorList.Count; i++)
		{
			uISpriteSelector = BgSpriteSelectorList[i];
			if (!(uISpriteSelector == null))
			{
				uISpriteSelector.SelectEnum(bgColorType);
			}
		}
	}

	public void EnableTouch(bool enable)
	{
		if (!(BoxCollider == null))
		{
			UIHelper.EnableCollider(BoxCollider, enable);
		}
	}

	public void ShowTooltip()
	{
		if (!(TooltipWidget == null))
		{
			TooltipTextInfo textInfo = new TooltipTextInfo();
			SoulWeaponStatTooltip.Open(TooltipWidget, textInfo, _soulWeaponInfo);
		}
	}

	public void HideTooltip()
	{
		if (!(TooltipWidget == null))
		{
			SoulWeaponStatTooltip.Hide(TooltipWidget);
		}
	}
}
