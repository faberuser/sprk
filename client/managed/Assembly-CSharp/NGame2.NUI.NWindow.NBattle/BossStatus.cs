using System;
using System.Collections.Generic;
using NGame2.NBattleField;
using NGame2.NBattleView;
using NGame2.NUI.NComponent2.NBattle.NSub;
using NGameAsset;
using NNguiExtension;
using NShared;
using NVespa;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NWindow.NBattle;

public class BossStatus : BaseWindow
{
	[Serializable]
	public class PhaseInfo
	{
		public int Phase;

		public Transform Root;
	}

	[Serializable]
	public class GradeInfo
	{
		public bool ShowShard;

		public MonsterGradeType Grade;

		public Transform Root;

		public UITexture IconTexture;

		public string CustomAssetPostfix;

		public PhaseInfo[] PhaseInfos;

		public UISprite HpGauge;

		public UILabel HpLabel;

		public UIWidget MpGridRoot;

		public UISprite MpGauge;

		public UISprite MpSplitter;

		public UILabel MpLabel;

		public Transform BottomDummy;

		public Transform BuffStatusPoint01;

		public Transform BuffStatusPoint02;

		public Transform BuffStatusPoint03;

		private List<UISprite> _listMpSplitter = new List<UISprite>();

		public void CreateMpSplitter(int mpCount)
		{
			ClearMpSplitterList();
			if (!(MpGridRoot == null))
			{
				float num = MpGridRoot.localSize.x / (float)mpCount;
				for (int i = 1; i < mpCount; i++)
				{
					UISprite uISprite = UnityEngine.Object.Instantiate(MpSplitter);
					uISprite.name = "MpSplitter_" + i;
					Vespa_Unity_Transform_Extension_Method.ParentResetTM(uISprite.transform, MpGridRoot.transform);
					uISprite.transform.localPosition = new Vector3(i * (int)num, 0f, 0f);
					UIHelper.EnableGameObject(uISprite, enable: true);
					_listMpSplitter.Add(uISprite);
				}
			}
		}

		public void ClearMpSplitterList()
		{
			foreach (UISprite item in _listMpSplitter)
			{
				VespaObject.DestroyGoAndDetachFromParent(item.gameObject);
			}
			_listMpSplitter.Clear();
		}
	}

	[Serializable]
	public class ManaGradeInfo
	{
		public GradeInfo GradeInfo;

		public UISpriteSelector FrameSelector;

		public UISpriteSelector HpGaugeSelector;

		public UISpriteSelector MpGaugeSelector;
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Battle/BossStatus";

	public GradeInfo[] GradeInfos;

	public ManaGradeInfo ManaGardeInfo;

	public UISimpleGrid CastingGrid;

	public Transform PatternTextAlarm;

	public Transform PatternImageAlarm;

	public UILabel PatternAlarmLabel;

	private bool _showShard;

	private Creature _boss;

	private CastingComponent _castingComponent;

	private GradeInfo _currentGradeInfo;

	protected BattleDefineData _battleDefineData;

	private static long _cachedMaxHpBeforeDead;

	private static bool _betterBossHpUI;

	private const int MpGaugeUnit = 1000;

	public void Open(Creature bossCreature, BattleDefineData battleDefineData, bool showShard = false)
	{
		Open(delegate
		{
			Init(bossCreature, battleDefineData, showShard);
		}, null);
	}

	public void Refresh()
	{
		_boss = null;
		_cachedMaxHpBeforeDead = 0L;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(GradeInfos); i++)
		{
			UIHelper.EnableGameObject(GradeInfos[i].Root, enable: false);
			UIHelper.ClearPlaneAssetTexture(GradeInfos[i].IconTexture);
		}
		UIHelper.EnableGameObject(ManaGardeInfo.GradeInfo.Root, enable: false);
		UIHelper.ClearPlaneAssetTexture(ManaGardeInfo.GradeInfo.IconTexture);
		Close();
	}

	public void Init(Creature bossCreature, BattleDefineData battleDefineData, bool showShard)
	{
		_showShard = showShard;
		_boss = bossCreature;
		_battleDefineData = battleDefineData;
		_cachedMaxHpBeforeDead = 0L;
		_betterBossHpUI = battleDefineData.BetterBossHpUI;
		SetupRoot();
		_castingComponent = UIHelper.AddGridItem(CastingGrid, CastingComponent.PrefabAuto);
	}

