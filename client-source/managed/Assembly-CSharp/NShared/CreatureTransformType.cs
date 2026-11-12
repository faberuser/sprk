using System;

namespace NShared;

[JsonSerializable(Javascript = true)]
[Flags]
public enum CreatureTransformType
{
	StaticPosition = 1,
	StaticRotation = 2,
	Wall = 4
}
