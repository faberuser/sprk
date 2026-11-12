using System;
using System.Collections.Generic;
using System.Linq;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class RaidInviteImpl : MonoBehaviour, RaidInviteComponent.Listener
{
	public UIScrollGrid MemberGrid;

	public UIButton InviteAllMemberButton;

	public Transform NoMemberRoot;

	private List<RaidInviteComponent> _componentList = new List<RaidInviteComponent>();

	private float _inviteCooltime;

	private static float _inviteAllMemberTime;

	public Action<RaidInviteComponent> InviteCallback { get; set; }

	public Action InviteAllMemberCallback { get; set; }

	public void Open(IList<CachedUserInfo> userInfos, long[] invitedAccountIds, RaidInviteComponent.InviteeType inviteeType, float inviteCooltime)
	{
		_inviteCooltime = inviteCooltime;
		RefreshMemberList(userInfos, invitedAccountIds, inviteeType);
	}

	public void Reset()
	{
		ClearMemberList();
		UpdateButtonState();
	}

	public void Update()
	{
		UpdateButtonState();
	}

	public void UpdateInvited(long accountId, bool invited)
	{
		RaidInviteComponent raidInviteComponent = _componentList.FirstOrDefault((RaidInviteComponent e) => e.AccountId == accountId);
		if ((bool)raidInviteComponent)
		{
			raidInviteComponent.SetInvited(invited);
		}
	}

	private void ClearMemberList()
	{
		FactoryBase.ReleaseAndClearList(_componentList);
	}

	private void RefreshMemberList(IList<CachedUserInfo> userInfos, long[] invitedAccountIds, RaidInviteComponent.InviteeType inviteeType)
	{
		ClearMemberList();
		if (userInfos == null)
		{
			return;
		}
		List<CachedUserInfo> list = new List<CachedUserInfo>(userInfos);
		if (list != null)
		{
			list.RemoveAll(RemoveCondition);
			list.Sort(delegate(CachedUserInfo lhs, CachedUserInfo rhs)
			{
				bool isOnline = lhs.IsOnline;
				int num2 = rhs.IsOnline.CompareTo(isOnline);
				if (num2 != 0)
				{
					return num2;
				}
				num2 = rhs.TeamLevel.CompareTo(lhs.TeamLevel);
				return (num2 != 0) ? num2 : lhs.Nick.CompareTo(rhs.Nick);
			});
		}
		for (int num = 0; num < list.Count; num++)
		{
			CachedUserInfo cachedUserInfo = list[num];
			RaidInviteComponent raidInviteComponent = UIHelper.AddGridItem(MemberGrid, RaidInviteComponent.PrefabAuto);
			raidInviteComponent.Open(this, inviteeType, cachedUserInfo);
			raidInviteComponent.SetInvited(invitedAccountIds != null && TypeIterator.Contains(invitedAccountIds, cachedUserInfo.AccountId));
			_componentList.Add(raidInviteComponent);
		}
		UIHelper.RepositionGrid(MemberGrid);
		UIHelper.EnableGameObject(NoMemberRoot, __Extension_Method_ICollection__.COUNT(_componentList) == 0);
	}

	public void ForceEnableNoMember(bool enable)
	{
		UIHelper.EnableGameObject(NoMemberRoot, enable);
	}

	public void OnClickInviteAllMember()
	{
		if (CanInviteAllMember())
		{
			Vespa_System_Action_Extension_Method.fire(InviteAllMemberCallback);
			float cooltimeEndTime = Time.time + _inviteCooltime;
			_componentList.ForEach(delegate(RaidInviteComponent e)
			{
				e.StartCooltime(cooltimeEndTime);
			});
			_inviteAllMemberTime = Time.time;
		}
	}

	private bool CanInviteAllMember()
	{
		if (__Extension_Method_ICollection__.COUNT(_componentList) > 0)
		{
			return Time.time - _inviteAllMemberTime > _inviteCooltime;
		}
		return false;
	}

	private void UpdateButtonState()
	{
		UIHelper.EnableButton(InviteAllMemberButton, CanInviteAllMember());
	}

	private bool RemoveCondition(CachedUserInfo userInfo)
	{
		if (userInfo.TeamLevel != 0)
		{
			return string.IsNullOrEmpty(userInfo.Nick);
		}
		return true;
	}

	public CachedUserInfo GetUserInfo(string nick)
	{
		RaidInviteComponent raidInviteComponent = _componentList.Find((RaidInviteComponent e) => e.Nick.Equals(nick));
		if (!(raidInviteComponent != null))
		{
			return null;
		}
		return raidInviteComponent.UserInfo;
	}

	void RaidInviteComponent.Listener.OnClickInvite(RaidInviteComponent caller)
	{
		caller.StartCooltime(Time.time + _inviteCooltime);
		Vespa_System_Action_Extension_Method.fire(InviteCallback, caller);
	}

	void RaidInviteComponent.Listener.OnClickComponent(RaidInviteComponent caller)
	{
		InspectUserManagement.InspectUserData inspectUser = new InspectUserManagement.InspectUserData(caller.AccountId, caller.AvatarHeroIndex, caller.TeamLevel, caller.Nick, InspectUserCategory.Raid);
		SceneAutoBehaviour<InspectUserManagement>.instance.OpenMenuPopup(inspectUser);
	}
}
