using System;
using System.Collections.Generic;
using NNguiExtension;
using NShared;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NSoulWeapon;

[Serializable]
public class SoulWeaponStatProgressInfo
{
	public ESoulWeaponStatType ESoulWeaponStatType = ESoulWeaponStatType.End;

	public List<UISprite> BonusRatioProgressSpriteList = new List<UISprite>();

	public List<Transform> FullProgressTransformList = new List<Transform>();

	public UISprite RatioProgressSprite;

	public UILabel RatioLabel;

	public UILabel BonusRatioLabel;

	public UISpriteSelector StatTypeIconSelector;

	private SoulWeaponInfo _soulWeaponInfo;

	private long _optionRatio;

	private long _optionBonusRatio;

	public void Open(SoulWeaponInfo soulWeapon, bool isRenew)
	{
		Init(soulWeapon, isRenew);
	}

	private void Init(SoulWeaponInfo soulWeaponInfo, bool isRenew)
	{
		_soulWeaponInfo = soulWeaponInfo;
		SetRatio(isRenew);
		SetStatTypeIcon();
		SetOptionRatioLabel();
		SetStatRatioProgress();
		SetStatBonusRatioProgress();
	}

	private void SetRatio(bool isRenew)
	{
		if (_soulWeaponInfo != null && ESoulWeaponStatType != ESoulWeaponStatType.End)
		{
			_optionRatio = (isRenew ? _soulWeaponInfo.GetRenewOptionRatio(ESoulWeaponStatType) : _soulWeaponInfo.GetOptionRatio(ESoulWeaponStatType));
			_optionBonusRatio = (isRenew ? _soulWeaponInfo.GetRenewOptionBonusRatio(ESoulWeaponStatType) : _soulWeaponInfo.GetOptionBonusRatio(ESoulWeaponStatType));
		}
	}

	private void SetStatTypeIcon()
	{
		if (!(StatTypeIconSelector == null))
		{
			StatTypeIconSelector.SelectEnum(ESoulWeaponStatType);
		}
	}

	private void SetOptionRatioLabel()
	{
		float num = (float)_optionRatio / 10f;
		float num2 = (float)_optionBonusRatio / 10f;
		UIHelper.SetLabelText(RatioLabel, num + "%");
		UIHelper.SetLabelText(BonusRatioLabel, num2 + "%");
		float num3 = 240f * (100f - num) / 100f;
		RatioLabel.transform.localPosition = new Vector3((ESoulWeaponStatType == ESoulWeaponStatType.Attack) ? (0f - num3) : num3, RatioLabel.transform.localPosition.y, RatioLabel.transform.localPosition.z);
	}

	private void SetStatRatioProgress()
	{
		if (_soulWeaponInfo != null && !(RatioProgressSprite == null) && ESoulWeaponStatType != ESoulWeaponStatType.End)
		{
			float num = _soulWeaponInfo.MaxOptionRatio + _soulWeaponInfo.MinOptionRatio;
			RatioProgressSprite.fillAmount = (float)_optionRatio / num;
		}
	}

	private void SetStatBonusRatioProgress()
	{
		if (_soulWeaponInfo == null || __Extension_Method_ICollection__.IsNullOrEmpty(BonusRatioProgressSpriteList))
		{
			return;
		}
		UISprite uISprite = null;
		Transform transform = null;
		float num = 0f;
		for (int i = 0; i < BonusRatioProgressSpriteList.Count; i++)
		{
			uISprite = BonusRatioProgressSpriteList[i];
			if (uISprite == null)
			{
				continue;
			}
			num = (float)_optionBonusRatio / _soulWeaponInfo.MaxBonusOptionRatio;
			if (i == 0)
			{
				num = (float)_optionBonusRatio * 24f / _soulWeaponInfo.MaxBonusOptionRatio / 100f;
				num += 0.64f;
			}
			uISprite.fillAmount = num;
			if (!__Extension_Method_ICollection__.IsNullOrEmpty(FullProgressTransformList) && FullProgressTransformList.Count >= i)
			{
				transform = FullProgressTransformList[i];
				if (!(transform == null))
				{
					UIHelper.EnableGameObject(transform, (float)_optionBonusRatio >= _soulWeaponInfo.MaxBonusOptionRatio);
				}
			}
		}
	}
}
