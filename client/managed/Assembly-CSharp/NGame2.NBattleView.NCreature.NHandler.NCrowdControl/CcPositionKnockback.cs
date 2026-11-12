using NGame2.NMessage;
using NShared;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NBattleView.NCreature.NHandler.NCrowdControl;

public class CcPositionKnockback : CrowdControl.ICcPosition
{
	private CrowdControl _crowdControl;

	private NShared.SetPosition _packet;

	private float _beginTime;

	private Vector3 _beginPosition;

	private float _duration;

	public CcPositionKnockback(CrowdControl crowdControl, NShared.SetPosition packet)
	{
		_crowdControl = crowdControl;
		_packet = packet;
		_beginTime = MainTimer.time;
		_beginPosition = _crowdControl.Owner.Position;
		OperationRuntime operationRuntime = SceneAutoBehaviour<BattleCallbackManager>.instance.GetOperationRuntime(packet.OperationUid);
		if (operationRuntime == null)
		{
			_duration = GameSetting.data.BattleView.CcKnockback_MaxDuration;
		}
		else
		{
			_duration = Mathf.Min(GameSetting.data.BattleView.CcKnockback_MaxDuration, (float)operationRuntime.OperationData.DurationMs * 0.001f);
		}
	}

	void CrowdControl.ICcPosition.ChainUpdate()
	{
		if (_packet != null)
		{
			float num = MainTimer.time - _beginTime;
			float num2 = Mathf.Clamp01(num / Vespa_Float_Extension_Method.NoneZero(_duration));
			Vector3 position = Vector3.Lerp(t: Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(GameSetting.data.BattleView.CcKnockback_PositionCurve, num2, num2), b: _packet.Position, a: _beginPosition);
			_crowdControl.Owner.SetBattlePos(position, validateBattleArea: true);
			if (num >= _duration)
			{
				_packet = null;
			}
		}
	}
}
