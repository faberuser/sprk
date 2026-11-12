namespace NGame2.NUI.NWindow.NBattle.NAnim;

public class KarmaCardInit : BaseKarmAnimState
{
	public KarmaCardInit(KarmaCardSelectWindow owner)
		: base(owner)
	{
	}

	public override void OnStart()
	{
		_owner.ShowCardRoot(show: false);
		_owner.SetCardPlateGridReposition();
		float cardSelectLeftTime = _owner.GetCardSelectLeftTime();
		_owner.SetCardSelectLeftTime((int)cardSelectLeftTime);
		SetEnd(isEnd: true);
	}

	public override State GetNextState()
	{
		return new KarmaCardSuffleWait(_owner);
	}
}
