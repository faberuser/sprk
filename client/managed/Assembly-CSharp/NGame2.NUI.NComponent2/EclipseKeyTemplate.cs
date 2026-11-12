using System;
using System.Collections;
using NNguiExtension;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

[Serializable]
public class EclipseKeyTemplate : MonoBehaviour
{
	public GameObject Root;

	public UISpriteSelector UISpriteSelector;

	public GameObject EffectRoot;

	private bool IsEmpty = true;

	public void SetKeyState(bool exist)
	{
		if (exist)
		{
			UISpriteSelector.Select("Normal");
			IsEmpty = false;
		}
		else
		{
			UISpriteSelector.Select("Empty");
			DisableEffect();
			IsEmpty = true;
		}
	}

	public void PlayEffect()
	{
		if (IsEmpty)
		{
			IsEmpty = false;
			EnableEffect();
			VespaCoroutine.Start(coPlayEffect());
		}
	}

	private IEnumerator coPlayEffect()
	{
		yield return new WaitForSeconds(0.4f);
		if (UISpriteSelector != null && UISpriteSelector.gameObject != null)
		{
			SetKeyState(exist: true);
		}
	}

	public bool IsEmptyKey()
	{
		return IsEmpty;
	}

	public void EnableEffect()
	{
		UIHelper.EnableGameObject(EffectRoot, enable: true);
	}

	public void DisableEffect()
	{
		UIHelper.EnableGameObject(EffectRoot, enable: false);
	}
}
