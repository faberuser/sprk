using System;
using System.Collections;
using System.Collections.Generic;
using NGame2.NBattleField;
using NShared;
using NVespa;
using UnityEngine;

namespace NGame2.NUI.NWindow.NBattle;

public class KillMeter : BaseWindowUpdater, IInspectorGuiDrawer
{
	[Serializable]
	public class DigitData
	{
		public Transform DigitRoot;

		public UISprite DigitSprite;

		public UISprite DigitShadowSprite;

		public Animator Animator;
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Battle/KillMeter";

	public Animator KoTextureAnimator;

	public string[] SpriteNameArray_0to9;

	public string[] AnimatorKoCountState_SmallerFirst;

	public string AnimatorDigitHoldState;

	public string AnimatorDigitStartState;

	public string AnimatorDigitLoopState;

	[HideInInspector]
	public List<DigitData> DigitUiList = new List<DigitData>();

	private TeamType _teamType;

	private int _digitNumber;

	private int _curDigitCount;

	private int _maxCount;

	private Coroutine _koAnimateCoroutine;

	public void Open(TeamType teamType)
	{
		SetUpdatePeriod(0.1f);
		_teamType = teamType;
		base.Open(delegate
		{
			Init();
		});
	}

	private void Init()
	{
		float num = Mathf.Pow(10f, __Extension_Method_ICollection__.COUNT(DigitUiList));
		_maxCount = (int)System.Math.Truncate(num);
		_digitNumber = 0;
		_curDigitCount = 1;
		for (int i = 0; i < DigitUiList.Count - 1; i++)
		{
			UIHelper.EnableGameObject(DigitUiList[i].DigitRoot, enable: false);
		}
		UpdateDigit(_digitNumber);
	}

	public override void UpdatePeriodically()
	{
		base.UpdatePeriodically();
		int teamKillMeter = SingletonAuto<BattleFacade>.instance.GetTeamKillMeter(_teamType);
		SetNumber(teamKillMeter);
	}

	private void SetNumber(int newDigit)
	{
		newDigit = ((newDigit >= _maxCount) ? (_maxCount - 1) : newDigit);
		if (_digitNumber != newDigit)
		{
			UpdateDigit(newDigit);
			_digitNumber = newDigit;
		}
	}

	private void UpdateDigit(int newDigit)
	{
		int digitCount = GetDigitCount(newDigit);
		UpdateDigitObject(digitCount);
		if (_koAnimateCoroutine != null)
		{
			VespaCoroutine.Stop(_koAnimateCoroutine);
		}
		_koAnimateCoroutine = VespaCoroutine.Start(KoStartAnimate(AnimatorKoCountState_SmallerFirst[digitCount - 1]));
		long num = newDigit;
		long num2 = 10L;
		int num3 = 0;
		while (num3 < digitCount)
		{
			long num4 = num % num2;
			num /= num2;
			int index = DigitUiList.Count - 1 - num3;
			num3++;
			DigitData digitData = DigitUiList[index];
			string spriteName = digitData.DigitSprite.spriteName;
			string text = SpriteNameArray_0to9[num4];
			Animator animator = digitData.Animator;
			if (!(spriteName == text))
			{
				digitData.DigitSprite.spriteName = SpriteNameArray_0to9[num4];
				if (digitData.DigitShadowSprite != null)
				{
					digitData.DigitShadowSprite.spriteName = SpriteNameArray_0to9[num4];
				}
				animator.Play(AnimatorDigitStartState, 0, 0f);
			}
		}
	}

	private IEnumerator KoStartAnimate(string state)
	{
		KoTextureAnimator.enabled = true;
		KoTextureAnimator.Play(state, 0, 0f);
		yield return new WaitForSeconds(0.1f);
		AnimatorClipInfo[] currentAnimatorClipInfo = KoTextureAnimator.GetCurrentAnimatorClipInfo(0);
		if (__Extension_Method_ICollection__.COUNT(currentAnimatorClipInfo) != 0)
		{
			float length = currentAnimatorClipInfo[0].clip.length;
			yield return new WaitForSeconds(length);
			KoTextureAnimator.enabled = false;
		}
	}

	private void UpdateDigitObject(int digitCount)
	{
		int num = digitCount - _curDigitCount;
		if (num < 0)
		{
			num *= -1;
			for (int i = 0; i < num; i++)
			{
				int index = DigitUiList.Count - _curDigitCount + i;
				UIHelper.EnableGameObject(DigitUiList[index].DigitRoot, enable: false);
			}
		}
		else if (num > 0)
		{
			for (int j = 0; j < num; j++)
			{
				int index2 = DigitUiList.Count - _curDigitCount - 1 - j;
				UIHelper.EnableGameObject(DigitUiList[index2].DigitRoot, enable: true);
			}
		}
		_curDigitCount = digitCount;
	}

	private int GetDigitCount(int value)
	{
		if (value == 0)
		{
			return 1;
		}
		return (int)System.Math.Truncate(Mathf.Log10(value)) + 1;
	}
}
