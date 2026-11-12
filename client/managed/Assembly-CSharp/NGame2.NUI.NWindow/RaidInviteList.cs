using System;
using System.Collections.Generic;
using System.Linq;
using NGame2.NAccount;
using NGame2.NGuild;
using NGame2.NScene.NPreloader;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow;

public class RaidInviteList : BaseWindow
{
	public interface Listener
	{
		void Invite(RaidInviteList caller, RaidInviteComponent component);

		void InviteAllFriend(RaidInviteList caller);

		void InviteAllGuild(RaidInviteList caller);

		void FindUser(string Nick, Action<CachedUserInfo> onComplete);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Raid/RaidInviteList";

	public RaidInviteImpl FriendInvitor;

	public RaidInviteImpl GuildInvitor;

	public RaidFindImpl FindInvitor;

	public float InviteCooltime = 10f;

	public void Open(Listener listener, long[] invitedAccountIds, bool enableFriend = true)
	{
		Open();
		if (enableFriend)
		{
			FriendInvitor.Open(GetFriendList(), invitedAccountIds, RaidInviteComponent.InviteeType.Friend, InviteCooltime);
			FriendInvitor.InviteAllMemberCallback = delegate
			{
				listener.InviteAllFriend(this);
			};
			FriendInvitor.InviteCallback = delegate(RaidInviteComponent component)
			{
				listener.Invite(this, component);
			};
		}
		UIHelper.EnableGameObject(FriendInvitor, enableFriend);
		GuildInvitor.Open(GetGuildList(), invitedAccountIds, RaidInviteComponent.InviteeType.Guild, InviteCooltime);
		GuildInvitor.InviteAllMemberCallback = delegate
		{
			listener.InviteAllGuild(this);
		};
		GuildInvitor.InviteCallback = delegate(RaidInviteComponent component)
		{
			listener.Invite(this, component);
		};
		if (!enableFriend)
		{
			GuildInvitor.ForceEnableNoMember(enableFriend);
		}
		FindInvitor.Open(invitedAccountIds, RaidInviteComponent.InviteeType.Found, InviteCooltime);
		FindInvitor.InviteCallback = delegate(RaidInviteComponent component)
		{
			listener.Invite(this, component);
		};
		FindInvitor.FindCallback = listener.FindUser;
	}

	private IList<CachedUserInfo> GetFriendList()
	{
		return GlobalAutoBehaviour<AccountManager>.instance.FriendManager.GetFriendInfos();
	}

	private IList<CachedUserInfo> GetGuildList()
	{
		if (!GlobalAutoBehaviour<GuildManager>.isValidInstance)
		{
			return null;
		}
		return GlobalAutoBehaviour<GuildManager>.instance.MemberManager?.Where((GuildMemberInfo e) => e.AccountId != GlobalAutoBehaviour<AccountManager>.instance.UserManager.UserInfo.AccountId).ToArray();
	}

	public override void ReserveCache(UiPreloader preloader)
	{
		base.ReserveCache(preloader);
	}

	public override void Reset()
	{
		base.Reset();
		FriendInvitor.Reset();
		GuildInvitor.Reset();
		FindInvitor.Reset();
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		FriendInvitor.Update();
		GuildInvitor.Update();
		FindInvitor.Update();
	}

	public void UpdateInvited(long accountId, bool invited)
	{
		FriendInvitor.UpdateInvited(accountId, invited);
		GuildInvitor.UpdateInvited(accountId, invited);
		FindInvitor.UpdateInvited(accountId, invited);
	}
}
