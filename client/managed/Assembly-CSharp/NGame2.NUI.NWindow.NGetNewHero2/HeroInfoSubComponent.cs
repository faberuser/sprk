using System;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NGameAsset;
using NNguiExtension;
using NShared;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow.NGetNewHero2;

public class HeroInfoSubComponent : MonoBehaviour
{
	[Serializable]
	public class CommonUiData
	{
		public UITexture ProfileTex;

		public UILabel HeroName;

		public UISpriteSelector StarSelector;

		public UISpriteSelector TranscendSelector;
	}

	[Serializable]
	public class ReqItemUiData
	{
		public UITexture ItemIcon;

		public UILabel CountLabel;
	}

	[Serializable]
	public class NewHeroUiData
	{
		[Serializable]
		public class SkillUiData
		{
			public UILabel ReqLevel;

			public UILabel SkillName;

			public UISimpleGrid SkillGrid;
		}

		[Serializable]
		public class HeroInfoUiData
		{
			public UIWidget Root;

			public UILabel Class;

			public UILabel Level;

			public UILabel AttAttributeLabel;

			public UISimpleGrid AttrTypeIconGrid;
		}

		[Serializable]
		public class HeroDescUiData
		{
			public UIWidget Root;

			public UILabel Desc;
		}

		public UIWidget Root;

		public HeroInfoUiData HeroInfoUi = new HeroInfoUiData();

		public HeroDescUiData HeroDescUi = new HeroDescUiData();

		public HeroSkillSubComponent Skill1;

		public HeroSkillSubComponent Skill2;

		public HeroSkillSubComponent Skill3;

		public HeroSkillSubComponent Skill4;

		public HeroSkillSubComponent GetSkillUi(int skillSlot)
		{
			return skillSlot switch
			{
				1 => Skill1, 
				2 => Skill2, 
				3 => Skill3, 
				4 => Skill4, 
				_ => null, 
			};
		}
	}

	[Serializable]
	public class AwakenUiData
	{
		public UIWidget Root;

		public UISimpleGrid StatGrid;

		public Transform AwakenCompleteRoot;

		public Transform ReqItemRoot;

		public UILabel UpgradeGuideLabel;

		public UILabel Label_AwakenInfo;

		public UILabel Label_TranscendInfo;

		public UILabel Label_MaxTranscended;

		public ReqItemUiData TranscendReqItem1;

		public ReqItemUiData TranscendReqItem2;

		public ReqItemUiData TranscendReqItem3;

		public ReqItemUiData TranscendReqItem4;

		public ReqItemUiData ReqItem1 = new ReqItemUiData();

		public ReqItemUiData ReqItem2 = new ReqItemUiData();

		public ReqItemUiData ReqItem3 = new ReqItemUiData();

		public ReqItemUiData GetReqItemUi(int index)
		{
			return index switch
			{
				1 => ReqItem1, 
				2 => ReqItem2, 
				3 => ReqItem3, 
				_ => null, 
			};
		}

		public ReqItemUiData GetTranscendReqItemUi(int index)
		{
			return index switch
			{
				1 => TranscendReqItem1, 
				2 => TranscendReqItem2, 
				3 => TranscendReqItem3, 
				4 => TranscendReqItem4, 
				_ => null, 
			};
		}
	}

	[Serializable]
	public class TranscendUiData
	{
		[Serializable]
		public class HeroStatAndTranscendInfoUiData
		{
			public UIWidget Root;

			public UISimpleGrid StatGrid;

			public Transform TranscendReqItemRoot;

			public Transform MaxTranscendedRoot;

			public ReqItemUiData TranscendReqItem1;

			public ReqItemUiData TranscendReqItem2;

			public ReqItemUiData TranscendReqItem3;

			public ReqItemUiData TranscendReqItem4;

			public ReqItemUiData GetTranscendReqItemUi(int index)
			{
				return index switch
				{
					1 => TranscendReqItem1, 
					2 => TranscendReqItem2, 
					3 => TranscendReqItem3, 
					4 => TranscendReqItem4, 
					_ => null, 
				};
			}
		}

