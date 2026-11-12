using System;
using System.Collections;
using System.Collections.Generic;
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

public class GuildManage : BaseWindow, GuildSkillComponent.Listener, GuildSkillVoteComponent.Listener, ManageGuildMemberComponent.Listener, GuildLogoEditPopup.Listener
{
	public enum ListType
	{
		GuildMember,
		Waiting,
		Skill,
		All
	}

	public interface Listener
	{
		void LevelUpGuild();

		void InitGuildSkillLevel();

		void BreakGuild();

		void LevelUpSkill(int SkillIndex, byte SkillLevel);

		void ApplyChangeSetting(int _level, string _introduce, int _joinWay, int _logoIdx, int _backIdx);

		void DonateGuild();

		void OnClickManageButton(ManageGuildMemberComponent comp);

		void OnClickAcceptJoin(ManageGuildMemberComponent comp, Action onFail);

		void OnClickRejectJoin(ManageGuildMemberComponent comp, Action onFail);

		void OnClickWithdraw();

		void OnClickVote(int skillIndex);

		void OpenNoticePopup();

		void SendGuildNotice();

		void OnChangeCountryFlag(string guildCountryCode);

		void OnClickGuildFlagSetting();
	}

	private enum SortIndex
	{
		Rank,
		Score,
		Login,
		TeamLevel
	}

	public enum SortType
	{
		Up,
		Down,
		Max
	}

	[Serializable]
	public class SortComponent
	{
		private SortType _sortMethod;

		public UISprite SpriteUp;

		public UISprite SpriteDown;

		public UIButton Button_Sort;

		public void SetSprite(SortType sortType)
		{
			UIHelper.EnableGameObject(SpriteUp, sortType == SortType.Up);
			UIHelper.EnableGameObject(SpriteDown, sortType == SortType.Down);
		}

		public void SetEnable(bool isEnabled)
		{
			UIHelper.EnableGameObject(Button_Sort, isEnabled);
		}
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Guild/GuildManage";

	private Listener _listener;

	public UIToggle Toggle_Inform;

	public UIToggle Toggle_Skill;

	public UIToggle Toggle_Manage;

	public UIToggle Toggle_Setting;

	public UIToggle Toggle_Member;

	public UIToggle Toggle_Waiting;

	public UIWidget Widget_Inform;

	public UIWidget Widget_Skill;

	public UIWidget Widget_Manage;

	public UIWidget Widget_Setting;

	public UIWidget Widget_ManageMember;

	public UIScrollGrid Grid_Member;

	public UIScrollGrid Grid_Skill;

	public UIScrollGrid Grid_ManageMember;

	public UIScrollGrid Grid_SkillVote;

	public UIButton Button_BreakGuild;

	public UILabel Label_Level;

	public UILabel Label_Name;

	public UILabel Label_MasterNick;

	public UILabel Label_GuildPoint;

	public UILabel Label_NoticeContent_Master;

	public UILabel Label_NoticeContent_Member;

	public UILabel Label_GuildIntroduce;

	public UILabel Label_MyRank;

	public UILabel[] Label_DonationGem;

	public UILabel[] Label_DonationPoint;

	public UILabel Label_DonationChance;

	public UIButton Button_Donate;

	public UIButton Button_LevelUp;

	public UIButton Button_Withdraw;

	public UIClipSprite Sprite_DonateGauge;

	public UITexture Texture_GuildLogo;

	public UILabel Label_NumMember;

	public UILabel Label_TrainingCenter;

	public UILabel Label_GuildPoint_Skill;

	public UILabel Label_SuppressPoint;

	public UILabel Label_InitSkill;

	public UIButton Button_InitSkill;

	public UIInput Input_GuildIntroduce;

	public UILabel Label_GuildIntroduceEdit;

	public UILabel Label_GuildLevelLimit;

	public UIButton Button_AddLevel;

	public UIButton Button_SubtractLevel;

	public UILabel Label_WaitingJoin;

	public UIToggle Toggle_JoinFree;

	public UIToggle Toggle_JoinAccept;

	public UIButton Button_ChangeCountryFlag;

	public UITexture Texture_CountryFlag;

	public UIScrollGrid Grid_GuildLogoIcon;

	public GameObject NoticeUI_Master;

	public GameObject NoticeUI_Member;

	public UIButton Button_SendGuildNotice;

	public UIButton Button_ChangeSetting;

	public UIWidget WidgetManageMemberToggle;

	public UIWidget WidgetWaitingMemberToggle;

	public UIWidget WidgetManageMemberGrid;

	public UIToggle Toggle_SortRank;

	public UIToggle Toggle_SortScore;

	public UIToggle Toggle_SortLogin;

	public UIToggle Toggle_SortWaitingLevel;

	public UIToggle Toggle_SortWaitingLogin;

	public SortComponent CompSortRank;

	public SortComponent CompSortScore;

	public SortComponent CompSortLogin;

	public SortComponent CompSortWaitingLevel;

	public SortComponent CompSortWaitingLogin;

	private List<ManageGuildMemberComponent> _ManageMemberList = new List<ManageGuildMemberComponent>();

	private List<GuildMemberInfoComponent> _MemberInfoList = new List<GuildMemberInfoComponent>();

	private List<GuildSkillComponent> _SkillList = new List<GuildSkillComponent>();

	private List<GuildSkillVoteComponent> _SkillVoteList = new List<GuildSkillVoteComponent>();

	public UITexture Texture_GuildLogo_Setting;

	public UITexture Texture_GuildBG_Setting;

	private int _joinLevel;

	private EGuildJoinWay _joinWay;

	private int _guildFlagIndex;

	private int _guildLogoIndex;

	private int _guildBackIndex;

	private bool _skillResetCoolElapsed;

