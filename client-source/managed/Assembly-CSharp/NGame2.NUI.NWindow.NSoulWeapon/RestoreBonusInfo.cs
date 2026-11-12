using System;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow.NSoulWeapon;

[Serializable]
public class RestoreBonusInfo
{
	public UISprite BonusProgressSprite;

	public Transform BonusItemIconComponentRootTransform;

	public UILabel BonusProgressLabel;

	public UIGame3DWidget UIGame3DWidget;

	private ItemIconComponent _bonusItemIconComponent;

	private long _soulStoneMileage;

	private float MaxSoulStoneMileage
	{
		get
		{
			int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("MaxSoulStoneMileage");
			if (valueInt <= 0)
			{
				return 10f;
			}
			return valueInt;
		}
	}

	public bool IsPossibleGetBonus => (float)_soulStoneMileage == MaxSoulStoneMileage;

	public void Open()
	{
		Init();
	}

	private void Init()
	{
		int num = 0;
		_soulStoneMileage = num;
		if (GlobalAutoBehaviour<AccountManager>.instance.UserManager != null)
		{
			_soulStoneMileage = GlobalAutoBehaviour<AccountManager>.instance.UserManager.SoulStoneMileage;
		}
		SetBonusProgressLabel();
		SetBonusProgressSprite();
		SetSoulStoneItemIconComponent();
	}

	public void Close()
	{
		if (_bonusItemIconComponent != null)
		{
			FactoryBase.SafeReleaseComponent(ref _bonusItemIconComponent);
		}
	}

	private void SetSoulStoneItemIconComponent()
	{
		if (BonusItemIconComponentRootTransform == null)
		{
			return;
		}
		if (_bonusItemIconComponent == null)
		{
			_bonusItemIconComponent = ItemIconComponent.PrefabAuto.Factory.Create(BonusItemIconComponentRootTransform);
			if (_bonusItemIconComponent == null)
			{
				return;
			}
			Vespa_Unity_Transform_Extension_Method.ResetLocalTM(_bonusItemIconComponent.transform);
			ItemData data = GameRoot.Instance.ItemDataContainer.GetData(5002);
			_bonusItemIconComponent.Open(data);
		}
		if (!(_bonusItemIconComponent != null))
		{
			return;
		}
		_bonusItemIconComponent.SetIconGrayScale(!IsPossibleGetBonus);
		if (IsPossibleGetBonus)
		{
			if (UIGame3DWidget != null)
			{
				UIGame3DWidget.Play(null);
			}
		}
		else if (UIGame3DWidget != null)
		{
			UIGame3DWidget.Stop(immediatelyClear: true);
		}
	}

	private void SetBonusProgressLabel()
	{
		UIHelper.SetLabelText(BonusProgressLabel, _soulStoneMileage + " / " + MaxSoulStoneMileage);
	}

	private void SetBonusProgressSprite()
	{
		if (!(BonusProgressSprite == null))
		{
			BonusProgressSprite.fillAmount = (float)_soulStoneMileage / MaxSoulStoneMileage;
		}
	}
}
