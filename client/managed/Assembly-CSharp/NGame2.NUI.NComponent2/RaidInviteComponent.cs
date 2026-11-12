using System.Collections.Generic;
using NGameAsset;
using NShared;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class RaidInviteComponent : LinkFactoryComponent, InitOnCreate
{
	public interface Listener
	{
		void OnClickInvite(RaidInviteComponent caller);

		void OnClickComponent(RaidInviteComponent caller);
	}

	public enum InviteeType
	{
		Friend,
		Guild,
		Found
	}

	public const string PrefabPath = "Prefab2/Component/Raid/RaidInviteComponent";

	public static PrefabAuto<RaidInviteComponent> PrefabAuto = new PrefabAuto<RaidInviteComponent>("Prefab2/Component/Raid/RaidInviteComponent");

	public UITexture PortraitTexture;

	public UILabel NameLabel;

	public UILabel LevelLabel;

	public Transform OfflineRoot;

	public Transform RequestRoot;

	public Transform RequestingRoot;

	public Transform InvitedRoot;

	public UIButton InviteButton;

	private Listener _listener;

	private long _accountId;

	private int _avatarHeroIndex;

	private int _teamLevel;

	private string _nick;

	private bool _isOnline;

	private InviteeType _inviteeType;

	private float _lastUpdateTime;

	private static Dictionary<long, float> _cooltimeEndTimeDictionary = new Dictionary<long, float>();

	private bool _invited;

	public long AccountId => _accountId;

	public int AvatarHeroIndex => _avatarHeroIndex;

	public int TeamLevel => _teamLevel;

	public string Nick => _nick;

	public InviteeType Type => _inviteeType;

	public CachedUserInfo UserInfo { get; set; }

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		ClosePortrait();
	}

	public void Open(Listener listener, InviteeType type, CachedUserInfo userInfo)
	{
		if (userInfo != null)
		{
			_listener = listener;
			_inviteeType = type;
			_accountId = userInfo.AccountId;
			_avatarHeroIndex = userInfo.AvatarHeroIndex;
			_teamLevel = userInfo.TeamLevel;
			_nick = userInfo.Nick;
			_isOnline = userInfo.IsOnline;
			_lastUpdateTime = 0f;
			_invited = false;
			UserInfo = userInfo;
			InitPortrait(_avatarHeroIndex);
			InitInfo(_nick, _teamLevel);
			UpdateButtonState();
		}
	}

	public void StartCooltime(float cooltimeEndTime)
	{
		_cooltimeEndTimeDictionary[_accountId] = cooltimeEndTime;
		UpdateButtonState();
	}

	public void SetInvited(bool invited)
	{
		_invited = invited;
		UpdateButtonState();
	}

	private void ClosePortrait()
	{
		UIHelper.ClearPlaneAssetTexture(PortraitTexture);
	}

	private void InitPortrait(int avatarHeroIndex)
	{
		string iconPath = UIAvatarHelper.GetIconPath(avatarHeroIndex);
		if (!String_Extension_Method.IsNullOrEmpty(iconPath))
		{
			AssetTexture plainTexture = UIHelper.GetPlainTexture(iconPath);
			UIHelper.ResetPlaneAssetTextureAsync(this, PortraitTexture, plainTexture);
		}
	}

	private void InitInfo(string nick, int level)
	{
		UIHelper.SetLabelText(NameLabel, nick);
		UIHelper.SetLabelText(LevelLabel, level);
	}

	private void Update()
	{
		if ((double)(Time.time - _lastUpdateTime) > 0.1)
		{
			_lastUpdateTime = Time.time;
			UpdateButtonState();
		}
	}

	public bool CanInvite()
	{
		float num = (_cooltimeEndTimeDictionary.ContainsKey(_accountId) ? _cooltimeEndTimeDictionary[_accountId] : 0f);
		if (Time.time < num)
		{
			return false;
		}
		return true;
	}

	private void UpdateButtonState()
	{
		if (UserInfo != null)
		{
			if (_invited)
			{
				UIHelper.EnableGameObject(InvitedRoot, enable: true);
				UIHelper.EnableGameObject(RequestRoot, enable: false);
				UIHelper.EnableGameObject(RequestingRoot, enable: false);
				UIHelper.EnableGameObject(OfflineRoot, enable: false);
			}
			else if (!UserInfo.IsOnline)
			{
				UIHelper.EnableGameObject(InvitedRoot, enable: false);
				UIHelper.EnableGameObject(RequestRoot, enable: false);
				UIHelper.EnableGameObject(RequestingRoot, enable: false);
				UIHelper.EnableGameObject(OfflineRoot, enable: true);
			}
			else
			{
				bool flag = CanInvite();
				UIHelper.EnableGameObject(InvitedRoot, enable: false);
				UIHelper.EnableGameObject(RequestRoot, flag);
				UIHelper.EnableGameObject(RequestingRoot, !flag);
				UIHelper.EnableGameObject(OfflineRoot, enable: false);
			}
		}
	}

	public void OnClickInvite()
	{
		if (CanInvite() && _listener != null)
		{
			_listener.OnClickInvite(this);
		}
	}

	public void OnClickComponent()
	{
		if (_listener != null)
		{
			_listener.OnClickComponent(this);
		}
	}
}