		[Serializable]
		public class TranscendAttrUiData
		{
			public UIWidget Root;

			public HeroTranscend[] TranscendAttrLocalEnums;

			public UILabel Label_SelectTranscendAttrSelect;

			public UILabel TranscendSkillPointLabel;

			public UILabel Label_SkillListTitle;

			public UISimpleGrid TranscendSkillListGrid;
		}

		public UIWidget Root;

		public HeroStatAndTranscendInfoUiData StatAndTranscendInfoUiData;

		public TranscendAttrUiData AttrUiData;
	}

	[Serializable]
	public class LimitBreakUiData
	{
		public UIWidget Root;

		public UILabel Label_LimitBreakResultDesc;

		public UILabel Label_LimitBreakMaxLevelDesc;

		public UILabel Label_LimitBreakTransPointDesc;
	}

	public Transform Root;

	public Transform RightPos;

	public Transform LeftPos;

	public CommonUiData CommonUi = new CommonUiData();

	public NewHeroUiData NewHeroUi = new NewHeroUiData();

	public AwakenUiData AwakenUi = new AwakenUiData();

	public TranscendUiData TranscendUi;

	public LimitBreakUiData LimitBreakUi;

	private GetNewHero2 _owner;

	private HeroInfo _heroInfo;

	private GetNewHero2.EOpenMode _openMode;

	private PoolingList<LabelFieldComponent> _statComponentList;

	private PoolingList<SkillTypeIconComponent> _ListSkillTypeIcons;

	private PoolingList<HeroTranscendSkillIconComponent> _ListTranscendSkillIcons;

	public void Open(GetNewHero2 owner, HeroInfo heroInfo, GetNewHero2.EOpenMode openMode, bool openLeftSide)
	{
		_owner = owner;
		_heroInfo = heroInfo;
		_openMode = openMode;
		float i_value = (openLeftSide ? LeftPos.localPosition.x : RightPos.localPosition.x);
		Root.localPosition = Vespa_Unity_Vector3_Extension_Method.ReplaceX(Root.localPosition, i_value);
		SetupCommon();
		if (_openMode == GetNewHero2.EOpenMode.NewHero)
		{
			SetupNewHeroUi();
		}
		else if (_openMode == GetNewHero2.EOpenMode.Awaken)
		{
			SetupAwaken();
		}
		else if (_openMode == GetNewHero2.EOpenMode.Transcend)
		{
			SetupTranscend();
		}
		else if (_openMode == GetNewHero2.EOpenMode.LimitBreak)
		{
			SetupLimitBreak();
		}
	}

	public void Reset()
	{
		_owner = null;
		_heroInfo = null;
		_openMode = GetNewHero2.EOpenMode.None;
		FactoryBase.ReleaseAndClearList(ref _statComponentList);
		FactoryBase.ReleaseAndClearList(ref _ListSkillTypeIcons);
		FactoryBase.ReleaseAndClearList(ref _ListTranscendSkillIcons);
	}

	private void SetupCommon()
	{
		if (_heroInfo == null)
		{
			return;
		}
		AssetTexture heroIcon = UIHelper.GetHeroIcon2(_heroInfo.HeroIndex, _heroInfo.Star);
		UIHelper.ResetPlaneAssetTextureAsync(this, CommonUi.ProfileTex, heroIcon);
		if (_heroInfo == null || _heroInfo.CreatureData == null)
		{
			return;
		}
		UIHelper.SetLabelText(CommonUi.HeroName, _heroInfo.CreatureData.Name);
		if (_openMode == GetNewHero2.EOpenMode.NewHero || _openMode == GetNewHero2.EOpenMode.Awaken || _openMode == GetNewHero2.EOpenMode.Transcend || _openMode == GetNewHero2.EOpenMode.LimitBreak)
		{
			if (_heroInfo.Transcended > 0)
			{
				if (CommonUi.TranscendSelector != null)
				{
					CommonUi.TranscendSelector.SelectInt(_heroInfo.Transcended);
				}
				UIHelper.EnableGameObject(CommonUi.TranscendSelector, enable: true);
				UIHelper.EnableGameObject(CommonUi.StarSelector, enable: false);
			}
			else
			{
				if (CommonUi.StarSelector != null)
				{
					CommonUi.StarSelector.SelectInt(_heroInfo.Star);
				}
				UIHelper.EnableGameObject(CommonUi.StarSelector, enable: true);
				UIHelper.EnableGameObject(CommonUi.TranscendSelector, enable: false);
			}
		}
		else if (_openMode == GetNewHero2.EOpenMode.None)
		{
			UIHelper.EnableGameObject(CommonUi.TranscendSelector, enable: false);
			UIHelper.EnableGameObject(CommonUi.StarSelector, enable: false);
		}
		else
		{
			VespaDebug.LogError("Not Implemented Yet: " + Vespa_String_Extension_Method.GetEnumString(_openMode));
			UIHelper.EnableGameObject(CommonUi.TranscendSelector, enable: false);
			UIHelper.EnableGameObject(CommonUi.StarSelector, enable: false);
		}
	}

