using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NWindow;
using NShared;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NGuild;

public class GuildMemberManager : NShared.GuildMemberManager
{
	private readonly UserObserverManager<GuildMemberInfo> _observerManager = new UserObserverManager<GuildMemberInfo>();

	public IUserObserverManager ObserverManager => _observerManager;

	public void Apply(GuildMemberInfo[] infos)
	{
		Reset(infos);
	}

	public void AddMember(GuildMemberInfo info)
	{
		Add(info);
	}

	public void RemoveMember(GuildMemberInfo info)
	{
		Remove(info.AccountId);
	}

	public GuildMemberManager()
	{
		GlobalAutoBehaviour<GuiManager>.instance.Add("GuildMember Info", this, delegate
		{
			if (Count() == 0)
			{
				GUILayout.Label("No Guild");
			}
			else
			{
				GuildMemberInfo myMemberInfo = GetMyMemberInfo();
				if (myMemberInfo == null)
				{
					GUILayout.Label("My GuildMember Info Does not exist");
				}
				else
				{
					GUILayout.Label("My Guild Member Info & Guild Point Info");
					string[] obj = new string[6]
					{
						"Rank : ",
						myMemberInfo.Rank.ToString(),
						" AccActivityPoint : ",
						null,
						null,
						null
					};
					int accActivityPoint = myMemberInfo.AccActivityPoint;
					obj[3] = ((long)accActivityPoint).ToString();
					obj[4] = " ContributeCount : ";
					obj[5] = myMemberInfo.ContributeCount.ToString();
					GUILayout.Label(string.Concat(obj));
					PlayerGuildPointInfo guildPointInfo = GlobalAutoBehaviour<AccountManager>.instance.UserManager.GuildPointInfo;
					GUILayout.Label("GuildPoint : " + guildPointInfo.GuildPoint + " DailyAccGuildPoint : " + guildPointInfo.DailyAccGuildPoint + " DailyAccGuildPointResetTime : " + guildPointInfo.DailyAccGuildPointResetTime);
				}
			}
		});
	}

	public GuildMemberInfo GetMasterMemberInfo()
	{
		IEnumerator<GuildMemberInfo> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			if (enumerator.Current.Rank == 1)
			{
				return enumerator.Current;
			}
		}
		return null;
	}

	public GuildMemberInfo GetMyMemberInfo()
	{
		return Get(GlobalAutoBehaviour<AccountManager>.instance.UserManager.Id);
	}

	public bool IsMaster(GuildMemberInfo memberinfo)
	{
		if (memberinfo != null)
		{
			return memberinfo.Rank == 1;
		}
		return false;
	}

	public bool IsAdmin(GuildMemberInfo memberinfo)
	{
		if (memberinfo != null)
		{
			return memberinfo.Rank == 2;
		}
		return false;
	}

	public void SwapMasterMemberInfo(long AccountId)
	{
		if (AccountId != GetMasterMemberInfo().AccountId)
		{
			GuildMemberInfo guildMemberInfo = Get(AccountId);
			if (guildMemberInfo != null)
			{
				GetMasterMemberInfo().Rank = guildMemberInfo.Rank;
				guildMemberInfo.Rank = 1;
			}
		}
	}

	public void SetOnline(long accountID, bool isOnline)
	{
		IEnumerator<GuildMemberInfo> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			if (enumerator.Current.AccountId == accountID)
			{
				enumerator.Current.IsOnline = isOnline;
				if (isOnline)
				{
					enumerator.Current.LastLoginTime = Vespa_DateTime_Extension_Method.ToNetworkString2(Common.GetUTCNow());
				}
				else
				{
					enumerator.Current.LastLogoutTime = Vespa_DateTime_Extension_Method.ToNetworkString2(Common.GetUTCNow());
				}
				_observerManager.AnnounceToObservers(enumerator.Current);
				break;
			}
		}
	}

	public void SetNick(long accountID, string nick)
	{
		IEnumerator<GuildMemberInfo> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			if (enumerator.Current.AccountId == accountID)
			{
				enumerator.Current.Nick = nick;
				_observerManager.AnnounceToObservers(enumerator.Current);
				break;
			}
		}
	}

	public void SetAvatarHero(long accountID, int avatarHeroIndex)
	{
		IEnumerator<GuildMemberInfo> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			if (enumerator.Current.AccountId == accountID)
			{
				enumerator.Current.AvatarHeroIndex = avatarHeroIndex;
				_observerManager.AnnounceToObservers(enumerator.Current);
				break;
			}
		}
	}

	public void SetRank(long accountID, EGuildMemberRank rank)
	{
		IEnumerator<GuildMemberInfo> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			if (enumerator.Current.AccountId == accountID)
			{
				enumerator.Current.Rank = (byte)rank;
				break;
			}
		}
	}
}
