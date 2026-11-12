using System;
using System.Collections.Generic;
using System.Linq;
using NGame2.NMessageLog.NProtocol;
using NGame2.NParty;
using NGame2.NSocket;
using NShared;
using NShared.NMessage;
using NShared.NParty;
using NVespa;
using NVespa.NSingleton;
using NVespa.NSocket;

namespace NGame2.NMessageLog;

public class MessageLogHandler : SingletonAuto<MessageLogHandler>, IMessageLogHandler
{
	private readonly MessageProtocolHandlerContainer<MessageNot> _handlerContainer;

	public MessageLogHandler()
	{
		_handlerContainer = new MessageProtocolHandlerContainer<MessageNot>("MESSAGE");
		RegisterHandler(new LoginProtocolHandler());
		RegisterHandler(new LogoutProtocolHandler());
		RegisterHandler(new EnterLobbyProtocolHandler());
		RegisterHandler(new BeginCampaignProtocolHandler());
		RegisterHandler(new RequestFriendProtocolHandler());
		RegisterHandler(new RequestFriendCanceledProtocolHandler());
		RegisterHandler(new RemoveFriendProtocolHandler());
		RegisterHandler(new AcceptJoinGuildProtocolHandler());
		RegisterHandler(new DelegateGuildMasterProtocolHandler());
		RegisterHandler(new BattleInfoChangedProtocolHandler());
		RegisterHandler(new SeasonChangedProtocolHandler());
		RegisterHandler(new GmPopupMessageArrivedProtocolHandler());
		RegisterHandler(new NoticeAddedProtocolHandler());
		RegisterHandler(new NoticeRemovedProtocolHandler());
		RegisterHandler(new ServerShutdownProtocolHandler());
		RegisterHandler(new PayShopItemEventAddedProtocolHandler());
		RegisterHandler(new PayShopItemEventRemovedProtocolHandler());
		RegisterHandler(new AvatarChangeNotProtocolHandler());
		RegisterHandler(new NewMailArrivedProtocolHandler());
		RegisterHandler(new DropBonusEventAddedProtocolHandler());
		RegisterHandler(new DropBonusEventRemovedProtocolHandler());
		RegisterHandler(new SentFriendshipPointProtocolHandler());
		RegisterHandler(new WithdrawedGuildMemeberProtocolHandler());
		RegisterHandler(new JoinedGuildProtocolHandler());
		RegisterHandler(new JoinGuildRequestAcceptedProtocolHandler());
		RegisterHandler(new KickedGuildMemberProtocolHandler());
		RegisterHandler(new SetGuildMemberRankProtocolHandler());
		RegisterHandler(new SetGuildNameProtocolHandler());
		RegisterHandler(new GuildSkillInitProtocolHandler());
		RegisterHandler(new GuildSkillLevelUpProtocolHandler());
		RegisterHandler(new GuildLevelUpProtocolHandler());
		RegisterHandler(new BecomeGuildGhostMemberProtocolHandler());
		RegisterHandler(new GuildLoginProtocolHandler());
		RegisterHandler(new GuildLogoutProtocolHandler());
		RegisterHandler(new ShopItemBoosterProtocolHandler());
		RegisterHandler(new WebPayShopPurchaseProtocolHandler());
		RegisterHandler(new GuildRaidBootyItemAddedHandler());
		RegisterHandler(new GuildRaidBeginHandler());
		RegisterHandler(new GuildRaidEndHandler());
		RegisterHandler(new GuildRaidOpenHandler());
		RegisterHandler(new GuildRaidBonusDungeonOpenHandler());
		RegisterHandler(new GuildRaidDummyDungeonOpenHandler());
		RegisterHandler(new WorldBossKilledProtocolHandler());
		RegisterHandler(new SetUserNickProtocolHandler());
		RegisterHandler(new SetUserAvatarHeroProtocolHandler());
		RegisterHandler(new ChatBanProtocolHandler());
		RegisterHandler(new GuildBuildingInvestProtocolHandler());
		RegisterHandler(new GuildBuildingLevelUpProtocolHandler());
		RegisterHandler(new GuildBuildingResourceProtocolHandler());
		RegisterHandler(new StaminaInfoChangedProtocolHandler());
		RegisterHandler(new GuildSuppressKilledProtocolHandler());
		RegisterHandler(new WebEventEventProtocolHandler());
		RegisterHandler(new RightAwayProductEventProtocolHandler());
		RegisterHandler(new GachaSelectShopEventProtocolHandler());
		RegisterHandler(new CurrencyInfoChagnedProtocolHandler());
		RegisterHandler(new PayShopDiscountEventProtocolHandler());
	}

