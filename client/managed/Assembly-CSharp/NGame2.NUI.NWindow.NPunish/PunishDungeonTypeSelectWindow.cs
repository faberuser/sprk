using NGame2.NAccount;
using NShared;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow.NPunish;

public class PunishDungeonTypeSelectWindow : RenewResourceWindow
{
	public interface IListener
	{
		void OnClickEnterScenario(PunishmentGroupData data);

		void OnClickEnterKarma(PunishmentGroupData data);

		void OnClickOpenKarma(PunishmentGroupData data);

		void OnClickEnterBoss(PunishmentGroupData data);

		void OnClickOpenBoss(PunishmentGroupData data);

		void OnClickResetBoss(PunishmentGroupData data);

		void OnClickBossRewardPopup(PunishmentGroupData data);
	}

	public static string PrefabPath = "Resources/Prefab2/Window/Raid/PunishmentDungeonTypeSelectWindow";

	private readonly string[] KARMA_GUIDE = new string[12]
	{
		"GAME_GUIDE", "", "GAME_GUIDE_0400_001_TITLE", "GAME_GUIDE_0400_001_DESC", "GAME_GUIDE_0400_002_TITLE", "GAME_GUIDE_0400_002_DESC", "GAME_GUIDE_0400_003_TITLE", "GAME_GUIDE_0400_003_DESC", "GAME_GUIDE_0400_004_TITLE", "GAME_GUIDE_0400_004_DESC",
		"GAME_GUIDE_0400_005_TITLE", "GAME_GUIDE_0400_005_DESC"
	};

	[Header("Scenario")]
	public UIBundleLoader ScenarioBundleLoader;

	public UILabel ScenarloNameLabel;

	public UILabel ScenarioDescLabel;

	[Header("Karma")]
	public UIBundleLoader KarmaBundleLoader;

	public UILabel KarmaNameLabel;

	public UILabel KarmaDescLabel;

	public UIWidget KarmaStaminaWidget;

	public UILabel KarmaStaminaLabel;

	public UIButton KarmaOpenButton;

	public UIButton KarmaEnterButton;

	public UIWidget KarmaExpireWidget;

	public UILabel KarmaExpireTimeLabel;

	public UIWidget KarmaCloseWidget;

	[Header("Boss")]
	public UIBundleLoader BossBundleLoader;

	public UILabel BossNameLabel;

	public UILabel BossDescLabel;

	public UIWidget BossStaminaWidget;

	public UILabel BossStaminaLabel;

	public UILabel BossClearLabel;

	public UIButton BossOpenButton;

	public UIButton BossEnterButton;

	public UIWidget BossResetButton;

	public UIWidget BossExpireWidget;

	public UILabel BossExpireTimeLabel;

	public UIWidget BossCloseWidget;

	private PunishmentGroupData _scenarioData;

	private PunishmentGroupData _karmaData;

	private PunishmentGroupData _bossData;

	private IListener _listener;

	private bool _showKarmaExpireLabel;

	private bool _showBossClearExpireLabel;

	private string _karmaExpireTime;

	private string _bossExpireTime;

	public void Open(PunishmentGroupData bossData, IListener listener)
	{
		_bossData = bossData;
		_scenarioData = GetData(EPunishmentDungeonType.Scenario);
		_karmaData = GetData(EPunishmentDungeonType.Karma);
		_listener = listener;
		Open(Set, null);
	}

	public override void Close()
	{
		base.Close();
		_bossData = null;
		_scenarioData = null;
		_karmaData = null;
		_listener = null;
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		if (_showKarmaExpireLabel)
		{
			UpdateExpiredTime(EPunishmentDungeonType.Karma, KarmaExpireTimeLabel, _karmaExpireTime);
		}
		if (_showBossClearExpireLabel)
		{
			UpdateExpiredTime(EPunishmentDungeonType.Boss, BossExpireTimeLabel, _bossExpireTime);
		}
	}

