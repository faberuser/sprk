using NGame2.NMessageLog;
using NGame2.NUI.NWindow;
using NShared;
using NShared.NLocalization;
using NShared.NMessage.NProtocol;
using NVespa;
using NVespa.NGlobal;

namespace NGame2.NMatch.NDuelState;

public class BeginDuel : StateBase
{
	private int[] _heroIndices;

	private int _leaderHeroIndex;

	private int _sendTick;

	public BeginDuel(long vsPlayerId, long groupId, int[] heroIndices, int leaderHeroIndex)
		: base(vsPlayerId, groupId)
	{
		_heroIndices = heroIndices;
		_leaderHeroIndex = leaderHeroIndex;
	}

	public override void ChainUpdate()
	{
		base.ChainUpdate();
		if (UnitySystemTimer.SystemTick - _sendTick > 10000)
		{
			CenterMessage.Open(LocalizationManager.GetString(Duel.DUEL_RESPONSE_BUSY));
			SingletonUpdater<DuelManager>.instance.ChangeState(this, new None(), pending: true);
		}
	}

	public override void OnBegin()
	{
		WaitingPopup.Open(this);
		NShared.NMessage.NProtocol.BeginDuel beginDuel = new NShared.NMessage.NProtocol.BeginDuel();
		beginDuel.GroupId = base.GroupId;
		SingletonAuto<MessageLogHandler>.instance.SendToClient(base.VsPlayerId, beginDuel);
		_sendTick = UnitySystemTimer.SystemTick;
	}

	public override void OnEnd()
	{
		WaitingPopup.Close(this);
	}

	public override void OnReceive(long senderId, NShared.NMessage.NProtocol.BeginDuel data)
	{
		SingletonUpdater<MatchManager>.instance.RegisterMatch_Pvp((MatchPvpType)SprkHonorDuel.PvpType(base.GroupId), _heroIndices, _leaderHeroIndex, base.GroupId, 0, null);
		SingletonUpdater<DuelManager>.instance.ChangeState(this, new None(), pending: true);
	}
}
