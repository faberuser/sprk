using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NComponent2.NPortrait;
using NGame2.NUI.NManager;
using NShared;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

[UIEvent(AllowDuringPatchDownloading = true)]
public class MonsterInfoPopup : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/MessagePopup/MonsterInfoPopup";

	public UILabel Label_Level;

	public UILabel Label_Name;

	public UILabel Label_Type;

	public UILabel Label_Hp;

	public UILabel Label_Attack;

	public UILabel Label_PDefense;

	public UILabel Label_MDefense;

	public UISimpleGrid AttrTypeIconGrid;

	public UISimpleGrid BossGrid;

	public UISimpleGrid MonsterGrid;

	[SerializeField]
	private UILabel LbMonsterTribe;

	private List<SkillTypeIconComponent> _ListSkillTypeIcons = new List<SkillTypeIconComponent>();

	private MonsterComponent _MonsterComponent;

	public static void Open(MonsterHeroInfo info, MonsterComponent.StockData stock, CreatureData creatureData, MonsterTierData tierData)
	{
		WindowPrefab<MonsterInfoPopup> windowPrefab = new WindowPrefab<MonsterInfoPopup>("Resources/Prefab2/Window/MessagePopup/MonsterInfoPopup", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal(info, stock, creatureData, tierData);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	public override void Reset()
	{
		base.Reset();
		FactoryBase.ReleaseAndClearList(_ListSkillTypeIcons);
		FactoryBase.SafeReleaseComponent(ref _MonsterComponent);
	}

	private void OpenInternal(MonsterHeroInfo info, MonsterComponent.StockData stock, CreatureData creatureData, MonsterTierData tierData)
	{
		base.Open();
		GameRoot.Instance.CreatureVisualDataContainer.GetCreatureName(creatureData, info.VisualInfo, LocalizationManager.GetStringDirect, out var creatureName, out var _);
		UIHelper.SetLabelText(Label_Level, "Lv" + info.HeroInfo.Level);
		UIHelper.SetLabelText(Label_Name, creatureName);
		if (!stock.ShowName)
		{
			_MonsterComponent = UIHelper.AddGridItem(MonsterGrid, MonsterComponent.PrefabAuto);
		}
		else
		{
			_MonsterComponent = UIHelper.AddGridItem(BossGrid, MonsterComponent.BossPrefabAuto);
		}
		if (_MonsterComponent != null)
		{
			_MonsterComponent.Open(info, stock, null, bDisableClick: true);
		}
		SkillAttrType skillAttrType = ((creatureData != null && creatureData.AttrType != null) ? creatureData.AttrType[0] : SkillAttrType.None);
		if (skillAttrType != SkillAttrType.None)
		{
			SkillTypeIconComponent skillTypeIconComponent = UIHelper.AddGridItem(AttrTypeIconGrid, SkillTypeIconComponent.PrefabAuto);
			skillTypeIconComponent.Open(creatureData.AttrType[0]);
			_ListSkillTypeIcons.Add(skillTypeIconComponent);
		}
		UIHelper.RepositionGrid(AttrTypeIconGrid);
		UIHelper.SetLabelText(Label_Type, LocalizationManager.GetString(creatureData.AttrType[0]));
		StatData.GetHeroMainStats(skillAttrType);
		StatData heroStat = SceneAutoBehaviour<StatDataCache>.instance.GetHeroStat(info.HeroInfo);
		switch (skillAttrType)
		{
		case SkillAttrType.Physical:
			UIHelper.SetLabelText(Label_Attack, heroStat.PhysicalAttack * tierData.PhysicalAttack / 1000);
			break;
		case SkillAttrType.Magical:
			UIHelper.SetLabelText(Label_Attack, heroStat.MagicalAttack * tierData.MagicalAttack / 1000);
			break;
		}
		UIHelper.SetLabelText(Label_Hp, heroStat.MaxHp * tierData.MaxHp / 1000);
		UIHelper.SetLabelText(Label_PDefense, heroStat.PhysicalDefense * tierData.PhysicalDefense / 1000);
		UIHelper.SetLabelText(Label_MDefense, heroStat.MagicalDefense * tierData.MagicalDefense / 1000);
		SetMonsterTribe(creatureData.Tribe);
	}

	private void SetMonsterTribe(string tribe)
	{
		if (LbMonsterTribe != null)
		{
			UIHelper.SetLabelText(LbMonsterTribe, tribe);
		}
	}
}
