using NVespa;

namespace NGame2.NConfig;

public class ClientDebugNetwork : SingletonAuto<ClientDebugNetwork>
{
	public static bool ForceSessionExpired;

	private LocalStorageInt _forceLatencyMs = new LocalStorageInt(0, typeof(ClientDebugNetwork).FullName + "ForceLatencyMs");

	private LocalStorageInt _forceTimeoutMs = new LocalStorageInt(0, typeof(ClientDebugNetwork).FullName + "ForceTimeoutMs");

	private LocalStorageBool _singleMatchTest = new LocalStorageBool(i_default: false, typeof(ClientDebugNetwork).FullName + "SingleMatchTest");

	private LocalStorageBool _ignorePingTest = new LocalStorageBool(i_default: false, typeof(ClientDebugNetwork).FullName + "IgnorePingTest");

	public int ForceLatencyMs
	{
		get
		{
			return _forceLatencyMs.Value;
		}
		set
		{
			_forceLatencyMs.Value = value;
		}
	}

	public int ForceTimeoutMs
	{
		get
		{
			return _forceTimeoutMs.Value;
		}
		set
		{
			_forceTimeoutMs.Value = value;
		}
	}

	public bool SingleMatchTest
	{
		get
		{
			return _singleMatchTest.Value;
		}
		set
		{
			_singleMatchTest.Value = value;
		}
	}

	public bool IgnorePingTest
	{
		get
		{
			return _ignorePingTest.Value;
		}
		set
		{
			_ignorePingTest.Value = value;
		}
	}

	public ClientDebugNetwork()
	{
		_forceLatencyMs.Init();
		_forceTimeoutMs.Init();
		_singleMatchTest.Init();
		_ignorePingTest.Init();
	}
}
