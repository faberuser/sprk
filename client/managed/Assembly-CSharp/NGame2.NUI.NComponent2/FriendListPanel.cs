using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NWindow;
using NShared;
using NShared.NLocalization;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class FriendListPanel : MonoBehaviour, FriendSlotComponent.Listener
{
	public interface Listener
	{
		void Whisper(long friendId, string message);

		void ShowHeroes(long friendId);

		void Delete(long friendId, string friendNick);

		void InviteParty(long friendId);

		void Duel(long friendId);

		void SendFriendshipPoint(long friendId);

		void RecvFriendshipPoint(long friendId);

		void SendRecvFriendshipPoint();
	}

	public const int MaxFriendCount = 50;

	public const string PrefabPath = "Resources/Prefab2/Component/Friend/FriendListPanel";

	public static PrefabAuto<FriendListPanel> PrefabAuto = new PrefabAuto<FriendListPanel>("Resources/Prefab2/Component/Friend/FriendListPanel");

	public UIScrollGrid FriendListGrid;

	public UILabel Label_NoFriend;

	public UILabel LabelCurrentCount;

	public UILabel LabelMaxCount;

	public UILabel Label_CurDailyAccFriendshipPoint;

	public UILabel Label_MaxDailyAccFriendshipPoint;

	public UIButton Button_SendRecvFriendshipPoint;

	private List<FriendSlotComponent> _componentList = new List<FriendSlotComponent>();

	private Listener _listener;

	public void Open(Listener listener)
	{
		Reset();
		base.gameObject.SetActive(value: true);
		_listener = listener;
		UIHelper.SetLabelText(LabelMaxCount, 50);
		SetupDailyAccFriendshipPoint();
		SetupFriendSlotComponent();
		UpdateSendRecvFriendButton();
	}

	public void Close()
	{
		base.gameObject.SetActive(value: false);
		Reset();
	}

	public void Reset()
	{
		_listener = null;
		ClearList();
	}

	public void UpdateList()
	{
		ClearList();
		SetupFriendSlotComponent();
	}

	public void UpdateStatus(long friendId)
	{
		FriendSlotComponent friendSlotComponent = GetFriendSlotComponent(friendId);
		if (!(friendSlotComponent == null))
		{
			friendSlotComponent.UpdateButtonStatus();
			UIHelper.RepositionGrid(FriendListGrid);
		}
	}

	public void UpdateAllStatus()
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_componentList); i++)
		{
			_componentList[i].UpdateButtonStatus();
		}
		UIHelper.RepositionGrid(FriendListGrid);
	}

	public void UpdateChangedInfo(long friendId)
	{
		FriendSlotComponent friendSlotComponent = GetFriendSlotComponent(friendId);
		if (!(friendSlotComponent == null))
		{
			friendSlotComponent.Refresh();
			UIHelper.RepositionGrid(FriendListGrid);
		}
	}

	public void UpdateSendRecvFriendButton()
	{
		UIHelper.EnableButton(Button_SendRecvFriendshipPoint, GlobalAutoBehaviour<AccountManager>.instance.SendRecvFriendshipPointManager.SendOrRecvFriendshipPointFriendExist());
	}

	private void ClearList()
	{
		FactoryBase.ReleaseAndClearList(_componentList);
	}

	private FriendSlotComponent GetFriendSlotComponent(long friendId)
	{
		return _componentList.Find((FriendSlotComponent e) => e.FriendInfo != null && e.FriendInfo.AccountId == friendId);
	}

	public void SetupDailyAccFriendshipPoint()
	{
		long dailyAccFriendshipPoint = GlobalAutoBehaviour<AccountManager>.instance.UserManager.MiscInfo.DailyAccFriendshipPoint;
		int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("MaxDailyAccFriendshipPoint");
		UIHelper.SetLabelText(Label_CurDailyAccFriendshipPoint, dailyAccFriendshipPoint);
		UIHelper.SetLabelText(Label_MaxDailyAccFriendshipPoint, valueInt);
	}

	private TimeSpan GetElapsedTime(FriendUserInfo friendUserInfo)
	{
		DateTime o_dateTime;
		bool num = Vespa_String_Extension_Method2.TryParseNetworkTime2(friendUserInfo.LastLoginTime, out o_dateTime);
		DateTime o_dateTime2;
		bool flag = Vespa_String_Extension_Method2.TryParseNetworkTime2(friendUserInfo.LastLogoutTime, out o_dateTime2);
		DateTime dateTime = ((num && flag) ? ((o_dateTime > o_dateTime2) ? o_dateTime : o_dateTime2) : ((!flag) ? o_dateTime : o_dateTime2));
		return NGame2.NUI.NWindow.Common.GetUTCNow() - dateTime;
	}

	private void SetupFriendSlotComponent()
	{
		FriendUserInfo[] friendInfos = GlobalAutoBehaviour<AccountManager>.instance.FriendManager.GetFriendInfos();
		Array.Sort(friendInfos, delegate(FriendUserInfo lhs, FriendUserInfo rhs)
		{
			if (lhs.IsOnline != rhs.IsOnline)
			{
				if (!lhs.IsOnline)
				{
					return 1;
				}
				return -1;
			}
			if (lhs.IsOnline && rhs.IsOnline)
			{
				return rhs.TeamLevel.CompareTo(lhs.TeamLevel);
			}
			TimeSpan elapsedTime = GetElapsedTime(lhs);
			TimeSpan elapsedTime2 = GetElapsedTime(rhs);
			int num3 = 0;
			int num4 = (int)elapsedTime.TotalDays;
			int num5 = (int)elapsedTime.TotalHours;
			int num6 = (int)elapsedTime.TotalMinutes;
			int num7 = (int)elapsedTime2.TotalDays;
			int num8 = (int)elapsedTime2.TotalHours;
			int num9 = (int)elapsedTime2.TotalMinutes;
			if (num4 >= 1 || num7 >= 1)
			{
				num3 = num4.CompareTo(num7);
			}
			else if (num5 >= 1 || num8 >= 1)
			{
				num3 = num5.CompareTo(num8);
			}
			else if (num6 >= 1 || num9 >= 1)
			{
				num3 = num6.CompareTo(num9);
			}
			if (num3 == 0)
			{
				num3 = rhs.TeamLevel.CompareTo(lhs.TeamLevel);
			}
			if (num3 == 0)
			{
				num3 = lhs.AccountId.CompareTo(rhs.AccountId);
			}
			return num3;
		});
		int num = 0;
		FriendUserInfo[] array = friendInfos;
		foreach (FriendUserInfo friendInfo in array)
		{
			FriendSlotComponent friendSlotComponent = UIHelper.AddGridItem(FriendListGrid, FriendSlotComponent.PrefabAuto);
			friendSlotComponent.Open(friendInfo, this);
			_componentList.Add(friendSlotComponent);
			num++;
		}
		UIHelper.SetLabelText(LabelCurrentCount, num);
		UIHelper.EnableGameObject(Label_NoFriend, num == 0);
		UIHelper.RepositionGrid(FriendListGrid);
	}

	private int CompareFriendSlot(Transform lhs, Transform rhs)
	{
		FriendSlotComponent component = lhs.GetComponent<FriendSlotComponent>();
		FriendSlotComponent component2 = rhs.GetComponent<FriendSlotComponent>();
		if (component == null || component2 == null)
		{
			return 0;
		}
		if (component.FriendInfo.IsOnline != component2.FriendInfo.IsOnline)
		{
			if (!component.FriendInfo.IsOnline)
			{
				return 1;
			}
			return -1;
		}
		if (component.FriendInfo.Nick != component2.FriendInfo.Nick)
		{
			string strA = component.FriendInfo.Nick ?? string.Empty;
			string strB = component2.FriendInfo.Nick ?? string.Empty;
			return string.Compare(strA, strB);
		}
		Vespa_String_Extension_Method2.TryParseNetworkTime2(component.FriendInfo.LastLogoutTime, out var o_dateTime);
		Vespa_String_Extension_Method2.TryParseNetworkTime2(component2.FriendInfo.LastLogoutTime, out var o_dateTime2);
		if (!(o_dateTime > o_dateTime2))
		{
			return 1;
		}
		return -1;
	}

	public void OnClickSendRecvFriendshipPoint()
	{
		if (_listener != null)
		{
			_listener.SendRecvFriendshipPoint();
		}
	}

	void FriendSlotComponent.Listener.Whisper(FriendSlotComponent This)
	{
		if (_listener != null)
		{
			_listener.Whisper(This.FriendInfo.AccountId, "TEST WHISPER MESSAGE");
		}
	}

	void FriendSlotComponent.Listener.ShowHeroes(FriendSlotComponent This)
	{
		if (_listener != null)
		{
			_listener.ShowHeroes(This.FriendInfo.AccountId);
		}
	}

	void FriendSlotComponent.Listener.Delete(FriendSlotComponent This)
	{
		if (_listener != null)
		{
			_listener.Delete(This.FriendInfo.AccountId, This.FriendInfo.Nick);
		}
	}

	void FriendSlotComponent.Listener.InviteParty(FriendSlotComponent This)
	{
		if (_listener != null)
		{
			LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
			string message = LocalizationManager.GetString(Friend.FRIEND_ASK_INVITE_PARTY, This.FriendInfo.Nick);
			MessagePopup.Open("", message, delegate
			{
				_listener.InviteParty(This.FriendInfo.AccountId);
			}, null);
		}
	}

	void FriendSlotComponent.Listener.Duel(FriendSlotComponent This)
	{
		if (_listener != null)
		{
			LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
			string message = "Request duel?";
			MessagePopup.Open("", message, delegate
			{
				_listener.Duel(This.FriendInfo.AccountId);
			}, null);
		}
	}

	void FriendSlotComponent.Listener.InviteAccept(FriendSlotComponent This)
	{
	}

	void FriendSlotComponent.Listener.InviteDeny(FriendSlotComponent This)
	{
	}

	void FriendSlotComponent.Listener.InviteRequest(FriendSlotComponent This)
	{
	}

	void FriendSlotComponent.Listener.sendFriendshipPoint(FriendSlotComponent This)
	{
		if (_listener != null && This.FriendInfo != null)
		{
			_listener.SendFriendshipPoint(This.FriendInfo.AccountId);
		}
	}

	void FriendSlotComponent.Listener.recvFriendshipPoint(FriendSlotComponent This)
	{
		if (_listener != null && This.FriendInfo != null)
		{
			_listener.RecvFriendshipPoint(This.FriendInfo.AccountId);
		}
	}
}
