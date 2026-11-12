using NGame2.NMessage;
using NShared;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NBattleView.NCreature.NHandler.NCrowdControl;

public class CcPositionDown : CrowdControl.ICcPosition
{
	private CrowdControl _crowdControl;

	private NShared.SetPosition _packet;

	private Vector3 _beginPosition;

	private ILocalTimer _operationTimer;

	private float _elapsedTime;

	private float _duration;

	public CcPositionDown(CrowdControl crowdControl, NShared.SetPosition packet)
	{
		_crowdControl = crowdControl;
		_packet = packet;
		_elapsedTime = 0f;
		_beginPosition = _crowdControl.Owner.Position;
		OperationRuntime operationRuntime = SceneAutoBehaviour<BattleCallbackManager>.instance.GetOperationRuntime(packet.OperationUid);
		if (operationRuntime == null)
		{
			_operationTimer = MainTimer.DefaultLocal;
			_duration = GameSetting.data.BattleView.CcDown_DownPositionDuration;
		}
		else
		{
			_operationTimer = operationRuntime.OperationTimer;
			_duration = Mathf.Min(GameSetting.data.BattleView.CcDown_DownPositionDuration, (float)operationRuntime.OperationData.DurationMs * 0.001f);
		}
	}

	void CrowdControl.ICcPosition.ChainUpdate()
	{
		if (_packet != null)
		{
			_elapsedTime += (float)_operationTimer.deltaTick * 0.001f;
			float num = Mathf.Clamp01(_elapsedTime / Vespa_Float_Extension_Method.NoneZero(_duration));
			float t = Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(GameSetting.data.BattleView.CcDown_DownPositionCurve, num, num);
			Vector3 b = _packet.Position;
			Vector3 position = Vector3.Lerp(_beginPosition, b, t);
			_crowdControl.Owner.SetBattlePos(position, validateBattleArea: true);
			if (_elapsedTime >= _duration)
			{
				_packet = null;
			}
		}
	}
}
