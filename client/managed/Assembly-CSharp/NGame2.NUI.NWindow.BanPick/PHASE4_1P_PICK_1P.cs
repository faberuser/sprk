using NShared;

namespace NGame2.NUI.NWindow.BanPick;

public class PHASE4_1P_PICK_1P : PhaseBase
{
	public PHASE4_1P_PICK_1P(MatchBanPick.Player me, MatchBanPick.Player opponent, IListener listener)
		: base(me, opponent, MatchBanPickState.PHASE4_1P_PICK_1P, listener)
	{
	}
}
