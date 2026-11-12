using System.Collections.Generic;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class AttributeIconComponent : UIScrollItemBehaviour
{
	public const string PrefabPath = "Prefab2/Component/ShopItem/AttributeIconComponent";

	public static PrefabAuto<AttributeIconComponent> PrefabAuto = new PrefabAuto<AttributeIconComponent>("Prefab2/Component/ShopItem/AttributeIconComponent");

	public UIWidget RootWidget;

	public UILabel ValueLabel;

	public UISprite ValueSprite;

	public UISimpleGrid AttrTypeIconGrid;

	private List<SkillTypeIconComponent> _ListSkillTypeIcons = new List<SkillTypeIconComponent>();

	private List<ClassTagIconComponent> _ListTagTypeIcons = new List<ClassTagIconComponent>();

	public void Open(string value, string iconPath)
	{
		UIHelper.SetLabelText(ValueLabel, value);
		ResetIcon();
		UIHelper.EnableGameObject(ValueSprite, !String_Extension_Method.IsNullOrEmpty(iconPath));
		ValueSprite.spriteName = iconPath;
	}

	public void Open(string value, SkillAttrType attrType)
	{
		UIHelper.SetLabelText(ValueLabel, value);
		ResetIcon();
		SkillTypeIconComponent skillTypeIconComponent = UIHelper.AddGridItem(AttrTypeIconGrid, SkillTypeIconComponent.PrefabAuto);
		skillTypeIconComponent.Open(attrType);
		_ListSkillTypeIcons.Add(skillTypeIconComponent);
	}

	public void Open(string value, CreatureTagType tagType)
	{
		UIHelper.SetLabelText(ValueLabel, value);
		ResetIcon();
		ClassTagIconComponent classTagIconComponent = UIHelper.AddGridItem(AttrTypeIconGrid, ClassTagIconComponent.PrefabAuto);
		classTagIconComponent.Open(tagType, null);
		_ListTagTypeIcons.Add(classTagIconComponent);
	}

	private void ResetIcon()
	{
		UIHelper.EnableGameObject(ValueSprite, enable: false);
		FactoryBase.ReleaseAndClearList(_ListSkillTypeIcons);
		FactoryBase.ReleaseAndClearList(_ListTagTypeIcons);
	}
}