	public bool IsBossValid()
	{
		return _boss != null;
	}

	public MonsterGradeType GetCurrentBossType()
	{
		if (!IsBossValid())
		{
			return MonsterGradeType.Normal;
		}
		return _boss.CreateInfo.Grade;
	}

	public override void Reset()
	{
		base.Reset();
		if (_castingComponent != null)
		{
			FactoryBase.ReleaseComponent(_castingComponent);
			_castingComponent = null;
		}
		if (_currentGradeInfo != null)
		{
			_currentGradeInfo.ClearMpSplitterList();
			_currentGradeInfo = null;
		}
	}

	public override void Close()
	{
		base.Close();
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		UpdateHpGauge(_boss, _currentGradeInfo, _battleDefineData.HPPercent);
		if (_battleDefineData.ShowBossManaGauge)
		{
			UpdateMpGauge(_boss, _currentGradeInfo);
		}
		UpdateCasting();
	}

	public static void UpdateHpGauge(Creature boss, GradeInfo gradeInfo, bool isPercent)
	{
		if (boss == null || gradeInfo == null)
		{
			return;
		}
		BattleFacade.CreatureCacheData creatureCache = boss.CreatureCache;
		UIHelper.SetSpriteFillAmount(gradeInfo.HpGauge, (float)creatureCache.HpRatioM * 0.001f);
		if (isPercent)
		{
			int num = Mathf.CeilToInt((float)creatureCache.Hp / (float)creatureCache.MaxHp * 100f);
			UIHelper.SetLabelText(gradeInfo.HpLabel, $"{num}%");
		}
		else if (_betterBossHpUI)
		{
			if (!creatureCache.IsDead)
			{
				_cachedMaxHpBeforeDead = creatureCache.MaxHp;
			}
			UIHelper.SetLabelText(gradeInfo.HpLabel, $"{Vespa_Long_Extension_Method.WithDamageUnit(creatureCache.Hp)} / {Vespa_Long_Extension_Method.WithDamageUnit(_cachedMaxHpBeforeDead)}");
		}
		else
		{
			UIHelper.SetLabelText(gradeInfo.HpLabel, $"{Vespa_Long_Extension_Method.WithDamageUnit(creatureCache.Hp)} / {Vespa_Long_Extension_Method.WithDamageUnit(creatureCache.MaxHp)}");
		}
	}

	public static void UpdateMpGauge(Creature boss, GradeInfo gradeInfo)
	{
		if (!(boss == null) && gradeInfo != null)
		{
			int num = ((!boss.IsDead) ? boss.CreatureCache.Mp : 0);
			long maxMp = boss.CreatureCache.MaxMp;
			UIHelper.SetSpriteFillAmount(gradeInfo.MpGauge, (float)num / (float)maxMp);
		}
	}

	private void UpdateCasting()
	{
		if (!(_boss == null))
		{
			_castingComponent.ChainUpdate(_boss.CastingValue);
		}
	}

	protected virtual void SetupRoot()
	{
		MonsterGradeType grade = _boss.CreateInfo.Grade;
		GradeInfo gradeInfo = GetGradeInfo(grade, _battleDefineData.ShowBossManaGauge, _battleDefineData.Code, _showShard);
		if (gradeInfo != null)
		{
			_currentGradeInfo = gradeInfo;
			UIHelper.EnableGameObject(gradeInfo.Root, enable: true);
			SetupIcon(this, _boss, _currentGradeInfo);
			SetupPhase(_currentGradeInfo);
			SetupMpGauge(_boss, _currentGradeInfo, _battleDefineData.ShowBossManaGauge);
			SetupHpLabel(_currentGradeInfo);
		}
	}

