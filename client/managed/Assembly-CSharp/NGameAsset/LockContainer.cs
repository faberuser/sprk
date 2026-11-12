using System.Collections.Generic;
using NVespa;

namespace NGameAsset;

public class LockContainer : SingletonAuto<LockContainer>
{
	public HashSet<LockCounter> Set = new HashSet<LockCounter>();
}
