using System;
using NGame2.NAccount;
using NGame2.NContents;
using NGame2.NGuild;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow.NTopMost;

public class GuidSuppressNoticeIconChecker : INoticeIconChcker
{
	private GuildSuppressNoticeComponent _comp;

	private bool _compActiveSelf;

	private bool _contentsStatus;

	private long _sessionIndex;

	private DateTime _guildSuppressStartTime;

	private DateTime _guildSuppressEndTime;

	void INoticeIconChcker.Init(IGrid grid)
	{
		if (grid != null)
		{
			_comp = UIHelper.AddGridItem(grid, GuildSuppressNoticeComponent.PrefabAuto);
			if (!(_comp == null))
			{
				_compActiveSelf = _comp.gameObject.activeSelf;
				SetContentsStatus();
			}
		}
	}

	bool INoticeIconChcker.Check(bool show)
	{
		if (show)
		{
			bool show2 = CanShow();
			return SetShow(show2);
		}
		return SetShow(show: false);
	}

	private bool CanShow()
	{
		if (!AccountManager.IsLogin())
		{
			_sessionIndex = -1L;
			return false;
		}
		if (!_contentsStatus)
		{
			_sessionIndex = -1L;
			return false;
		}
		if (GlobalAutoBehaviour<GuildManager>.isValidInstance && GlobalAutoBehaviour<GuildManager>.instance.JoinedGuildId == 0L)
		{
			_sessionIndex = -1L;
			return false;
		}
		GuildSuppressSessionInfo guildSuppressSessionInfo = GlobalAutoBehaviour<AccountManager>.instance.UserManager.GetGuildSuppressSessionInfo();
		if (guildSuppressSessionInfo == null)
		{
			_sessionIndex = -1L;
			return false;
		}
		if (_sessionIndex != guildSuppressSessionInfo.SessionIndex)
		{
			GuildSuppressSessionData guildSuppressSessionData = RestoredPortal.ConquestSession(GameRoot.Instance.GuildSuppressSessionDataContainer, (int)guildSuppressSessionInfo.SessionIndex);
			if (guildSuppressSessionData != null)
			{
				Vespa_String_Extension_Method2.TryParseNetworkTime2(guildSuppressSessionData.GlobalBattleStart, out _guildSuppressStartTime);
				Vespa_String_Extension_Method2.TryParseNetworkTime2(guildSuppressSessionData.GlobalBattleEnd, out _guildSuppressEndTime);
			}
			else
			{
				Vespa_String_Extension_Method2.TryParseNetworkTime2(guildSuppressSessionInfo.BattleStartedTime, out _guildSuppressStartTime);
				Vespa_String_Extension_Method2.TryParseNetworkTime2(guildSuppressSessionInfo.BattleEndTime, out _guildSuppressEndTime);
			}
			_sessionIndex = guildSuppressSessionInfo.SessionIndex;
		}
		DateTime currentServerUTCTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime();
		bool num = _guildSuppressStartTime <= currentServerUTCTime;
		bool flag = _guildSuppressEndTime > currentServerUTCTime;
		bool result = num && flag;
		if (!flag)
		{
			GuildSuppressSessionData guildSuppressSessionData2 = RestoredPortal.ConquestSession(index: (int)(_sessionIndex + 1), container: GameRoot.Instance.GuildSuppressSessionDataContainer);
			if (guildSuppressSessionData2 != null)
			{
				Vespa_String_Extension_Method2.TryParseNetworkTime2(guildSuppressSessionData2.GlobalBattleStart, out _guildSuppressStartTime);
				Vespa_String_Extension_Method2.TryParseNetworkTime2(guildSuppressSessionData2.GlobalBattleEnd, out _guildSuppressEndTime);
			}
		}
		return result;
	}

	private bool SetShow(bool show)
	{
		if (_compActiveSelf != show)
		{
			_compActiveSelf = show;
			UIHelper.EnableGameObject(_comp, show);
			return true;
		}
		return false;
	}

	void INoticeIconChcker.CheckContentsStatus()
	{
		SetContentsStatus();
	}

	private void SetContentsStatus()
	{
		_contentsStatus = SingletonAuto<ContentsStatusManager>.instance.GetContentsStatus(ContentsStatusType.GuildSuppress) == ContentsStatus.On;
	}

	void INoticeIconChcker.Close()
	{
		FactoryBase.SafeReleaseComponent(ref _comp);
	}
}
