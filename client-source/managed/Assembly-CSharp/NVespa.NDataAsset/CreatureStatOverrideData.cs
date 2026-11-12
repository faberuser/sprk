using System;
using NShared;

namespace NVespa.NDataAsset;

[Serializable]
public class CreatureStatOverrideData
{
	public int HeroIndex;

	public StatType StatType;

	public int Mult;

	public int Add;
}