	public void Refresh()
	{
		Set();
	}

	private void Set()
	{
		Init();
		SetDungeon(_scenarioData, ScenarioBundleLoader, ScenarloNameLabel, ScenarioDescLabel);
		SetDungeon(_karmaData, KarmaBundleLoader, KarmaNameLabel, KarmaDescLabel);
		SetDungeon(_bossData, BossBundleLoader, BossNameLabel, BossDescLabel);
		SetOpenDungeon(_karmaData, KarmaOpenButton, KarmaEnterButton, null, KarmaStaminaWidget, KarmaStaminaLabel, KarmaCloseWidget, KarmaExpireWidget, KarmaExpireTimeLabel);
		SetOpenDungeon(_bossData, BossOpenButton, BossEnterButton, BossResetButton, BossStaminaWidget, BossStaminaLabel, BossCloseWidget, BossExpireWidget, BossExpireTimeLabel);
	}

	private void SetOpenDungeon(PunishmentGroupData data, UIButton openButton, UIButton enterButton, UIWidget resetButton, UIWidget staminaWidget, UILabel staminaLabel, UIWidget closeWidget, UIWidget expireWidget, UILabel expireLabel)
	{
		if (data == null || data.PunishmentDungeonType == EPunishmentDungeonType.Scenario)
		{
			return;
		}
		OpenPunishmentRaidInfo openPunishmentRaidInfo = GlobalAutoBehaviour<AccountManager>.instance.OpenPunishmentRaidManager.Get(data.GroupIndex, data.PunishmentDungeonType);
		bool flag = NGame2.NUI.NWindow.NPunish.Helper.IsOpenPunishRaid(openPunishmentRaidInfo);
		UIHelper.EnableGameObject(openButton, !flag);
		UIHelper.EnableGameObject(closeWidget, !flag);
		UIHelper.EnableGameObject(enterButton, flag);
		UIHelper.EnableGameObject(resetButton, flag);
		UIHelper.EnableGameObject(staminaWidget, !flag);
		if (!flag)
		{
			int openStamina = NGame2.NUI.NWindow.NPunish.Helper.GetOpenStamina(data);
			SetStaminaLabel(staminaLabel, openStamina);
		}
		bool flag2 = false;
		string text = string.Empty;
		if (openPunishmentRaidInfo == null)
		{
			flag2 = false;
		}
		else
		{
			text = openPunishmentRaidInfo.ExpireTime;
			int expireRemainTime = NGame2.NUI.NWindow.NPunish.Helper.GetExpireRemainTime(text);
			flag2 = openPunishmentRaidInfo != null && expireRemainTime > 0;
		}
		UIHelper.EnableGameObject(expireWidget, flag2);
		if (data.PunishmentDungeonType == EPunishmentDungeonType.Karma)
		{
			_showKarmaExpireLabel = flag2;
		}
		else if (data.PunishmentDungeonType == EPunishmentDungeonType.Boss)
		{
			_showBossClearExpireLabel = flag2;
		}
		if (flag2)
		{
			if (data.PunishmentDungeonType == EPunishmentDungeonType.Karma)
			{
				_karmaExpireTime = text;
			}
			else if (data.PunishmentDungeonType == EPunishmentDungeonType.Boss)
			{
				int clearCount = NGame2.NUI.NWindow.NPunish.Helper.GetClearCount(openPunishmentRaidInfo);
				SetBossClearLabel(clearCount);
				_bossExpireTime = text;
			}
			SetExpiredTimeLabel(expireLabel, text);
		}
	}

	private void SetDungeon(PunishmentGroupData data, UIBundleLoader loader, UILabel nameLabel, UILabel descLabel)
	{
		if (data != null)
		{
			if (loader != null)
			{
				string imagePath = data.ImagePath;
				UIHelper.SetBundleTexture(loader, imagePath);
			}
			if (nameLabel != null)
			{
				string stringDirect = LocalizationManager.GetStringDirect(data.NameKey);
				UIHelper.SetLabelText(nameLabel, stringDirect);
			}
			if (descLabel != null)
			{
				string stringDirect2 = LocalizationManager.GetStringDirect(data.DescKey);
				UIHelper.SetLabelText(descLabel, stringDirect2);
			}
		}
	}