	private float _lastUpdateSkillResetTime;

	private float SendNoticeTime = 5f;

	private string _guildCountryCode = "VESPA";

	private SortType _rankSortType;

	private SortType _scoreSortType;

	private SortType _loginSortType;

	private SortType _waitingLevelSortType;

	private SortType _waitingLoginSortType;

	private SortIndex _sortIndex;

	private SortIndex _waitingSortIndex;

	private SortType _sortType;

	private SortType _waitingSortType;

	[HideInInspector]
	public int GuildFlagIndex => _guildFlagIndex;

	[HideInInspector]
	public int GuildLogoIndex => _guildLogoIndex;

	[HideInInspector]
	public int GuildBackIndex => _guildBackIndex;

	public void Init(Listener listener)
	{
		Reset();
		_listener = listener;
		Input_GuildIntroduce.characterLimit = GameRoot.Instance.ConstantDataContainer.GetValueInt("MaxGuildIntroduceLength");
		Toggle_Inform.Set(state: true);
		RefreshBaseUI(bInit: true);
	}

	public override void Awake()
	{
		base.Awake();
		_lastUpdateSkillResetTime = Time.realtimeSinceStartup;
	}

	private void SetNumGuildMember()
	{
		int num = GlobalAutoBehaviour<GuildManager>.instance.MemberManager.Count();
		int num2 = GameRoot.Instance.GuildLevelDataContainer.GetData(GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info.Level)?.MaxMember ?? 0;
		UIHelper.SetLabelText(Label_NumMember, num + "/" + num2);
	}

	private void RefreshInform(GuildMemberInfo myInfo, bool bInit)
	{
		GuildMemberInfo guildMemberInfo = ((GlobalAutoBehaviour<GuildManager>.instance.MemberManager != null) ? GlobalAutoBehaviour<GuildManager>.instance.MemberManager.GetMasterMemberInfo() : null);
		string text = ((guildMemberInfo != null) ? guildMemberInfo.Nick : "Unknown");
		UIHelper.SetLabelText(Label_Level, GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info.Level);
		UIHelper.SetLabelText(Label_Name, GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info.Name);
		UIHelper.SetLabelText(Label_GuildPoint, GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info.ActivityPoint);
		UIHelper.SetLabelText(Label_MasterNick, text);
		UIHelper.SetLabelText(Label_GuildIntroduce, GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info.Introduce);
		UIHelper.SetLabelText(Label_MyRank, ManageGuildMemberComponent.GetGuildRankString(myInfo.Rank));
		AssetTexture guildLogoIcon = UIHelper.GetGuildLogoIcon2(GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info.Logo);
		UIHelper.ResetPlaneAssetTextureAsync(this, Texture_GuildLogo, guildLogoIcon);
		UIHelper.EnableGameObject(Button_Withdraw, myInfo.Rank != 1);
		UIHelper.EnableGameObject(Button_LevelUp, myInfo.Rank == 1);
		IList<GuildContributeRewardData> allData = GameRoot.Instance.GuildContributeRewardDataContainer.GetAllData();
		int contributedGem = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info.ContributedGem;
		int contributedGem2 = allData[__Extension_Method_ICollection__.COUNT(allData) - 1].ContributedGem;
		Sprite_DonateGauge.ClipFillAmount = Mathf.Clamp((float)contributedGem / (float)contributedGem2, 0f, 1f);
		string stringDirect = LocalizationManager.GetStringDirect("GUILD_DONATION_CHANCE", myInfo.ContributeCount);
		UIHelper.SetLabelText(Label_DonationChance, stringDirect);
		UIHelper.EnableButton(Button_Donate, myInfo.ContributeCount > 0 && contributedGem < contributedGem2);
		if (bInit)
		{
			UpdateGuildNotice(GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info.Notice);
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(allData); i++)
			{
				UILabel label = Label_DonationGem[i];
				int contributedGem3 = allData[i].ContributedGem;
				UIHelper.SetLabelText(label, Vespa_Int_Extension_Method.ToCommaString(contributedGem3));
				UILabel label2 = Label_DonationPoint[i];
				int rewardGuildActivityPoint = allData[i].RewardGuildActivityPoint;
				UIHelper.SetLabelText(label2, "+" + Vespa_Int_Extension_Method.ToCommaString(rewardGuildActivityPoint) + "pt");
			}
			LoadMemberInfoList();
		}
		else
		{
			RefreshMemberInfoList();
		}
	}

	private void RefreshSkill(GuildMemberInfo myInfo, bool bInit)
	{
		MyGuildInfo info = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info;
		if (info != null)
		{
			GuildBuildingInfo guildBuildingInfo = info.GetGuildBuildingInfo(EGuildBuildingType.TraningCenter);
			if (guildBuildingInfo != null)
			{
				UIHelper.SetLabelText(Label_TrainingCenter, LocalizationManager.GetStringDirect("GUILD_TRAINING_CENTER_LEVEL", guildBuildingInfo.BuildingLevel));
			}
			UIHelper.SetLabelText(Label_GuildPoint_Skill, info.ActivityPoint);
			UIHelper.SetLabelText(Label_SuppressPoint, info.SuppressPoint);
			if (bInit)
			{
				LoadSkillList();
			}
		}
	}

	private void RefreshManage(GuildMemberInfo myInfo, bool bInit)
	{
		if (bInit)
		{
			OnChangeManageToggle();
		}
		else if (Toggle_Member.value)
		{
			RefreshManageMember(bInit: false);
		}
		else if (Toggle_Waiting.value)
		{
			RefreshWaitingMember(bInit: false);
		}
		else if (Toggle_Setting.value)
		{
			RefreshSetting(myInfo, bInit: true);
		}
		UpdateWaitingMemberLabel();
		bool flag = myInfo.Rank == 1;
		UIHelper.EnableGameObject(Button_BreakGuild, flag);
		if (flag)
		{
			UIHelper.EnableButton(Button_BreakGuild, GlobalAutoBehaviour<GuildManager>.instance.MemberManager.Count() == 1);
		}
	}

