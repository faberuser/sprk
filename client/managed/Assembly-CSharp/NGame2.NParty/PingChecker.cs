using System;

namespace NGame2.NParty;

public class PingChecker
{
	private DateTime _lastSendPingTime;

	private DateTime _lastCheckTime;

	private Action _sendToPingMaster;

	private Action _checkExpriedMember;

	private float _pingInterval;

	private float _checkInterval;

	private float _expiredTime;

	public void Start(Action sendToPingMaster, Action checkExpriedMember, float pingInterval, float checkInterval, float expriedTime)
	{
		_sendToPingMaster = sendToPingMaster;
		_checkExpriedMember = checkExpriedMember;
		_pingInterval = pingInterval;
		_sendToPingMaster = sendToPingMaster;
		_expiredTime = expriedTime;
		_lastSendPingTime = DateTime.UtcNow;
		_lastCheckTime = DateTime.UtcNow;
	}

	public void Close()
	{
		_sendToPingMaster = null;
		_checkExpriedMember = null;
		_pingInterval = 0f;
		_checkInterval = 0f;
		_expiredTime = 0f;
	}

	public void Update()
	{
		SendPingToMaster();
		CheckExpriedMember();
	}

	private void SendPingToMaster()
	{
		if (_sendToPingMaster != null && (DateTime.UtcNow - _lastSendPingTime).TotalSeconds > (double)_pingInterval)
		{
			Vespa_System_Action_Extension_Method.fire(_sendToPingMaster);
			_lastSendPingTime = DateTime.UtcNow;
		}
	}

	private void CheckExpriedMember()
	{
		if (_checkExpriedMember != null)
		{
			double totalSeconds = (DateTime.UtcNow - _lastCheckTime).TotalSeconds;
			if (totalSeconds > (double)_expiredTime)
			{
				_lastCheckTime = DateTime.UtcNow;
			}
			else if (totalSeconds > (double)_checkInterval)
			{
				Vespa_System_Action_Extension_Method.fire(_checkExpriedMember);
				_lastCheckTime = DateTime.UtcNow;
			}
		}
	}
}
