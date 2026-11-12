using System.Collections.Generic;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;

namespace NGame2.NUI.NWindow;

public class ItemIconTooltip : SimpleTooltip<ItemTooltipTextInfo>
{
	public const string PrefabPath = "Resources/Prefab2/Window/Tooltip/ItemIconTooltip";

	private static WindowPrefab<ItemIconTooltip> _instance = new WindowPrefab<ItemIconTooltip>("Resources/Prefab2/Window/Tooltip/ItemIconTooltip", GameUIRoot.ELayerType.UI);

	public UILabel Desc1Label;

	public UIWidget WidgetEquipClassTag;

	public UIWidget WidgetEquipCreatureLimit;

	public UILabel Desc2Label;

	public UILabel CreatureLimitLabel;

	public UISprite SpriteBg;

	public UISimpleGrid ClassTagGrid;

	public UIWidget EquipRuneSlotRoot;

	public EquipRuneSlotInfoComponent EquipRuneSlotInfoComponent;

	private List<ClassTagIconComponent> _ListClassTagIcons = new List<ClassTagIconComponent>();

	private List<SkillTypeIconComponent> _ListSkillTypeIcons = new List<SkillTypeIconComponent>();

	public static ItemIconTooltip instance => _instance.instance;

	public static void Open(UIWidget ownerWidget, ItemTooltipTextInfo textInfo)
	{
		SimpleTooltip<ItemTooltipTextInfo>.Open(_instance, ownerWidget, textInfo);
	}

	public static void Hide(UIWidget ownerWidget)
	{
		SimpleTooltip<ItemTooltipTextInfo>.Hide(_instance, ownerWidget);
	}

	public override void Reset()
	{
		base.Reset();
		FactoryBase.ReleaseAndClearList(_ListClassTagIcons);
		FactoryBase.ReleaseAndClearList(_ListSkillTypeIcons);
	}

	protected override void CalcBounds()
	{
		SpriteBg.ForceUpdateAnchors(includeChildren: true);
		RootWidget.width = SpriteBg.width;
		RootWidget.height = SpriteBg.height;
		RootWidget.ForceUpdateAnchors(includeChildren: true);
	}

	protected override void SetupText(ItemTooltipTextInfo textInfo)
	{
		base.SetupText(textInfo);
		UIHelper.SetLabelText(Desc1Label, textInfo.Desc1.Value);
		UIHelper.EnableGameObject(Desc1Label, !String_Extension_Method.IsNullOrEmpty(textInfo.Desc1.Value));
		SetupEquipRuneSlot(textInfo);
		SetupEquipClassTag(textInfo);
		SetupEquipClassLimit(textInfo);
	}

	private void SetupEquipRuneSlot(ItemTooltipTextInfo textInfo)
	{
		UIHelper.EnableGameObject(EquipRuneSlotRoot, !__Extension_Method_ICollection__.IsNullOrEmpty(textInfo.EquipRuneSlotTypes));
		if (EquipRuneSlotInfoComponent != null)
		{
			EquipRuneSlotInfoComponent.DirectRefresh(textInfo.EquipRuneSlotTypes);
		}
	}

	private void SetupEquipClassTag(ItemTooltipTextInfo textInfo)
	{
		UIHelper.EnableGameObject(WidgetEquipClassTag, textInfo.DisplayEquipClassTags);
		if (!textInfo.DisplayEquipClassTags)
		{
			return;
		}
		if (textInfo.EquipClassTags.Count > 0)
		{
			textInfo.EquipClassTags.ForEach(delegate(CreatureTagType tagType)
			{
				ClassTagIconComponent classTagIconComponent2 = UIHelper.AddGridItem(ClassTagGrid, ClassTagIconComponent.PrefabAuto);
				classTagIconComponent2.Open(tagType, null);
				_ListClassTagIcons.Add(classTagIconComponent2);
			});
		}
		if (textInfo.EquipAllClassTag)
		{
			ClassTagIconComponent classTagIconComponent = UIHelper.AddGridItem(ClassTagGrid, ClassTagIconComponent.PrefabAuto);
			classTagIconComponent.OpenAllClassTag(null);
			_ListClassTagIcons.Add(classTagIconComponent);
		}
		UIHelper.RepositionGrid(ClassTagGrid);
		UIHelper.SetLabelText(Desc2Label, textInfo.Desc2.Value);
	}

	private void SetupEquipClassLimit(ItemTooltipTextInfo textInfo)
	{
		UIHelper.EnableGameObject(WidgetEquipCreatureLimit, textInfo.DisplayEquipCreatureLimit);
		if (textInfo.DisplayEquipCreatureLimit)
		{
			UIHelper.SetLabelText(CreatureLimitLabel, (!String_Extension_Method.IsNullOrEmpty(textInfo.CreatureLimit)) ? textInfo.CreatureLimit : string.Empty);
		}
	}
}
