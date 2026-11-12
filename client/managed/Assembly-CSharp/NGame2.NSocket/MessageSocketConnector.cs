using System;
using NGame2.NChatting;
using NShared;
using NShared.NLocalization;
using NShared.NMessage;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using NVespa.NSocket;
using UnityEngine;

namespace NGame2.NSocket;

public class MessageSocketConnector : SocketConnector
{
	public const int PingIntarval = 30;

	private long _accountId;

	private long _guildId;

	private long[] _friendIds;

	private bool _initialized;

	private DateTime _lastConnectTriedTime;

	private DateTime _lastPingTime;

	private IMessageLogHandler _messageLogHandler;

	private int _tryCount;

	private double _tryTime;

	private const int MAX_TRY_SECOND = 16;

	public Action<LoginRes> OnMessageServerConnected { get; set; }

	public Action<int> OnEnterChannel { get; set; }

	public Action OnMessageServerDisconnected { get; set; }

	public long GuildId => _guildId;

	public MessageServerInfo ServerInfo { get; private set; }

	public int ChannelNo { get; private set; }

	public void Init(long accountId, long guildId, long[] friendIds, MessageServerInfo serverInfo)
	{
		Application.runInBackground = true;
		base.SocketHandler.Register<MessageNot>(OnMessageNot);
		_accountId = accountId;
		_guildId = guildId;
		_friendIds = friendIds;
		ServerInfo = serverInfo;
		_initialized = true;
		_lastConnectTriedTime = DateTime.UtcNow;
		_lastPingTime = DateTime.UtcNow;
		ResetTryCount();
		TryConnect();
	}

	public override void ChainUpdate()
	{
		base.ChainUpdate();
		if (_initialized && base.State == ConnectState.Disconnected && (DateTime.UtcNow - _lastConnectTriedTime).TotalSeconds > _tryTime)
		{
			TryConnect();
		}
		if (base.State == ConnectState.Connected && (DateTime.UtcNow - _lastPingTime).TotalSeconds > 30.0)
		{
			SendPing();
		}
	}

	private void SendPing()
	{
		_lastPingTime = DateTime.UtcNow;
		NShared.NMessage.PingReq data = new NShared.NMessage.PingReq();
		Request<NShared.NMessage.PingReq, NShared.NMessage.PingRes>(data, null);
	}

	private void TryConnect()
	{
		IncreaseTryCount();
		_lastConnectTriedTime = DateTime.UtcNow;
		Connect(ServerInfo);
	}

	public void Connect(MessageServerInfo serverInfo)
	{
		if (serverInfo != null)
		{
			string address = serverInfo.Address;
			int port = serverInfo.Port;
			Connect(address, port, delegate(bool connected)
			{
				OnConnected(connected);
			});
		}
	}

	private void OnConnected(bool connected)
	{
		_lastConnectTriedTime = DateTime.UtcNow;
		if (connected)
		{
			ResetTryCount();
			LoginReq loginReq = new LoginReq();
			loginReq.AccountId = _accountId;
			loginReq.GuildId = _guildId;
			loginReq.FriendIds = _friendIds;
			loginReq.ChannelNo = ChannelNo;
			Request<LoginReq, LoginRes>(loginReq, OnLoginRes);
		}
	}

	private void OnLoginRes(LoginRes response)
	{
		if (response == null)
		{
			VespaDebug.LogError("Failed to message login : response is null.");
		}
		else if (response.Result != LoginResType.Success)
		{
			VespaDebug.LogError("Failed to message login : " + response.Result);
			Close();
		}
		else
		{
			ChannelNo = response.ChannelNo;
			Vespa_System_Action_Extension_Method.fire(OnMessageServerConnected, response);
			Vespa_System_Action_Extension_Method.fire(OnEnterChannel, ChannelNo);
		}
	}

	public override void OnClosed()
	{
		base.OnClosed();
		if (!SystemStatus.IsApplicationQuit)
		{
			Vespa_System_Action_Extension_Method.fire(OnMessageServerDisconnected);
		}
	}

	public void SetMessageLogHandler(IMessageLogHandler handler)
	{
		_messageLogHandler = handler;
	}

	private void OnMessageNot(MessageNot data)
	{
		if (_messageLogHandler != null)
		{
			_messageLogHandler.Handle(data);
		}
	}

	public void ChangeFriend(long[] friendIds)
	{
		_friendIds = friendIds;
		ChangeFriendReq changeFriendReq = new ChangeFriendReq();
		changeFriendReq.FriendIds = friendIds;
		Request<ChangeFriendReq, ChangeFriendRes>(changeFriendReq, delegate
		{
		});
	}

	public void ChangeGuild(long guildId)
	{
		if (guildId != _guildId)
		{
			_guildId = guildId;
			ChangeGuildReq changeGuildReq = new ChangeGuildReq();
			changeGuildReq.GuildId = guildId;
			Request<ChangeGuildReq, ChangeGuildRes>(changeGuildReq, delegate
			{
			});
		}
	}

	public void ChangeChannel(int channelNo)
	{
		if (channelNo <= 0 || channelNo == ChannelNo)
		{
			return;
		}
		ChangeChannelReq changeChannelReq = new ChangeChannelReq();
		changeChannelReq.ChannelNo = channelNo;
		Request(changeChannelReq, delegate(ChangeChannelRes response)
		{
			if (response != null && response.Changed)
			{
				ChannelNo = channelNo;
				Vespa_System_Action_Extension_Method.fire(OnEnterChannel, channelNo);
			}
			else
			{
				string chat = LocalizationManager.GetString(Chatting.CHAT_CHANNEL_IS_FULL);
				GlobalAutoBehaviour<ChattingManager>.instance.AddSystemChat(chat, ChatType.Channel, showPreview: false, 0L);
			}
		});
	}

	private void IncreaseTryCount()
	{
		if (_tryCount >= 1)
		{
			VespaDebug.LogError("MessageSocket RetryCount = " + _tryCount + " Time : " + _tryTime);
		}
		_tryCount++;
		_tryTime = Mathf.Min(Mathf.Pow(2f, _tryCount - 1), 16f);
	}

	private void ResetTryCount()
	{
		_tryCount = 0;
		_tryTime = 0.0;
	}
}
