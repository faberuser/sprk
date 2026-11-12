using NGame2.NUI.NManager;
using NGame2.NUI.NWindow;
using NShared.NMessage.NProtocol;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NMatch.NDuelState;

public class CompleteMySetting : StateBase
{
	private int[] _heroIndices;

	private int _leaderHeroIndex;

	public CompleteMySetting(long vsPlayerId, long groupId, int[] heroIndices, int leaderHeroIndex)
		: base(vsPlayerId, groupId)
	{
		_heroIndices = heroIndices;
		_leaderHeroIndex = leaderHeroIndex;
	}

	public override void OnBegin()
	{
		SceneAutoBehaviour<DuelManagement>.instance.OpenDuelWaitingPopup(base.VsPlayerId, base.GroupId, DuelWaitingPopup.EWaitingType.WaitingPartySetting);
	}

	public override void OnEnd()
	{
		SceneAutoBehaviour<DuelManagement>.instance.CloseDuelWaitingPopup();
	}

	public override void OnReceive(long senderId, CompletePartySetting data)
	{
		SingletonUpdater<DuelManager>.instance.ChangeState(this, new BeginDuel(base.VsPlayerId, base.GroupId, _heroIndices, _leaderHeroIndex));
	}
}
