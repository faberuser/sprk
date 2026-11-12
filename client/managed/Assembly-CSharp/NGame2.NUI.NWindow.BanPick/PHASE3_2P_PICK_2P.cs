using NShared;

namespace NGame2.NUI.NWindow.BanPick;

public class PHASE3_2P_PICK_2P : PhaseBase
{
	public PHASE3_2P_PICK_2P(MatchBanPick.Player me, MatchBanPick.Player opponent, IListener listener)
		: base(me, opponent, MatchBanPickState.PHASE3_2P_PICK_2P, listener)
	{
	}
}