	private void SetStaminaLabel(UILabel label, int staminaCount)
	{
		UIHelper.SetLabelText(label, $"- {staminaCount}");
	}

	private void SetBossClearLabel(int clearCount)
	{
		UIHelper.SetLabelText(BossClearLabel, LocalizationManager.GetStringDirect("NOTICE_DUNGEON_CLEAR_COUNT", clearCount));
	}

	private void SetExpiredTimeLabel(UILabel expireLabel, string expireTime)
	{
		int expireRemainTime = NGame2.NUI.NWindow.NPunish.Helper.GetExpireRemainTime(expireTime);
		long remainTime = expireRemainTime;
		UIHelper.SetLabelText(expireLabel, UIHelper.GetRemainTimeTextAll(remainTime));
	}

	private void UpdateExpiredTime(EPunishmentDungeonType dungeonType, UILabel expireLabel, string expireTime)
	{
		int expireRemainTime = NGame2.NUI.NWindow.NPunish.Helper.GetExpireRemainTime(expireTime);
		long num = expireRemainTime;
		int num2 = 0;
		if (num <= num2)
		{
			switch (dungeonType)
			{
			case EPunishmentDungeonType.Karma:
				SetOpenDungeon(_karmaData, KarmaOpenButton, KarmaEnterButton, null, KarmaStaminaWidget, KarmaStaminaLabel, KarmaCloseWidget, KarmaExpireWidget, KarmaExpireTimeLabel);
				break;
			case EPunishmentDungeonType.Boss:
				SetOpenDungeon(_bossData, BossOpenButton, BossEnterButton, BossResetButton, BossStaminaWidget, BossStaminaLabel, BossCloseWidget, BossExpireWidget, BossExpireTimeLabel);
				break;
			}
		}
		else
		{
			UIHelper.SetLabelText(expireLabel, UIHelper.GetRemainTimeTextAll(num));
		}
	}

	private PunishmentGroupData GetData(EPunishmentDungeonType punishmentDungeonType)
	{
		if (_bossData == null)
		{
			return null;
		}
		int groupIndex = _bossData.GroupIndex;
		return GameRoot.Instance.PunishmentGroupDataContainer.GetData(groupIndex, punishmentDungeonType);
	}

	public void OnClickEnterScenerio()
	{
		if (_listener != null && _scenarioData != null)
		{
			_listener.OnClickEnterScenario(_scenarioData);
		}
	}

	public void OnClickEnterKarma()
	{
		if (_listener != null && _karmaData != null)
		{
			_listener.OnClickEnterKarma(_karmaData);
		}
	}

	public void OnClickOpenKarma()
	{
		if (_listener != null && _karmaData != null)
		{
			_listener.OnClickOpenKarma(_karmaData);
		}
	}

	public void OnClickEnterBoss()
	{
		if (_listener != null && _bossData != null)
		{
			_listener.OnClickEnterBoss(_bossData);
		}
	}

	public void OnClickOpenBoss()
	{
		if (_listener != null && _bossData != null)
		{
			_listener.OnClickOpenBoss(_bossData);
		}
	}

	public void OnClickResetBoss()
	{
		if (_listener != null && _bossData != null)
		{
			_listener.OnClickResetBoss(_bossData);
		}
	}

	public void OnClickBossRewardPopup()
	{
		if (_listener != null && _bossData != null)
		{
			_listener.OnClickBossRewardPopup(_bossData);
		}
	}

	public void OnClickKarmaGuidePopup()
	{
		EventGuidePopup.Open(KARMA_GUIDE);
	}
}
