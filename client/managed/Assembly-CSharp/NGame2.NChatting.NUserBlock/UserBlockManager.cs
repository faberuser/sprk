using System.Collections.Generic;
using NVespa;

namespace NGame2.NChatting.NUserBlock;

public class UserBlockManager : SingletonAuto<UserBlockManager>
{
	private LocalStorageBlockedUserArray _blockedUsers;

	public UserBlockManager()
	{
		_blockedUsers = new LocalStorageBlockedUserArray(null, "UserBlockManager");
	}

	public void BlockUser(long accountId, string userName)
	{
		_blockedUsers.AddBlockedUser(accountId, userName);
	}

	public void UnblockUser(long accountId)
	{
		_blockedUsers.RemoveBlockedUser(accountId);
	}

	public bool IsBlockedUser(long accountId)
	{
		return _blockedUsers.IsBlockedUser(accountId);
	}

	public int GetBlockedUserCount()
	{
		return _blockedUsers.GetBlockedUserCount();
	}

	public int GetMaxBlockUserCount()
	{
		return 100;
	}

	public bool CanBlockMore()
	{
		return GetBlockedUserCount() < GetMaxBlockUserCount();
	}

	public List<BlockedUser> GetAllBlockedUserList()
	{
		return _blockedUsers.GetAllBlockedUserList();
	}
}
