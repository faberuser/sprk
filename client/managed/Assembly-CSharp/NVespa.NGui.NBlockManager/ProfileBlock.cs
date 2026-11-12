using System;

namespace NVespa.NGui.NBlockManager;

public class ProfileBlock : IDisposable
{
	public ProfileBlock Open(string name)
	{
		return this;
	}

	public virtual void Dispose()
	{
	}
}
