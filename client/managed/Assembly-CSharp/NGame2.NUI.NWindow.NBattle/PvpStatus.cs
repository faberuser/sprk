using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NBattleField;
using NGame2.NContentsDefine;
using NGame2.NContentsInfo;
using NGame2.NUI.NComponent2;
using NGameAsset;
using NNguiExtension;
using NShared;
using NVespa;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow.NBattle;

public class PvpStatus : BaseWindow
{
	public interface Listener
	{
		int GetRemainTime();

		bool IsBattleEnabled();
	}

	public class MatchUserInfo
	{
		public string Name { get; set; }

		public int TierIndex { get; set; }

		public int WorldTierIndex { get; set; }

		public int AvatarIndex { get; set; }

		public int MatchScore { get; set; }

		public int Rank { get; set; }

		public TeamType TeamType { get; set; }

		public long AccountId { get; set; }

		public MatchUserInfo()
		{
		}

		public MatchUserInfo(MatchedAccountInfo matchedAccountInfo)
		{
			if (matchedAccountInfo != null)
			{
				CachedUserInfo userInfo = matchedAccountInfo.UserInfo;
				PlayerBattleInfo battleInfo = matchedAccountInfo.BattleInfo;
				Name = matchedAccountInfo.Nick;
				Rank = matchedAccountInfo.Rank;
				TeamType = matchedAccountInfo.TeamType;
				AvatarIndex = userInfo?.AvatarHeroIndex ?? 0;
				TierIndex = battleInfo?.WorldTierIndex ?? 0;
				WorldTierIndex = battleInfo?.WorldTierIndex ?? 0;
				MatchScore = battleInfo?.MatchScore ?? 0;
				AccountId = userInfo?.AccountId ?? 0;
			}
		}

		public MatchUserInfo(ReplayTeamInfo replayTeamInfo)
		{
			if (replayTeamInfo != null)
			{
				Name = replayTeamInfo.NickName;
				Rank = replayTeamInfo.TierRank;
				TeamType = replayTeamInfo.TeamType;
				AvatarIndex = replayTeamInfo.AvaterIndex;
				TierIndex = replayTeamInfo.TierIndex;
				MatchScore = replayTeamInfo.MatchScore;
				AccountId = replayTeamInfo.AccountId;
			}
		}
	}

	[Serializable]
	public class TeamInfo
	{
		public enum HpGaugeType
		{
			Me,
			Enemy
		}

		public UISpriteSelector GaugeSpriteSelect;

		public UISprite GaugeSprite;

		public UILabel HpLabel;

		public UILabel HpPercentageLabel;

		public UITexture AvatarTexture;

		public UILabel NameLabel;

		public Transform TierIconRoot;

		public Transform LuckyTierIconRoot;

		public UILabel MatchScoreLabel;

		public GameObject PointBoxRoot;
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Battle/PvpStatus";

	public UILabel RemainMinuteLabel;

	public UILabel RemainSecondLabel;

	public TeamInfo LeftTeam;

	public TeamInfo RightTeam;

	public Transform BanPick_Design_L;

	public Transform BanPick_Design_R;

	private MatchArenaType MatchArenaType;

	private MatchUserInfo _leftInfo;

	private MatchUserInfo _rightInfo;

	private NGame2.NContentsInfo.MatchArena.MatchSeasonInfo MatchSeasonInfo;

	private Listener _listener;

	private List<MyMatchTierIconComponent> _tierIconList = new List<MyMatchTierIconComponent>();

	public void Open(MatchUserInfo userInfo1, MatchUserInfo userInfo2, Listener listener, MatchArenaType matchArenaType, NGame2.NContentsInfo.MatchArena.MatchSeasonInfo seasonInfo)
	{
		Open(delegate
		{
			Init(userInfo1, userInfo2, listener, matchArenaType, seasonInfo);
		}, null);
	}

	public void Init(MatchUserInfo userInfo1, MatchUserInfo userInfo2, Listener listener, MatchArenaType matchArenaType, NGame2.NContentsInfo.MatchArena.MatchSeasonInfo seasonInfo)
	{
		_leftInfo = ((userInfo1.TeamType == TeamType.Left) ? userInfo1 : userInfo2);
		_rightInfo = ((userInfo1.TeamType == TeamType.Right) ? userInfo1 : userInfo2);
		MatchArenaType = matchArenaType;
		_listener = listener;
		MatchSeasonInfo = seasonInfo;
		InitInfo();
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		UpdateGauge();
		UpdateRemainTime();
	}

