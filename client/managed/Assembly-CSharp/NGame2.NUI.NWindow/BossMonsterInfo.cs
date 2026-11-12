using System.Collections.Generic;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NDescription;
using NShared;
using NVespa;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;

namespace NGame2.NUI.NWindow;

public class BossMonsterInfo : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/Dungeon/BossMonsterInfo";

	public UILabel Label_ClearTip;

	public UISimpleGrid SkillIconGrid;

	public UIGrid SkillDescGrid;

	public UILabel[] Label_SkillDescs;

	private List<HeroSkillIconComponent> _ListSkillIcons = new List<HeroSkillIconComponent>();

	private CreatureData _creatureData;

	private HeroInfo _HeroInfo;

	public void Open(HeroInfo heroInfo)
	{
		_HeroInfo = heroInfo;
		_creatureData = GameRoot.Instance.CreatureDataContainer.GetData(heroInfo.HeroIndex);
		if (_creatureData != null)
		{
			base.Open();
			SetupClearTip();
			SetupMonsterSkill();
		}
	}

	public override void Reset()
	{
		base.Reset();
		FactoryBase.ReleaseAndClearList(_ListSkillIcons);
	}

	private void SetupClearTip()
	{
		UIHelper.SetLabelText(Label_ClearTip, _creatureData.ClearTip);
	}

	private void SetupMonsterSkill()
	{
		for (int i = 1; i < 4; i++)
		{
			SetSkillIconAndDesc(i);
		}
		UIHelper.RepositionGrid(SkillIconGrid);
		SkillDescGrid.Reposition();
	}

	private void SetSkillIconAndDesc(int SkillSlotIndex)
	{
		if (SkillSlotIndex <= Label_SkillDescs.Length)
		{
			int skillIndexBySlot = _creatureData.GetSkillIndexBySlot(SkillSlotIndex);
			UILabel uILabel = Label_SkillDescs[SkillSlotIndex - 1];
			if (skillIndexBySlot == 0)
			{
				UIHelper.EnableGameObject(uILabel, enable: false);
				return;
			}
			UIHelper.EnableGameObject(uILabel, enable: true);
			HeroSkillIconComponent heroSkillIconComponent = UIHelper.AddGridItem(SkillIconGrid, HeroSkillIconComponent.PrefabAuto);
			heroSkillIconComponent.Open(_HeroInfo, SkillSlotIndex, null);
			_ListSkillIcons.Add(heroSkillIconComponent);
			GameRoot.Instance.SkillDataContainer.GetData(skillIndexBySlot);
			string byHeroInfo = SingletonAuto<SkillDescContainer>.instance.GetByHeroInfo(skillIndexBySlot, _HeroInfo);
			UIHelper.SetLabelText(uILabel, byHeroInfo);
		}
	}

	public void OnClickExitButton()
	{
		Close();
	}
}
