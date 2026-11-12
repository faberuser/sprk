using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NGuild;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NHelper.NGuild;
using NGameAsset;
using NShared;
using NShared.GetGuildSuppressStatusBoard;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class GuildSuppressStatusBoard : LobbyFullWindow, GuildSuppressStatusBoardGuildComponent.IClickListener
{
	public class GuildInfoKey : Tuple<long, string>
	{
		public GuildInfoKey(long id, string serverName)
			: base(id, serverName)
		{
		}

		public GuildInfoKey()
			: base(0L, "")
		{
		}
	}

	public class GuildInfos2
	{
		private Dictionary<GuildInfoKey, GuildInfo> _dicGuildInfo = new Dictionary<GuildInfoKey, GuildInfo>();

		private Dictionary<GuildInfoKey, GuildSuppressGroupRankerInfo> _dicGroupRankerInfo = new Dictionary<GuildInfoKey, GuildSuppressGroupRankerInfo>();

		private Dictionary<GuildInfoKey, GuildSuppressMemberScoreInfo[]> _dicMemberScoreInfo = new Dictionary<GuildInfoKey, GuildSuppressMemberScoreInfo[]>();

		private Dictionary<GuildInfoKey, GuildSuppressPlayInfo[]> _dicPlayInfo = new Dictionary<GuildInfoKey, GuildSuppressPlayInfo[]>();

		private Dictionary<GuildInfoKey, GuildSuppressMemberPlayHistory[]> _dicHistoryInfo = new Dictionary<GuildInfoKey, GuildSuppressMemberPlayHistory[]>();

		private GuildSuppressSessionInfo _sessionInfo;

		private GuildSuppressGroupRankerInfo[] _cachedRankerInfos;

		public void Apply(NShared.GetGuildSuppressStatusBoard.Response response)
		{
			Apply(response.GuildSuppressSessionInfo);
			Apply(response.GuildSuppressGroupRankerInfos);
			Apply(response.GuildSuppressStatusBoardInfos);
		}

		private void Apply(GuildSuppressStatusBoardInfo[] boardInfos)
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(boardInfos); i++)
			{
				GuildSuppressStatusBoardInfo guildSuppressStatusBoardInfo = boardInfos[i];
				if (guildSuppressStatusBoardInfo == null)
				{
					continue;
				}
				int guildId = guildSuppressStatusBoardInfo.GuildSuppressPlayInfos[0].GuildId;
				string serverGroup = guildSuppressStatusBoardInfo.GuildSuppressPlayInfos[0].ServerGroup;
				GuildInfoKey key = new GuildInfoKey(guildId, serverGroup);
				if (!__Extension_Method_ICollection__.IsNullOrEmpty(guildSuppressStatusBoardInfo.GuildSuppressMemberScores))
				{
					Array.Sort(guildSuppressStatusBoardInfo.GuildSuppressMemberScores, delegate(GuildSuppressMemberScoreInfo lhs, GuildSuppressMemberScoreInfo rhs)
					{
						int num = rhs.Score.CompareTo(lhs.Score);
						return (num != 0) ? num : lhs.AccountId.CompareTo(rhs.AccountId);
					});
				}
				Apply2(_dicGuildInfo, key, guildSuppressStatusBoardInfo.GuildInfo);
				Apply2(_dicHistoryInfo, key, guildSuppressStatusBoardInfo.GuildSuppressMemberPlayHistorys);
				Apply2(_dicMemberScoreInfo, key, guildSuppressStatusBoardInfo.GuildSuppressMemberScores);
				Apply2(_dicPlayInfo, key, guildSuppressStatusBoardInfo.GuildSuppressPlayInfos);
			}
		}

		private void Apply(GuildSuppressGroupRankerInfo[] rankerInfos)
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(rankerInfos); i++)
			{
				GuildSuppressGroupRankerInfo guildSuppressGroupRankerInfo = rankerInfos[i];
				if (guildSuppressGroupRankerInfo != null)
				{
					long guildId = guildSuppressGroupRankerInfo.GuildId;
					string serverGroup = guildSuppressGroupRankerInfo.ServerGroup;
					GuildInfoKey key = new GuildInfoKey(guildId, serverGroup);
					Apply2(_dicGroupRankerInfo, key, guildSuppressGroupRankerInfo);
				}
			}
			Array.Sort(rankerInfos, delegate(GuildSuppressGroupRankerInfo lhs, GuildSuppressGroupRankerInfo rhs)
			{
				int num = rhs.Score.CompareTo(lhs.Score);
				if (num != 0)
				{
					return num;
				}
				GuildInfoKey key2 = new GuildInfoKey(lhs.GuildId, lhs.ServerGroup);
				GuildInfoKey key3 = new GuildInfoKey(rhs.GuildId, rhs.ServerGroup);
				GuildInfo guildInfo = GetGuildInfo(key2);
				GuildInfo guildInfo2 = GetGuildInfo(key3);
				return (guildInfo == null || guildInfo2 == null) ? lhs.Rank.CompareTo(rhs.Rank) : guildInfo2.Level.CompareTo(guildInfo.Level);
			});
			_cachedRankerInfos = rankerInfos;
		}

		private void Apply(GuildSuppressSessionInfo info)
		{
			_sessionInfo = info;
		}

		private void Apply2<T>(Dictionary<GuildInfoKey, T> dic, GuildInfoKey key, T value) where T : class
		{
			dic[key] = value;
		}

		private void Apply2<T>(Dictionary<GuildInfoKey, T[]> dic, GuildInfoKey key, T[] values) where T : class
		{
			dic[key] = values;
		}

		public void Close()
		{
			_cachedRankerInfos = null;
			_sessionInfo = null;
			_dicGuildInfo.Clear();
			_dicGroupRankerInfo.Clear();
			_dicMemberScoreInfo.Clear();
			_dicPlayInfo.Clear();
		}

		public GuildSuppressGroupRankerInfo[] GetGuildRankerInfos()
		{
			return _cachedRankerInfos;
		}

		public GuildSuppressGroupRankerInfo GetGuildRankerInfo(GuildInfoKey key)
		{
			_dicGroupRankerInfo.TryGetValue(key, out var value);
			return value;
		}

		public GuildInfo GetGuildInfo(GuildInfoKey key)
		{
			_dicGuildInfo.TryGetValue(key, out var value);
			return value;
		}

		public GuildSuppressMemberScoreInfo[] GetGuildMemberScoreInfos(GuildInfoKey key)
		{
			_dicMemberScoreInfo.TryGetValue(key, out var value);
			return value;
		}

		public GuildSuppressMemberPlayHistory[] GetGuildHistoryInfos(GuildInfoKey key)
		{
			_dicHistoryInfo.TryGetValue(key, out var value);
			return value;
		}

		public GuildSuppressPlayInfo GetSuppressPlayInfo(GuildInfoKey key, int raidIndex, int level)
		{
			_dicPlayInfo.TryGetValue(key, out var value);
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(value); i++)
			{
				GuildSuppressPlayInfo guildSuppressPlayInfo = value[i];
				if (guildSuppressPlayInfo != null && guildSuppressPlayInfo.RaidIndex == raidIndex && guildSuppressPlayInfo.Level == level)
				{
					return guildSuppressPlayInfo;
				}
			}
			return null;
		}

		public GuildSuppressSessionInfo GetSessionInfo()
		{
			return _sessionInfo;
		}
	}

	public class GuildInfos
	{
		private Dictionary<long, GuildInfo> _dicGuildInfo = new Dictionary<long, GuildInfo>();

		private Dictionary<long, GuildSuppressGroupRankerInfo> _dicGroupRankerInfo = new Dictionary<long, GuildSuppressGroupRankerInfo>();

		private Dictionary<long, GuildSuppressMemberScoreInfo[]> _dicMemberScoreInfo = new Dictionary<long, GuildSuppressMemberScoreInfo[]>();

		private Dictionary<long, GuildSuppressPlayInfo[]> _dicPlayInfo = new Dictionary<long, GuildSuppressPlayInfo[]>();

		private Dictionary<long, GuildSuppressMemberPlayHistory[]> _dicHistoryInfo = new Dictionary<long, GuildSuppressMemberPlayHistory[]>();

		private GuildSuppressSessionInfo _sessionInfo;

		private GuildSuppressGroupRankerInfo[] _cachedRankerInfos;

		public void Apply(NShared.GetGuildSuppressStatusBoard.Response response)
		{
			Apply(response.GuildSuppressSessionInfo);
			Apply(response.GuildSuppressGroupRankerInfos);
			Apply(response.GuildSuppressStatusBoardInfos);
		}

		private void Apply(GuildSuppressStatusBoardInfo[] infos)
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(infos); i++)
			{
				GuildSuppressStatusBoardInfo info = infos[i];
				Apply(info);
			}
		}

		private void Apply(GuildSuppressStatusBoardInfo info)
		{
			if (info == null)
			{
				return;
			}
			GuildInfo guildInfo = info.GuildInfo;
			long id = guildInfo.Id;
			if (!__Extension_Method_ICollection__.IsNullOrEmpty(info.GuildSuppressMemberScores))
			{
				Array.Sort(info.GuildSuppressMemberScores, delegate(GuildSuppressMemberScoreInfo lhs, GuildSuppressMemberScoreInfo rhs)
				{
					int num = rhs.Score.CompareTo(lhs.Score);
					return (num != 0) ? num : lhs.AccountId.CompareTo(rhs.AccountId);
				});
			}
			Apply(_dicGuildInfo, id, guildInfo);
			Apply(_dicHistoryInfo, id, info.GuildSuppressMemberPlayHistorys);
			Apply(_dicMemberScoreInfo, id, info.GuildSuppressMemberScores);
			Apply(_dicPlayInfo, id, info.GuildSuppressPlayInfos);
		}

		private void Apply(GuildSuppressGroupRankerInfo[] infos)
		{
			if (__Extension_Method_ICollection__.IsNullOrEmpty(infos))
			{
				return;
			}
			Array.Sort(infos, delegate(GuildSuppressGroupRankerInfo lhs, GuildSuppressGroupRankerInfo rhs)
			{
				int num2 = rhs.Score.CompareTo(lhs.Score);
				if (num2 != 0)
				{
					return num2;
				}
				GuildInfo guildInfo = GetGuildInfo(lhs.GuildId);
				GuildInfo guildInfo2 = GetGuildInfo(rhs.GuildId);
				return (guildInfo == null || guildInfo2 == null) ? lhs.Rank.CompareTo(rhs.Rank) : guildInfo2.Level.CompareTo(guildInfo.Level);
			});
			_cachedRankerInfos = infos;
			for (int num = 0; num < __Extension_Method_ICollection__.COUNT(infos); num++)
			{
				GuildSuppressGroupRankerInfo guildSuppressGroupRankerInfo = infos[num];
				if (guildSuppressGroupRankerInfo != null)
				{
					guildSuppressGroupRankerInfo.Rank = num + 1;
					Apply(_dicGroupRankerInfo, guildSuppressGroupRankerInfo.GuildId, guildSuppressGroupRankerInfo);
				}
			}
		}

		private void Apply(GuildSuppressSessionInfo info)
		{
			_sessionInfo = info;
		}

		private void Apply2<T>(Dictionary<GuildInfoKey, T> dic, long id, string serverName, T value) where T : class
		{
			GuildInfoKey key = new GuildInfoKey(id, serverName);
			dic[key] = value;
		}

		private void Apply2<T>(Dictionary<GuildInfoKey, T[]> dic, long id, string serverName, T[] values) where T : class
		{
			GuildInfoKey key = new GuildInfoKey(id, serverName);
			dic[key] = values;
		}

		private void Apply<T>(Dictionary<long, T> dic, long key, T value) where T : class
		{
			dic[key] = value;
		}

		private void Apply<T>(Dictionary<long, T[]> dic, long key, T[] values) where T : class
		{
			dic[key] = values;
		}

		public void Close()
		{
			_cachedRankerInfos = null;
			_sessionInfo = null;
			_dicGuildInfo.Clear();
			_dicGroupRankerInfo.Clear();
			_dicMemberScoreInfo.Clear();
			_dicPlayInfo.Clear();
		}

		public GuildSuppressGroupRankerInfo[] GetGuildRankerInfos()
		{
			return _cachedRankerInfos;
		}

		public GuildSuppressGroupRankerInfo GetGuildRankerInfo(long guildId)
		{
			_dicGroupRankerInfo.TryGetValue(guildId, out var value);
			return value;
		}

		public GuildInfo GetGuildInfo(long guildId)
		{
			_dicGuildInfo.TryGetValue(guildId, out var value);
			return value;
		}

		public GuildSuppressMemberScoreInfo[] GetGuildMemberScoreInfos(long guildId)
		{
			_dicMemberScoreInfo.TryGetValue(guildId, out var value);
			return value;
		}

		public GuildSuppressMemberPlayHistory[] GetGuildHistoryInfos(long guildId)
		{
			_dicHistoryInfo.TryGetValue(guildId, out var value);
			return value;
		}

		public GuildSuppressPlayInfo GetSuppressPlayInfo(long guildId, int raidIndex, int level)
		{
			_dicPlayInfo.TryGetValue(guildId, out var value);
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(value); i++)
			{
				GuildSuppressPlayInfo guildSuppressPlayInfo = value[i];
				if (guildSuppressPlayInfo != null && guildSuppressPlayInfo.RaidIndex == raidIndex && guildSuppressPlayInfo.Level == level)
				{
					return guildSuppressPlayInfo;
				}
			}
			return null;
		}

		public GuildSuppressSessionInfo GetSessionInfo()
		{
			return _sessionInfo;
		}
	}

	public interface IListener : GuildSuppressStatusBoardBossComponent.IClickListener
	{
		void OnCilickRefresh();
	}

	public const string PrefabPath = "Resources/Prefab2/Window/GuildSuppress/GuildSuppressStatusBoard";

	public const string BGM = "EntryMenu.GuildSuppressBattleBGM";

	public UIBaseGrid GuildGrid;

	public UIBaseGrid GuildMemberGrid;

	public UIBaseGrid BossGrid;

	public UITexture GuildLogoTexture;

	public UITexture GuildLogoBgTexture;

	public UITexture GuildCountyTexture;

	public GameObject GuildMyGo;

	public UILabel GuildNameLabel;

	public UILabel GUildLevelLabel;

	public GameObject NothingRoot;

	public UIButton RefreshButton;

	public GameObject RefreshNoticeGo;

	public UILabel RefreshLabel;

	public UILabel RemainTimeLabel;

	private List<Component> _listGuildComponent = new List<Component>();

	private List<Component> _listGuildUserComponent = new List<Component>();

	private List<Component> _listBossComponent = new List<Component>();

	private GuildInfos2 _infos = new GuildInfos2();

	private IListener _listener;

	private GuildSuppressSessionData _sessionData;

	private DateTime _lastUpdateTime;

	private DateTime _lastRefreshUpdateTime;

	private int _cachedRefreshCoolTime = 180;

	private bool _refreshEnable;

	private GuildInfoKey _myGuildInfoKey;

	private GuildInfoKey _selectGuildInfoKey;

	private bool _isEnableRefresh;

	private GuildInfoKey MyGuildInfoKey
	{
		get
		{
			if (_myGuildInfoKey == null)
			{
				_myGuildInfoKey = new GuildInfoKey(MyGuildId, MyServerName);
			}
			return _myGuildInfoKey;
		}
	}

	private long MyGuildId
	{
		get
		{
			if (!GlobalAutoBehaviour<GuildManager>.isValidInstance)
			{
				return -1L;
			}
			return GlobalAutoBehaviour<GuildManager>.instance.JoinedGuildId;
		}
	}

	private string MyServerName
	{
		get
		{
			if (!AccountManager.IsLogin())
			{
				return string.Empty;
			}
			return GlobalAutoBehaviour<AccountManager>.instance.CountryServerName;
		}
	}

	private long MyAccountId
	{
		get
		{
			if (!AccountManager.IsLogin())
			{
				return 0L;
			}
			return GlobalAutoBehaviour<AccountManager>.instance.UserInfo.AccountId;
		}
	}

	public void Open(NShared.GetGuildSuppressStatusBoard.Response res, IListener listener)
	{
		_infos.Apply(res);
		_listener = listener;
		Open(delegate
		{
			Setup(0L);
			SetRefreshTime();
		}, delegate
		{
			SingletonAuto<BgmManager2>.instance.Request("EntryMenu.GuildSuppressBattleBGM", isEvent: true);
		});
	}

	public void Setup(long guildId = 0L, string serverName = "")
	{
		SetSelectGuildId(guildId, serverName);
		SetGuildComponents();
		SetGuildUserComponent();
		SetBossComponent();
		SetSelectGuildInfo();
	}

	private void SetSelectGuildId(long guildId, string serverName)
	{
		if (guildId == 0L && String_Extension_Method.IsNullOrEmpty(serverName))
		{
			_selectGuildInfoKey = MyGuildInfoKey;
		}
		else
		{
			_selectGuildInfoKey = new GuildInfoKey(guildId, serverName);
		}
	}

	private void SetGuildComponents()
	{
		ReleaseGuildComponent();
		GuildSuppressGroupRankerInfo[] guildRankerInfos = _infos.GetGuildRankerInfos();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(guildRankerInfos); i++)
		{
			GuildSuppressGroupRankerInfo guildSuppressGroupRankerInfo = guildRankerInfos[i];
			if (guildSuppressGroupRankerInfo == null)
			{
				continue;
			}
			long guildId = guildSuppressGroupRankerInfo.GuildId;
			string serverGroup = guildSuppressGroupRankerInfo.ServerGroup;
			GuildInfoKey guildInfoKey = new GuildInfoKey(guildId, serverGroup);
			GuildInfo guildInfo = _infos.GetGuildInfo(guildInfoKey);
			if (guildInfo != null)
			{
				GuildSuppressStatusBoardGuildComponent guildSuppressStatusBoardGuildComponent = UIHelper.AddGridItem(GuildGrid, GuildSuppressStatusBoardGuildComponent.PrefabAuto);
				if (!(guildSuppressStatusBoardGuildComponent == null))
				{
					bool isMe = guildInfoKey.Equals(MyGuildInfoKey);
					bool isSelect = guildInfoKey.Equals(_selectGuildInfoKey);
					guildSuppressStatusBoardGuildComponent.Open(guildInfo, guildSuppressGroupRankerInfo, isMe, isSelect, this);
					_listGuildComponent.Add(guildSuppressStatusBoardGuildComponent);
				}
			}
		}
		UIHelper.RepositionGrid(GuildGrid);
	}

	private void SetGuildUserComponent()
	{
		ReleaseGuildUserComponent();
		GuildSuppressMemberScoreInfo[] guildMemberScoreInfos = _infos.GetGuildMemberScoreInfos(_selectGuildInfoKey);
		bool flag = __Extension_Method_ICollection__.IsNullOrEmpty(guildMemberScoreInfos);
		UIHelper.EnableGameObject(NothingRoot, flag);
		if (flag)
		{
			return;
		}
		int num = System.Math.Min(__Extension_Method_ICollection__.COUNT(guildMemberScoreInfos), 3);
		for (int i = 0; i < num; i++)
		{
			GuildSuppressMemberScoreInfo guildSuppressMemberScoreInfo = guildMemberScoreInfos[i];
			if (guildSuppressMemberScoreInfo != null)
			{
				GuildSuppressStatusBoardUserComponent guildSuppressStatusBoardUserComponent = UIHelper.AddGridItem(GuildMemberGrid, GuildSuppressStatusBoardUserComponent.PrefabAuto);
				if (!(guildSuppressStatusBoardUserComponent == null))
				{
					guildSuppressStatusBoardUserComponent.Open(guildSuppressMemberScoreInfo, i, guildSuppressMemberScoreInfo.AccountId == MyAccountId);
					_listGuildUserComponent.Add(guildSuppressStatusBoardUserComponent);
				}
			}
		}
		UIHelper.RepositionGrid(GuildMemberGrid);
	}

	private void SetBossComponent()
	{
		ReleaseBossComponent();
		GuildSuppressSessionInfo sessionInfo = _infos.GetSessionInfo();
		if (sessionInfo == null)
		{
			return;
		}
		GuildSuppressHelper.CreateDungeonList(sessionInfo, delegate(int raidIndex, int level, int suppressDungeonIndex)
		{
			GuildSuppressDungeonData data = GameRoot.Instance.GuildSuppressDungeonDataContainer.GetData(suppressDungeonIndex);
			if (data != null)
			{
				GuildSuppressPlayInfo suppressPlayInfo = _infos.GetSuppressPlayInfo(_selectGuildInfoKey, raidIndex, level);
				if (suppressPlayInfo != null)
				{
					GuildSuppressStatusBoardBossComponent guildSuppressStatusBoardBossComponent = UIHelper.AddGridItem(BossGrid, GuildSuppressStatusBoardBossComponent.PrefabAuto);
					if (!(guildSuppressStatusBoardBossComponent == null))
					{
						guildSuppressStatusBoardBossComponent.Open(suppressPlayInfo, data, level, _listener);
						_listBossComponent.Add(guildSuppressStatusBoardBossComponent);
					}
				}
			}
		});
		UIHelper.RepositionGrid(BossGrid);
	}

	private void SetSelectGuildInfo()
	{
		GuildSuppressGroupRankerInfo guildRankerInfo = _infos.GetGuildRankerInfo(_selectGuildInfoKey);
		GuildInfo guildInfo = _infos.GetGuildInfo(_selectGuildInfoKey);
		int guildLogo = guildRankerInfo.GuildLogo;
		byte back = guildInfo.Back;
		string countryCode = guildRankerInfo.CountryCode;
		byte level = guildInfo.Level;
		string guildName = guildRankerInfo.GuildName;
		bool enable = _selectGuildInfoKey.Equals(MyGuildInfoKey);
		GuildSuppressHelper.SetGuildLogo(guildLogo, back, GuildLogoTexture, GuildLogoBgTexture, this);
		GuildSuppressHelper.SetGuildCountyTexture(countryCode, GuildCountyTexture, this);
		GuildSuppressHelper.SetGuildLevel(GUildLevelLabel, level);
		UIHelper.SetLabelText(GuildNameLabel, guildName);
		UIHelper.EnableGameObject(GuildMyGo, enable);
	}

	private void SetRefreshTime()
	{
		_lastRefreshUpdateTime = ((_lastRefreshUpdateTime == DateTime.MinValue) ? DateTime.Now : _lastRefreshUpdateTime);
		_cachedRefreshCoolTime = GameRoot.Instance.ConstantDataContainer.GetValueInt("GuildSuppressStatusBoardRefreshTime", 180);
	}

	private void UpdateRereshTime()
	{
		int num = (int)(DateTime.Now - _lastRefreshUpdateTime).TotalSeconds;
		bool flag = num >= _cachedRefreshCoolTime;
		if (UIHelper.IsEnabledSelfGameObjet(RefreshNoticeGo) != flag)
		{
			UIHelper.EnableGameObject(RefreshNoticeGo, flag);
			_isEnableRefresh = flag;
			if (flag)
			{
				string stringDirect = LocalizationManager.GetStringDirect("GUILD_SUPPRESS_STATUS_BOARD_AVAILABLE");
				UIHelper.SetLabelText(RefreshLabel, stringDirect);
			}
		}
		if (!flag)
		{
			int remainTime = Mathf.Max(0, _cachedRefreshCoolTime - num);
			UIHelper.SetLabelText(RefreshLabel, LocalizationManager.GetStringDirect("GUILD_SUPPRESS_STATUS_BOARD_REFRESH_TIMER", UIHelper.GetRemainTimeText(remainTime)));
		}
	}

	private void UpdateGuildSuppressRemainTime()
	{
		GuildSuppressSessionInfo sessionInfo = _infos.GetSessionInfo();
		if (sessionInfo != null)
		{
			if (_sessionData == null)
			{
				_sessionData = RestoredPortal.ConquestSession(GameRoot.Instance.GuildSuppressSessionDataContainer, (int)sessionInfo.SessionIndex);
			}
			int remainSeconds = GuildSuppressHelper.GetRemainSeconds(_sessionData, sessionInfo);
			long num = remainSeconds;
			int num2 = 0;
			if (num >= num2)
			{
				UIHelper.SetLabelText(RemainTimeLabel, LocalizationManager.GetString(GuildRaid.GUILDRAID_STATE_OPENED, UIHelper.GetRemainTimeLabelText(num)));
			}
		}
	}

	private bool CanUpdate()
	{
		DateTime now = DateTime.Now;
		if ((now - _lastUpdateTime).TotalSeconds < 1.0)
		{
			return false;
		}
		_lastUpdateTime = now;
		return true;
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		if (CanUpdate())
		{
			UpdateGuildSuppressRemainTime();
			UpdateRereshTime();
		}
	}

	public override void OnClosing()
	{
		base.OnClosing();
		ReleaseBossComponent();
		ReleaseGuildComponent();
		ReleaseGuildUserComponent();
		SingletonAuto<BgmManager2>.instance.StopEvent("EntryMenu.GuildSuppressBattleBGM");
		_infos.Close();
		_myGuildInfoKey = null;
		_selectGuildInfoKey = null;
		_sessionData = null;
	}

	public void ReleaseGuildComponent()
	{
		FactoryBase.ReleaseAndClearList(_listGuildComponent);
	}

	public void ReleaseGuildUserComponent()
	{
		FactoryBase.ReleaseAndClearList(_listGuildUserComponent);
	}

	public void ReleaseBossComponent()
	{
		FactoryBase.ReleaseAndClearList(_listBossComponent);
	}

	public void OnClickRefresh()
	{
		if (!_isEnableRefresh)
		{
			MessagePopup.OpenInform(LocalizationManager.GetStringDirect("GUILD_SUPPRESS_TIMER_NOT_READY"));
			return;
		}
		GuildSuppressHelper.RequestGuildSuppressStatusBoard(delegate(NShared.GetGuildSuppressStatusBoard.Response response)
		{
			if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult, delegate
				{
					Close();
				});
			}
			else if (response.Result != ResultType.Success)
			{
				MessagePopup.OpenError(response.Result, delegate
				{
					Close();
				});
			}
			else
			{
				_infos.Apply(response);
				_lastRefreshUpdateTime = DateTime.Now;
				Setup(_selectGuildInfoKey.Item1, _selectGuildInfoKey.Item2);
				SetRefreshTime();
			}
		});
		if (_listener != null)
		{
			_listener.OnCilickRefresh();
		}
	}

	public void OnClickOpenPopup()
	{
		GuildInfo guildInfo = _infos.GetGuildInfo(_selectGuildInfoKey);
		GuildSuppressGroupRankerInfo guildRankerInfo = _infos.GetGuildRankerInfo(_selectGuildInfoKey);
		GuildSuppressMemberPlayHistory[] guildHistoryInfos = _infos.GetGuildHistoryInfos(_selectGuildInfoKey);
		GuildSuppressMemberScoreInfo[] guildMemberScoreInfos = _infos.GetGuildMemberScoreInfos(_selectGuildInfoKey);
		GuildSuppressMemberScoreInfo[] guildMemberScoreInfos2 = _infos.GetGuildMemberScoreInfos(MyGuildInfoKey);
		bool isMyGuild = _selectGuildInfoKey.Equals(MyGuildInfoKey);
		GuildSuppressMemberScoreInfo myScoreInfo = null;
		int myRank = -1;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(guildMemberScoreInfos2); i++)
		{
			GuildSuppressMemberScoreInfo guildSuppressMemberScoreInfo = guildMemberScoreInfos2[i];
			if (guildSuppressMemberScoreInfo.AccountId == MyAccountId)
			{
				myScoreInfo = guildSuppressMemberScoreInfo;
				myRank = i + 1;
			}
		}
		GuildSuppressStatusPopup.OpenPopup(guildRankerInfo, guildInfo, guildHistoryInfos, guildMemberScoreInfos, myScoreInfo, myRank, isMyGuild);
	}

	void GuildSuppressStatusBoardGuildComponent.IClickListener.OnClick(long guildId, string serverName)
	{
		Setup(guildId, serverName);
	}
}
