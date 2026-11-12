using System;
using System.Collections.Generic;
using LitJson;
using NGame2.N3D;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NRender3D;
using NGameAsset;
using NGameAsset.NCreature;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NWindow;

[Serializable]
public class BaseBoss3dView
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

		bool ILoadJson.Load(JsonData json)
		{
			ChapterIndex = json["ChapterIndex"].GETINT(ChapterIndex);
			DungeonIndex = json["DungeonIndex"].GETINT(DungeonIndex);
			CreatureScale = json["CreatureScale"].GETFLOAT(CreatureScale);
			CreatureDirAngle = json["CreatureDirAngle"].GETFLOAT(CreatureDirAngle);
			CameraHeightAdd = json["CameraHeightAdd"].GETFLOAT(CameraHeightAdd);
			CameraAngleX = json["CameraAngleX"].GETFLOAT(CameraAngleX);
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
			return jsonData;
		}
	}

	[HideInInspector]
	public UILabel NameLabel;

	[HideInInspector]
	public UILabel NamePrefixLabel;

	[HideInInspector]
	public UILabel DescriptionLabel;

	[HideInInspector]
	public UISimpleGrid AttrTypeGrid;

	[HideInInspector]
	public UITexture BossPreviewTexture;

	[HideInInspector]
	public float BossZoomOutRate = 1f;

	[HideInInspector]
	public AdjustCameraData DefaultAdjustCamera = new AdjustCameraData();

	[HideInInspector]
	public List<AdjustCameraData> AdjustCameraList = new List<AdjustCameraData>();

	private int _chapterIndex;

	private int _dungeonIndex;

	private AdjustCameraData _bossAdjustCamera;

	private List<SkillTypeIconComponent> _bossAttrTypeComponentList = new List<SkillTypeIconComponent>();

	private List<SkillTypeIconComponent> _bossWeakTypeComponentList = new List<SkillTypeIconComponent>();

	private HeroPreview _bossPreview;

	public CreatureData BossCreatureData { get; private set; }

	public CampaignWaveData BossWaveData { get; private set; }

	public string BackgroundName { get; private set; }

	public CreatureStock.ELoadLevel LoadLevel { get; private set; }

	public void Open(int chapterIndex, int dungeonIndex, string backgroundName, CreatureStock.ELoadLevel loadLevel)
	{
		CampaignWaveData bossWaveData;
		CreatureData bossCreatureData = GameRoot.Instance.CampaignDungeonDataContainer.GetBossCreatureData(chapterIndex, dungeonIndex, DungeonDifficultyType.Easy, scenario: false, out bossWaveData);
		if (bossCreatureData == null)
		{
			VespaDebug.LogError("Boss creature not found : chapterIndex=" + chapterIndex + " dungeonIndex=" + dungeonIndex);
		}
		else
		{
			Open(bossCreatureData, bossWaveData, backgroundName, loadLevel);
		}
	}

	public void Open(CreatureData bossCreatureData, CampaignWaveData waveData, string backgroundName, CreatureStock.ELoadLevel loadLevel)
	{
		BossCreatureData = bossCreatureData;
		BossWaveData = waveData;
		BackgroundName = backgroundName;
		LoadLevel = loadLevel;
		_bossAdjustCamera = DefaultAdjustCamera;
		if (waveData == null)
		{
			return;
		}
		_chapterIndex = waveData.ChapterIndex;
		_dungeonIndex = waveData.DungeonIndex;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(AdjustCameraList); i++)
		{
			AdjustCameraData adjustCameraData = AdjustCameraList[i];
			if (adjustCameraData.ChapterIndex == _chapterIndex && adjustCameraData.DungeonIndex == _dungeonIndex)
			{
				_bossAdjustCamera = adjustCameraData;
				break;
			}
		}
	}

	public void Init()
	{
		Clear();
		InitBossBasicInfo();
		InitBossAttrInfo();
		InitBossWeakInfo();
		InitBossPreview();
	}

	public void Clear()
	{
		FactoryBase.ReleaseAndClearList(_bossAttrTypeComponentList);
		FactoryBase.ReleaseAndClearList(_bossWeakTypeComponentList);
		CloseBossPreview();
	}

	public void Update()
	{
	}

	private void InitBossBasicInfo()
	{
		string visualInfo = ((BossWaveData != null) ? BossWaveData.BossVisualInfo : null);
		string creatureName = string.Empty;
		string creatureNamePrefix = string.Empty;
		GameRoot.Instance.CreatureVisualDataContainer.GetCreatureName(BossCreatureData, visualInfo, LocalizationManager.GetStringDirect, out creatureName, out creatureNamePrefix);
		UIHelper.SetLabelText(NameLabel, creatureName);
		UIHelper.SetLabelText(NamePrefixLabel, creatureNamePrefix);
		string creatureDesc = string.Empty;
		GameRoot.Instance.CreatureVisualDataContainer.GetCreatureDesc(BossCreatureData, visualInfo, LocalizationManager.GetStringDirect, out creatureDesc);
		UIHelper.SetLabelText(DescriptionLabel, creatureDesc);
	}

	private void InitBossAttrInfo()
	{
		if (!(AttrTypeGrid == null))
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(BossCreatureData.AttrType); i++)
			{
				SkillTypeIconComponent skillTypeIconComponent = UIHelper.AddGridItem(AttrTypeGrid, SkillTypeIconComponent.PrefabAuto);
				skillTypeIconComponent.Open(BossCreatureData.AttrType[i]);
				_bossAttrTypeComponentList.Add(skillTypeIconComponent);
			}
		}
	}

	private void InitBossWeakInfo()
	{
		if (!(AttrTypeGrid == null))
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(BossCreatureData.WeakType); i++)
			{
				SkillTypeIconComponent skillTypeIconComponent = UIHelper.AddGridItem(AttrTypeGrid, SkillTypeIconComponent.PrefabAuto);
				skillTypeIconComponent.Open(BossCreatureData.WeakType[i]);
				_bossWeakTypeComponentList.Add(skillTypeIconComponent);
			}
		}
	}

	public void ActivatePreview(bool isActive)
	{
		if (!isActive)
		{
			CloseBossPreview();
		}
		else
		{
			InitBossPreview();
		}
	}

	private void InitBossPreview()
	{
		if (BossPreviewTexture == null)
		{
			return;
		}
		RenderFacadeBuilder renderFacadeBuilder = new RenderFacadeBuilder(null, TeamType.Left, BossCreatureData.CodeName, 0);
		renderFacadeBuilder.SetPreload(AnimationKeyData.EmptyPreloadSet);
		UIHelper.InitHero3dPreview(ref _bossPreview, BossPreviewTexture, BossCreatureData.Index, 0, renderFacadeBuilder, BackgroundName, perspective: true);
		if (_bossPreview != null)
		{
			_bossPreview.RenderFacade.Action.SetBattleMode(isBattleAction: true, blendMotion: false);
			_bossPreview.DefaultPerspective.ZoomOutRate = BossZoomOutRate;
			if (_bossAdjustCamera != null)
			{
				_bossPreview.DefaultPerspective.ZoomOut_Offset += Vector3.up * _bossAdjustCamera.CameraHeightAdd;
				_bossPreview.DefaultPerspective.LookAngleX = _bossAdjustCamera.CameraAngleX;
				_bossPreview.RenderFacade.SetRuntimeScale("BaseBoss3dView", _bossAdjustCamera.CreatureScale);
				VectorXZ direction = NVespa.Math.Rotate(new VectorXZ(0f, 1f), _bossAdjustCamera.CreatureDirAngle);
				_bossPreview.RenderFacade.Action.SetDirection(direction, blendMotion: false);
			}
		}
	}

	public void SetCustomIdle(DefineCreature.EMotion motionType, string idleName)
	{
		if (!(_bossPreview == null))
		{
			_bossPreview.RenderFacade.Action.SetCustomIdle(new AnimationKeyData(motionType, idleName));
		}
	}

	protected void CloseBossPreview()
	{
		UIHelper.CloseHero3dPreview(ref _bossPreview, BossPreviewTexture);
	}
}