	private void SetupNewHeroUi()
	{
		if (_heroInfo == null || _heroInfo.CreatureData == null)
		{
			return;
		}
		UIHelper.EnableGameObject(NewHeroUi.Root, enable: true);
		UIHelper.EnableGameObject(AwakenUi.Root, enable: false);
		UIHelper.EnableGameObject(TranscendUi.Root, enable: false);
		UIHelper.EnableGameObject(LimitBreakUi.Root, enable: false);
		string text = LocalizationManager.GetString(_heroInfo.CreatureData.TagType);
		UIHelper.SetLabelText(NewHeroUi.HeroInfoUi.Class, text);
		UIHelper.SetLabelText(NewHeroUi.HeroInfoUi.Level, $"Lv {_heroInfo.Level}");
		SkillAttrType skillAttrType = ((_heroInfo.CreatureData.AttrType != null) ? _heroInfo.CreatureData.AttrType[0] : SkillAttrType.None);
		UIHelper.SetLabelText(NewHeroUi.HeroInfoUi.AttAttributeLabel, LocalizationManager.GetString(skillAttrType));
		SkillTypeIconComponent skillTypeIconComponent = UIHelper.AddGridItem(NewHeroUi.HeroInfoUi.AttrTypeIconGrid, SkillTypeIconComponent.PrefabAuto);
		skillTypeIconComponent.Open(skillAttrType);
		_ListSkillTypeIcons.Add(skillTypeIconComponent);
		UIHelper.SetLabelText(NewHeroUi.HeroDescUi.Desc, LocalizationManager.GetStringDirect("GETHERO_DESC_" + _heroInfo.HeroIndex));
		for (int i = 1; i <= 4; i++)
		{
			HeroSkillSubComponent skillUi = NewHeroUi.GetSkillUi(i);
			if (!(skillUi == null))
			{
				int skillLevel = _heroInfo.GetSkillLevel(i, isUserHero: true);
				skillUi.Open(_heroInfo, i, null);
				skillUi.SetExternalSkillLevel($"Lv {skillLevel}");
			}
		}
	}

