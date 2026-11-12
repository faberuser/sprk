using NShared;

namespace NGame2.NUI.NWindow.BanPick;

public class PHASE6_1P_PICK_1P : PhaseBase
{
	public PHASE6_1P_PICK_1P(MatchBanPick.Player me, MatchBanPick.Player opponent, IListener listener)
		: base(me, opponent, MatchBanPickState.PHASE6_1P_PICK_1P, listener)
	{
	}
}