	private GradeInfo GetGradeInfo(MonsterGradeType monsterGradeType, bool showBossManaGauge, string battleDefineCode, bool showShard)
	{
		GradeInfo result = null;
		if (showBossManaGauge)
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(GradeInfos); i++)
			{
				GradeInfo gradeInfo = GradeInfos[i];
				if (gradeInfo != null)
				{
					UIHelper.EnableGameObject(gradeInfo.Root, enable: false);
				}
			}
			UIHelper.EnableGameObject(ManaGardeInfo.GradeInfo.Root, enable: true);
			if (ManaGardeInfo.FrameSelector != null && !ManaGardeInfo.FrameSelector.Select(battleDefineCode))
			{
				ManaGardeInfo.FrameSelector.SelectByIndex0();
			}
			if (ManaGardeInfo.HpGaugeSelector != null)
			{
				ManaGardeInfo.HpGaugeSelector.Select(battleDefineCode);
			}
			if (ManaGardeInfo.MpGaugeSelector != null)
			{
				ManaGardeInfo.MpGaugeSelector.Select(battleDefineCode);
			}
			result = ManaGardeInfo.GradeInfo;
		}
		else
		{
			for (int j = 0; j < __Extension_Method_ICollection__.COUNT(GradeInfos); j++)
			{
				GradeInfo gradeInfo2 = GradeInfos[j];
				if (gradeInfo2 != null)
				{
					if (gradeInfo2.ShowShard == showShard && gradeInfo2.Grade == monsterGradeType)
					{
						result = gradeInfo2;
					}
					else
					{
						UIHelper.EnableGameObject(gradeInfo2.Root, enable: false);
					}
				}
			}
			UIHelper.EnableGameObject(ManaGardeInfo.GradeInfo.Root, enable: false);
		}
		return result;
	}

	public static void SetupIcon(Component root, Creature boss, GradeInfo gradeInfo)
	{
		if (gradeInfo != null)
		{
			AssetTexture assetTexture = null;
			UIHelper.ResetPlaneAssetTextureAsync(assetTex: (!String_Extension_Method.IsNullOrEmpty(gradeInfo.CustomAssetPostfix)) ? UIHelper.GetMonsterCirclePortraitAsset(boss.CreatureTableData.CodeName, boss.CreateInfo.HeroInfo.Star) : UIHelper.GetHeroIcon2(boss.CreatureTableData.Index, boss.CreateInfo.HeroInfo.Star), componentRoot: root, uiTex: gradeInfo.IconTexture);
		}
	}

	public static void SetupPhase(GradeInfo gradeInfo)
	{
		if (gradeInfo == null)
		{
			return;
		}
		PhaseInfo phaseInfo = null;
		int waveIndex = SingletonAuto<BattleFacade>.instance.GetBattleCache().WaveIndex;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(gradeInfo.PhaseInfos); i++)
		{
			PhaseInfo phaseInfo2 = gradeInfo.PhaseInfos[i];
			if (phaseInfo2.Phase == waveIndex)
			{
				phaseInfo = phaseInfo2;
			}
			else
			{
				UIHelper.EnableGameObject(phaseInfo2.Root, enable: false);
			}
		}
		if (phaseInfo != null)
		{
			UIHelper.EnableGameObject(phaseInfo.Root, enable: true);
		}
	}

	public static void SetupMpGauge(Creature boss, GradeInfo gradeInfo, bool showBossManaGauge)
	{
		if (gradeInfo != null)
		{
			UIHelper.SetSpriteFillAmount(gradeInfo.MpGauge, 0f);
			UIHelper.EnableGameObject(gradeInfo.MpGauge, showBossManaGauge);
			if (gradeInfo.MpGridRoot != null)
			{
				long num = boss.CreatureCache.MaxMp / 1000;
				gradeInfo.CreateMpSplitter((int)num);
			}
		}
	}

	public static void SetupHpLabel(GradeInfo gradeInfo)
	{
		if (gradeInfo == null || gradeInfo.HpLabel == null)
		{
			return;
		}
		GameObject gameObject = gradeInfo.HpLabel.gameObject;
		if (!(gameObject == null))
		{
			TweenAlpha component = gameObject.GetComponent<TweenAlpha>();
			if (component != null)
			{
				component.enabled = true;
				component.ResetToBeginning();
				component.PlayForward();
			}
		}
	}

	public Transform GetBottomDummy()
	{
		if (_currentGradeInfo == null)
		{
			return null;
		}
		return _currentGradeInfo.BottomDummy;
	}

	public Transform GetBuffStatusPoint01()
	{
		if (_currentGradeInfo == null)
		{
			return null;
		}
		return _currentGradeInfo.BuffStatusPoint01;
	}

	public Transform GetBuffStatusPoint02()
	{
		if (_currentGradeInfo == null)
		{
			return null;
		}
		return _currentGradeInfo.BuffStatusPoint02;
	}

	public Transform GetBuffStatusPoint03()
	{
		if (_currentGradeInfo == null)
		{
			return null;
		}
		return _currentGradeInfo.BuffStatusPoint03;
	}
}
