using System;
using NGame2.NAccount;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow.NSoulWeapon;

[Serializable]
public class SelectSoulWeaponReinforceExpInfo
{
	public UISprite ExpProgressSprite;

	private SoulWeaponInfo _soulWeaponInfo;

	public int RequireExp
	{
		get
		{
			if (_soulWeaponInfo == null)
			{
				return -1;
			}
			if (GlobalAutoBehaviour<AccountManager>.instance.SoulWeaponManager == null)
			{
				return -1;
			}
			return _soulWeaponInfo.GetSoulWeaponReinforceData()?.RequiredExp ?? (-1);
		}
	}

	public float Progress
	{
		get
		{
			if (ExpProgressSprite == null)
			{
				return 0f;
			}
			return ExpProgressSprite.fillAmount;
		}
	}

	public void Open(SoulWeaponInfo soulWeaponInfo)
	{
		Init(soulWeaponInfo);
	}

	private void Init(SoulWeaponInfo soulWeaponInfo)
	{
		_soulWeaponInfo = soulWeaponInfo;
		InitProgress();
	}

	private void InitProgress()
	{
		if (ExpProgressSprite == null)
		{
			return;
		}
		if (_soulWeaponInfo == null)
		{
			ExpProgressSprite.fillAmount = 0f;
			return;
		}
		int requireExp = RequireExp;
		switch (requireExp)
		{
		case -1:
			ExpProgressSprite.fillAmount = 0f;
			break;
		case 1:
			ExpProgressSprite.fillAmount = 1f;
			break;
		default:
			ExpProgressSprite.fillAmount = (float)_soulWeaponInfo.Exp / (float)requireExp;
			break;
		}
	}
}
