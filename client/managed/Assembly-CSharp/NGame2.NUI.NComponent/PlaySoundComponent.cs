using NVespa;
using UnityEngine;

namespace NGame2.NUI.NComponent;

public class PlaySoundComponent : MonoBehaviour, UICamera.IOnPressListener, UICamera.IOnClickListener
{
	public enum Trigger
	{
		OnClick,
		OnMouseOver,
		OnMouseOut,
		OnPress,
		OnRelease,
		Custom
	}

	public AudioClip audioClip;

	public Trigger trigger;

	private bool mIsOver;

	[Range(0f, 1f)]
	public float volume = 1f;

	[Range(0f, 2f)]
	public float pitch = 1f;

	private bool canPlay
	{
		get
		{
			if (!base.enabled)
			{
				return false;
			}
			UIButton component = GetComponent<UIButton>();
			if (!(component == null))
			{
				return component.isEnabled;
			}
			return true;
		}
	}

	private float playVolume => volume;

	private void OnHover(bool isOver)
	{
		if (trigger == Trigger.OnMouseOver)
		{
			if (mIsOver == isOver)
			{
				return;
			}
			mIsOver = isOver;
		}
		if (canPlay && ((isOver && trigger == Trigger.OnMouseOver) || (!isOver && trigger == Trigger.OnMouseOut)))
		{
			PlaySound();
		}
	}

	public void OnPress(bool isPressed)
	{
		if (trigger == Trigger.OnPress)
		{
			if (mIsOver == isPressed)
			{
				return;
			}
			mIsOver = isPressed;
		}
		if (canPlay && ((isPressed && trigger == Trigger.OnPress) || (!isPressed && trigger == Trigger.OnRelease)))
		{
			PlaySound();
		}
	}

	public void OnClick()
	{
		if (canPlay && trigger == Trigger.OnClick)
		{
			PlaySound();
		}
	}

	private void OnSelect(bool isSelected)
	{
		if (canPlay && (!isSelected || UICamera.currentScheme == UICamera.ControlScheme.Controller))
		{
			OnHover(isSelected);
		}
	}

	public void Play()
	{
		PlaySound();
	}

	private void PlaySound()
	{
		if (SystemOptionData.Sound_EnableGameSound)
		{
			NGUITools.PlaySound(audioClip, playVolume, pitch);
		}
	}
}
