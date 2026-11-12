using System.Collections.Generic;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;

namespace NGame2.NUI.NWindow;

public class HeroIconTooltip : SimpleTooltip<HeroTooltipTextInfo>
{
	public const string PrefabPath = "Resources/Prefab2/Window/Tooltip/HeroIconTooltip";

	private static WindowPrefab<HeroIconTooltip> _instance = new WindowPrefab<HeroIconTooltip>("Resources/Prefab2/Window/Tooltip/HeroIconTooltip", GameUIRoot.ELayerType.UI);

	public UIWidget WidgetCreatureClassTag;

	public UIWidget WidgetCreatureAttrDefect;

	public UILabel Label_ClassName;

	public UILabel Label_AttType;

	public UISimpleGrid CreatureClassTagGrid;

	public UISimpleGrid AttrTypeIconGrid;

	public UISimpleGrid WeakTypeIconGrid;

	private List<ClassTagIconComponent> _ListClassTagIcons = new List<ClassTagIconComponent>();

	private List<SkillTypeIconComponent> _ListSkillTypeIcons = new List<SkillTypeIconComponent>();

	public static HeroIconTooltip instance => _instance.instance;

	public static void Open(UIWidget ownerWidget, HeroTooltipTextInfo textInfo)
	{
		SimpleTooltip<HeroTooltipTextInfo>.Open(_instance, ownerWidget, textInfo);
	}

	public static void Hide(UIWidget ownerWidget)
	{
		SimpleTooltip<HeroTooltipTextInfo>.Hide(_instance, ownerWidget);
	}

	public override void Reset()
	{
		base.Reset();
		FactoryBase.ReleaseAndClearList(_ListClassTagIcons);
		FactoryBase.ReleaseAndClearList(_ListSkillTypeIcons);
	}

	protected override void SetupText(HeroTooltipTextInfo textInfo)
	{
		base.SetupText(textInfo);
		SetupCreatureClassTag(textInfo.CreatureTagType);
		SetupCreatureAttrDefect(textInfo);
	}

	private void SetupCreatureClassTag(CreatureTagType tagType)
	{
		UIHelper.EnableGameObject(WidgetCreatureClassTag, tagType != CreatureTagType.None);
		if (tagType != CreatureTagType.None)
		{
			ClassTagIconComponent classTagIconComponent = UIHelper.AddGridItem(CreatureClassTagGrid, ClassTagIconComponent.PrefabAuto);
			classTagIconComponent.Open(tagType, null);
			_ListClassTagIcons.Add(classTagIconComponent);
			UIHelper.RepositionGrid(CreatureClassTagGrid);
			UIHelper.SetLabelText(Label_ClassName, LocalizationManager.GetString(tagType));
		}
	}

	private void SetupCreatureAttrDefect(HeroTooltipTextInfo textInfo)
	{
		if (!textInfo.DisplayCreatureAttrDefect)
		{
			UIHelper.EnableGameObject(WidgetCreatureAttrDefect, enable: false);
			return;
		}
		UIHelper.EnableGameObject(WidgetCreatureAttrDefect, enable: true);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(textInfo.ListAttrTypes); i++)
		{
			SkillTypeIconComponent skillTypeIconComponent = UIHelper.AddGridItem(AttrTypeIconGrid, SkillTypeIconComponent.PrefabAuto);
			skillTypeIconComponent.Open(textInfo.ListAttrTypes[i]);
			_ListSkillTypeIcons.Add(skillTypeIconComponent);
		}
		SkillAttrType skillAttrType = ((textInfo.ListAttrTypes.Count > 0) ? textInfo.ListAttrTypes[0] : SkillAttrType.None);
		UIHelper.SetLabelText(Label_AttType, LocalizationManager.GetString(skillAttrType));
		UIHelper.RepositionGrid(AttrTypeIconGrid);
		for (int j = 0; j < __Extension_Method_ICollection__.COUNT(textInfo.ListWeakTypes); j++)
		{
			SkillTypeIconComponent skillTypeIconComponent2 = UIHelper.AddGridItem(WeakTypeIconGrid, SkillTypeIconComponent.PrefabAuto);
			skillTypeIconComponent2.Open(textInfo.ListWeakTypes[j]);
			_ListSkillTypeIcons.Add(skillTypeIconComponent2);
		}
		UIHelper.RepositionGrid(WeakTypeIconGrid);
	}
}