	public override void Reset()
	{
		base.Reset();
		FactoryBase.ReleaseAndClearList(_tierIconList);
	}

	private void InitInfo()
	{
		InitInfo(LeftTeam, _leftInfo);
		InitInfo(RightTeam, _rightInfo);
	}

	private void SetupTierIconComponent(TeamInfo teamInfo, MatchUserInfo userInfo, MatchTierData tierData)
	{
		if (tierData != null)
		{
			MyMatchTierIconComponent myMatchTierIconComponent = MyMatchTierIconComponent.PrefabAuto.Factory.Create(teamInfo.TierIconRoot);
			Vespa_Unity_Transform_Extension_Method.ResetLocalTM(myMatchTierIconComponent.transform);
			myMatchTierIconComponent.Open(tierData.Tier, tierData.Level, VisualTierRank.CreateFromServer(userInfo.Rank), MatchArenaType);
			myMatchTierIconComponent.HideName();
			_tierIconList.Add(myMatchTierIconComponent);
		}
	}

	private void SetupTierIconComponentDisableLevel(TeamInfo teamInfo, MatchUserInfo userInfo, MatchTierData tierData)
	{
		if (tierData != null)
		{
			MyMatchTierIconComponent myMatchTierIconComponent = MyMatchTierIconComponent.PrefabAuto.Factory.Create(teamInfo.TierIconRoot);
			Vespa_Unity_Transform_Extension_Method.ResetLocalTM(myMatchTierIconComponent.transform);
			myMatchTierIconComponent.Open(tierData.Tier, tierData.Level, VisualTierRank.CreateFromServer(userInfo.Rank), MatchArenaType);
			myMatchTierIconComponent.HideName();
			myMatchTierIconComponent.DisableLevel();
			_tierIconList.Add(myMatchTierIconComponent);
		}
	}

	private void SetupTierIconComponentNotTier(TeamInfo teamInfo)
	{
		if (MatchSeasonInfo != null)
		{
			int seasonIndex = MatchSeasonInfo.SeasonIndex;
			if (MatchSeasonInfo.MatchSeasonType == MatchSeasonType.Free)
			{
				seasonIndex = 0;
			}
			MyMatchTierIconComponent myMatchTierIconComponent = MyMatchTierIconComponent.PrefabAuto.Factory.Create(teamInfo.TierIconRoot);
			Vespa_Unity_Transform_Extension_Method.ResetLocalTM(myMatchTierIconComponent.transform);
			myMatchTierIconComponent.Open(MatchSeasonInfo.MatchSeasonType, seasonIndex);
			myMatchTierIconComponent.HideName();
			_tierIconList.Add(myMatchTierIconComponent);
		}
	}

