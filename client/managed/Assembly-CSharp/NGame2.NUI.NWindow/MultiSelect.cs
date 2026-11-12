using System;
using System.Collections.Generic;
using NGame2.NDropBonusEvent;
using NGame2.NUI.NManager.NLobby;
using NGameAsset;
using NShared;
using NShared.NRaid;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NSingleton;
using NVespa.NUtil;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class MultiSelect : LobbyFullWindow
{
	public interface Listener
	{
		void RefreshChallengeRaidInfo();

		bool IsAbleChallengeRaidNotice();

		ChallengeRaidInfo GetChallengeRaidNoticeInfo();

		bool IsAbleChallengeRaidSeason();

		ChallengeRaidInfo GetChallengeRaidSeasonInfo();

		bool IsAbleChallengeRaidEnter();

		ChallengeRaidInfo GetChallengeRaidEnterInfo();

		void OnEnterPartyDungeon();

		void OnEnterDragonRaid();

		void OnEnterDragonRaidHard();

		void OnEnterChallengeRaid();

		void OnEnterFieldRaid();

		void OnEnterTechnoMagicRaid();
	}

	[Serializable]
	public class ButtonGroup
	{
		public GameObject Button;

		public UIWidget WidgetLocked;

		public UISprite EvnetImage;
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Raid/MultiSelect";

	private StaminaType StaminaType;

	public ButtonGroup BtnGroupDragonRaid;

	public ButtonGroup BtnGroupDragonRaidHard;

	public ButtonGroup BtnGroupChallengeRaid;

	public ButtonGroup BtnGroupPartyDungeon;

	public ButtonGroup BtnGroupFieldRaid;

	public ButtonGroup BtnGroupTechnoMagicRaid;

	public UILabel Lbl_CommingSoon;

	public UILabel Lbl_RemainTime;

	public UILabel Lbl_FieldOpenCondition;

	public UITexture BossImageTexture;

	public UISprite Spr_BossDefault;

	public UIScrollGrid ScrollGrid;

	private Listener _listener;

	private float _lastRefreshInfoTime;

	private int _refreshCountTick;

	private string _bossImageTexturePath = string.Empty;

	private const string CHALLEGNERAID_LEFT_TIME = "CHALLEGNERAID_LEFT_TIME";

	private const string CHALLEGNERAID_BATTLE_END = "CHALLEGNERAID_BATTLE_END";

	private const string CHALLEGNERAID_OPEN_TIME = "CHALLEGNERAID_OPEN_TIME";

	private const string CHALLEGNERAID_NO_BATTLE_TIME = "CHALLEGNERAID_NO_BATTLE_TIME";

	public void Open(Listener listener)
	{
		if (listener == null)
		{
			VespaDebug.LogError("RaidSelect::Open / listener == null");
			return;
		}
		_listener = listener;
		Open(delegate
		{
			Init();
		});
	}

	public void Init()
	{
		_lastRefreshInfoTime = Time.time;
		_listener.RefreshChallengeRaidInfo();
		_refreshCountTick = 0;
		SetChallengeRaidTexture();
		SetChallengeRaidRemainTime();
		SetChallengeRaidButton();
		SetPartyDungeonButton();
		SetDraogonRaidButton();
		SetDraogonRaidHardButton();
		SetFieldRaidButton();
		SetTechnoMagicRaidButton();
		SingletonUpdater<Executer>.instance.NextRun(this, delegate
		{
			UIHelper.RepositionGrid(ScrollGrid);
			UIHelper.ResetGridPosition(ScrollGrid);
		});
	}

	private void SetTechnoMagicRaidButton()
	{
		bool flag = RaidHelper.CanEnterTechnoMagicRaid();
		UIHelper.EnableGameObject(BtnGroupTechnoMagicRaid.Button, flag);
		UIHelper.EnableGameObject(BtnGroupTechnoMagicRaid.WidgetLocked, !flag);
		bool flag2 = GlobalAutoBehaviour<DropBonusEventManager>.instance.GetEffectiveEvent(EventBonusType.BonusRaidPoint, null) != null;
		UIHelper.EnableGameObject(BtnGroupTechnoMagicRaid.EvnetImage, flag && flag2);
	}

	private void SetPartyDungeonButton()
	{
		bool flag = MultiPartyManagement.CheckPartyDungeonContentsStatus();
		UIHelper.EnableGameObject(BtnGroupPartyDungeon.Button, flag);
		UIHelper.EnableGameObject(BtnGroupPartyDungeon.WidgetLocked, !flag);
		UIHelper.EnableGameObject(Lbl_CommingSoon, !flag);
	}

	private void SetDraogonRaidButton()
	{
		bool enable = GlobalAutoBehaviour<DropBonusEventManager>.instance.GetAllEffectiveEvent().Exists((DropBonusEvent effectiveEvent) => (effectiveEvent.BonusType == EventBonusType.BonusRaidPoint || effectiveEvent.BonusType == EventBonusType.BonusRaidReward) ? true : false);
		UIHelper.EnableGameObject(BtnGroupDragonRaid.EvnetImage, enable);
	}

	private void SetDraogonRaidHardButton()
	{
		bool flag = RaidHelper.CanEnterHardMode();
		flag = false;
		UIHelper.EnableGameObject(BtnGroupDragonRaidHard.Button, flag);
		UIHelper.EnableGameObject(BtnGroupDragonRaidHard.WidgetLocked, !flag);
		bool flag2 = GlobalAutoBehaviour<DropBonusEventManager>.instance.GetEffectiveEvent(EventBonusType.BonusRaidPoint, null) != null;
		UIHelper.EnableGameObject(BtnGroupDragonRaidHard.EvnetImage, flag && flag2);
	}

	private void SetFieldRaidButton()
	{
		bool flag = RaidHelper.CanEnterFieldRaid();
		if (!flag)
		{
			IList<RaidData> raidDataListByType = GameRoot.Instance.RaidDataContainer.GetRaidDataListByType(RaidType.FieldRaid);
			RaidData raidData = null;
			string text = "";
			string text2 = "";
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(raidDataListByType); i++)
			{
				raidData = raidDataListByType[i];
				if (raidData.Level <= 1)
				{
					text = RaidHelper.FieldRaidOpenNode(raidData);
					if (!String_Extension_Method.IsNullOrEmpty(text))
					{
						text2 = LocalizationManager.GetStringDirect("FIELD_RAID_OPEN_CONDITION", text);
						break;
					}
				}
			}
			UIHelper.SetLabelText(Lbl_FieldOpenCondition, text2);
		}
		UIHelper.EnableGameObject(BtnGroupFieldRaid.Button, flag);
		UIHelper.EnableGameObject(BtnGroupFieldRaid.WidgetLocked, !flag);
		UIHelper.EnableGameObject(BtnGroupFieldRaid.EvnetImage, enable: false);
	}

	private void SetChallengeRaidTexture()
	{
		bool num = _listener.IsAbleChallengeRaidSeason();
		bool flag = _listener.IsAbleChallengeRaidNotice();
		bool flag2 = num || flag;
		UIHelper.EnableGameObject(Spr_BossDefault, !flag2);
		if (num)
		{
			int challengeRaidIndex = _listener.GetChallengeRaidSeasonInfo().ChallengeRaidIndex;
			SetChallengeRaidBossTexture(challengeRaidIndex);
		}
		else if (flag)
		{
			int challengeRaidIndex2 = _listener.GetChallengeRaidNoticeInfo().ChallengeRaidIndex;
			SetChallengeRaidBossTexture(challengeRaidIndex2);
		}
	}

	private void SetChallengeRaidBossTexture(int challengeRaidIndex)
	{
		ChallengeRaidData data = GameRoot.Instance.ChallengeRaidDataContainer.GetData(challengeRaidIndex);
		if (data != null && !String_Extension_Method.IsNullOrEmpty(data.RaidSelectImage) && !(_bossImageTexturePath == data.RaidSelectImage))
		{
			AssetTexture assetTexture = new AssetTexture();
			assetTexture.Open(data.RaidSelectImage);
			UIHelper.ResetPlaneAssetTextureAsync(this, BossImageTexture, assetTexture);
			_bossImageTexturePath = data.RaidSelectImage;
		}
	}

	private void SetChallengeRaidButton()
	{
		bool flag = _listener != null && _listener.IsAbleChallengeRaidSeason();
		UIHelper.EnableGameObject(BtnGroupChallengeRaid.Button, flag);
		UIHelper.EnableGameObject(BtnGroupChallengeRaid.WidgetLocked, !flag);
		bool flag2 = false;
		ChallengeRaidInfo challengeRaidSeasonInfo = _listener.GetChallengeRaidSeasonInfo();
		if (challengeRaidSeasonInfo != null)
		{
			int challengeRaidIndex = challengeRaidSeasonInfo.ChallengeRaidIndex;
			ChallengeRaidData data = GameRoot.Instance.ChallengeRaidDataContainer.GetData(challengeRaidIndex);
			if (data != null)
			{
				flag2 = data.ShowEnterButtonEventImage;
			}
		}
		UIHelper.EnableGameObject(BtnGroupChallengeRaid.EvnetImage, flag && flag2);
	}

	private int GetRemainTime(string endTime)
	{
		if (!Vespa_String_Extension_Method2.TryParseNetworkTime2(endTime, out var o_dateTime))
		{
			return 0;
		}
		return System.Math.Max((int)(o_dateTime - NGame2.NUI.NWindow.Common.GetUTCNow()).TotalSeconds, 0);
	}

	private void SetChallengeRaidRemainTime()
	{
		UIHelper.EnableGameObject(Lbl_RemainTime, enable: true);
		ChallengeRaidInfo challengeRaidInfo = null;
		int num = 0;
		long num2 = num;
		if (_listener == null)
		{
			UIHelper.SetLabelText(Lbl_RemainTime, LocalizationManager.GetStringDirect("CHALLEGNERAID_NO_BATTLE_TIME"));
			return;
		}
		bool flag = _listener.IsAbleChallengeRaidSeason();
		bool flag2 = _listener.IsAbleChallengeRaidEnter();
		bool flag3 = _listener.IsAbleChallengeRaidNotice();
		if (flag && flag2)
		{
			challengeRaidInfo = _listener.GetChallengeRaidEnterInfo();
			int num3 = ((challengeRaidInfo != null) ? GetRemainTime(challengeRaidInfo.EndTime) : 0);
			num2 = num3;
			UIHelper.SetLabelText(Lbl_RemainTime, LocalizationManager.GetStringDirect("CHALLEGNERAID_LEFT_TIME", UIHelper.GetRemainTimeLabelText(num2)));
		}
		else if (flag && !flag2)
		{
			UIHelper.SetLabelText(Lbl_RemainTime, LocalizationManager.GetStringDirect("CHALLEGNERAID_BATTLE_END"));
		}
		else if (!flag && flag3)
		{
			challengeRaidInfo = _listener.GetChallengeRaidNoticeInfo();
			int num4 = ((challengeRaidInfo != null) ? GetRemainTime(challengeRaidInfo.EndTime) : 0);
			num2 = num4;
			UIHelper.SetLabelText(Lbl_RemainTime, LocalizationManager.GetStringDirect("CHALLEGNERAID_OPEN_TIME", UIHelper.GetRemainTimeLabelText(num2)));
		}
		else if (!flag && !flag3)
		{
			UIHelper.SetLabelText(Lbl_RemainTime, LocalizationManager.GetStringDirect("CHALLEGNERAID_NO_BATTLE_TIME"));
		}
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		RefreshInfo();
	}

	private void RefreshInfo()
	{
		if (!(Time.time - _lastRefreshInfoTime < 1f))
		{
			_lastRefreshInfoTime = Time.time;
			SetChallengeRaidTexture();
			SetChallengeRaidRemainTime();
			SetChallengeRaidButton();
			_refreshCountTick++;
			if (_refreshCountTick > 10)
			{
				_refreshCountTick = 0;
				_listener.RefreshChallengeRaidInfo();
			}
		}
	}

	public override void Reset()
	{
		base.Reset();
	}

	public void OnClickClose()
	{
		Close();
	}

	public void OnClickPartyDungeon()
	{
		if (_listener != null)
		{
			_listener.OnEnterPartyDungeon();
		}
	}

	public void OnClickDragonRaid()
	{
		if (_listener != null)
		{
			_listener.OnEnterDragonRaid();
		}
	}

	public void OnClickDragonRaidHard()
	{
		if (_listener != null)
		{
			_listener.OnEnterDragonRaidHard();
		}
	}

	public void OnClickChallengeRaid()
	{
		if (_listener != null)
		{
			_listener.OnEnterChallengeRaid();
		}
	}

	public void OnClickFieldRaid()
	{
		if (_listener != null)
		{
			_listener.OnEnterFieldRaid();
		}
	}

	public void OnClickTechnoMagicRaid()
	{
		if (_listener != null)
		{
			_listener.OnEnterTechnoMagicRaid();
		}
	}
}
