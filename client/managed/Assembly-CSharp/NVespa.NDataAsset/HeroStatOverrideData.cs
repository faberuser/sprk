using System;
using NShared;

namespace NVespa.NDataAsset;

[Serializable]
public class HeroStatOverrideData
{
	public int SlotIndex;

	public StatType StatType;

	public int Mult;

	public int Add;
}
