using System.Diagnostics;
using NVespa;

namespace NGameAsset;

public class LockCounter : ILockCounter
{
	public string Name { get; private set; }

	public int Count { get; private set; }

	public int UnlockTick { get; private set; }

	[Conditional("GAMEDEBUG_CALLSTACKDUMP")]
	public static void SetCallstackComment(string comment)
	{
	}

	[Conditional("GAMEDEBUG_CALLSTACKDUMP")]
	public static void SetCallstackInstanceId(int instanceId)
	{
	}

	public void Reset()
	{
		Count = 0;
		UnlockTick = 0;
	}

	[Conditional("GAMEDEBUG_CALLSTACKDUMP")]
	public void SetName(string name)
	{
		Name = name;
	}

	public void Increase()
	{
		Count++;
		UnlockTick = UnitySystemTimer.unscaledTick;
	}

	public void Decrease(CallData lockCall)
	{
		Count--;
		UnlockTick = UnitySystemTimer.unscaledTick;
	}

	public void OnDestroy()
	{
	}

	public string DumpSummary()
	{
		return string.Empty;
	}
}
