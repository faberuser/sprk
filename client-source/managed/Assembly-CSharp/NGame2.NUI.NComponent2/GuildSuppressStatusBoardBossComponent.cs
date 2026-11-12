using System.Collections.Generic;
using NGame2.NUI.NHelper.NGuild;
using NGame2.NUI.NWindow;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class GuildSuppressStatusBoardBossComponent : UIScrollItemBehaviour, InitOnCreate, ChallengeRaidBanInfoComponent.Listener
{
	public interface IClickListener
	{
		void OnClickBossInfo(GuildSuppressDungeonData suppressDungeonData, int raidIndex, int level);
	}

	public const string PrefabPath = "Prefab2/Component/GuildSuppress/GuildSuppressStatusBoardBossComponent";

	public static PrefabAuto<GuildSuppressStatusBoardBossComponent> PrefabAuto = new PrefabAuto<GuildSuppressStatusBoardBossComponent>("Prefab2/Component/GuildSuppress/GuildSuppressStatusBoardBossComponent");

	public UIWidget WidgetRoot;

	public UITexture BossTexture;

	public UILabel NameLabel;

	public UILabel ScoreLabel;

	public UIBaseGrid BanGrid;

	public GameObject BossHpRoot;

	public UISprite HpSprite;

	public UILabel HpLabel;

	public GameObject DeadRoot;

	private List<ChallengeRaidBanInfoComponent> _listBanComponent = new List<ChallengeRaidBanInfoComponent>();

	private GuildSuppressDungeonData _guildSuppressDungeonData;

	private GuildSuppressPlayInfo _guildSuppressPlayInfo;

	private CreatureData _bossCreatureData;

	private CampaignWaveData _bossWaveData;

	private BanRuleData _banRuleData;

	private int _level;

	private IClickListener _listener;

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		ReleaseBanComponent();
		ReleaseData();
	}

	public void Open(GuildSuppressPlayInfo playerInfo, GuildSuppressDungeonData suppressDungeonData, int level, IClickListener listener)
	{
		if (playerInfo != null)
		{
			_listener = listener;
			SetCachedData(playerInfo, suppressDungeonData, level);
			SetPortrait();
			SetName();
			SetScore();
			SetBanComponents();
			SetBossHp();
			SetDead();
		}
	}

	private void SetCachedData(GuildSuppressPlayInfo playerInfo, GuildSuppressDungeonData suppressDungeonData, int level)
	{
		_level = level;
		_guildSuppressPlayInfo = playerInfo;
		_guildSuppressDungeonData = suppressDungeonData;
		_bossCreatureData = GuildSuppressHelper.GetBossCreatureData(playerInfo.RaidIndex, _level, out _bossWaveData);
		_banRuleData = GuildSuppressHelper.GetBanRuleData(_guildSuppressDungeonData);
	}

	private void ReleaseData()
	{
		_listener = null;
		_guildSuppressDungeonData = null;
		_guildSuppressPlayInfo = null;
		_bossCreatureData = null;
		_bossWaveData = null;
		_banRuleData = null;
	}

	private void SetDead()
	{
		if (_guildSuppressPlayInfo != null)
		{
			bool enable = _guildSuppressPlayInfo.IsDead();
			UIHelper.EnableGameObject(DeadRoot, enable);
		}
	}

	private void SetPortrait()
	{
		if (_guildSuppressDungeonData != null)
		{
			GuildSuppressHelper.SetBossPortrait(_guildSuppressDungeonData.StatusBoardPortraitPath, BossTexture, this);
		}
	}

	private void SetName()
	{
		GuildSuppressHelper.SetBossNameLabel(_bossCreatureData, _bossWaveData, NameLabel, null);
	}

	private void SetScore()
	{
		if (_guildSuppressPlayInfo != null)
		{
			long totalDamage = _guildSuppressPlayInfo.TotalDamage;
			UIHelper.SetLabelText(ScoreLabel, Vespa_Long_Extension_Method.ToCommaString(totalDamage));
		}
	}

	private void SetBanComponents()
	{
		ReleaseBanComponent();
		GuildSuppressHelper.CreateBanComponent(_banRuleData, delegate(RaidBanType banType, string banValue)
		{
			ChallengeRaidBanInfoComponent challengeRaidBanInfoComponent = UIHelper.AddGridItem(BanGrid, ChallengeRaidBanInfoComponent.PrefabAuto);
			challengeRaidBanInfoComponent.Open(this, banType, banValue);
			_listBanComponent.Add(challengeRaidBanInfoComponent);
		});
		UIHelper.EnableGameObject(BanGrid, enable: false);
		UIHelper.EnableGameObject(BanGrid, enable: true);
		UIHelper.RepositionGrid(BanGrid);
	}

	private void ReleaseBanComponent()
	{
		FactoryBase.ReleaseAndClearList(_listBanComponent);
	}

	private void SetBossHp()
	{
		if (_guildSuppressPlayInfo == null)
		{
			return;
		}
		int sessionIndex = _guildSuppressPlayInfo.SessionIndex;
		int raidIndex = _guildSuppressPlayInfo.RaidIndex;
		int level = _level;
		long bossRemainHp;
		long bossMaxHp;
		bool bossHp = GuildSuppressHelper.GetBossHp(sessionIndex, raidIndex, level, _guildSuppressPlayInfo.MonsterHp0, out bossRemainHp, out bossMaxHp);
		UIHelper.EnableGameObject(BossHpRoot, bossHp);
		if (bossHp)
		{
			float num = 1f - (float)bossRemainHp / (float)bossMaxHp;
			float fillAmount = Mathf.Clamp01(1f - num);
			UIHelper.SetSpriteFillAmount(HpSprite, fillAmount);
			BattleDefineData battleDefineData = GuildSuppressHelper.GetBattleDefineData(raidIndex, level);
			if (battleDefineData == null || battleDefineData.HPPercent)
			{
				int num2 = Mathf.CeilToInt((float)bossRemainHp / (float)bossMaxHp * 100f);
				UIHelper.SetLabelText(HpLabel, num2.ToString());
			}
			else
			{
				UIHelper.SetLabelText(HpLabel, Vespa_Long_Extension_Method.ToCommaString(bossRemainHp));
			}
		}
	}

	public void OnClickBossInfo()
	{
		if (_listener != null)
		{
			_listener.OnClickBossInfo(_guildSuppressDungeonData, _guildSuppressPlayInfo.RaidIndex, _level);
		}
	}

	void ChallengeRaidBanInfoComponent.Listener.OnHideTooltip(ChallengeRaidBanInfoComponent comp)
	{
		ChallengeRaidBanTooltip.Hide(WidgetRoot);
	}

	void ChallengeRaidBanInfoComponent.Listener.OnShowTooltip(ChallengeRaidBanInfoComponent comp)
	{
		TooltipTextInfo tooltipTextInfo = new TooltipTextInfo();
		tooltipTextInfo.Name.Value = _banRuleData.RuleName;
		tooltipTextInfo.Desc.Value = _banRuleData.RuleName;
		ChallengeRaidBanTooltip.Open(WidgetRoot, tooltipTextInfo);
	}
}
