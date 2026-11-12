using System;
using System.Collections.Generic;
using System.Linq;
using NGame2.NAccount;
using NGame2.NGuild;
using NShared;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWorldMap.NMapLayout.NPcAvatar.NConstructor;

public class GuildMemberUserInfoGetter : IUserInfoGetter
{
	private List<CachedUserInfo> _exceptUserPools;

	public GuildMemberUserInfoGetter(List<CachedUserInfo> exceptUserPools)
	{
		_exceptUserPools = exceptUserPools;
	}

	private GuildMemberInfo[] GetGuildMemberInfosToArray(Func<GuildMemberInfo, bool> func = null)
	{
		if (!GlobalAutoBehaviour<GuildManager>.isValidInstance)
		{
			return null;
		}
		if (GlobalAutoBehaviour<GuildManager>.instance.MemberManager == null)
		{
			return null;
		}
		if (func != null)
		{
			return GlobalAutoBehaviour<GuildManager>.instance.MemberManager.GetValues().Where(func).ToArray();
		}
		return GlobalAutoBehaviour<GuildManager>.instance.MemberManager.GetValues();
	}

	private List<CachedUserInfo> AllPcUserInfos()
	{
		GuildMemberInfo[] guildMemberInfosToArray = GetGuildMemberInfosToArray((GuildMemberInfo e) => e.IsOnline && e.AccountId != GlobalAutoBehaviour<AccountManager>.instance.UserInfo.AccountId);
		List<CachedUserInfo> list = new List<CachedUserInfo>();
		for (int num = 0; num < __Extension_Method_ICollection__.COUNT(guildMemberInfosToArray); num++)
		{
			list.Add(guildMemberInfosToArray[num]);
		}
		return list;
	}

	CachedUserInfo IUserInfoGetter.GetUserInfo()
	{
		List<CachedUserInfo> allPcUserInfos = AllPcUserInfos();
		if (__Extension_Method_ICollection__.COUNT(_exceptUserPools) > 0)
		{
			int i = __Extension_Method_ICollection__.COUNT(allPcUserInfos) - 1;
			while (i >= 0)
			{
				if (_exceptUserPools.Find((CachedUserInfo user) => user.AccountId == allPcUserInfos[i].AccountId) != null)
				{
					allPcUserInfos.RemoveAt(i);
				}
				int num = i - 1;
				i = num;
			}
		}
		if (__Extension_Method_ICollection__.COUNT(allPcUserInfos) == 0)
		{
			return null;
		}
		int index = UnityEngine.Random.Range(0, __Extension_Method_ICollection__.COUNT(allPcUserInfos));
		return allPcUserInfos[index];
	}
}