	private void SetupAwaken()
	{
		if (_heroInfo == null || _heroInfo.CreatureData == null)
		{
			return;
		}
		UIHelper.EnableGameObject(NewHeroUi.Root, enable: false);
		UIHelper.EnableGameObject(AwakenUi.Root, enable: true);
		UIHelper.EnableGameObject(TranscendUi.Root, enable: false);
		UIHelper.EnableGameObject(LimitBreakUi.Root, enable: false);
		StatType[] heroMainStats = StatData.GetHeroMainStats((_heroInfo.CreatureData.AttrType != null) ? _heroInfo.CreatureData.AttrType[0] : SkillAttrType.None);
		_heroInfo.Star--;
		StatData heroStat = SceneAutoBehaviour<StatDataCache>.instance.GetHeroStat(_heroInfo);
		int maxLevel = _heroInfo.GetMaxLevel();
		_heroInfo.Star++;
		StatData heroStat2 = SceneAutoBehaviour<StatDataCache>.instance.GetHeroStat(_heroInfo);
		int maxLevel2 = _heroInfo.GetMaxLevel();
		if (maxLevel != maxLevel2)
		{
			LabelFieldComponent labelFieldComponent = UIHelper.AddGridItem(AwakenUi.StatGrid, LabelFieldComponent.PrefabAuto);
			labelFieldComponent.Open(LocalizationManager.GetString(Hero.HERO_MAX_LEVEL_INCREASED), "Lv " + maxLevel2, labelFieldComponent.ValueEnhancedColor);
			_statComponentList.Add(labelFieldComponent);
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(heroMainStats); i++)
		{
			StatType statType = heroMainStats[i];
			long stat = heroStat2.GetStat(statType);
			long stat2 = heroStat.GetStat(statType);
			LabelFieldComponent labelFieldComponent2 = UIHelper.AddGridItem(AwakenUi.StatGrid, LabelFieldComponent.PrefabAuto);
			labelFieldComponent2.Open(statType, stat, stat - stat2);
			_statComponentList.Add(labelFieldComponent2);
		}
		AwakenUi.StatGrid.RepositionGrid();
		int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("HeroMaxStar");
		if (_heroInfo.Star >= valueInt)
		{
			UIHelper.EnableGameObject(AwakenUi.Label_AwakenInfo, enable: false);
			UIHelper.EnableGameObject(AwakenUi.Label_TranscendInfo, enable: true);
			UIHelper.EnableGameObject(AwakenUi.AwakenCompleteRoot, enable: true);
			UIHelper.EnableGameObject(AwakenUi.ReqItemRoot, enable: false);
			sbyte transcended = _heroInfo.Transcended;
			_heroInfo.Transcended = 0;
			SetupTranscendReqItems(AwakenUi.GetTranscendReqItemUi);
			_heroInfo.Transcended = transcended;
			return;
		}
		UIHelper.EnableGameObject(AwakenUi.AwakenCompleteRoot, enable: false);
		UIHelper.EnableGameObject(AwakenUi.ReqItemRoot, enable: true);
		UIHelper.EnableGameObject(AwakenUi.Label_AwakenInfo, enable: true);
		UIHelper.EnableGameObject(AwakenUi.Label_TranscendInfo, enable: false);
		CreatureAwakeChallengeItemData data = GameRoot.Instance.CreatureAwakeChallengeItemDataContainer.GetData(_heroInfo.CreatureData.TagType);
		if (data == null)
		{
			return;
		}
		for (int j = 1; j <= 3; j++)
		{
			ReqItemUiData reqItemUi = AwakenUi.GetReqItemUi(j);
			string itemCode = data.GetItemCode(j);
			if (reqItemUi != null && !String_Extension_Method.IsNullOrEmpty(itemCode))
			{
				int awakeHeroItemCount = HeroTrialDungeonManagement.GetAwakeHeroItemCount(_heroInfo.Star);
				AssetTexture itemIcon = UIHelper.GetItemIcon2(itemCode);
				UIHelper.ResetPlaneAssetTextureAsync(this, reqItemUi.ItemIcon, itemIcon);
				UIHelper.SetLabelText(reqItemUi.CountLabel, awakeHeroItemCount.ToString());
			}
		}
	}

	private void SetupTranscend()
	{
		if (_heroInfo != null && _heroInfo.CreatureData != null)
		{
			UIHelper.EnableGameObject(NewHeroUi.Root, enable: false);
			UIHelper.EnableGameObject(AwakenUi.Root, enable: false);
			UIHelper.EnableGameObject(TranscendUi.Root, enable: true);
			UIHelper.EnableGameObject(LimitBreakUi.Root, enable: false);
			SetupTranscendAttrInfo();
		}
	}