	private void InitInfo(TeamInfo teamInfo, MatchUserInfo userInfo)
	{
		if (userInfo == null)
		{
			UIHelper.SetLabelText(teamInfo.NameLabel, "Unknown");
			UIHelper.SetLabelText(teamInfo.MatchScoreLabel, "??");
			return;
		}
		UIHelper.SetLabelText(teamInfo.NameLabel, userInfo.Name);
		UIHelper.SetLabelText(teamInfo.MatchScoreLabel, userInfo.MatchScore);
		UIHelper.EnableGameObject(BanPick_Design_L, enable: false);
		UIHelper.EnableGameObject(BanPick_Design_R, enable: false);
		UIHelper.EnableGameObject(teamInfo.PointBoxRoot, enable: true);
		if (teamInfo.GaugeSpriteSelect != null)
		{
			TeamInfo.HpGaugeType value = TeamInfo.HpGaugeType.Enemy;
			if (AccountManager.IsLogined() && GlobalAutoBehaviour<AccountManager>.instance.UserManager != null)
			{
				long id = GlobalAutoBehaviour<AccountManager>.instance.UserManager.Id;
				value = ((userInfo.AccountId != id) ? TeamInfo.HpGaugeType.Enemy : TeamInfo.HpGaugeType.Me);
			}
			teamInfo.GaugeSpriteSelect.SelectEnum(value);
		}
		string battlePortraitPath = UIAvatarHelper.GetBattlePortraitPath(userInfo.AvatarIndex);
		if (!String_Extension_Method.IsNullOrEmpty(battlePortraitPath))
		{
			AssetTexture plainTexture = UIHelper.GetPlainTexture(battlePortraitPath);
			UIHelper.ResetPlaneAssetTextureAsync(this, teamInfo.AvatarTexture, plainTexture);
		}
		switch (MatchArenaType)
		{
		case MatchArenaType.Normal:
		{
			MatchTierData data3 = GameRoot.Instance.MatchTierDataContainer.GetData(userInfo.TierIndex);
			if (data3 != null)
			{
				SetupTierIconComponent(teamInfo, userInfo, data3);
			}
			else
			{
				SetupTierIconComponentNotTier(teamInfo);
			}
			break;
		}
		case MatchArenaType.BanPick:
			if (MatchSeasonInfo != null)
			{
				if (MatchSeasonInfo.ActiveTier)
				{
					MatchBanPickTierData data2 = GameRoot.Instance.GlobalBanPickTierDataContainer.GetData(userInfo.WorldTierIndex);
					SetupTierIconComponent(teamInfo, userInfo, data2);
				}
				else
				{
					SetupTierIconComponentNotTier(teamInfo);
				}
			}
			break;
		case MatchArenaType.GuildArena:
			UIHelper.EnableGameObject(teamInfo.PointBoxRoot, enable: false);
			break;
		case MatchArenaType.Ordeal:
		{
			int key = ((userInfo.WorldTierIndex > 0) ? userInfo.WorldTierIndex : userInfo.TierIndex);
			MatchTierData data = GameRoot.Instance.MatchTierDataContainer.GetData(key);
			if (data != null)
			{
				SetupTierIconComponentDisableLevel(teamInfo, userInfo, data);
			}
			break;
		}
		case MatchArenaType.WorldBanPick:
		case MatchArenaType.Arena3:
			break;
		}
	}

	private void UpdateGauge()
	{
		if (_listener == null)
		{
			return;
		}
		long leftMaxHp = 0L;
		long leftHp = 0L;
		long rightMaxHp = 0L;
		long rightHp = 0L;
		DoLoopBattleCreature(delegate(BattleFacade.CreatureCacheData creatureData)
		{
			if (creatureData != null)
			{
				if (creatureData.TeamType == TeamType.Left)
				{
					leftMaxHp += creatureData.MaxHp;
					leftHp += creatureData.Hp;
				}
				else
				{
					rightMaxHp += creatureData.MaxHp;
					rightHp += creatureData.Hp;
				}
			}
		});
		UpdateGauge(LeftTeam, leftMaxHp, leftHp);
		UpdateGauge(RightTeam, rightMaxHp, rightHp);
	}

	private void DoLoopBattleCreature(Action<BattleFacade.CreatureCacheData> cacheAction)
	{
		if (SingletonAuto<BattleFacade>.isValidInstance)
		{
			int creatureCount = SingletonAuto<BattleFacade>.instance.GetCreatureCount();
			for (int i = 0; i < creatureCount; i++)
			{
				BattleFacade.CreatureCacheData creatureCacheByIndex = SingletonAuto<BattleFacade>.instance.GetCreatureCacheByIndex(i);
				Vespa_System_Action_Extension_Method.fire(cacheAction, creatureCacheByIndex);
			}
		}
	}

	private void UpdateGauge(TeamInfo teamInfo, long maxHp, long hp)
	{
		if (teamInfo != null)
		{
			long num = ((maxHp > 0) ? (hp * 100 / maxHp) : 0);
			UIHelper.SetSpriteFillAmount(teamInfo.GaugeSprite, (float)num / 100f);
			UIHelper.SetLabelText(teamInfo.HpLabel, hp);
			UIHelper.SetLabelText(teamInfo.HpPercentageLabel, num);
		}
	}

	private void UpdateRemainTime()
	{
		if (_listener != null && _listener.IsBattleEnabled())
		{
			int remainTime = _listener.GetRemainTime();
			int num = remainTime / 60;
			int num2 = remainTime % 60;
			UIHelper.SetLabelText(RemainMinuteLabel, num.ToString("D2"));
			UIHelper.SetLabelText(RemainSecondLabel, num2.ToString("D2"));
		}
	}
}
