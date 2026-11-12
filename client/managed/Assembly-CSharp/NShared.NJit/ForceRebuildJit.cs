using System;

namespace NShared.NJit;

public class ForceRebuildJit : IDisposable
{
	private static int _rebuildDepth;

	public static bool Rebuild => _rebuildDepth > 0;

	public ForceRebuildJit()
	{
		_rebuildDepth++;
	}

	void IDisposable.Dispose()
	{
		_rebuildDepth--;
	}
}
