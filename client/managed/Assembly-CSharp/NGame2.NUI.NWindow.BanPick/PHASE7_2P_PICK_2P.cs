using NShared;

namespace NGame2.NUI.NWindow.BanPick;

public class PHASE7_2P_PICK_2P : PhaseBase
{
	public PHASE7_2P_PICK_2P(MatchBanPick.Player me, MatchBanPick.Player opponent, IListener listener)
		: base(me, opponent, MatchBanPickState.PHASE7_2P_PICK_2P, listener)
	{
	}
}
