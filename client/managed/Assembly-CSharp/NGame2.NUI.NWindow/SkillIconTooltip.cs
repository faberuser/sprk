using System.Collections.Generic;
using NGame2.NUtil.NStringProxy.NTypeText;
using NVespa.NNGUIExtension;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class SkillIconTooltip : SimpleTooltip<SkillTooltipTextInfo>
{
	public const string PrefabPath = "Resources/Prefab2/Window/Tooltip/SkillIconTooltip";

	private static WindowPrefab<SkillIconTooltip> _instance = new WindowPrefab<SkillIconTooltip>("Resources/Prefab2/Window/Tooltip/SkillIconTooltip", GameUIRoot.ELayerType.UI);

	public UILabel SkillLevelLabel;

	public UILabel CooltimeLabel;

	public UIWidget ManaWidget;

	public UIGrid ManaGrid;

	public GameObject ManaGaugeTemplate;

	public UILabel ReqManaZeroLabel;

	public GameObject GoContainer1;

	public GameObject GoContainer2;

	private List<GameObject> _manaGaugeList = new List<GameObject>();

	public static SkillIconTooltip instance => _instance.instance;

	public static void Open(UIWidget ownerWidget, SkillTooltipTextInfo textInfo)
	{
		SimpleTooltip<SkillTooltipTextInfo>.Open(_instance, ownerWidget, textInfo);
	}

	public static void Hide(UIWidget ownerWidget)
	{
		SimpleTooltip<SkillTooltipTextInfo>.Hide(_instance, ownerWidget);
	}

	protected override void Open(UIWidget ownerWidget, TooltipPrintInfo info, SkillTooltipTextInfo textInfo, bool repositionWithCoroutine)
	{
		OnOffOnlyDesc(textInfo.IsOnlyDesc);
		base.Open(ownerWidget, info, textInfo, repositionWithCoroutine);
	}

	public void OnOffOnlyDesc(bool isOnlyDesc)
	{
		UIHelper.EnableGameObject(GoContainer1, !isOnlyDesc);
		UIHelper.EnableGameObject(GoContainer2, !isOnlyDesc);
		SetupDimension();
	}

	public override void Reset()
	{
		base.Reset();
		ClearManaGauge();
	}

	protected override void OnClose()
	{
		base.OnClose();
		ClearManaGauge();
	}

	private void ClearManaGauge()
	{
		foreach (GameObject manaGauge in _manaGaugeList)
		{
			VespaObject.DestroyGo(manaGauge);
		}
		_manaGaugeList.Clear();
	}

	protected override void SetupText(SkillTooltipTextInfo textInfo)
	{
		base.SetupText(textInfo);
		UIHelper.EnableGameObject(ManaWidget, textInfo.isActiveSkill);
		UIHelper.EnableGameObject(TypeLabel, !textInfo.isActiveSkill);
		UIHelper.EnableGameObject(ReqManaZeroLabel, enable: false);
		if (textInfo.SkillLevel != 0)
		{
			UIHelper.EnableGameObject(SkillLevelLabel, enable: true);
			UIHelper.SetLabelText(SkillLevelLabel, "Lv " + textInfo.SkillLevel);
		}
		else
		{
			UIHelper.EnableGameObject(SkillLevelLabel, enable: false);
		}
		if (textInfo.CooltimeMs != 0)
		{
			UIHelper.EnableGameObject(CooltimeLabel, enable: true);
			UIHelper.SetLabelText(CooltimeLabel, SkillBuilder.GetCooltimeDisplayText(textInfo.CooltimeMs));
		}
		else
		{
			UIHelper.EnableGameObject(CooltimeLabel, enable: false);
		}
		if (textInfo.TooltipType == SkillTooltipTextInfo.SkillTooltipType.SoulWeapon || textInfo.TooltipType == SkillTooltipTextInfo.SkillTooltipType.DungeonSkill)
		{
			UIHelper.EnableGameObject(ReqManaZeroLabel, enable: false);
			UIHelper.EnableGameObject(ManaWidget, enable: false);
			return;
		}
		if (textInfo.ReqManaGauge == 0)
		{
			if (textInfo.isActiveSkill)
			{
				UIHelper.EnableGameObject(ReqManaZeroLabel, enable: true);
				UIHelper.EnableGameObject(ManaWidget, enable: false);
			}
			return;
		}
		UIHelper.EnableGameObject(ReqManaZeroLabel, enable: false);
		UIHelper.EnableGameObject(ManaWidget, enable: true);
		for (int i = 0; i < textInfo.ReqManaGauge; i++)
		{
			GameObject gameObject = Object.Instantiate(ManaGaugeTemplate);
			Vespa_Unity_Transform_Extension_Method.ParentResetTM(gameObject.transform, ManaGrid.transform);
			gameObject.SetActive(value: true);
			_manaGaugeList.Add(gameObject);
		}
		ManaGaugeTemplate.SetActive(value: false);
		ManaGrid.Reposition();
	}

	protected override void OnComplete()
	{
		base.OnComplete();
		ManaGrid.Reposition();
	}
}
