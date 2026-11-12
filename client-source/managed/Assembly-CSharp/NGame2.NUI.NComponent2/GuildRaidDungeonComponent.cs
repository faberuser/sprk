using NGame2.NContentsDefine;
using NGame2.NGuild;
using NGame2.NUI.NGuildRaid;
using NGameAsset;
using NShared;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class GuildRaidDungeonComponent : UIScrollItemBehaviour, InitOnCreate
{
	public interface Listener
	{
		void OnClickEnter(GuildRaidDungeonInfo info);

		void OnClickViewInfo(GuildRaidDungeonComponent caller);
	}

	public class GuildRaidDungeonCompInfo
	{
		public Listener Listener { get; set; }

		public GuildRaidDungeonInfo GuildRaidDungeonInfo { get; set; }
	}

	public const string PrefabPath = "Prefab2/Component/GuildRaid/GuildRaidDungeonComponent";

	public static PrefabAuto<GuildRaidDungeonComponent> PrefabAuto = new PrefabAuto<GuildRaidDungeonComponent>("Prefab2/Component/GuildRaid/GuildRaidDungeonComponent");

	public UILabel NameLabel;

	public UILabel NamePrefixLabel;

	public UITexture BossImageTexture;

	public UIClipSprite HpGaugeSprite;

	public UILabel RemainHpLabel;

	public UIButton EnterButton;

	public UIButton BossViewInfoButton;

	public Transform OpenedRoot;

	public Transform FinishModeRoot;

	public Transform ClearedRoot;

	public UIGame3DWidget FinishHpEffectWidget;

	public UIGame3DWidget BonusDungeonEffectWidget;

	public UILabel FailWarningLabel;

	public UILabel StepLabel;

	public UISprite StepSprite;

	public Transform StepRootTransform;

	private GuildRaidStepColorInfo _stepColorInfo = new GuildRaidStepColorInfo();

	private GuildRaidDungeonCompInfo _info;

	private GuildRaidChapterData _guildRaidChapterData;

	private GuildRaidDungeonData _guildRaidDungeonData;

	private CreatureData _bossCreatureData;

	private CampaignWaveData _bossWaveData;

	private bool _isOpen;

	private bool _playOpeningEffect;

	private bool _playClearEffect;

	private bool _openedEffectEnabled;

	private bool _isFinishMode;

	public GuildRaidDungeonData GuildRaidDungeonData => _guildRaidDungeonData;

	public CreatureData BossCreatureData => _bossCreatureData;

	private Color StepColor
	{
		get
		{
			GuildRaidDungeonInfo guildRaidDungeonInfo = GuildRaidDungeonInfo;
			if (guildRaidDungeonInfo == null)
			{
				return Color.white;
			}
			if (_stepColorInfo == null)
			{
				return Color.white;
			}
			return _stepColorInfo.GetStepColor(guildRaidDungeonInfo.Step);
		}
	}

	private GuildRaidDungeonInfo GuildRaidDungeonInfo
	{
		get
		{
			if (_info == null)
			{
				return null;
			}
			return _info.GuildRaidDungeonInfo;
		}
	}

	private int GuildRaidChapterIndex
	{
		get
		{
			if (GuildRaidDungeonInfo == null)
			{
				return 0;
			}
			return GuildRaidDungeonInfo.CurrentChapterIndex;
		}
	}

	private int GuildRaidDungeonIndex
	{
		get
		{
			if (GuildRaidDungeonInfo == null)
			{
				return 1;
			}
			return GuildRaidDungeonInfo.CurrentDungeonIndex;
		}
	}

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		_isOpen = false;
		_playOpeningEffect = false;
		_playClearEffect = false;
		_openedEffectEnabled = false;
		_isFinishMode = false;
		UIHelper.ReleaseTextureInfo(BossImageTexture);
	}

	public void Open(GuildRaidDungeonCompInfo info, bool openingEffect, bool clearEffect)
	{
		_info = info;
		_isOpen = true;
		_playOpeningEffect = openingEffect;
		_playClearEffect = clearEffect;
		_openedEffectEnabled = false;
		Init();
		InitBossImage();
		InitDungeonState();
		InitHpGauge();
		SetBonusDungeonEffect();
	}

	private void Init()
	{
		if (_info == null)
		{
			return;
		}
		GuildRaidDungeonInfo guildRaidDungeonInfo = _info.GuildRaidDungeonInfo;
		if (guildRaidDungeonInfo == null)
		{
			return;
		}
		int chapterIndex = guildRaidDungeonInfo.ChapterIndex;
		int dungeonIndex = guildRaidDungeonInfo.DungeonIndex;
		_guildRaidDungeonData = GetGuildRaidDungeonData();
		if (_guildRaidDungeonData != null)
		{
			_bossCreatureData = GameRoot.Instance.CampaignDungeonDataContainer.GetBossCreatureData(chapterIndex, dungeonIndex, _guildRaidDungeonData.Difficulty, scenario: false, out _bossWaveData);
			if (_bossCreatureData != null)
			{
				_isFinishMode = guildRaidDungeonInfo.FinishMode != 0;
				UIHelper.EnableGameObject(FinishModeRoot, _isFinishMode);
				string visualInfo = ((_bossWaveData != null) ? _bossWaveData.BossVisualInfo : null);
				string creatureName = string.Empty;
				string creatureNamePrefix = string.Empty;
				GameRoot.Instance.CreatureVisualDataContainer.GetCreatureName(_bossCreatureData, visualInfo, LocalizationManager.GetStringDirect, out creatureName, out creatureNamePrefix);
				UIHelper.SetLabelText(NameLabel, creatureName);
				UIHelper.SetLabelText(NamePrefixLabel, creatureNamePrefix);
				SetStepLabel();
			}
		}
	}

	private void InitBossImage()
	{
		if (_guildRaidDungeonData != null)
		{
			AssetTexture plainTexture = UIHelper.GetPlainTexture(_guildRaidDungeonData.PortraitPath);
			UIHelper.ResetPlaneAssetTextureAsync(this, BossImageTexture, plainTexture);
		}
	}

	private void InitDungeonState()
	{
		SetOpenCloseDungeon(isOpen: true);
		if (_info == null)
		{
			return;
		}
		GuildRaidDungeonInfo guildRaidDungeonInfo = _info.GuildRaidDungeonInfo;
		if (guildRaidDungeonInfo != null)
		{
			UIHelper.EnableButton(BossViewInfoButton, !guildRaidDungeonInfo.CheckDummyDungeon);
			if (GameRoot.Instance.GuildRaidDungeonDataContainer.GetChapterLastStep(guildRaidDungeonInfo.ChapterIndex) > 0 && guildRaidDungeonInfo.IsDead())
			{
				SetOpenCloseDungeon(isOpen: false);
				UIHelper.EnableGameObject(HpGaugeSprite, enable: false);
				UIHelper.EnableButton(EnterButton, enable: false);
			}
		}
	}

	private void SetOpenCloseDungeon(bool isOpen)
	{
		UIHelper.EnableGameObject(OpenedRoot, isOpen);
		UIHelper.EnableGameObject(HpGaugeSprite, isOpen);
		UIHelper.EnableButton(EnterButton, isOpen);
		UIHelper.EnableGameObject(ClearedRoot, !isOpen);
		if (BossImageTexture != null)
		{
			BossImageTexture.isGrayscale = !isOpen;
		}
	}

	private void InitHpGauge()
	{
		if (_info != null && _guildRaidDungeonData != null)
		{
			int guildRaidChapterIndex = GuildRaidChapterIndex;
			int guildRaidDungeonIndex = GuildRaidDungeonIndex;
			long totalMaxHp = 0L;
			long totalRemainHp = 0L;
			long bossMaxHp = 0L;
			long bossRemainHp = 0L;
			GuildManager.GetGuildRaidHpInfo(UIHelper.GetMonsterInfoFromDungeon(guildRaidChapterIndex, guildRaidDungeonIndex, _guildRaidDungeonData.Difficulty, isScenario: false, showDuplicated: true), _info.GuildRaidDungeonInfo, out totalMaxHp, out totalRemainHp, out bossMaxHp, out bossRemainHp);
			float num = Mathf.Clamp01((float)bossRemainHp / (float)bossMaxHp);
			UIHelper.SetSpriteFillAmount(HpGaugeSprite, num);
			UIHelper.SetLabelText(RemainHpLabel, Vespa_Long_Extension_Method.ToCommaString(totalRemainHp));
			if (_isFinishMode && (bool)FinishHpEffectWidget)
			{
				Vector3 a = (HpGaugeSprite.worldCorners[0] + HpGaugeSprite.worldCorners[1]) / 2f;
				Vector3 b = (HpGaugeSprite.worldCorners[2] + HpGaugeSprite.worldCorners[3]) / 2f;
				FinishHpEffectWidget.transform.position = Vector3.Lerp(a, b, num);
				FinishHpEffectWidget.Play(null);
			}
			SetFinishblowLabel();
		}
	}

	private void SetFinishblowLabel()
	{
		if (_guildRaidDungeonData != null)
		{
			UIHelper.SetLabelText(FailWarningLabel, LocalizationManager.GetStringDirect("GUILD_RAID_FINISH_BLOW_INFO", (float)_guildRaidDungeonData.FinishModeRatio / 10f));
		}
	}

	public void SetComplete()
	{
		UIHelper.SetWidgetGrayScale(BossImageTexture, grayScale: true);
		UIHelper.EnableGameObject(HpGaugeSprite, enable: false);
		UIHelper.EnableButton(EnterButton, enable: false);
		UIHelper.EnableGameObject(ClearedRoot, enable: true);
	}

	private void SetBonusDungeonEffect()
	{
		if (!(BonusDungeonEffectWidget == null))
		{
			BonusDungeonEffectWidget.Stop(immediatelyClear: true);
			GuildRaidDungeonInfo guildRaidDungeonInfo = GuildRaidDungeonInfo;
			if (guildRaidDungeonInfo != null && guildRaidDungeonInfo.CheckBonusDungeon)
			{
				BonusDungeonEffectWidget.Play(null);
			}
		}
	}

	private void SetStepLabel()
	{
		UIHelper.EnableGameObject(StepRootTransform, enable: false);
		GuildRaidDungeonInfo guildRaidDungeonInfo = GuildRaidDungeonInfo;
		if (guildRaidDungeonInfo != null && guildRaidDungeonInfo.Step >= 0 && !guildRaidDungeonInfo.CheckDummyDungeon)
		{
			string text = LocalizationManager.GetStringDirect("GUILD_RAID_STEP_STAGES", guildRaidDungeonInfo.Step);
			int chapterLastStep = GameRoot.Instance.GuildRaidDungeonDataContainer.GetChapterLastStep(guildRaidDungeonInfo.ChapterIndex);
			if (guildRaidDungeonInfo.Step >= chapterLastStep)
			{
				text = "Final";
			}
			if (guildRaidDungeonInfo.CheckBonusDungeon)
			{
				text = "Bonus";
			}
			UIHelper.EnableGameObject(StepRootTransform, enable: true);
			Color stepColor = StepColor;
			UIHelper.SetLabelText(StepLabel, text);
			UIHelper.SetLabelColor(StepLabel, stepColor);
			if (StepSprite != null)
			{
				StepSprite.color = stepColor;
			}
		}
	}

	private GuildRaidDungeonData GetGuildRaidDungeonData()
	{
		GuildRaidDungeonInfo guildRaidDungeonInfo = GuildRaidDungeonInfo;
		if (guildRaidDungeonInfo == null)
		{
			return null;
		}
		if (guildRaidDungeonInfo.CheckBonusDungeon)
		{
			GuildRaidChapterData data = GameRoot.Instance.GuildRaidChapterDataContainer.GetData(guildRaidDungeonInfo.ChapterIndex);
			if (data == null)
			{
				return null;
			}
			return GameRoot.Instance.GuildRaidBonusDungeonDataContainer.GetData(new GuildRaidBonusDungeonDataKey(guildRaidDungeonInfo.ChapterIndex, guildRaidDungeonInfo.DungeonIndex, data.BonusGroup));
		}
		if (guildRaidDungeonInfo.CheckDummyDungeon)
		{
			return GameRoot.Instance.GuildRaidDummyDungeonDataContainer.GetData(new ChapterDungeonKey2(guildRaidDungeonInfo.ChapterIndex, guildRaidDungeonInfo.DungeonIndex));
		}
		return GameRoot.Instance.GuildRaidDungeonDataContainer.GetData(new GuildRaidDungeonDataKey(guildRaidDungeonInfo.ChapterIndex, guildRaidDungeonInfo.DungeonIndex, guildRaidDungeonInfo.Step));
	}

	public void OnClickEnter()
	{
		if (_info != null && _info.Listener != null)
		{
			CurrencyMaxCap.WarningOrExecute(CurrencyType.GuildPoint, delegate
			{
				_info.Listener.OnClickEnter(GuildRaidDungeonInfo);
			});
		}
	}

	public void OnClickViewInfo()
	{
		if (_info != null && _info.Listener != null)
		{
			_info.Listener.OnClickViewInfo(this);
		}
	}
}
