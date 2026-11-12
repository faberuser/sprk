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

public class MonsterArchiveInfoPopup : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/MessagePopup/MonsterArchiveInfoPopup";

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

	public UIWidget ComboBoxWidget;

	[SerializeField]
	private UILabel LbMonsterTribe;

	private List<SkillTypeIconComponent> _ListSkillTypeIcons = new List<SkillTypeIconComponent>();

	private MonsterComponent _MonsterComponent;

	private NormalMonsterArchiveData _monArchiveData;

	private MonsterHeroInfo _monsterInfo;

	private int _curChatperIndex;

	private DungeonDifficultyType _curDifficulty;

	private bool _isEnableDifficulty;

	public static void Open(NormalMonsterArchiveData MonsterArchiveData, int chapterIndex, bool IsEnableDifficulty = false)
	{
		WindowPrefab<MonsterArchiveInfoPopup> windowPrefab = new WindowPrefab<MonsterArchiveInfoPopup>("Resources/Prefab2/Window/MessagePopup/MonsterArchiveInfoPopup", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal(MonsterArchiveData, chapterIndex, IsEnableDifficulty);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	public override void Reset()
	{
		base.Reset();
		Clear();
	}

	private void Clear()
	{
		FactoryBase.ReleaseAndClearList(_ListSkillTypeIcons);
		FactoryBase.SafeReleaseComponent(ref _MonsterComponent);
	}

	private void OpenInternal(NormalMonsterArchiveData MonsterArchiveData, int chapterIndex, bool IsEnableDifficulty)
	{
		base.Open();
		_monArchiveData = MonsterArchiveData;
		_monsterInfo = MonsterArchiveData.GetMainMonsterInfo();
		_curChatperIndex = chapterIndex;
		_isEnableDifficulty = IsEnableDifficulty;
		_curDifficulty = MonsterArchiveData.MainDifficulty;
		SetMonsterInfo();
	}

	private void SetMonsterInfo()
	{
		Clear();
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(_monsterInfo.HeroInfo.HeroIndex);
		MonsterTierData data2 = GameRoot.Instance.MonsterTierDataContainer.GetData(_monsterInfo.Tier);
		if (data2 != null)
		{
			GameRoot.Instance.CreatureVisualDataContainer.GetCreatureName(data, _monsterInfo.VisualInfo, LocalizationManager.GetStringDirect, out var creatureName, out var _);
			UIHelper.SetLabelText(Label_Level, "Lv" + _monsterInfo.HeroInfo.Level);
			UIHelper.SetLabelText(Label_Name, creatureName);
			_MonsterComponent = UIHelper.AddGridItem(MonsterGrid, MonsterComponent.PrefabAuto);
			if (_MonsterComponent != null)
			{
				_monsterInfo.IsBoss = false;
				_MonsterComponent.Open(_monsterInfo, default(MonsterComponent.StockData), null, bDisableClick: true);
			}
			SkillAttrType skillAttrType = ((data != null && data.AttrType != null) ? data.AttrType[0] : SkillAttrType.None);
			if (skillAttrType != SkillAttrType.None)
			{
				SkillTypeIconComponent skillTypeIconComponent = UIHelper.AddGridItem(AttrTypeIconGrid, SkillTypeIconComponent.PrefabAuto);
				skillTypeIconComponent.Open(data.AttrType[0]);
				_ListSkillTypeIcons.Add(skillTypeIconComponent);
			}
			UIHelper.RepositionGrid(AttrTypeIconGrid);
			UIHelper.SetLabelText(Label_Type, LocalizationManager.GetString(data.AttrType[0]));
			StatData.GetHeroMainStats(skillAttrType);
			StatData heroStat = SceneAutoBehaviour<StatDataCache>.instance.GetHeroStat(_monsterInfo.HeroInfo);
			switch (skillAttrType)
			{
			case SkillAttrType.Physical:
				UIHelper.SetLabelText(Label_Attack, heroStat.PhysicalAttack * data2.PhysicalAttack / 1000);
				break;
			case SkillAttrType.Magical:
				UIHelper.SetLabelText(Label_Attack, heroStat.MagicalAttack * data2.MagicalAttack / 1000);
				break;
			}
			UIHelper.SetLabelText(Label_Hp, heroStat.MaxHp * data2.MaxHp / 1000);
			UIHelper.SetLabelText(Label_PDefense, heroStat.PhysicalDefense * data2.PhysicalDefense / 1000);
			UIHelper.SetLabelText(Label_MDefense, heroStat.MagicalDefense * data2.MagicalDefense / 1000);
			SetMonsterTribe(data.Tribe);
		}
	}

	private void SetMonsterTribe(string tribe)
	{
		if (LbMonsterTribe != null)
		{
			UIHelper.SetLabelText(LbMonsterTribe, tribe);
		}
	}
}
