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

public class RaidFindImpl : MonoBehaviour, RaidInviteComponent.Listener
{
	public UILabel searchLabel;

	public UILabel canSearchLabel;

	public UILabel canNotFindLabel;

	public UIButton findButton;

	public UISimpleGrid userGrid;

	private RaidInviteComponent _foundComponent;

	private float _inviteCooltime;

	private List<long> _invitedAccountIds = new List<long>();

	private RaidInviteComponent.InviteeType _inviteeType;

	public Action<RaidInviteComponent> InviteCallback { get; set; }

	public Action<string, Action<CachedUserInfo>> FindCallback { get; set; }

	public void Open(long[] invitedAccountIds, RaidInviteComponent.InviteeType inviteeType, float inviteCooltime)
	{
		_invitedAccountIds.Clear();
		if (invitedAccountIds != null)
		{
			_invitedAccountIds = invitedAccountIds.ToList();
		}
		_inviteCooltime = inviteCooltime;
		_inviteeType = inviteeType;
		RefreshUI(null, bInit: true);
	}

	public void Reset()
	{
		FactoryBase.SafeReleaseComponent(ref _foundComponent);
		UpdateButtonState();
	}

	public void Update()
	{
		UpdateButtonState();
	}

	public void UpdateInvited(long accountId, bool invited)
	{
		if (_foundComponent != null && _foundComponent.AccountId == accountId)
		{
			_foundComponent.SetInvited(invited);
		}
		if (invited && !_invitedAccountIds.Contains(accountId))
		{
			_invitedAccountIds.Add(accountId);
		}
		else if (!invited && _invitedAccountIds.Contains(accountId))
		{
			_invitedAccountIds.Remove(accountId);
		}
	}

	private void RefreshUI(CachedUserInfo userInfo, bool bInit = false)
	{
		FactoryBase.SafeReleaseComponent(ref _foundComponent);
		UIHelper.EnableGameObject(canSearchLabel, bInit);
		if (bInit)
		{
			UIHelper.SetLabelText(searchLabel, string.Empty);
			UIHelper.EnableGameObject(canNotFindLabel, enable: false);
			return;
		}
		UIHelper.EnableGameObject(canNotFindLabel, userInfo == null);
		if (userInfo != null)
		{
			_foundComponent = userGrid.AddGridItem(RaidInviteComponent.PrefabAuto);
			_foundComponent.Open(this, _inviteeType, userInfo);
			_foundComponent.SetInvited(_invitedAccountIds != null && _invitedAccountIds.Contains(userInfo.AccountId));
		}
		UIHelper.RepositionGrid(userGrid);
	}

	private bool CanFind()
	{
		if (String_Extension_Method.IsNullOrEmpty(searchLabel.text))
		{
			return false;
		}
		if (!(_foundComponent == null))
		{
			return _foundComponent.CanInvite();
		}
		return true;
	}

	private void UpdateButtonState()
	{
		UIHelper.EnableButton(findButton, CanFind());
	}

	public void OnClickFind()
	{
		Vespa_System_Action_Extension_Method.fire(FindCallback, searchLabel.text, delegate(CachedUserInfo userInfo)
		{
			RefreshUI(userInfo);
		});
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
