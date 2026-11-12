using NShared;

namespace NGame2.NUI.NWindow.BanPick;

public class PHASE5_2P_PICK_2P : PhaseBase
{
	public PHASE5_2P_PICK_2P(MatchBanPick.Player me, MatchBanPick.Player opponent, IListener listener)
		: base(me, opponent, MatchBanPickState.PHASE5_2P_PICK_2P, listener)
	{
	}
}
