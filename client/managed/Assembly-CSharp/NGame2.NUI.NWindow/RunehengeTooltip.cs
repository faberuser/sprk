using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using NGame2.NUI.NHelper;
using NShared;
using NVespa.NGlobal;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class RunehengeTooltip : SimpleTooltip<RunehengeTooltipTextInfo>
{
	public const string PrefabPath = "Resources/Prefab2/Window/Tooltip/RunehengeTooltip";

	private static WindowPrefab<RunehengeTooltip> _instance = new WindowPrefab<RunehengeTooltip>("Resources/Prefab2/Window/Tooltip/RunehengeTooltip", GameUIRoot.ELayerType.UI);

	public UIScrollView ScrollView;

	public UIWidget Widget_Sprite;

	public UIWidget Widget_Root;

	public UIWidget Widget_Label;

	public int MaxHeight;

	public int ExtraHeight;

	private Coroutine _coroutine;

	public static void Open(UIWidget ownerWidget, RunehengeTooltipTextInfo info)
	{
		SimpleTooltip<RunehengeTooltipTextInfo>.Open(_instance, ownerWidget, info);
	}

	public static void Hide(UIWidget ownerWidget)
	{
		SimpleTooltip<RunehengeTooltipTextInfo>.Hide(_instance, ownerWidget);
	}

	protected override void SetupText(RunehengeTooltipTextInfo textInfo)
	{
		if (_coroutine != null)
		{
			VespaCoroutine.Stop(_coroutine);
			_coroutine = null;
		}
		if (textInfo.HeroInfo == null)
		{
			return;
		}
		Dictionary<Enum, long> appliedRuneTotalStat = RuneHelper.GetAppliedRuneTotalStat(textInfo.HeroInfo, textInfo.RunePage);
		StringBuilder stringBuilder = new StringBuilder();
		foreach (KeyValuePair<Enum, long> item in appliedRuneTotalStat)
		{
			string text = LocalizationManager.GetString(item.Key);
			string statDisplayValue = StatData.GetStatDisplayValue(item.Key, item.Value);
			stringBuilder.AppendLine(text + "  " + statDisplayValue);
		}
		stringBuilder.AppendLine();
		foreach (string item2 in RuneHelper.GetAppliedRuneTotalAddSkill(textInfo.HeroInfo, textInfo.RunePage))
		{
			stringBuilder.AppendLine(item2);
			stringBuilder.AppendLine();
		}
		textInfo.Desc.Value = stringBuilder.ToString();
		base.SetupText(textInfo);
		int height = Widget_Label.height;
		if (height <= MaxHeight)
		{
			Widget_Root.height = height + ExtraHeight;
		}
		else
		{
			Widget_Root.height = MaxHeight + ExtraHeight;
		}
		Widget_Root.ForceUpdateAnchors(includeChildren: true);
		Widget_Sprite.ForceUpdateAnchors(includeChildren: true);
		_coroutine = VespaCoroutine.Start(CoResetScrollView());
	}

	private IEnumerator CoResetScrollView()
	{
		yield return null;
		if (ScrollView != null)
		{
			ScrollView.ResetPosition();
		}
	}

	public override void Reset()
	{
		base.Reset();
	}
}
