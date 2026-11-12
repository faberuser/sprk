using NGame2.NMessage;
using NGame2.NRender.NRenderFacade;
using NGameAsset;
using NGameAsset.NCreature;
using NVespa.NSingleton;

namespace NGame2.NBattleView.NCreature.NHandler.NCrowdControl;

public class CcMotionKnockback : CrowdControl.ICcMotion
{
	private int _actionUid;

	private CrowdControl _crowdControl;

	public CcMotionKnockback(CrowdControl crowdControl)
	{
		_crowdControl = crowdControl;
		OperationRuntime operationRuntime = SceneAutoBehaviour<BattleCallbackManager>.instance.GetOperationRuntime(_crowdControl.Packet.OperationUid);
		NGame2.NRender.NRenderFacade.AnimationData.StockData stock = new NGame2.NRender.NRenderFacade.AnimationData.StockData
		{
			State = NGame2.NRender.NRenderFacade.AnimationData.EState.Damage,
			AnimationKey = AnimationKeyData.CreateMotionType(DefineCreature.EMotion.Damage),
			TimeLength = ((operationRuntime == null) ? 0.5f : ((float)operationRuntime.OperationData.DurationMs * 0.001f))
		};
		_crowdControl.Owner.Action.Motion(stock, operationRuntime?.OperationTimer);
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