	public void RegisterHandler(IMessageProtocolHandler handler)
	{
		_handlerContainer.RegisterHandler(handler);
	}

	public bool RemoveHandler(IMessageProtocolHandler handler)
	{
		return _handlerContainer.RemoveHandler(handler);
	}

	void IMessageLogHandler.Handle(MessageNot messageNot)
	{
		_handlerContainer.Handle(messageNot);
	}

	public void SendToClient<T>(long receiverId, T data) where T : class
	{
		SendToClient(new long[1] { receiverId }, data);
	}

	public void SendToClient<T>(long[] receiverIds, T data, Action onSuccess = null, Action onFail = null) where T : class
	{
		if (receiverIds != null && receiverIds.Length != 0)
		{
			SendMessageReq(MessageGroupType.None, receiverIds, data, onSuccess, onFail);
		}
	}

	public void SendToFriend<T>(T data) where T : class
	{
		SendMessageReq(MessageGroupType.Friend, null, data);
	}

	public void SendToGuild<T>(T data) where T : class
	{
		SendMessageReq(MessageGroupType.Guild, null, data);
	}

	public void SendToChannel<T>(T data) where T : class
	{
		SendMessageReq(MessageGroupType.Channel, null, data);
	}

	public void SendToWorld<T>(T data) where T : class
	{
		SendMessageReq(MessageGroupType.World, null, data);
	}

	public void SendToParty<T>(T data) where T : class
	{
		List<long> list = new List<long>();
		if (GlobalAutoBehaviour<PartyManager>.isValidInstance && GlobalAutoBehaviour<PartyManager>.instance.PartyInfo != null)
		{
			List<MemberInfo> partyMembers = GlobalAutoBehaviour<PartyManager>.instance.PartyInfo.PartyMembers;
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(partyMembers); i++)
			{
				MemberInfo memberInfo = partyMembers[i];
				list.Add(memberInfo.AccountId);
			}
		}
		if (__Extension_Method_ICollection__.COUNT(list) == 0)
		{
			VespaDebug.LogError("Failed to send party message : party not found.");
		}
		else
		{
			SendMessageReq(MessageGroupType.None, list, data);
		}
	}

	private void SendMessageReq<T>(MessageGroupType groupType, IList<long> receiverIds, T data, Action onSuccess = null, Action onFail = null) where T : class
	{
		MessageReq messageReq = new MessageReq();
		messageReq.ReceiverIds = receiverIds?.ToArray();
		messageReq.Message = ToMessageLogString(data);
		messageReq.GroupType = groupType;
		if (groupType == MessageGroupType.None)
		{
			if (__Extension_Method_ICollection__.COUNT(receiverIds) == 0)
			{
				VespaDebug.LogError("Cannot send message with grouptype=none, receivers=null");
				return;
			}
			_ = "[" + string.Join(",", receiverIds.ToList().ConvertAll((long e) => e.ToString()).ToArray()) + "]";
		}
		else
		{
			groupType.ToString();
		}
		GlobalAutoBehaviour<SocketServiceManager>.instance.MessageConnector.Request(messageReq, delegate(MessageRes response)
		{
			if (response == null)
			{
				VespaDebug.LogError("Failed to send normal message : response is null.");
				Vespa_System_Action_Extension_Method.fire(onFail);
			}
			else if (response.Result != MessageResType.Success)
			{
				VespaDebug.LogError("Failed to send normal message : result=" + response.Result);
				Vespa_System_Action_Extension_Method.fire(onFail);
			}
			else
			{
				Vespa_System_Action_Extension_Method.fire(onSuccess);
			}
		});
	}

	private static string ToMessageLogString<T>(T data) where T : class
	{
		string text = JsonMarshaler.EncodeJsonString(data);
		if (!string.IsNullOrEmpty(text))
		{
			return typeof(T).Name + " " + text;
		}
		return typeof(T).Name;
	}
}
