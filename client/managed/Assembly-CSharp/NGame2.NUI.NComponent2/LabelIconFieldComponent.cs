using System.Collections.Generic;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class LabelIconFieldComponent : LabelFieldComponent
{
	public new const string PrefabPath = "Prefab2/Component/LabelIconFieldComponent";

	public new static PrefabAuto<LabelIconFieldComponent> PrefabAuto = new PrefabAuto<LabelIconFieldComponent>("Prefab2/Component/LabelIconFieldComponent");

	public UISimpleGrid AttrTypeIconGrid;

	private List<SkillTypeIconComponent> _ListSkillTypeIcons = new List<SkillTypeIconComponent>();

	public void Open(string field, string value, SkillAttrType attrType)
	{
		Open(field, value);
		FactoryBase.ReleaseAndClearList(_ListSkillTypeIcons);
		SkillTypeIconComponent skillTypeIconComponent = UIHelper.AddGridItem(AttrTypeIconGrid, SkillTypeIconComponent.PrefabAuto);
		skillTypeIconComponent.Open(attrType);
		_ListSkillTypeIcons.Add(skillTypeIconComponent);
	}

	public void OpenCustom(string field, string value, string customIconName)
	{
		Open(field, value);
		FactoryBase.ReleaseAndClearList(_ListSkillTypeIcons);
		SkillTypeIconComponent skillTypeIconComponent = UIHelper.AddGridItem(AttrTypeIconGrid, SkillTypeIconComponent.PrefabAuto);
		skillTypeIconComponent.Open(customIconName);
		_ListSkillTypeIcons.Add(skillTypeIconComponent);
	}
}