	private void RefreshManageMember(bool bInit)
	{
		if (Toggle_SortRank.value || Toggle_SortScore.value || Toggle_SortLogin.value)
		{
			CompSortRank.SetEnable(Toggle_SortRank.value);
			CompSortRank.SetSprite(_rankSortType);
			CompSortScore.SetEnable(Toggle_SortScore.value);
			CompSortScore.SetSprite(_scoreSortType);
			CompSortLogin.SetEnable(Toggle_SortLogin.value);
			CompSortLogin.SetSprite(_loginSortType);
			if (Toggle_SortRank.value)
			{
				_sortIndex = SortIndex.Rank;
				_sortType = _rankSortType;
			}
			else if (Toggle_SortScore.value)
			{
				_sortIndex = SortIndex.Score;
				_sortType = _scoreSortType;
			}
			else if (Toggle_SortLogin.value)
			{
				_sortIndex = SortIndex.Login;
				_sortType = _loginSortType;
			}
			if (bInit)
			{
				LoadManageMemberList();
			}
			else
			{
				UIHelper.RepositionGrid(Grid_ManageMember);
			}
		}
	}

	private void UpdateWaitingMemberLabel()
	{
		int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("MaxRecvJoinGuildRequest");
		int num = ((GlobalAutoBehaviour<GuildManager>.instance.JoinRequestPlayerManager != null) ? GlobalAutoBehaviour<GuildManager>.instance.JoinRequestPlayerManager.Count() : 0);
		UIHelper.SetLabelText(Label_WaitingJoin, LocalizationManager.GetStringDirect("GUILD_JOINWAIT", num, valueInt));
	}

	private void RefreshWaitingMember(bool bInit)
	{
		if (Toggle_SortWaitingLevel.value || Toggle_SortWaitingLogin.value)
		{
			CompSortWaitingLevel.SetEnable(Toggle_SortWaitingLevel.value);
			CompSortWaitingLevel.SetSprite(_waitingLevelSortType);
			CompSortWaitingLogin.SetEnable(Toggle_SortWaitingLogin.value);
			CompSortWaitingLogin.SetSprite(_waitingLoginSortType);
			if (Toggle_SortWaitingLevel.value)
			{
				_waitingSortIndex = SortIndex.TeamLevel;
				_waitingSortType = _waitingLevelSortType;
			}
			else if (Toggle_SortWaitingLogin.value)
			{
				_waitingSortIndex = SortIndex.Login;
				_waitingSortType = _waitingLoginSortType;
			}
			if (bInit)
			{
				LoadWaitingList();
			}
			else
			{
				UIHelper.RepositionGrid(Grid_ManageMember);
			}
		}
	}

	private void RefreshSetting(GuildMemberInfo myInfo, bool bInit)
	{
		UIHelper.EnableButton(Button_AddLevel, myInfo.Rank == 1);
		UIHelper.EnableButton(Button_SubtractLevel, myInfo.Rank == 1);
		UIHelper.EnableButton(Button_ChangeCountryFlag, myInfo.Rank == 1);
		UIHelper.EnableGameObject(Input_GuildIntroduce, myInfo.Rank == 1);
		if (bInit || myInfo.Rank != 1)
		{
			UIHelper.SetLabelText(Label_GuildIntroduceEdit, GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info.Introduce);
			_joinLevel = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info.ReqTeamLevel;
			SetLevel(_joinLevel);
			Toggle_JoinAccept.Set(GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info.JoinWay == 2);
			Toggle_JoinFree.Set(GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info.JoinWay == 1);
			UIHelper.EnableButton(Button_ChangeSetting, enable: false);
			_guildFlagIndex = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info.Flag;
			_guildLogoIndex = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info.Logo;
			if (_guildLogoIndex <= 0 || _guildLogoIndex > GameRoot.Instance.ConstantDataContainer.GetValueInt("MaxGuildIcon"))
			{
				_guildLogoIndex = 1;
			}
			_guildCountryCode = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info.CountryCode;
			if (String_Extension_Method.IsNullOrEmpty(_guildCountryCode))
			{
				_guildCountryCode = "VESPA";
			}
			_guildBackIndex = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info.Back;
			if (_guildBackIndex <= 0 || _guildBackIndex > GameRoot.Instance.ConstantDataContainer.GetValueInt("MaxGuildBG"))
			{
				_guildBackIndex = 1;
			}
		}
		UpdateSelectedGuildLogo();
		UpdateCountryFlag();
	}

	public void RefreshBaseUI(bool bInit = false)
	{
		GuildMemberInfo myInfo = GlobalAutoBehaviour<GuildManager>.instance.MemberManager.GetMyMemberInfo();
		if (myInfo == null)
		{
			return;
		}
		if (myInfo.Rank <= 2 && GlobalAutoBehaviour<GuildManager>.instance.JoinRequestPlayerManager == null)
		{
			SceneAutoBehaviour<GuildManagement2>.instance.GetJoinRequestPlayers(delegate
			{
				RefreshBaseUIInternal(myInfo, bInit);
			});
		}
		else
		{
			SetNumGuildMember();
			RefreshBaseUIInternal(myInfo, bInit);
		}
	}