	private void SetupLimitBreak()
	{
		if (_heroInfo != null && _heroInfo.CreatureData != null)
		{
			UIHelper.EnableGameObject(NewHeroUi.Root, enable: false);
			UIHelper.EnableGameObject(AwakenUi.Root, enable: false);
			UIHelper.EnableGameObject(TranscendUi.Root, enable: false);
			UIHelper.EnableGameObject(LimitBreakUi.Root, enable: true);
			UIHelper.SetLabelText(LimitBreakUi.Label_LimitBreakResultDesc, LocalizationManager.GetStringDirect("LIMITBREAK_ACHIEVEMENT_STEP_INFO", _heroInfo.LimitBreakLevel));
			UIHelper.SetLabelText(LimitBreakUi.Label_LimitBreakMaxLevelDesc, LocalizationManager.GetStringDirect("LIMITBREAK_STEP_MAXLEVEL_INFO", _heroInfo.GetMaxLevel()));
			int addTranscendSkillPoint = GameRoot.Instance.HeroLimitBreakDataContainer.GetAddTranscendSkillPoint(_heroInfo.HeroIndex, _heroInfo.LimitBreakLevel);
			UIHelper.EnableGameObject(LimitBreakUi.Label_LimitBreakTransPointDesc, addTranscendSkillPoint > 0);
			UIHelper.SetLabelText(LimitBreakUi.Label_LimitBreakTransPointDesc, LocalizationManager.GetStringDirect("LIMITBREAK_STEP_TRANSPOINT_INFO", addTranscendSkillPoint));
		}
	}

	private void SetupTranscendAttrInfo()
	{
		sbyte transcended = _heroInfo.Transcended;
		int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("HeroMaxTranscend");
		if (transcended > valueInt)
		{
			VespaDebug.LogError("Current Transcended is more than MaxHeroTranscended");
			return;
		}
		string text = LocalizationManager.GetString(HeroTranscend.TRANSCEND_RESULT_MSG, transcended);
		UIHelper.SetLabelText(TranscendUi.AttrUiData.Label_SelectTranscendAttrSelect, text);
		int num = _heroInfo.CreatureData.GetTranscendSkillPoint[_heroInfo.Transcended - 1];
		string stringDirect = LocalizationManager.GetStringDirect("ADDED_TRANSCEND_SKILL_POINT", num);
		UIHelper.SetLabelText(TranscendUi.AttrUiData.TranscendSkillPointLabel, stringDirect);
		Enum key = null;
		if (transcended <= TranscendUi.AttrUiData.TranscendAttrLocalEnums.Length)
		{
			key = TranscendUi.AttrUiData.TranscendAttrLocalEnums[transcended - 1];
		}
		string text2 = LocalizationManager.GetString(key);
		UIHelper.SetLabelText(TranscendUi.AttrUiData.Label_SkillListTitle, text2);
		if (transcended == 3 || transcended == 4)
		{
			SetupTranscendSkillInfo(3);
			SetupTranscendSkillInfo(4);
		}
		else
		{
			SetupTranscendSkillInfo(transcended);
		}
	}

	private void SetupTranscendSkillInfo(int curTranscended)
	{
		CreatureData creatureData = _heroInfo.CreatureData;
		int[] transcendSkill = creatureData.GetTranscendSkill(curTranscended);
		if (transcendSkill == null)
		{
			return;
		}
		for (int i = 0; i < transcendSkill.Length; i++)
		{
			HeroTranscendSkillIconComponent heroTranscendSkillIconComponent = UIHelper.AddGridItem(TranscendUi.AttrUiData.TranscendSkillListGrid, HeroTranscendSkillIconComponent.PrefabAuto2);
			CreatureData.TranscendSkill transcendSkillData = creatureData.GetTranscendSkillData(curTranscended, i);
			if (transcendSkillData != null)
			{
				heroTranscendSkillIconComponent.Open(_heroInfo, transcendSkillData);
				_ListTranscendSkillIcons.Add(heroTranscendSkillIconComponent);
			}
		}
		UIHelper.RepositionGrid(TranscendUi.AttrUiData.TranscendSkillListGrid);
	}

	public void OnToggleHeroInfo(UIToggle toggle)
	{
		if (!(toggle == null) && NewHeroUi != null && NewHeroUi.HeroInfoUi != null)
		{
			UIHelper.EnableGameObject(NewHeroUi.HeroInfoUi.Root, toggle.value);
		}
	}

