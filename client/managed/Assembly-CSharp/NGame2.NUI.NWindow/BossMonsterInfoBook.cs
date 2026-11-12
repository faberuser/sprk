using System;
using System.Collections.Generic;
using LitJson;
using NGame2.N3D;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NRender3D;
using NGameAsset;
using NGameAsset.NCreature;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class BossMonsterInfoBook : BaseWindow
{
	[Serializable]
	public class AdjustCameraData : IBuilder, ILoadJson, IJsonInterface, IMakeJson
	{
		public int ChapterIndex;

		public int DungeonIndex;

		public float CreatureScale = 1f;

		public float CreatureDirAngle;

		public float CameraHeightAdd;

		public float CameraAngleX = 5f;

		public float CameraAngleY = 5f;

		bool ILoadJson.Load(JsonData json)
		{
			ChapterIndex = json["ChapterIndex"].GETINT(ChapterIndex);
			DungeonIndex = json["DungeonIndex"].GETINT(DungeonIndex);
			CreatureScale = json["CreatureScale"].GETFLOAT(CreatureScale);
			CreatureDirAngle = json["CreatureDirAngle"].GETFLOAT(CreatureDirAngle);
			CameraHeightAdd = json["CameraHeightAdd"].GETFLOAT(CameraHeightAdd);
			CameraAngleX = json["CameraAngleX"].GETFLOAT(CameraAngleX);
			CameraAngleY = json["CameraAngleY"].GETFLOAT(CameraAngleY);
			return true;
		}

		JsonData IMakeJson.MakeJson()
		{
			JsonData jsonData = new JsonData();
			jsonData.AddFieldLong("ChapterIndex", ChapterIndex);
			jsonData.AddFieldLong("DungeonIndex", DungeonIndex);
			jsonData.AddFieldDouble("CreatureScale", CreatureScale);
			jsonData.AddFieldDouble("CreatureDirAngle", CreatureDirAngle);
			jsonData.AddFieldDouble("CameraHeightAdd", CameraHeightAdd);
			jsonData.AddFieldDouble("CameraAngleX", CameraAngleX);
			jsonData.AddFieldDouble("CameraAngleY", CameraAngleY);
			return jsonData;
		}
	}

	public interface Listener
	{
		void OnPrevMonster();

		void OnNextMonster();

		void OnToggle(EToggle toggle);
	}

	public enum EToggle
	{
		None,
		Profile,
		Skill,
		Stat
	}

	[Serializable]
	public class TabInfo
	{
		public EToggle EToggleType;

		public UIToggle Toggle;

		public Transform DisableTransform;
	}

	public struct BossMonsterInfo
	{
		public Listener IListener;

		public ArchiveType ArchiveType;

		public MonsterHeroInfo MonsterHeroInfo;

		public string PreviewBg;
	}

	[HideInInspector]
	public AdjustCameraData DefaultAdjustCamera = new AdjustCameraData();

	public const string PrefabPath = "Resources/Prefab2/Window/IllustratedBook/BossMonsterInfoBook";

	public UILabel LabelMonsterName;

	public UILabel LabelNamePrefix;

	public UILabel LabelProfileName;

	public UILabel LabelProfileNamePrefix;

	public UIWidget PortraitWidget;

	public UIBundleLoader PortraitBundleTex;

	public UITexture MonsterPreviewTexture;

	public List<TabInfo> TabInfoList = new List<TabInfo>();

	public UIWidget WidgetProfile;

	public UIWidget WidgetSkill;

	public UIWidget WidgetStat;

	public UILabel Label_ArchiveStory;

	public UISimpleGrid AttrTypeIconGrid;

	public UILabel Label_Type;

	public UILabel Label_Hp;

	public UILabel Label_Attack;

	public UILabel Label_PDefense;

	public UILabel Label_MDefense;

	public UIScrollView Scroll_ArchiveStory;

	public RaidInfoSkillList SkillList;

	public Transform DisableSkillTabTransform;

	public Transform DisableStatTabTransform;

	[SerializeField]
	private UILabel LbMonsterTribe;

	private EToggle _toggle = EToggle.Profile;

	private Listener _listener;

	private MonsterHeroInfo _monsterInfo;

	private HeroPreview _monsterPreview;

	private CreatureData _monsterData;

	private ArchiveType _curArchiveType;

	private AdjustCameraData _bossAdjustCamera;

	private BossMonsterInfo? _bossMonsterInfo;

	private List<RaidSkillComponent> _skillComponentList = new List<RaidSkillComponent>();

	private List<SkillTypeIconComponent> _ListSkillTypeIcons = new List<SkillTypeIconComponent>();

	public MonsterHeroInfo MonsterInfo => _monsterInfo;

	private bool EnableSkillToggle
	{
		get
		{
			if (_curArchiveType == ArchiveType.GuildRaid)
			{
				if (_monsterData == null)
				{
					return false;
				}
				List<int> skillIndexList = _monsterData.GetSkillIndexList(includeExtra: true);
				if (__Extension_Method_ICollection__.IsNullOrEmpty(skillIndexList))
				{
					return false;
				}
				for (int i = 0; i < skillIndexList.Count; i++)
				{
					if (skillIndexList[i] > 0)
					{
						return true;
					}
				}
				return false;
			}
			if (_curArchiveType != ArchiveType.Conquest && _curArchiveType != ArchiveType.ChapterBoss)
			{
				return _curArchiveType != ArchiveType.Eclipse;
			}
			return false;
		}
	}

	private bool EnableStatToggle
	{
		get
		{
			if (_curArchiveType != ArchiveType.Conquest && _curArchiveType != ArchiveType.ChapterBoss)
			{
				return _curArchiveType == ArchiveType.Eclipse;
			}
			return true;
		}
	}

	private string CirclePortraitPath
	{
		get
		{
			if (_monsterData == null)
			{
				return string.Empty;
			}
			if (_monsterInfo == null || _monsterInfo.HeroInfo == null)
			{
				return string.Empty;
			}
			sbyte star = _monsterInfo.HeroInfo.Star;
			string text = _monsterData.CodeName;
			if (_curArchiveType == ArchiveType.GuildRaid)
			{
				star = 0;
				text = GetGuildRaidEtcCirclePortraitPath("GuildRaidRefinementETC_1");
				if (String_Extension_Method.IsNullOrEmpty(text))
				{
					text = GetGuildRaidEtcCirclePortraitPath("GuildRaidRefinementETC_2");
				}
				if (String_Extension_Method.IsNullOrEmpty(text))
				{
					star = _monsterInfo.HeroInfo.Star;
					text = _monsterData.CodeName;
				}
			}
			return UIHelper.GetMonsterCirclePortraitPath(text, star);
		}
	}

	public override void Reset()
	{
		base.Reset();
		Clear();
		UIHelper.CloseHero3dPreview(ref _monsterPreview, MonsterPreviewTexture);
	}

	private void Clear()
	{
		ClearProfile();
		ClearSkill();
		ClearStat();
	}

	private void ClearProfile()
	{
	}

	private void ClearSkill()
	{
		FactoryBase.ReleaseAndClearList(_skillComponentList);
	}

	private void ClearStat()
	{
		FactoryBase.ReleaseAndClearList(_ListSkillTypeIcons);
	}

	public string GetBackgroundImage(ArchiveType ArchiveType)
	{
		string result = "Normal";
		switch (ArchiveType)
		{
		case ArchiveType.Raid:
		case ArchiveType.ChallengeRaid:
		{
			IList<RaidData> allData = GameRoot.Instance.RaidDataContainer.GetAllData();
			if (allData == null)
			{
				break;
			}
			foreach (RaidData item in allData)
			{
				CampaignWaveData data2 = GameRoot.Instance.CampaignWaveDataContainer.GetData(item.ChapterIndex, item.DungeonIndex, DungeonDifficultyType.Easy, scenario: false, 1);
				if (data2 == null)
				{
					VespaDebug.LogError("wave data == nullchapterIndex : " + item.ChapterIndex + "dungeonIndex : " + item.DungeonIndex);
				}
				else if (data2.BossCreatureIndex == _monsterData.Index)
				{
					result = item.PreviewBg;
					break;
				}
			}
			break;
		}
		case ArchiveType.GuildRaid:
			result = _bossMonsterInfo.Value.PreviewBg;
			break;
		case ArchiveType.GuildSuppress:
		{
			ICollection<int> raidIndices = GameRoot.Instance.RaidDataContainer.GetRaidIndices(RaidType.GuildSuppress);
			if (raidIndices == null)
			{
				break;
			}
			foreach (int item2 in raidIndices)
			{
				foreach (int raidLevel in GameRoot.Instance.RaidDataContainer.GetRaidLevels(item2))
				{
					RaidData data3 = GameRoot.Instance.RaidDataContainer.GetData(item2, raidLevel);
					if (data3 != null && GameRoot.Instance.CampaignWaveDataContainer.GetData(data3.ChapterIndex, data3.DungeonIndex, DungeonDifficultyType.Easy, scenario: false, 1).BossCreatureIndex == _monsterData.Index)
					{
						result = data3.PreviewBg;
						break;
					}
				}
			}
			break;
		}
		case ArchiveType.WorldBoss:
		{
			IList<WorldBossData> allData2 = GameRoot.Instance.WorldBossDataContainer.GetAllData();
			if (allData2 == null)
			{
				break;
			}
			foreach (WorldBossData item3 in allData2)
			{
				if (GameRoot.Instance.CampaignWaveDataContainer.GetData(item3.ChapterIndex, item3.DungeonIndex, DungeonDifficultyType.Easy, scenario: false, 1).BossCreatureIndex == _monsterData.Index)
				{
					result = item3.PreviewBg;
					break;
				}
			}
			break;
		}
		case ArchiveType.ChapterBoss:
		case ArchiveType.Conquest:
		case ArchiveType.GodkingTrial:
		case ArchiveType.Eclipse:
		case ArchiveType.ShakmehDungeon:
		case ArchiveType.TechnoMagic:
		case ArchiveType.PunishmentRaid:
		{
			ClientCreatureData data = GameRoot.Instance.ClientCreatureDataContainer.GetData(_monsterData.CodeName);
			if (data != null)
			{
				result = data.ArchivePreviewBg;
			}
			break;
		}
		}
		return result;
	}

	public void Init(BossMonsterInfo? info)
	{
		if (info.HasValue)
		{
			_bossMonsterInfo = info;
			_curArchiveType = info.Value.ArchiveType;
			_listener = info.Value.IListener;
			_monsterInfo = info.Value.MonsterHeroInfo;
			if (_monsterInfo != null && _monsterInfo.HeroInfo != null)
			{
				_monsterData = GameRoot.Instance.CreatureDataContainer.GetData(_monsterInfo.HeroInfo.HeroIndex);
			}
			_toggle = EToggle.Profile;
			_bossAdjustCamera = DefaultAdjustCamera;
			RefreshInfo();
		}
	}

	public void RefreshInfo()
	{
		Clear();
		InitToggle();
		InitMonsterName();
		SetupMonster3D();
		SetMonsterInfo();
	}

	private void InitToggle()
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(TabInfoList))
		{
			return;
		}
		EToggle eToggle = EToggle.Profile;
		_ = EnableSkillToggle;
		TabInfo tabInfo = null;
		for (int i = 0; i < TabInfoList.Count; i++)
		{
			tabInfo = TabInfoList[i];
			if (tabInfo != null && !(tabInfo.Toggle == null))
			{
				tabInfo.Toggle.Set(eToggle == tabInfo.EToggleType);
				SetEnableToggle(tabInfo);
			}
		}
	}

	private void SetEnableToggle(TabInfo tabInfo)
	{
		if (tabInfo != null)
		{
			bool flag = true;
			if (tabInfo.EToggleType == EToggle.Skill)
			{
				flag = EnableSkillToggle;
			}
			else if (tabInfo.EToggleType == EToggle.Stat)
			{
				flag = EnableStatToggle;
			}
			UIHelper.EnableToggle(tabInfo.Toggle, flag);
			UIHelper.EnableGameObject(tabInfo.Toggle, flag);
			UIHelper.EnableGameObject(tabInfo.DisableTransform, !flag);
		}
	}

	public void InitMonsterName()
	{
		string creatureName = string.Empty;
		string creatureNamePrefix = string.Empty;
		GameRoot.Instance.CreatureVisualDataContainer.GetCreatureName(_monsterData, _monsterInfo.VisualInfo, LocalizationManager.GetStringDirect, out creatureName, out creatureNamePrefix);
		UIHelper.SetLabelText(LabelMonsterName, creatureName);
		UIHelper.SetLabelText(LabelNamePrefix, creatureNamePrefix);
		UIHelper.SetBundleTexture(PortraitBundleTex, CirclePortraitPath);
		bool flag = GlobalAutoBehaviour<AccountManager>.instance.ArchiveManager.isClearMonster(_curArchiveType, _monsterInfo.HeroInfo.HeroIndex);
		if (PortraitWidget.isGrayscale == flag)
		{
			UIHelper.SetWidgetGrayScale(PortraitWidget, !flag);
			UIHelper.EnableGameObject(PortraitWidget, enable: false);
			UIHelper.EnableGameObject(PortraitWidget, enable: true);
		}
	}

	public void SetupMonster3D()
	{
		if (MonsterPreviewTexture == null)
		{
			return;
		}
		RenderFacadeBuilder renderFacadeBuilder = null;
		if (_monsterInfo != null)
		{
			renderFacadeBuilder = new RenderFacadeBuilder(null, TeamType.Right, _monsterInfo);
			renderFacadeBuilder.SetPreload(AnimationKeyData.EmptyPreloadSet);
		}
		string backgroundImage = GetBackgroundImage(_curArchiveType);
		UIHelper.InitArchive3dPreview(ref _monsterPreview, MonsterPreviewTexture, (_monsterInfo != null) ? _monsterInfo.HeroInfo.HeroIndex : 0, 0, renderFacadeBuilder, backgroundImage);
		if (!(_monsterPreview != null))
		{
			return;
		}
		_monsterPreview.RenderFacade.Action.SetBattleMode(isBattleAction: true, blendMotion: false);
		_monsterPreview.DefaultPerspective.ZoomOutRate = 1f;
		if (_bossAdjustCamera != null)
		{
			_monsterPreview.DefaultPerspective.ZoomOut_Offset += Vector3.up * _bossAdjustCamera.CameraHeightAdd;
			float lookAngleX = _bossAdjustCamera.CameraAngleX;
			float lookAngleY = _bossAdjustCamera.CameraAngleY;
			ClientCreatureData data = GameRoot.Instance.ClientCreatureDataContainer.GetData(_monsterData.CodeName);
			if (data != null)
			{
				if (data.ArchiveCameraAngleX != 0f)
				{
					lookAngleX = data.ArchiveCameraAngleX;
				}
				if (data.ArchiveCameraAngleY != 0f)
				{
					lookAngleY = data.ArchiveCameraAngleY;
				}
			}
			_monsterPreview.DefaultPerspective.LookAngleX = lookAngleX;
			_monsterPreview.DefaultPerspective.LookAngleY = lookAngleY;
			_monsterPreview.RenderFacade.SetRuntimeScale("BossMonsterInfoBook", _bossAdjustCamera.CreatureScale);
			VectorXZ direction = NVespa.Math.Rotate(new VectorXZ(0f, 1f), _bossAdjustCamera.CreatureDirAngle);
			_monsterPreview.RenderFacade.Action.SetDirection(direction, blendMotion: false);
		}
		if (!String_Extension_Method.IsNullOrEmpty(_monsterInfo.BookCustomIdle))
		{
			_monsterPreview.RenderFacade.Action.SetCustomIdle(new AnimationKeyData(DefineCreature.EMotion.NamedAction, _monsterInfo.BookCustomIdle));
		}
	}

	public void SetMonsterInfo()
	{
		UIHelper.EnableGameObject(WidgetProfile, _toggle == EToggle.Profile);
		UIHelper.EnableGameObject(WidgetSkill, _toggle == EToggle.Skill);
		UIHelper.EnableGameObject(WidgetStat, _toggle == EToggle.Stat);
		switch (_toggle)
		{
		case EToggle.Profile:
			SetProfile();
			break;
		case EToggle.Skill:
			SetSkill();
			break;
		case EToggle.Stat:
			SetStat();
			break;
		}
	}

	private void SetProfile()
	{
		ClearProfile();
		if (Scroll_ArchiveStory != null)
		{
			Scroll_ArchiveStory.ResetPosition();
		}
		string creatureName = string.Empty;
		string creatureNamePrefix = string.Empty;
		string stringDirect = LocalizationManager.GetStringDirect(_monsterData.ArchiveStory);
		GameRoot.Instance.CreatureVisualDataContainer.GetCreatureName(_monsterData, _monsterInfo.VisualInfo, LocalizationManager.GetStringDirect, out creatureName, out creatureNamePrefix);
		UIHelper.SetLabelText(LabelProfileName, creatureName);
		UIHelper.SetLabelText(LabelProfileNamePrefix, creatureNamePrefix);
		UIHelper.SetLabelText(Label_ArchiveStory, stringDirect);
	}

	private void SetSkill()
	{
		if (!(SkillList == null))
		{
			SkillList.Open(_monsterInfo, null);
		}
	}

	private void SetStat()
	{
		ClearStat();
		SkillAttrType skillAttrType = ((_monsterData != null && _monsterData.AttrType != null) ? _monsterData.AttrType[0] : SkillAttrType.None);
		if (skillAttrType != SkillAttrType.None)
		{
			SkillTypeIconComponent skillTypeIconComponent = UIHelper.AddGridItem(AttrTypeIconGrid, SkillTypeIconComponent.PrefabAuto);
			skillTypeIconComponent.Open(_monsterData.AttrType[0]);
			_ListSkillTypeIcons.Add(skillTypeIconComponent);
		}
		UIHelper.RepositionGrid(AttrTypeIconGrid);
		UIHelper.SetLabelText(Label_Type, LocalizationManager.GetString(_monsterData.AttrType[0]));
		if (LbMonsterTribe != null)
		{
			UIHelper.SetLabelText(LbMonsterTribe, _monsterData.Tribe);
		}
		StatData.GetHeroMainStats(skillAttrType);
		StatData heroStat = SceneAutoBehaviour<StatDataCache>.instance.GetHeroStat(_monsterInfo.HeroInfo);
		MonsterTierData data = GameRoot.Instance.MonsterTierDataContainer.GetData(_monsterInfo.Tier);
		switch (skillAttrType)
		{
		case SkillAttrType.Physical:
			UIHelper.SetLabelText(Label_Attack, heroStat.PhysicalAttack * data.PhysicalAttack / 1000);
			break;
		case SkillAttrType.Magical:
			UIHelper.SetLabelText(Label_Attack, heroStat.MagicalAttack * data.MagicalAttack / 1000);
			break;
		}
		UIHelper.SetLabelText(Label_Hp, heroStat.MaxHp * data.MaxHp / 1000);
		UIHelper.SetLabelText(Label_PDefense, heroStat.PhysicalDefense * data.PhysicalDefense / 1000);
		UIHelper.SetLabelText(Label_MDefense, heroStat.MagicalDefense * data.MagicalDefense / 1000);
	}

	public void SelectMonster(MonsterHeroInfo monsterInfo)
	{
		_monsterInfo = monsterInfo;
		_monsterData = GameRoot.Instance.CreatureDataContainer.GetData(monsterInfo.HeroInfo.HeroIndex);
		InitMonsterName();
	}

	private string GetGuildRaidEtcCirclePortraitPath(string key)
	{
		if (_curArchiveType != ArchiveType.GuildRaid)
		{
			return string.Empty;
		}
		string[] valueArray = GameRoot.Instance.ConstantDataContainer.GetValueArray<string>(key);
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(valueArray) && valueArray.Length == 2)
		{
			if (!int.TryParse(valueArray[0], out var result))
			{
				return string.Empty;
			}
			if (_monsterInfo == null || _monsterInfo.HeroInfo == null)
			{
				return string.Empty;
			}
			if (result == _monsterInfo.HeroInfo.HeroIndex)
			{
				return valueArray[1];
			}
		}
		return string.Empty;
	}

	public void OnClickPrevMonster()
	{
		_listener.OnPrevMonster();
	}

	public void OnClickNextMonster()
	{
		_listener.OnNextMonster();
	}

	public void OnToggleChanged(UIToggle toggle)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(TabInfoList))
		{
			return;
		}
		TabInfo tabInfo = null;
		for (int i = 0; i < TabInfoList.Count; i++)
		{
			tabInfo = TabInfoList[i];
			if (tabInfo != null && tabInfo.Toggle.value)
			{
				if (_toggle == tabInfo.EToggleType)
				{
					return;
				}
				_toggle = tabInfo.EToggleType;
				break;
			}
		}
		SetMonsterInfo();
		SingletonUpdater<GCManager>.instance.IPhoneLowMemCollectLite();
	}
}