	private void RefreshBaseUIInternal(GuildMemberInfo myInfo, bool bInit)
	{
		if (myInfo.Rank > 2 && Toggle_Manage.value)
		{
			Toggle_Inform.Set(state: true);
		}
		if (myInfo.Rank > 1 && Toggle_Manage.value && Toggle_Setting.value)
		{
			Toggle_Member.Set(state: true);
		}
		UIHelper.EnableGameObject(Toggle_Manage, myInfo.Rank <= 2);
		UIHelper.EnableGameObject(Toggle_Setting, myInfo.Rank == 1);
		if (Toggle_Inform.value)
		{
			RefreshInform(myInfo, bInit);
		}
		else if (Toggle_Skill.value)
		{
			RefreshSkill(myInfo, bInit);
		}
		else if (Toggle_Manage.value)
		{
			RefreshManage(myInfo, bInit);
		}
	}

	public override void Reset()
	{
		base.Reset();
		FactoryBase.ReleaseAndClearList(_SkillList);
		FactoryBase.ReleaseAndClearList(_SkillVoteList);
		FactoryBase.ReleaseAndClearList(_MemberInfoList);
		FactoryBase.ReleaseAndClearList(_ManageMemberList);
		_rankSortType = (_scoreSortType = (_loginSortType = SortType.Up));
		_waitingLevelSortType = (_waitingLoginSortType = SortType.Up);
	}

	public void UpdateGuildNotice(string notice)
	{
		GuildMemberInfo myMemberInfo = GlobalAutoBehaviour<GuildManager>.instance.MemberManager.GetMyMemberInfo();
		if (myMemberInfo != null)
		{
			bool flag = myMemberInfo.Rank == 1;
			if (!flag)
			{
				_ = Label_NoticeContent_Member;
			}
			else
			{
				_ = Label_NoticeContent_Master;
			}
			UIHelper.EnableGameObject(NoticeUI_Master, flag);
			UIHelper.EnableGameObject(NoticeUI_Member, !flag);
			UIHelper.SetLabelText(Label_NoticeContent_Master, notice);
			UIHelper.SetLabelText(Label_NoticeContent_Member, notice);
		}
	}

	private void UpdateSkillList()
	{
		if (_SkillList == null)
		{
			return;
		}
		List<GuildSkillLevelData> guildSkillLevelDataList = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GetGuildSkillLevelDataList();
		foreach (GuildSkillComponent comp in _SkillList)
		{
			GuildSkillLevelData skillLevelData = guildSkillLevelDataList.Find((GuildSkillLevelData e) => e.GuildSkillIndex == comp.SkillData.GuildSkillIndex);
			comp.RefreshLevel(skillLevelData);
		}
	}

	private void UpdateSkillVoteList()
	{
		if (_SkillVoteList == null)
		{
			return;
		}
		foreach (GuildSkillVoteComponent skillVote in _SkillVoteList)
		{
			skillVote.Refresh();
		}
	}

	private void UpdateMemberInfoList()
	{
		if (_MemberInfoList == null)
		{
			return;
		}
		foreach (GuildMemberInfoComponent memberInfo in _MemberInfoList)
		{
			memberInfo.Refresh();
		}
	}

	private void UpdateManageMemberList()
	{
		if (_ManageMemberList == null)
		{
			return;
		}
		foreach (ManageGuildMemberComponent manageMember in _ManageMemberList)
		{
			manageMember.Refresh();
		}
	}

	private bool ReloadNeededForMemberInfoList()
	{
		if (GlobalAutoBehaviour<GuildManager>.instance.MemberManager.Count() != _MemberInfoList.Count)
		{
			return true;
		}
		foreach (GuildMemberInfoComponent memberInfo in _MemberInfoList)
		{
			if (memberInfo.MemberInfo == null || GlobalAutoBehaviour<GuildManager>.instance.MemberManager.Get(memberInfo.MemberInfo.AccountId) == null)
			{
				return true;
			}
		}
		return false;
	}

	private bool ReloadNeededForManageMemberList()
	{
		HashSet<long> hashSet = new HashSet<long>();
		foreach (GuildMemberInfo item in GlobalAutoBehaviour<GuildManager>.instance.MemberManager)
		{
			if (item.Rank != 1)
			{
				hashSet.Add(item.AccountId);
			}
		}
		if (hashSet.Count != _ManageMemberList.Count)
		{
			return true;
		}
		foreach (ManageGuildMemberComponent manageMember in _ManageMemberList)
		{
			if (manageMember.MemberInfo == null || !hashSet.Contains(manageMember.MemberInfo.AccountId))
			{
				return true;
			}
		}
		return false;
	}

	private bool ReloadNeededForWaitingList()
	{
		if (GlobalAutoBehaviour<GuildManager>.instance.JoinRequestPlayerManager.Count() != _ManageMemberList.Count)
		{
			return true;
		}
		foreach (ManageGuildMemberComponent manageMember in _ManageMemberList)
		{
			if (manageMember.WaitingInfo == null || GlobalAutoBehaviour<GuildManager>.instance.JoinRequestPlayerManager.Get(manageMember.WaitingInfo.AccountId) == null)
			{
				return true;
			}
		}
		return false;
	}

	private void RefreshMemberInfoList()
	{
		GuildMemberInfo myMemberInfo = GlobalAutoBehaviour<GuildManager>.instance.MemberManager.GetMyMemberInfo();
		if (myMemberInfo != null)
		{
			UIHelper.SetLabelText(Label_MyRank, ManageGuildMemberComponent.GetGuildRankString(myMemberInfo.Rank));
		}
		if (ReloadNeededForMemberInfoList())
		{
			LoadMemberInfoList();
		}
		else
		{
			UpdateMemberInfoList();
		}
	}

	private void RefreshManageMemberList()
	{
		if (ReloadNeededForManageMemberList())
		{
			LoadManageMemberList();
		}
		else
		{
			UpdateManageMemberList();
		}
	}

