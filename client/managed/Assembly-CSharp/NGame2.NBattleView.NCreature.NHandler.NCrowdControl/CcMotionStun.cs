namespace NGame2.NBattleView.NCreature.NHandler.NCrowdControl;

public class CcMotionStun : CrowdControl.ICcMotion
{
	private int _actionUid;

	private CrowdControl _crowdControl;

	public CcMotionStun(CrowdControl crowdControl)
	{
		_crowdControl = crowdControl;
		_crowdControl.Owner.Action.Stun(null);
		_actionUid = _crowdControl.Owner.RenderFacade.AnimationData.Runtime.Uid;
	}

	void CrowdControl.ICcMotion.ChainUpdate()
	{
	}

	void CrowdControl.ICcMotion.ReleaseCc()
	{
		if (_crowdControl.Owner.RenderFacade.AnimationData.Runtime.Uid == _actionUid)
		{
			_crowdControl.Owner.Action.Idle();
		}
	}
}
