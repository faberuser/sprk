using System;
using NGame2.NContentsDefine;
using NShared;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NBattle;

public class HeroBattleStatComponent : LinkFactoryComponent, InitOnCreate
{
	[Serializable]
	public class StatGroup
	{
		public UISprite Graph;

		public UILabel Label;

		public UILabel PercentageLabel;
	}

	public const string LeftPrefabPath = "Prefab2/Component/Battle/HeroBattleStatLeftComponent";

	public const string RightPrefabPath = "Prefab2/Component/Battle/HeroBattleStatRightComponent";

	public static PrefabAuto<HeroBattleStatComponent> LeftPrefabAuto = new PrefabAuto<HeroBattleStatComponent>("Prefab2/Component/Battle/HeroBattleStatLeftComponent");

	public static PrefabAuto<HeroBattleStatComponent> RightPrefabAuto = new PrefabAuto<HeroBattleStatComponent>("Prefab2/Component/Battle/HeroBattleStatRightComponent");

	public Transform PortraitRoot;

	public GameObject DamageRoot;

	public StatGroup DamageStat;

	public float GaugeSpeed = 1f;

	public UILabel NameLabel;

	public UILabel Rank1Label;

	public UILabel RankLabel;

	public Transform[] StarObjects;

	public UILabel TranscendLabel;

	private CachedHeroInfo _heroInfo;

	private BattleStatisticsManager.CreatureInfo _creatureInfo;

	private HeroPortraitComponent _portrait;

	private float _currentGaugeRatio;

	private int _targetGaugeRatioM;

	private long _targetDamage;

	public long Value => _targetDamage;

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		Close();
	}

	public bool Open(BattleStatisticsManager.CreatureInfo creatureInfo)
	{
		_creatureInfo = creatureInfo;
		_heroInfo = creatureInfo.HeroInfo;
		_portrait = HeroPortraitComponent.SmallPrefabAuto.Factory.Create(PortraitRoot);
		_portrait.Open(creatureInfo.HeroInfo, null, null, HeroPortraitComponent.EPortraitMode.Icon);
		_portrait.ShowStar(show: false);
		Vespa_Unity_Transform_Extension_Method.ResetLocalTM(_portrait.transform);
		UIHelper.EnableGameObject(DamageRoot, enable: false);
		InitStar();
		InitTranscend();
		InitName();
		return true;
	}

	private void InitStar()
	{
		sbyte star = _heroInfo.Star;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(StarObjects); i++)
		{
			int num = i + 1;
			UIHelper.EnableGameObject(StarObjects[i], num <= star);
		}
	}

	private void InitTranscend()
	{
		if (_heroInfo.Transcended > 0)
		{
			UIHelper.EnableGameObject(TranscendLabel, enable: true);
			UIHelper.SetLabelText(TranscendLabel, "+" + _heroInfo.Transcended);
		}
		else
		{
			UIHelper.EnableGameObject(TranscendLabel, enable: false);
		}
	}

	private void InitName()
	{
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(_heroInfo.HeroIndex);
		if (_creatureInfo.IsBoss)
		{
			string creatureName = string.Empty;
			string creatureNamePrefix = string.Empty;
			GameRoot.Instance.CreatureVisualDataContainer.GetCreatureName(data, _creatureInfo.VisualInfo, LocalizationManager.GetStringDirect, out creatureName, out creatureNamePrefix);
			string text = creatureNamePrefix + " " + creatureName;
			UIHelper.SetLabelText(NameLabel, text);
		}
		else
		{
			string text2 = ((data != null) ? data.Name : "Unknown");
			UIHelper.SetLabelText(NameLabel, text2);
		}
	}

	public void Close()
	{
		FactoryBase.SafeReleaseComponent(ref _portrait);
		_currentGaugeRatio = 0f;
		_targetGaugeRatioM = 0;
	}

	public void SetRank(VisualTierRank visualTierRank)
	{
		UIHelper.EnableGameObject(Rank1Label, enable: false);
		UIHelper.EnableGameObject(RankLabel, enable: false);
		if (visualTierRank.Value == 1)
		{
			UIHelper.EnableGameObject(Rank1Label, enable: true);
			UIHelper.SetRankLabelText(Rank1Label, visualTierRank);
		}
		else
		{
			UIHelper.EnableGameObject(RankLabel, enable: true);
			UIHelper.SetRankLabelText(RankLabel, visualTierRank);
		}
	}

	public void SetDamage(long damage, long max)
	{
		if (DamageStat != null)
		{
			UIHelper.EnableGameObject(DamageRoot, enable: true);
			UIHelper.SetSpriteFillAmount(DamageStat.Graph, 0f);
			UIHelper.SetLabelText(DamageStat.Label, 0);
			UIHelper.SetLabelText(DamageStat.PercentageLabel, "0%");
			_currentGaugeRatio = 0f;
			_targetGaugeRatioM = (int)((max > 0) ? ((float)damage / (float)max * 1000f) : 0f);
			_targetDamage = damage;
		}
	}

	private void Update()
	{
		UpdateGaugeAnim();
	}

	private void UpdateGaugeAnim()
	{
		if (_targetGaugeRatioM == 0 && _targetDamage > 0)
		{
			SetGauge(0f, _targetDamage, 0);
			return;
		}
		float num = (float)_targetGaugeRatioM / 1000f;
		if (!(_currentGaugeRatio >= num))
		{
			float num2 = GaugeSpeed * Time.deltaTime;
			_currentGaugeRatio += num2;
			_currentGaugeRatio = Mathf.Min(_currentGaugeRatio, num);
			if (_currentGaugeRatio >= num)
			{
				int percent = _targetGaugeRatioM / 10;
				SetGauge(num, _targetDamage, percent);
			}
			else
			{
				long damage = (long)((float)_targetDamage * (_currentGaugeRatio / num));
				int percent2 = (int)(_currentGaugeRatio * 100f);
				SetGauge(_currentGaugeRatio, damage, percent2);
			}
		}
	}

	private void SetGauge(float gaugeRatio, long damage, int percent)
	{
		UIHelper.SetSpriteFillAmount(DamageStat.Graph, gaugeRatio);
		UIHelper.SetLabelText(DamageStat.Label, Vespa_Long_Extension_Method.ToCommaString(damage));
		UIHelper.SetLabelText(DamageStat.PercentageLabel, percent + "%");
	}
}