	private void RefreshWaitingList()
	{
		if (ReloadNeededForWaitingList())
		{
			LoadWaitingList();
			UpdateWaitingMemberLabel();
		}
		else
		{
			UpdateManageMemberList();
		}
	}

	public void RefreshCurList(ListType listType)
	{
		if (Toggle_Inform.value)
		{
			if (listType == ListType.GuildMember || listType == ListType.All)
			{
				RefreshMemberInfoList();
			}
		}
		else if (Toggle_Skill.value)
		{
			if (listType == ListType.Skill || listType == ListType.All)
			{
				UpdateSkillList();
				UpdateSkillVoteList();
			}
		}
		else
		{
			if (!Toggle_Manage.value)
			{
				return;
			}
			if (Toggle_Member.value)
			{
				if (listType == ListType.GuildMember || listType == ListType.All)
				{
					RefreshManageMemberList();
				}
			}
			else if (Toggle_Waiting.value && (listType == ListType.Waiting || listType == ListType.All))
			{
				RefreshWaitingList();
			}
		}
	}

	private void LoadMemberInfoList()
	{
		FactoryBase.ReleaseAndClearList(_MemberInfoList);
		Grid_Member.Grid.sorting = UIGrid.Sorting.Custom;
		Grid_Member.Grid.onCustomSort = (Transform x, Transform y) => SortBase<GuildMemberInfoComponent>(x, y, SortGuildMemberInfo);
		IEnumerator<GuildMemberInfo> enumerator = GlobalAutoBehaviour<GuildManager>.instance.MemberManager.GetEnumerator();
		while (enumerator.MoveNext())
		{
			GuildMemberInfoComponent guildMemberInfoComponent = UIHelper.AddGridItem(Grid_Member, GuildMemberInfoComponent.PrefabAuto);
			guildMemberInfoComponent.Open(enumerator.Current);
			_MemberInfoList.Add(guildMemberInfoComponent);
		}
		UIHelper.RepositionGrid(Grid_Member);
	}

	private void LoadManageMemberList()
	{
		FactoryBase.ReleaseAndClearList(_ManageMemberList);
		Grid_ManageMember.Grid.sorting = UIGrid.Sorting.Custom;
		Grid_ManageMember.Grid.onCustomSort = (Transform x, Transform y) => SortBase<ManageGuildMemberComponent>(x, y, SortManageMemberInfo);
		IEnumerator<GuildMemberInfo> enumerator = GlobalAutoBehaviour<GuildManager>.instance.MemberManager.GetEnumerator();
		while (enumerator.MoveNext())
		{
			if (enumerator.Current.Rank != 1)
			{
				ManageGuildMemberComponent manageGuildMemberComponent = UIHelper.AddGridItem(Grid_ManageMember, ManageGuildMemberComponent.PrefabAuto);
				manageGuildMemberComponent.Open(enumerator.Current, this);
				_ManageMemberList.Add(manageGuildMemberComponent);
			}
		}
		UIHelper.RepositionGrid(Grid_ManageMember);
	}

	private void LoadWaitingList()
	{
		FactoryBase.ReleaseAndClearList(_ManageMemberList);
		Grid_ManageMember.Grid.sorting = UIGrid.Sorting.Custom;
		Grid_ManageMember.Grid.onCustomSort = (Transform x, Transform y) => SortBase<ManageGuildMemberComponent>(x, y, SortWaitingMemberInfo);
		IEnumerator<GuildJoinRequestPlayerInfo> enumerator = GlobalAutoBehaviour<GuildManager>.instance.JoinRequestPlayerManager.GetEnumerator();
		while (enumerator.MoveNext())
		{
			ManageGuildMemberComponent manageGuildMemberComponent = UIHelper.AddGridItem(Grid_ManageMember, ManageGuildMemberComponent.PrefabAutoWaiting);
			manageGuildMemberComponent.Open(enumerator.Current, this);
			_ManageMemberList.Add(manageGuildMemberComponent);
		}
		UIHelper.RepositionGrid(Grid_ManageMember);
	}

	private void LoadSkillList()
	{
		FactoryBase.ReleaseAndClearList(_SkillList);
		foreach (GuildSkillLevelData guildSkillLevelData in GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GetGuildSkillLevelDataList())
		{
			GuildSkillData data = GameRoot.Instance.GuildSkillDataContainer.GetData(guildSkillLevelData.GuildSkillIndex);
			GuildSkillComponent guildSkillComponent = UIHelper.AddGridItem(Grid_Skill, GuildSkillComponent.PrefabAuto);
			guildSkillComponent.Open(this, data, guildSkillLevelData);
			_SkillList.Add(guildSkillComponent);
		}
		UIHelper.RepositionGrid(Grid_Skill);
	}

