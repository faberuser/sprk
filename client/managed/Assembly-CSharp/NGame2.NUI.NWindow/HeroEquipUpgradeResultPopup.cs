using NGame2.NAccount.NExtension;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class HeroEquipUpgradeResultPopup : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/Hero/HeroEquipUpgradeResultPopup";

	public Transform IconParent;

	public UIGame3DWidget[] EffectWidget;

	public UILabel DefaultTitleLabel;

	public UILabel CustomTitleLabel;

	private ItemIconComponent _itemIconComponent;

	public void Open(EquipItemInfo equipInfo, string customTitle = null)
	{
		Open(delegate
		{
			Init(equipInfo, customTitle);
		}, null);
	}

	public void Init(EquipItemInfo equipInfo, string customTitle = null)
	{
		equipInfo.LinkSoulWeaponInfo();
		_itemIconComponent = ItemIconComponent.PrefabAuto.Factory.Create(IconParent);
		_itemIconComponent.ResetPosition();
		if (equipInfo != null)
		{
			_itemIconComponent.Open(equipInfo, null);
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(EffectWidget); i++)
			{
				UIGame3DWidget uIGame3DWidget = EffectWidget[i];
				if ((bool)uIGame3DWidget)
				{
					uIGame3DWidget.Play(null);
				}
			}
		}
		else
		{
			_itemIconComponent.OpenEmpty(null);
		}
		UIHelper.EnableGameObject(DefaultTitleLabel, enable: true);
		UIHelper.EnableGameObject(CustomTitleLabel, enable: false);
		if (!string.IsNullOrEmpty(customTitle))
		{
			SetCustomTitle(customTitle);
		}
	}

	public void SetCustomTitle(string text)
	{
		UIHelper.EnableGameObject(DefaultTitleLabel, enable: false);
		UIHelper.EnableGameObject(CustomTitleLabel, enable: true);
		UIHelper.SetLabelText(CustomTitleLabel, text);
	}

	public override void Reset()
	{
		base.Reset();
		FactoryBase.SafeReleaseComponent(ref _itemIconComponent);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(EffectWidget); i++)
		{
			UIGame3DWidget uIGame3DWidget = EffectWidget[i];
			if ((bool)uIGame3DWidget)
			{
				uIGame3DWidget.Stop(immediatelyClear: true);
			}
		}
	}
}
