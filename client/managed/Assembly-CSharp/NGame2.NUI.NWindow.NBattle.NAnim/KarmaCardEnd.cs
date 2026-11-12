namespace NGame2.NUI.NWindow.NBattle.NAnim;

public class KarmaCardEnd : BaseKarmAnimState
{
	public KarmaCardEnd(KarmaCardSelectWindow owner)
		: base(owner)
	{
	}

	public override void OnStart()
	{
		_owner.ForCardPlate(delegate(KarmaCardSelectWindow.CardPlate cardPlate)
		{
			cardPlate.Close();
		});
		_owner.ShowCardRoot(show: false);
		_owner.SetSelectCard(isSelect: false);
		SetEnd(isEnd: true);
	}

	public override State GetNextState()
	{
		return null;
	}
}