	private void LoadSkillVoteList()
	{
		FactoryBase.ReleaseAndClearList(_SkillVoteList);
		IList<GuildSkillData> allData = GameRoot.Instance.GuildSkillDataContainer.GetAllData();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(allData); i++)
		{
			GuildSkillVoteComponent guildSkillVoteComponent = UIHelper.AddGridItem(Grid_SkillVote, GuildSkillVoteComponent.PrefabAuto);
			guildSkillVoteComponent.Open(this, allData[i]);
			_SkillVoteList.Add(guildSkillVoteComponent);
		}
		UIHelper.RepositionGrid(Grid_SkillVote);
	}

	public bool IsWaitingPanelEnabled()
	{
		if (IsOpen() && Toggle_Manage.value)
		{
			return Toggle_Waiting.value;
		}
		return false;
	}

	private void UpdateCountryFlag()
	{
		CountryCodeData dataByCode = GameRoot.Instance.CountryCodeDataContainer.GetDataByCode(_guildCountryCode);
		if (dataByCode != null)
		{
			AssetTexture plainTexture = UIHelper.GetPlainTexture(dataByCode.IconPath);
			UIHelper.ResetPlaneAssetTextureAsync(this, Texture_CountryFlag, plainTexture);
		}
	}

	public void OnChangeToggle()
	{
		if (!Toggle_Inform.value && !Toggle_Skill.value && !Toggle_Manage.value)
		{
			return;
		}
		GuildMemberInfo myMemberInfo = GlobalAutoBehaviour<GuildManager>.instance.MemberManager.GetMyMemberInfo();
		if (myMemberInfo != null)
		{
			if (Toggle_Inform.value)
			{
				RefreshInform(myMemberInfo, bInit: true);
			}
			if (Toggle_Skill.value)
			{
				RefreshSkill(myMemberInfo, bInit: true);
			}
			if (Toggle_Manage.value)
			{
				RefreshManage(myMemberInfo, bInit: true);
			}
			UIHelper.EnableGameObject(Widget_Inform, Toggle_Inform.value);
			UIHelper.EnableGameObject(Widget_Skill, Toggle_Skill.value);
			UIHelper.EnableGameObject(Widget_Manage, Toggle_Manage.value);
		}
	}

	public void OnChangeManageToggle()
	{
		if (!Toggle_Setting.value && !Toggle_Member.value && !Toggle_Waiting.value)
		{
			return;
		}
		GuildMemberInfo myMemberInfo = GlobalAutoBehaviour<GuildManager>.instance.MemberManager.GetMyMemberInfo();
		if (myMemberInfo != null)
		{
			if (Toggle_Setting.value)
			{
				RefreshSetting(myMemberInfo, bInit: true);
			}
			if (Toggle_Member.value)
			{
				RefreshManageMember(bInit: true);
			}
			if (Toggle_Waiting.value)
			{
				RefreshWaitingMember(bInit: true);
			}
			UIHelper.EnableGameObject(WidgetManageMemberToggle, Toggle_Member.value);
			UIHelper.EnableGameObject(WidgetWaitingMemberToggle, Toggle_Waiting.value);
			UIHelper.EnableGameObject(WidgetManageMemberGrid, Toggle_Member.value || Toggle_Waiting.value);
			UIHelper.EnableGameObject(Widget_Setting, Toggle_Setting.value);
		}
	}

	public void OnClickLevelUpGuild()
	{
		_listener.LevelUpGuild();
	}

	public void OnClickBreakGuild()
	{
		_listener.BreakGuild();
	}

	public void OnClickInitGuildSkillLevel()
	{
		_listener.InitGuildSkillLevel();
	}

	public void OnClickApplySetting()
	{
		UIHelper.EnableButton(Button_ChangeSetting, enable: false);
		if (int.TryParse(Label_GuildLevelLimit.text, out var result))
		{
			_listener.ApplyChangeSetting(result, Label_GuildIntroduceEdit.text, (int)_joinWay, _guildLogoIndex, _guildBackIndex);
		}
	}

	public void OnClickGuildFlagSetting()
	{
		if (_listener != null)
		{
			_listener.OnClickGuildFlagSetting();
		}
	}

	public void OnClickApplyNotice()
	{
		if (_listener != null)
		{
			_listener.OpenNoticePopup();
		}
	}

	public void OnClickSendNotice()
	{
		if (_listener != null)
		{
			_listener.SendGuildNotice();
			ExtensionMethod_VespaCoroutine.StartCoroutineThis(this, CheckSendGuildNoticeButton());
		}
	}

	private IEnumerator CheckSendGuildNoticeButton()
	{
		UIHelper.EnableButton(Button_SendGuildNotice, enable: false, grayscaleIfDisable: true);
		float time = Time.time;
		while (true)
		{
			float num = Time.time - time;
			Mathf.Ceil(SendNoticeTime - num);
			if (num >= SendNoticeTime)
			{
				break;
			}
			yield return null;
		}
		UIHelper.EnableButton(Button_SendGuildNotice, enable: true, grayscaleIfDisable: true);
	}

	private void CheckApplySettingButton()
	{
		bool flag = _joinLevel != GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info.ReqTeamLevel;
		flag |= (byte)_joinWay != GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info.JoinWay;
		flag |= _guildLogoIndex != GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info.Logo;
		flag |= _guildBackIndex != GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info.Back;
		flag |= string.Compare(Label_GuildIntroduceEdit.text, GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info.Introduce) != 0;
		UIHelper.EnableButton(Button_ChangeSetting, flag);
	}

	private void SetLevel(int newLevel)
	{
		int teamLevel = GlobalAutoBehaviour<AccountManager>.instance.UserManager.TeamLevel;
		if (newLevel >= 1 && newLevel <= teamLevel)
		{
			_joinLevel = newLevel;
			UIHelper.SetLabelText(Label_GuildLevelLimit, _joinLevel.ToString());
			UIHelper.EnableButton(Button_AddLevel, _joinLevel < teamLevel);
			UIHelper.EnableButton(Button_SubtractLevel, _joinLevel > 1);
			GuildMemberInfo myMemberInfo = GlobalAutoBehaviour<GuildManager>.instance.MemberManager.GetMyMemberInfo();
			if (myMemberInfo != null && myMemberInfo.Rank == 1)
			{
				CheckApplySettingButton();
			}
		}
	}

	public void SetGuildCountryCode(string countryCode)
	{
		_guildCountryCode = countryCode;
		UpdateCountryFlag();
	}

	public void OnClickAddLevel()
	{
		SetLevel(_joinLevel + 1);
	}

	public void OnClickSubtractLevel()
	{
		SetLevel(_joinLevel - 1);
	}

	public void OnClickWithdraw()
	{
		string stringDirect = LocalizationManager.GetStringDirect("GUILD_WITHDRAW");
		string stringDirect2 = LocalizationManager.GetStringDirect("GUILD_WITHDRAW_POPUP_MESSAGE", GetRestrictHour());
		MessagePopup.Open(stringDirect, stringDirect2, delegate
		{
			_listener.OnClickWithdraw();
		}, null);
	}

	private int GetRestrictHour()
	{
		int guildWithdrawCount = GlobalAutoBehaviour<AccountManager>.instance.UserManager.MiscInfo.GuildWithdrawCount;
		return GameRoot.Instance.GuildPenaltyDataContainer.GetRestrictHour(guildWithdrawCount + 1);
	}

	public void OnChangeIntroduceInput()
	{
		GuildMemberInfo myMemberInfo = GlobalAutoBehaviour<GuildManager>.instance.MemberManager.GetMyMemberInfo();
		if (myMemberInfo != null && myMemberInfo.Rank == 1)
		{
			CheckApplySettingButton();
		}
	}

	public void OnChangeJoinWayToggle()
	{
		if (Toggle_JoinFree.value)
		{
			_joinWay = EGuildJoinWay.FreeJoin;
		}
		else if (Toggle_JoinAccept.value)
		{
			_joinWay = EGuildJoinWay.AcceptJoin;
		}
		GuildMemberInfo myMemberInfo = GlobalAutoBehaviour<GuildManager>.instance.MemberManager.GetMyMemberInfo();
		if (myMemberInfo != null && myMemberInfo.Rank == 1)
		{
			CheckApplySettingButton();
		}
	}

	public void OnChangeSortToggle()
	{
		if (Toggle_SortRank.value)
		{
			RefreshManageMember(bInit: false);
		}
		else if (Toggle_SortScore.value)
		{
			RefreshManageMember(bInit: false);
		}
		else if (Toggle_SortLogin.value)
		{
			RefreshManageMember(bInit: false);
		}
	}

	public void OnChangeWaitingSortToggle()
	{
		if (Toggle_SortWaitingLevel.value)
		{
			RefreshWaitingMember(bInit: false);
		}
		else if (Toggle_SortWaitingLogin.value)
		{
			RefreshWaitingMember(bInit: false);
		}
	}

	private void ToggleSortButton(ref SortType sortType)
	{
		sortType++;
		if (sortType == SortType.Max)
		{
			sortType = SortType.Up;
		}
	}

	public void OnClickRankSortButton()
	{
		ToggleSortButton(ref _rankSortType);
		RefreshManageMember(bInit: false);
	}

	public void OnClickScoreSortButton()
	{
		ToggleSortButton(ref _scoreSortType);
		RefreshManageMember(bInit: false);
	}

	public void OnClickLoginSortButton()
	{
		ToggleSortButton(ref _loginSortType);
		RefreshManageMember(bInit: false);
	}

	public void OnClickWaitingLevelSortButton()
	{
		ToggleSortButton(ref _waitingLevelSortType);
		RefreshWaitingMember(bInit: false);
	}

	public void OnClickWaitingLoginSortButton()
	{
		ToggleSortButton(ref _waitingLoginSortType);
		RefreshWaitingMember(bInit: false);
	}

	public void OnClickDonationButton()
	{
		IList<GuildContributeRewardData> allData = GameRoot.Instance.GuildContributeRewardDataContainer.GetAllData();
		int contributedGem = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info.ContributedGem;
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(allData); i++)
		{
			if (contributedGem < allData[i].ContributedGem)
			{
				num = allData[i].ContributedGem - contributedGem;
				num2 = allData[i].RewardGuildActivityPoint;
				break;
			}
		}
		int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("ReqGemContributeGuild");
		string stringDirect = LocalizationManager.GetStringDirect("GUILD_DONATION");
		string stringDirect2 = LocalizationManager.GetStringDirect("GUILD_DONATION_POPUP_CONTENT", valueInt, num, num2);
		string stringDirect3 = LocalizationManager.GetStringDirect("GUILD_DONATION_POPUP_YES");
		Action onYes = delegate
		{
			_listener.DonateGuild();
		};
		long totalGem = GlobalAutoBehaviour<AccountManager>.instance.UserManager.TotalGem;
		int num3 = valueInt;
		PreserveMessagePopup2.OpenEnabled(stringDirect, stringDirect2, stringDirect3, PreserveMessagePopup2.EPriceType.Gem, valueInt, onYes, totalGem >= num3);
	}

	private void UpdateSelectedGuildLogo()
	{
		AssetTexture guildLogoIcon = UIHelper.GetGuildLogoIcon2(_guildLogoIndex);
		UIHelper.ResetPlaneAssetTextureAsync(this, Texture_GuildLogo_Setting, guildLogoIcon);
		AssetTexture guildBackGround2D = UIHelper.GetGuildBackGround2D(_guildBackIndex);
		UIHelper.ResetPlaneAssetTextureAsync(this, Texture_GuildBG_Setting, guildBackGround2D);
	}

	public void OnClickChangeCountryFlag()
	{
		if (_listener != null)
		{
			_listener.OnChangeCountryFlag(_guildCountryCode);
		}
	}

	private int SortGuildMemberInfo(GuildMemberInfoComponent component1, GuildMemberInfoComponent component2)
	{
		GuildMemberInfo memberInfo = component1.MemberInfo;
		GuildMemberInfo memberInfo2 = component2.MemberInfo;
		if (memberInfo == null || memberInfo2 == null)
		{
			if (memberInfo != null)
			{
				return -1;
			}
			if (memberInfo2 != null)
			{
				return 1;
			}
			return 0;
		}
		if (memberInfo.AccountId == memberInfo2.AccountId)
		{
			return 0;
		}
		if (memberInfo.AccountId == GlobalAutoBehaviour<AccountManager>.instance.UserInfo.AccountId)
		{
			return -1;
		}
		if (memberInfo2.AccountId == GlobalAutoBehaviour<AccountManager>.instance.UserInfo.AccountId)
		{
			return 1;
		}
		int num = 0;
		num = memberInfo.Rank.CompareTo(memberInfo2.Rank);
		if (num != 0)
		{
			return num;
		}
		num = -memberInfo.AccActivityPoint.CompareTo(memberInfo2.AccActivityPoint);
		if (num != 0)
		{
			return num;
		}
		return memberInfo.Nick.CompareTo(memberInfo2.Nick);
	}

	private int SortWaitingMemberInfo(ManageGuildMemberComponent component1, ManageGuildMemberComponent component2)
	{
		GuildJoinRequestPlayerInfo waitingInfo = component1.WaitingInfo;
		GuildJoinRequestPlayerInfo waitingInfo2 = component2.WaitingInfo;
		if (waitingInfo == null || waitingInfo2 == null)
		{
			if (waitingInfo != null)
			{
				return -1;
			}
			if (waitingInfo2 != null)
			{
				return 1;
			}
			return 0;
		}
		int num = 0;
		if (_waitingSortIndex == SortIndex.TeamLevel)
		{
			num = waitingInfo.TeamLevel.CompareTo(waitingInfo2.TeamLevel);
		}
		else if (_waitingSortIndex == SortIndex.Login)
		{
			Vespa_String_Extension_Method2.TryParseNetworkTime2(waitingInfo.LastLoginTime, out var o_dateTime);
			Vespa_String_Extension_Method2.TryParseNetworkTime2(waitingInfo2.LastLoginTime, out var o_dateTime2);
			num = o_dateTime.CompareTo(o_dateTime2);
		}
		if (_waitingSortType != SortType.Down)
		{
			return -num;
		}
		return num;
	}

	private int SortManageMemberInfo(ManageGuildMemberComponent component1, ManageGuildMemberComponent component2)
	{
		GuildMemberInfo memberInfo = component1.MemberInfo;
		GuildMemberInfo memberInfo2 = component2.MemberInfo;
		if (memberInfo == null || memberInfo2 == null)
		{
			if (memberInfo != null)
			{
				return -1;
			}
			if (memberInfo2 != null)
			{
				return 1;
			}
			return 0;
		}
		int num = 0;
		if (_sortIndex == SortIndex.Rank)
		{
			num = -memberInfo.Rank.CompareTo(memberInfo2.Rank);
		}
		else if (_sortIndex == SortIndex.Score)
		{
			num = memberInfo.AccActivityPoint.CompareTo(memberInfo2.AccActivityPoint);
		}
		else if (_sortIndex == SortIndex.Login)
		{
			Vespa_String_Extension_Method2.TryParseNetworkTime2(memberInfo.LastLoginTime, out var o_dateTime);
			Vespa_String_Extension_Method2.TryParseNetworkTime2(memberInfo2.LastLoginTime, out var o_dateTime2);
			num = o_dateTime.CompareTo(o_dateTime2);
		}
		if (_sortType != SortType.Down)
		{
			return -num;
		}
		return num;
	}

	private int SortBase<T>(Transform a, Transform b, Comparison<T> SortFunc)
	{
		T component = a.GetComponent<T>();
		T component2 = b.GetComponent<T>();
		if (component == null || component2 == null)
		{
			if (component != null)
			{
				return -1;
			}
			if (component2 != null)
			{
				return 1;
			}
			return 0;
		}
		return SortFunc(component, component2);
	}

	void GuildSkillComponent.Listener.OnClickLevelUp(GuildSkillLevelData skillLevelData)
	{
		_listener.LevelUpSkill(skillLevelData.GuildSkillIndex, (byte)(skillLevelData.GuildSkillLevel + 1));
	}

	void ManageGuildMemberComponent.Listener.OnClickManageButton(ManageGuildMemberComponent comp)
	{
		_listener.OnClickManageButton(comp);
	}

	private void RemoveWaitingMember(long accountId)
	{
		if (GlobalAutoBehaviour<GuildManager>.instance.JoinRequestPlayerManager != null)
		{
			GlobalAutoBehaviour<GuildManager>.instance.JoinRequestPlayerManager.Remove(accountId);
		}
		RefreshCurList(ListType.Waiting);
	}

	void ManageGuildMemberComponent.Listener.OnClickAcceptJoin(ManageGuildMemberComponent comp)
	{
		if (comp.WaitingInfo != null)
		{
			_listener.OnClickAcceptJoin(comp, delegate
			{
				RemoveWaitingMember(comp.WaitingInfo.AccountId);
			});
		}
	}

	void ManageGuildMemberComponent.Listener.OnClickRejectJoin(ManageGuildMemberComponent comp)
	{
		if (comp.WaitingInfo != null)
		{
			_listener.OnClickRejectJoin(comp, delegate
			{
				RemoveWaitingMember(comp.WaitingInfo.AccountId);
			});
		}
	}

	void GuildSkillVoteComponent.Listener.OnClickVote(int skillIndex)
	{
		_listener.OnClickVote(skillIndex);
	}

	void GuildLogoEditPopup.Listener.OnClickReset()
	{
	}

	void GuildLogoEditPopup.Listener.OnClickApply(int iconIndex, int bgIndex)
	{
		_guildLogoIndex = iconIndex;
		_guildBackIndex = bgIndex;
		UpdateSelectedGuildLogo();
		GuildMemberInfo myMemberInfo = GlobalAutoBehaviour<GuildManager>.instance.MemberManager.GetMyMemberInfo();
		if (myMemberInfo != null && myMemberInfo.Rank == 1)
		{
			CheckApplySettingButton();
		}
	}
}
