using System.Collections.Generic;
using AnimationOrTween;
using NGameAsset;
using NShared;
using NVespa;
using UnityEngine;

namespace NGame2.NUI.NWindow.NSoulWeapon;

public class PlayAnimSoulWeaponReinforce : MonoBehaviour
{
	public interface IListener
	{
		void FinishAnimation(SoulWeaponInfo soulWeaponInfo, bool isSuccess);
	}

	public Animator Animator;

	public List<TweenPosition> TweenList = new List<TweenPosition>();

	private const string BaseAnimName = "Anim_Soul_ReinForce";

	private const string DefaultAnimName = "Anim_Soul_ReinForce00";

	private IListener _iListener;

	private string _playAnimName;

	private SoulWeaponInfo _originSoulWeaponInfo;

	private bool _isSuccess;

	public void Open(IListener iListener)
	{
		Init(iListener);
	}

	private void Init(IListener iListener)
	{
		_iListener = iListener;
		_playAnimName = "";
		PlayTween(isForward: false);
		PlayAnimDefault();
	}

	public void Close()
	{
		if (Animator != null)
		{
			Animator.Play("Anim_Soul_ReinForce00", -1, 0f);
		}
	}

	public void PlayAnimEtherInjecion(float progress)
	{
		if (!(Animator == null))
		{
			Animator.StopPlayback();
			string text = "";
			text = "Anim_Soul_ReinForce00";
			if (progress >= 1f)
			{
				text = "Anim_Soul_ReinForce03";
			}
			else if (progress >= 0.835f)
			{
				text = "Anim_Soul_ReinForce03";
			}
			else if (progress >= 0.5f)
			{
				text = "Anim_Soul_ReinForce02";
			}
			else if (progress >= 0.165f)
			{
				text = "Anim_Soul_ReinForce01";
			}
			if (_playAnimName != text)
			{
				Animator.Play(text);
				_playAnimName = text;
			}
		}
	}

	private void PlayAnimDefault()
	{
		if (Animator != null)
		{
			Animator.Play("Anim_Soul_ReinForce00");
		}
	}

	private void PlayAnimReinforce()
	{
		if (!(Animator == null))
		{
			string stateName = "Anim_Soul_ReinForce03_End";
			Animator.Play(stateName);
			PlaySound("UI.SoulWeapon.EtherStart");
		}
	}

	public void StartAnim(SoulWeaponInfo soulWeaponInfo, bool isSuccess)
	{
		if (soulWeaponInfo != null)
		{
			_originSoulWeaponInfo = soulWeaponInfo;
			_isSuccess = isSuccess;
			PlayTween(isForward: true);
		}
	}

	public void PlayTween(bool isForward)
	{
		TweenPosition tweenPosition = null;
		for (int i = 0; i < TweenList.Count; i++)
		{
			tweenPosition = TweenList[i];
			if (tweenPosition != null)
			{
				tweenPosition.Play(isForward);
			}
		}
	}

	public void FinishAnimEtherInjection(string nextAnimName)
	{
	}

	public void FinishAnimReinforce()
	{
		if (_iListener != null)
		{
			_iListener.FinishAnimation(_originSoulWeaponInfo, _isSuccess);
		}
	}

	public void FinishTween(TweenPosition tween)
	{
		if (tween.direction == Direction.Forward)
		{
			PlayAnimReinforce();
		}
	}

	public void FinishTweenTryReinforce(TweenPosition tween)
	{
		if (tween.direction == Direction.Forward)
		{
			PlayAnimReinforce();
		}
	}

	private void PlaySound(string soundCode)
	{
		SoundStock stock = new SoundStock
		{
			Code = soundCode
		};
		SingletonAuto<SoundContainer>.instance.PlayOnce(stock, 10f);
	}
}
