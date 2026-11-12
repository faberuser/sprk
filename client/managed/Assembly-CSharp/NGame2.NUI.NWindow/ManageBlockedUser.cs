using System.Collections.Generic;
using NGame2.NChatting.NUserBlock;
using NGame2.NUI.NComponent2;
using NVespa;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class ManageBlockedUser : BaseWindow, BlockedUserComponent.Listener
{
	public interface Listener
	{
		void OnUnblock(ManageBlockedUser caller, long accountId, string userName);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/GameOption/ManageBlockedUser";

	public UIScrollGrid Grid;

	public Transform NoUserRoot;

	public UILabel CountLabel;

	private Listener _listener;

	private List<BlockedUserComponent> _componentList = new List<BlockedUserComponent>();

	public void Open(Listener listener)
	{
		base.Open();
		_listener = listener;
		InitBlockedUserList();
		UpdateCountLabel();
	}

	public override void Reset()
	{
		base.Reset();
		ClearBlockedUserList();
	}

	public void OnClickExitButton()
	{
		Close();
	}

	private void ClearBlockedUserList()
	{
		FactoryBase.ReleaseAndClearList(_componentList);
	}

	private void InitBlockedUserList()
	{
		List<BlockedUser> allBlockedUserList = SingletonAuto<UserBlockManager>.instance.GetAllBlockedUserList();
		if (__Extension_Method_ICollection__.COUNT(allBlockedUserList) == 0)
		{
			UIHelper.EnableGameObject(NoUserRoot, enable: true);
			return;
		}
		UIHelper.EnableGameObject(NoUserRoot, enable: false);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(allBlockedUserList); i++)
		{
			BlockedUser blockedUser = allBlockedUserList[i];
			BlockedUserComponent blockedUserComponent = Grid.AddGridItem(BlockedUserComponent.PrefabAuto);
			blockedUserComponent.Open(this, blockedUser.AccountId, blockedUser.UserName);
			_componentList.Add(blockedUserComponent);
		}
		Grid.RepositionGrid();
	}

	public void RemoveBlockedUser(long accountId)
	{
		BlockedUserComponent comp = _componentList.Find((BlockedUserComponent e) => e.AccountId == accountId);
		if (!(comp == null))
		{
			_componentList.Remove(comp);
			FactoryBase.SafeReleaseComponent(ref comp);
			Grid.RepositionGrid();
			UpdateCountLabel();
		}
	}

	private void UpdateCountLabel()
	{
		int maxBlockUserCount = SingletonAuto<UserBlockManager>.instance.GetMaxBlockUserCount();
		int blockedUserCount = SingletonAuto<UserBlockManager>.instance.GetBlockedUserCount();
		UIHelper.SetLabelText(CountLabel, blockedUserCount + "/" + maxBlockUserCount);
	}

	void BlockedUserComponent.Listener.OnClickUnblock(BlockedUserComponent caller)
	{
		if (!(caller == null) && _listener != null)
		{
			long accountId = caller.AccountId;
			string userName = caller.UserName;
			_listener.OnUnblock(this, accountId, userName);
		}
	}
}
