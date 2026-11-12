using System.Collections.Generic;
using LitJson;
using NVespa;

namespace NGame2.NChatting.NUserBlock;

public class LocalStorageBlockedUserArray
{
	private NVespa.LocalStorageGenericArray<BlockedUser> _inner;

	private Dictionary<long, long> _accountIdToIndex;

	public LocalStorageBlockedUserArray(BlockedUser i_default, string i_keyPrefix)
	{
		_inner = new NVespa.LocalStorageGenericArray<BlockedUser>(i_default, i_keyPrefix, GetLocalValue, SetLocalValue, EqualValue);
		_accountIdToIndex = new Dictionary<long, long>();
		Init();
	}

	private BlockedUser GetLocalValue(string key, BlockedUser defaultValue)
	{
		if (!SprkTestPrefs.HasKey(key))
		{
			return defaultValue;
		}
		string text = SprkTestPrefs.GetString(key);
		if (String_Extension_Method.IsNullOrEmpty(text))
		{
			return defaultValue;
		}
		return BlockedUser.CreateFromJson(JsonMapper.ToObject(text)) ?? defaultValue;
	}

	private void SetLocalValue(string key, BlockedUser value)
	{
		if (value == null)
		{
			SprkTestPrefs.DeleteKey(key);
		}
		else
		{
			SprkTestPrefs.SetString(key, value.ToJson());
		}
	}

	private bool EqualValue(BlockedUser lhs, BlockedUser rhs)
	{
		if (lhs == null && rhs == null)
		{
			return true;
		}
		if (lhs == null || rhs == null)
		{
			return false;
		}
		return lhs.AccountId == rhs.AccountId;
	}

	private void Init()
	{
		List<BlockedUser> allBlockedUserList = GetAllBlockedUserList();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(allBlockedUserList); i++)
		{
			BlockedUser blockedUser = allBlockedUserList[i];
			_accountIdToIndex[blockedUser.AccountId] = i;
		}
	}

	public List<BlockedUser> GetAllBlockedUserList()
	{
		List<BlockedUser> list = null;
		int num = 1000;
		int num2 = 0;
		while (true)
		{
			if (num2 >= num)
			{
				VespaDebug.LogError("Read blockedUser to max limit of " + num);
				break;
			}
			BlockedUser blockedUser = _inner[num2];
			if (blockedUser == null || blockedUser.AccountId == 0L)
			{
				break;
			}
			list = list ?? new List<BlockedUser>();
			list.Add(blockedUser);
			num2++;
		}
		return list;
	}

	public bool IsBlockedUser(long accountId)
	{
		return _accountIdToIndex.ContainsKey(accountId);
	}

	public int GetBlockedUserCount()
	{
		return __Extension_Method_ICollection__.COUNT(_accountIdToIndex);
	}

	public void AddBlockedUser(long accountId, string userName)
	{
		long num = __Extension_Method_ICollection__.COUNT(_accountIdToIndex);
		if (IsBlockedUser(accountId))
		{
			num = _accountIdToIndex[accountId];
		}
		_inner[num] = new BlockedUser
		{
			AccountId = accountId,
			UserName = userName
		};
		_accountIdToIndex[accountId] = num;
	}

	public bool RemoveBlockedUser(long accountId)
	{
		if (!IsBlockedUser(accountId))
		{
			return false;
		}
		long num = _accountIdToIndex[accountId];
		int num2 = __Extension_Method_ICollection__.COUNT(_accountIdToIndex) - 1;
		BlockedUser blockedUser = _inner[num2];
		long accountId2 = blockedUser.AccountId;
		if (accountId2 == accountId)
		{
			_accountIdToIndex.Remove(accountId);
			_inner[num] = null;
		}
		else
		{
			_inner[num] = blockedUser;
			_inner[num2] = null;
			_accountIdToIndex.Remove(accountId);
			_accountIdToIndex[accountId2] = num;
		}
		return true;
	}
}
