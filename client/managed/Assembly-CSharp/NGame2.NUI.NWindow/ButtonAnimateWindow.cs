using System;
using System.Collections;
using NShared;
using NVespa;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class ButtonAnimateWindow : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/Tutorial/ButtonAnimateWindow";

	private Coroutine _flyCoroutine;

	public UIGame3DWidget ButtonEffect;

	public Transform ButtonEffectRoot;

	private int _effectIndex = -1;

	public void Init(ERightMenuButton type, Vector3 posStart)
	{
		_effectIndex = type.GetEffectIndex();
		if (_effectIndex != -1)
		{
			base.Open();
			ButtonEffectRoot.localPosition = posStart;
			ButtonEffect.Stop(immediatelyClear: true);
			ButtonEffect.PlayEntry(_effectIndex, null);
		}
	}

	public void StartButtonFlyAnimation(Vector3 posEnd, float flyDuration, float effectDuration)
	{
		if (_flyCoroutine != null)
		{
			VespaCoroutine.StopScene(_flyCoroutine);
			_flyCoroutine = null;
		}
		ButtonEffect.Stop(immediatelyClear: true);
		ButtonEffect.PlayEntry(_effectIndex, null);
		ButtonEffect.PlayEntryPhase(_effectIndex, "End");
		_flyCoroutine = VespaCoroutine.StartScene(coFlyAnimation(ButtonEffectRoot.localPosition, posEnd, flyDuration, effectDuration, delegate
		{
			EndButtonFlyAnimation();
		}));
	}

	private IEnumerator coFlyAnimation(Vector3 beginPos, Vector3 endPos, float flyDuration, float effectDuration, Action onComplete)
	{
		float elapsedTime = -0.05f;
		do
		{
			yield return null;
			elapsedTime += GlobalTimer.deltaTime;
			Vector3 localPosition = Vector3.Lerp(beginPos, endPos, Mathf.Clamp01(elapsedTime / flyDuration));
			ButtonEffectRoot.localPosition = localPosition;
		}
		while (!(elapsedTime > effectDuration));
		Vespa_System_Action_Extension_Method.fire(onComplete);
	}

	public void EndButtonFlyAnimation()
	{
		Close();
	}

	public override void Reset()
	{
		base.Reset();
		if (_flyCoroutine != null)
		{
			VespaCoroutine.StopScene(_flyCoroutine);
			_flyCoroutine = null;
		}
		ButtonEffect.Stop(immediatelyClear: true);
	}
}
