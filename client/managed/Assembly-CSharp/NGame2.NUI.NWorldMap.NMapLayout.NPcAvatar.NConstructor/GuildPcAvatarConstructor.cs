using System;
using System.Collections.Generic;
using System.Linq;
using NGame2.NAccount;
using NGame2.NGuild;
using NShared;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWorldMap.NMapLayout.NPcAvatar.NConstructor;

public class GuildPcAvatarConstructor : BaseConstructor
{
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

	private int GetGuildMemberCount()
	{
		if (!GlobalAutoBehaviour<GuildManager>.isValidInstance)
		{
			return 0;
		}
		if (GlobalAutoBehaviour<GuildManager>.instance.MemberManager == null)
		{
			return 0;
		}
		return GlobalAutoBehaviour<GuildManager>.instance.MemberManager.Count();
	}

	private int GetGuildOlineMemberCount()
	{
		return __Extension_Method_ICollection__.COUNT(GetGuildMemberInfosToArray((GuildMemberInfo e) => e.IsOnline && e.AccountId != GlobalAutoBehaviour<AccountManager>.instance.UserInfo.AccountId));
	}

	public override List<CachedUserInfo> AllPcUserInfos()
	{
		GuildMemberInfo[] guildMemberInfosToArray = GetGuildMemberInfosToArray((GuildMemberInfo e) => e.IsOnline && e.AccountId != GlobalAutoBehaviour<AccountManager>.instance.UserInfo.AccountId);
		List<CachedUserInfo> list = new List<CachedUserInfo>();
		for (int num = 0; num < __Extension_Method_ICollection__.COUNT(guildMemberInfosToArray); num++)
		{
			list.Add(guildMemberInfosToArray[num]);
		}
		return list;
	}

	public override CachedUserInfo GetPcAvatarUserInfo(List<CachedUserInfo> exceptUserPools)
	{
		List<CachedUserInfo> list = AllPcUserInfos();
		for (int num = __Extension_Method_ICollection__.COUNT(list) - 1; num >= 0; num--)
		{
			for (int num2 = __Extension_Method_ICollection__.COUNT(exceptUserPools) - 1; num2 >= 0; num2--)
			{
				if (list[num].AccountId == exceptUserPools[num2].AccountId)
				{
					list.RemoveAt(num);
					break;
				}
			}
		}
		if (__Extension_Method_ICollection__.COUNT(list) == 0)
		{
			return null;
		}
		int index = UnityEngine.Random.Range(0, __Extension_Method_ICollection__.COUNT(list));
		return list[index];
	}

	public override BasePcAvatar GetAvatarCreater()
	{
		return new InteractionPcAvatar();
	}

	public override BasePcMediator GetMediator()
	{
		return new GuildMediator();
	}
}
