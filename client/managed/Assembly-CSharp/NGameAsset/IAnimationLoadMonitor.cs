using System.Collections.Generic;

namespace NGameAsset;

public interface IAnimationLoadMonitor
{
	Dictionary<string, bool> GetLoadReports();
}
