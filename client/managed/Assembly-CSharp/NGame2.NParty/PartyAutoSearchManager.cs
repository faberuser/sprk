using System;
using NGame2.NAccount;
using NGame2.NUI.NWindow;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using NVespa.NWeb;

namespace NGame2.NParty;

public class PartyAutoSearchManager : GlobalAutoBehaviour<PartyAutoSearchManager>, PartyAutoSearch.Listener
{
	private interface ISearch
	{
		bool Execute();

		void SetLastRecvTimeCallbak(Action callback);
	}

	private class AutoSearchHelper<Treq, Tres> : ISearch where Treq : Request, IServiceRequest where Tres : Response, new()
	{
		private Treq _req;

		private Action<Tres> _action;

		private Action _setLastRecvTime;

		public AutoSearchHelper(Treq request, Action<Tres> action)
		{
			_req = request;
			_action = action;
		}

		public bool Execute()
		{
			return SingletonUpdater<WebServiceManager>.instance.Requester.Request(_req, WebRequestFlag.None, delegate(Tres res)
			{
				Vespa_System_Action_Extension_Method.fire(_action, res);
				Vespa_System_Action_Extension_Method.fire(_setLastRecvTime);
			}) != null;
		}

		public void SetLastRecvTimeCallbak(Action callback)
		{
			_setLastRecvTime = callback;
		}
	}

	private const int DEFAULT_INTERVAL_TIME = 10;

	private const int DEFAULT_WAIT_RECV_TIME = 60;

	private bool _isStart;

	private int _intervalTime = 10;

	private DateTime _lastRecvTime;

	private DateTime _lastSendTime;

	private ISearch _autoSearch;

	public bool DoAutoSearch => _autoSearch != null;

	public void InitSearch<TReq, TRes>(TReq request, Action<TRes> action) where TReq : Request, IServiceRequest where TRes : Response, new()
	{
		if (request != null && action != null)
		{
			if (_autoSearch != null)
			{
				CloseSearch();
			}
			_autoSearch = new AutoSearchHelper<TReq, TRes>(request, action);
			_autoSearch.SetLastRecvTimeCallbak(SetLastRecvTime);
			Starts();
		}
	}

	public void SetInterval(int intervalTime)
	{
		_intervalTime = intervalTime;
	}

	public void Starts()
	{
		if (_autoSearch != null)
		{
			PartyAutoSearch.OpenGlobal(this);
			_isStart = true;
			Send();
		}
	}

	public void CloseSearch(bool isCloseUI = true)
	{
		_autoSearch = null;
		_isStart = false;
		if (isCloseUI)
		{
			PartyAutoSearch.instance.CancelInternal();
		}
	}

	public void PauseSearch()
	{
		_isStart = false;
	}

	public void ResumeSearch()
	{
		_isStart = true;
	}

	public void SetLastRecvTime()
	{
		_lastRecvTime = DateTime.Now;
	}

	private bool IsCanReq()
	{
		if (_intervalTime <= 0)
		{
			return false;
		}
		if (_lastSendTime > _lastRecvTime)
		{
			if ((DateTime.Now - _lastSendTime).TotalSeconds < 60.0)
			{
				return false;
			}
			SetLastRecvTime();
		}
		if ((DateTime.Now - _lastRecvTime).TotalSeconds < (double)_intervalTime)
		{
			return false;
		}
		return true;
	}

	private void Send()
	{
		_lastSendTime = DateTime.Now;
		if (!_autoSearch.Execute())
		{
			CloseSearch();
		}
	}

	private void Update()
	{
		UpdateSearch();
	}

	private void UpdateSearch()
	{
		if (DoAutoSearch && _isStart)
		{
			if (!AccountManager.IsLogined())
			{
				CloseSearch();
			}
			else if (IsCanReq())
			{
				Send();
			}
		}
	}

	public void ShowCenterMessageJoinRoomFail()
	{
		PartyAutoSearch.instance.ShowCenterMessageJoinRoomFail();
	}

	void PartyAutoSearch.Listener.OnClickCancel()
	{
		CloseSearch(isCloseUI: false);
	}
}
