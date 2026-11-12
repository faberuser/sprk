using System.Collections.Generic;
using NNguiExtension;
using NShared;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NHero;

public class HeroInfoPanel : MonoBehaviour, IHeroPanel
{
	[Header("Info Component")]
	public UILabel Label_HeroName;

	public UISpriteSelector StarSelector;

	public UISpriteSelector TranscendStarSelector;

	public UIScrollGrid SkillListGrid;

	public UISimpleGrid AttributeListGrid;

	private HeroInfo _heroInfo;

	private List<PayShopSkillIconComponent> _listSkillIcons = new List<PayShopSkillIconComponent>();

	private List<AttributeIconComponent> _listAttributeIcons = new List<AttributeIconComponent>();

	public void Open(HeroInfo heroInfo)
	{
		_heroInfo = heroInfo;
		Init();
	}

	private void Init()
	{
		SetHeroName();
		SetHeroGrade();
		SetSkill();
		SetHeroAttribute();
	}

	public void SetHero(HeroInfo heroInfo)
	{
		Reset();
		_heroInfo = heroInfo;
		SetHeroName();
		SetHeroGrade();
		SetSkill();
		SetHeroAttribute();
	}

	private void SetHeroName()
	{
		bool enable = !String_Extension_Method.IsNullOrEmpty(_heroInfo.CreatureData.Title);
		UIHelper.EnableGameObject(Label_HeroName, enable);
		UIHelper.SetLabelText(Label_HeroName, _heroInfo.CreatureData.Title);
	}

	private void SetHeroGrade()
	{
		UIHelper.EnableGameObject(StarSelector, _heroInfo.Transcended == 0);
		UIHelper.EnableGameObject(TranscendStarSelector, _heroInfo.Transcended > 0);
		if (_heroInfo.Transcended == 0 && StarSelector != null)
		{
			StarSelector.SelectInt(_heroInfo.Star);
		}
		if (_heroInfo.Transcended > 0 && TranscendStarSelector != null)
		{
			TranscendStarSelector.SelectInt(_heroInfo.Transcended);
		}
	}

	private void SetSkill()
	{
		List<int> skillIndexList = _heroInfo.CreatureData.GetSkillIndexList(includeExtra: false);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(skillIndexList); i++)
		{
			int skillSlotIndex = i + 1;
			if (skillIndexList[i] != 0)
			{
				PayShopSkillIconComponent payShopSkillIconComponent = UIHelper.AddGridItem(SkillListGrid, PayShopSkillIconComponent.PrefabAuto);
				payShopSkillIconComponent.Open(_heroInfo, skillSlotIndex);
				_listSkillIcons.Add(payShopSkillIconComponent);
			}
		}
		UIHelper.RepositionGrid(SkillListGrid);
	}

	private void SetHeroAttribute()
	{
		AttributeIconComponent attributeIconComponent = UIHelper.AddGridItem(AttributeListGrid, AttributeIconComponent.PrefabAuto);
		attributeIconComponent.Open(LocalizationManager.GetString(_heroInfo.CreatureData.TagType), _heroInfo.CreatureData.TagType);
		_listAttributeIcons.Add(attributeIconComponent);
		AttributeIconComponent attributeIconComponent2 = UIHelper.AddGridItem(AttributeListGrid, AttributeIconComponent.PrefabAuto);
		SkillAttrType skillAttrType = ((_heroInfo.CreatureData.AttrType != null) ? _heroInfo.CreatureData.AttrType[0] : SkillAttrType.None);
		attributeIconComponent2.Open(LocalizationManager.GetString(skillAttrType), skillAttrType);
		_listAttributeIcons.Add(attributeIconComponent2);
		foreach (Tuple<string, string> heroPosition in _heroInfo.CreatureData.GetHeroPositionList())
		{
			string item = heroPosition.Item1;
			string stringDirect = LocalizationManager.GetStringDirect(heroPosition.Item2);
			if (String_Extension_Method.IsNullOrEmpty(item) || String_Extension_Method.IsNullOrEmpty(stringDirect))
			{
				break;
			}
			AttributeIconComponent attributeIconComponent3 = UIHelper.AddGridItem(AttributeListGrid, AttributeIconComponent.PrefabAuto);
			attributeIconComponent3.Open(stringDirect, item);
			_listAttributeIcons.Add(attributeIconComponent3);
		}
		UIHelper.RepositionGrid(AttributeListGrid);
	}

	public void Close()
	{
		UIHelper.EnableGameObject(Label_HeroName, enable: false);
		UIHelper.EnableGameObject(StarSelector, enable: false);
		UIHelper.EnableGameObject(TranscendStarSelector, enable: false);
		Reset();
	}

	public void Reset()
	{
		_heroInfo = null;
		FactoryBase.ReleaseAndClearList(_listSkillIcons);
		FactoryBase.ReleaseAndClearList(_listAttributeIcons);
	}
}
