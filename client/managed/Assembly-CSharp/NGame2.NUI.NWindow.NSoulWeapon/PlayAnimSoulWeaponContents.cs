using NGame2.NAccount;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow.NSoulWeapon;

public class PlayAnimSoulWeaponContents : MonoBehaviour
{
	public SoulWeaponContentsType SoulWeaponContentsType;

	public string AnimKey;

	public Animator Animator;

	public bool CheckState
	{
		get
		{
			switch (SoulWeaponContentsType)
			{
			case SoulWeaponContentsType.Manage:
			case SoulWeaponContentsType.Reinforce:
			case SoulWeaponContentsType.Upgrade:
				return GlobalAutoBehaviour<AccountManager>.instance.SoulWeaponManager.HaveSoulWeapon();
			case SoulWeaponContentsType.Liberation:
				return UIHelper.IsOpenSoulWeaponLiberationButton();
			case SoulWeaponContentsType.RestoreSoulStone:
				return true;
			default:
				return false;
			}
		}
	}

	public void Open()
	{
		Init();
	}

	private void Init()
	{
		PlayAnimDefault();
	}

	private void PlayAnimDefault()
	{
		if (CheckState)
		{
			PlayAnimNormal();
		}
		else
		{
			PlayAnimDisable();
		}
	}

	private void PlayAnimNormal()
	{
		if (!String_Extension_Method.IsNullOrEmpty(AnimKey))
		{
			PlayAnim(AnimKey + "_Normal");
		}
	}

	private void PlayAnimDisable()
	{
		if (!String_Extension_Method.IsNullOrEmpty(AnimKey))
		{
			PlayAnim(AnimKey + "_Disabel");
		}
	}

	private void PlayAnimSelectDisable()
	{
		if (!String_Extension_Method.IsNullOrEmpty(AnimKey))
		{
			PlayAnim(AnimKey + "_DisabelSelect");
		}
	}

	private void PlayAnim(string animKey)
	{
		if (!String_Extension_Method.IsNullOrEmpty(animKey) && Animator != null)
		{
			Debug.Log("animKey : " + animKey);
			Animator.Play(animKey);
		}
	}
}
