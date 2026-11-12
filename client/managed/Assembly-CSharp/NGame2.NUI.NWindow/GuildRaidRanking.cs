using System;
using System.Collections.Generic;
using System.Linq;
using NGame2.NAccount;
using NGame2.NGuild;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NGameAsset;
using NShared;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class GuildRaidRanking : LobbyFullWindow, GuildRaidRankerComponent.Listener
{
	[Serializable]
	public class MainBossInfo
	{
		public int ChapterIndex;

		public int DungeonIndex;

		public string BossImgPath;
	}

	public interface Listener
	{
		void OnClickOpenGuildRaid(GuildRaidRanking caller);
	}

	public UITexture GuildLogoTexture;

	public UITexture GuildBackTexture;

	public UITexture MyRankerHeroTexture;

	public UILabel MyRankerNameLabel;

	public UILabel MyRankerScoreLabel;

	public UITexture LastBossImageTexture;

	public UILabel LastBossNameLabel;

	public UILabel LastBossNamePrefixLabel;

	public UILabel InitTimeLabel;

	public UISprite HpGaugeSprite;

	public UILabel HpPercentageLabel;

	public UIButton RaidOpenButton;

	public UIButton RaidEnterButton;

	public Transform OpenedRoot;

	public UIBaseGrid RankerGrid;

	public Transform EmptyRankerRoot;

	public MainBossInfo MainBossInfo_;

	public const string PrefabPath = "Resources/Prefab2/Window/GuildRaid/GuildRaidRanking";

	private Listener _listener;

	private GuildRaidState _guildRaidState;

	private List<GuildRaidRankerComponent> _rankerComponentList = new List<GuildRaidRankerComponent>();

	public void Open(Listener listener, Action onOpen = null)
	{
		_listener = listener;
		Open(delegate
		{
			Init();
			Vespa_System_Action_Extension_Method.fire(onOpen);
		});
	}

	public void Init()
	{
		InitCurrentState();
		InitProgress();
		InitRanker();
		InitGuildRaidInitTime();
	}

	private void InitCurrentState()
	{
		SetBossDesc();
		InitGuildLogo();
		InitGuildRaidState();
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
	}

	public override void Reset()
	{
		base.Reset();
		ClearRanker();
	}

	private void ClearRanker()
	{
		FactoryBase.ReleaseAndClearList(_rankerComponentList);
	}

	private void SetBossDesc()
	{
		if (MainBossInfo_ != null)
		{
			if (!String_Extension_Method.IsNullOrEmpty(MainBossInfo_.BossImgPath))
			{
				AssetTexture assetTexture = new AssetTexture();
				assetTexture.Open(MainBossInfo_.BossImgPath);
				UIHelper.ResetPlaneAssetTextureAsync(this, LastBossImageTexture, assetTexture);
			}
			CampaignWaveData bossWaveData;
			CreatureData bossCreatureData = GameRoot.Instance.CampaignDungeonDataContainer.GetBossCreatureData(MainBossInfo_.ChapterIndex, MainBossInfo_.DungeonIndex, DungeonDifficultyType.Normal, scenario: false, out bossWaveData);
			if (bossCreatureData != null)
			{
				string creatureName = string.Empty;
				string creatureNamePrefix = string.Empty;
				string visualInfo = bossWaveData?.BossVisualInfo;
				GameRoot.Instance.CreatureVisualDataContainer.GetCreatureName(bossCreatureData, visualInfo, LocalizationManager.GetStringDirect, out creatureName, out creatureNamePrefix);
				UIHelper.SetLabelText(LastBossNameLabel, creatureName);
				UIHelper.SetLabelText(LastBossNamePrefixLabel, creatureNamePrefix);
			}
		}
	}

	private void InitGuildLogo()
	{
		if (GlobalAutoBehaviour<GuildManager>.isValidInstance && GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager != null)
		{
			AssetTexture guildLogoIcon = UIHelper.GetGuildLogoIcon2(GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info.Logo);
			UIHelper.ResetPlaneAssetTextureAsync(this, GuildLogoTexture, guildLogoIcon);
			int num = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info.Back;
			if (num <= 0 || num > GameRoot.Instance.ConstantDataContainer.GetValueInt("MaxGuildBG"))
			{
				num = 1;
			}
			AssetTexture guildBackGround2D = UIHelper.GetGuildBackGround2D(num);
			UIHelper.ResetPlaneAssetTextureAsync(this, GuildBackTexture, guildBackGround2D);
		}
	}

	private void InitGuildRaidState()
	{
		UIHelper.EnableGameObject(OpenedRoot, _guildRaidState != GuildRaidState.Ended);
	}

	private void InitProgress()
	{
		int num = 0;
		SetProgress(num);
		Dictionary<int, NShared.GuildRaidDungeonInfo> guildRaidDungeonInfoDic = GlobalAutoBehaviour<GuildManager>.instance.GuildRaidDungeonInfoDic;
		if (__Extension_Method_ICollection__.IsNullOrEmpty(guildRaidDungeonInfoDic))
		{
			return;
		}
		List<NShared.GuildRaidDungeonInfo> list = guildRaidDungeonInfoDic.Values.ToList();
		if (__Extension_Method_ICollection__.IsNullOrEmpty(list))
		{
			return;
		}
		NShared.GuildRaidDungeonInfo guildRaidDungeonInfo = null;
		for (int i = 0; i < list.Count(); i++)
		{
			guildRaidDungeonInfo = list[i];
			if (guildRaidDungeonInfo != null)
			{
				num += Mathf.Max((!guildRaidDungeonInfo.IsDead()) ? (guildRaidDungeonInfo.Step - 1) : guildRaidDungeonInfo.Step, 0);
			}
		}
		SetProgress(num);
	}

	private void SetProgress(int progress)
	{
		float num = Mathf.Clamp01((float)progress / (float)GameRoot.Instance.GuildRaidDungeonDataContainer.TotalStep);
		UIHelper.SetSpriteFillAmount(HpGaugeSprite, num);
		UIHelper.SetLabelText(HpPercentageLabel, Mathf.CeilToInt(num * 100f));
	}

	private void InitRanker()
	{
		UIHelper.EnableGameObject(EmptyRankerRoot, enable: true);
		UserGuildManager myGuildManager = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager;
		if (myGuildManager == null)
		{
			return;
		}
		GuildRaidManager guildRaidManager = myGuildManager.GuildRaidManager;
		if (guildRaidManager == null)
		{
			return;
		}
		IEnumerable<GuildRaidMemberTotalScoreInfo> guildRaidMemberScores = guildRaidManager.GetGuildRaidMemberScores();
		List<GuildRaidMemberTotalScoreInfo> list = new List<GuildRaidMemberTotalScoreInfo>();
		if (guildRaidMemberScores.Count() > 0)
		{
			list.AddRange(guildRaidMemberScores);
			list.Sort((GuildRaidMemberTotalScoreInfo l, GuildRaidMemberTotalScoreInfo r) => r.Score.CompareTo(l.Score));
		}
		NGame2.NGuild.GuildMemberManager memberManager = GlobalAutoBehaviour<GuildManager>.instance.MemberManager;
		if (memberManager == null)
		{
			return;
		}
		ClearRanker();
		int num = 1;
		foreach (GuildRaidMemberTotalScoreInfo item in list)
		{
			GuildMemberInfo guildMemberInfo = memberManager.Get(item.AccountId);
			if (guildMemberInfo != null)
			{
				if (item.AccountId == GlobalAutoBehaviour<AccountManager>.instance.UserInfo.AccountId)
				{
					AssetTexture plainTexture = UIHelper.GetPlainTexture(UIAvatarHelper.GetBattlePortraitPath(guildMemberInfo.AvatarHeroIndex));
					UIHelper.ResetPlaneAssetTextureAsync(this, MyRankerHeroTexture, plainTexture);
					UIHelper.SetLabelText(MyRankerNameLabel, guildMemberInfo.Nick);
					UIHelper.SetLabelText(MyRankerScoreLabel, Vespa_Long_Extension_Method.ToCommaString(item.Score));
				}
				GuildRaidRankerComponent guildRaidRankerComponent = UIHelper.AddGridItem(RankerGrid, GuildRaidRankerComponent.PrefabAuto);
				guildRaidRankerComponent.SetData(item, guildMemberInfo, num, this);
				_rankerComponentList.Add(guildRaidRankerComponent);
				num++;
			}
		}
		RankerGrid.RepositionGrid();
		UIHelper.EnableGameObject(EmptyRankerRoot, __Extension_Method_ICollection__.COUNT(_rankerComponentList) == 0);
	}

	private void InitGuildRaidInitTime()
	{
		string[] valueArray = GameRoot.Instance.ConstantDataContainer.GetValueArray<string>("GlobalGuildRaidApplyTime");
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(valueArray) && valueArray.Length == 4)
		{
			string text = valueArray[0];
			string obj = valueArray[1];
			string text2 = text.Substring(0, 3).ToUpper();
			string stringDirect = LocalizationManager.GetStringDirect("DAY_OF_WEEK_" + text2);
			string[] array = obj.Split(':');
			string text3 = string.Empty;
			string text4 = string.Empty;
			if (array.Length > 1)
			{
				text3 = array[0];
				text4 = array[1];
			}
			UIHelper.SetLabelText(InitTimeLabel, LocalizationManager.GetStringDirect("GUILD_RAID_RENEWAL_TIME ", stringDirect, text3, text4));
		}
	}

	void GuildRaidRankerComponent.Listener.OnClickComponent(long accountId)
	{
		NGame2.NGuild.GuildMemberManager memberManager = GlobalAutoBehaviour<GuildManager>.instance.MemberManager;
		if (memberManager != null)
		{
			GuildMemberInfo guildMemberInfo = memberManager.Get(accountId);
			if (guildMemberInfo != null && guildMemberInfo.AccountId != GlobalAutoBehaviour<AccountManager>.instance.UserInfo.AccountId)
			{
				InspectUserManagement.InspectUserData inspectUser = new InspectUserManagement.InspectUserData(guildMemberInfo, InspectUserCategory.Guild);
				SceneAutoBehaviour<InspectUserManagement>.instance.OpenMenuPopup(inspectUser);
			}
		}
	}

	public void OnClickOpenGuildRaid()
	{
		if (_listener != null)
		{
			_listener.OnClickOpenGuildRaid(this);
		}
	}
}