	public void OnToggleHeroDesc(UIToggle toggle)
	{
		if (!(toggle == null) && NewHeroUi != null && NewHeroUi.HeroDescUi != null)
		{
			UIHelper.EnableGameObject(NewHeroUi.HeroDescUi.Root, toggle.value);
		}
	}

	public void OnToggleHeroTranscendInfo(UIToggle toggle)
	{
		if (!(toggle == null) && TranscendUi != null && TranscendUi.StatAndTranscendInfoUiData != null)
		{
			UIHelper.EnableGameObject(TranscendUi.StatAndTranscendInfoUiData.Root, toggle.value);
		}
	}

	public void OnToggleTranscendAttr(UIToggle toggle)
	{
		if (!(toggle == null) && TranscendUi != null && TranscendUi.AttrUiData != null)
		{
			UIHelper.EnableGameObject(TranscendUi.AttrUiData.Root, toggle.value);
		}
	}

	private void SetupTranscendReqItems(Func<int, ReqItemUiData> ItemUiGetter)
	{
		int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("HeroMaxTranscend");
		if (_heroInfo.Transcended == valueInt)
		{
			UIHelper.EnableGameObject(TranscendUi.StatAndTranscendInfoUiData.TranscendReqItemRoot, enable: false);
			UIHelper.EnableGameObject(TranscendUi.StatAndTranscendInfoUiData.MaxTranscendedRoot, enable: true);
			return;
		}
		UIHelper.EnableGameObject(TranscendUi.StatAndTranscendInfoUiData.TranscendReqItemRoot, enable: true);
		UIHelper.EnableGameObject(TranscendUi.StatAndTranscendInfoUiData.MaxTranscendedRoot, enable: false);
		if (_heroInfo.Transcended > valueInt)
		{
			VespaDebug.LogError("Current Hero Transcended is more than MaxHeroTranscended");
			return;
		}
		sbyte transcended = _heroInfo.Transcended;
		int awakeHeroItemCount = HeroTrialDungeonManagement.GetAwakeHeroItemCount(_heroInfo.Star, transcended);
		CreatureAwakeChallengeItemData data = GameRoot.Instance.CreatureAwakeChallengeItemDataContainer.GetData(_heroInfo.CreatureData.TagType);
		if (data == null)
		{
			VespaDebug.LogError("awakeChallenge item data is null, CreatureTagType = " + _heroInfo.CreatureData.TagType);
			return;
		}
		for (int i = 1; i <= 3; i++)
		{
			ReqItemUiData reqItemUiData = ItemUiGetter(i);
			string transcendItemCode = data.GetTranscendItemCode(i);
			if (reqItemUiData != null && !String_Extension_Method.IsNullOrEmpty(transcendItemCode))
			{
				AssetTexture itemIcon = UIHelper.GetItemIcon2(transcendItemCode);
				UIHelper.ResetPlaneAssetTextureAsync(this, reqItemUiData.ItemIcon, itemIcon);
				UIHelper.SetLabelText(reqItemUiData.CountLabel, awakeHeroItemCount);
			}
		}
		CreatureStarData nextData = GameRoot.Instance.CreatureStarDataContainer.GetNextData(_heroInfo.Star, _heroInfo.Transcended);
		if (nextData == null)
		{
			VespaDebug.LogError("creatureStar Data is not found, heroStar = " + _heroInfo.Star + ", Transcended = " + _heroInfo.Transcended);
			return;
		}
		ItemData data2 = GameRoot.Instance.ItemDataContainer.GetData(nextData.HeroTranscendItemIndex);
		if (data2 == null)
		{
			VespaDebug.LogError("itemData is not found, itemIndex = " + nextData.HeroTranscendItemIndex);
			return;
		}
		ReqItemUiData reqItemUiData2 = ItemUiGetter(4);
		if (reqItemUiData2 != null)
		{
			AssetTexture itemIcon2 = UIHelper.GetItemIcon2(data2.Code);
			UIHelper.ResetPlaneAssetTextureAsync(this, reqItemUiData2.ItemIcon, itemIcon2);
			UIHelper.SetLabelText(reqItemUiData2.CountLabel, nextData.HeroTranscendItemCount);
		}
	}
}
