using System;

namespace NShared;

[Flags]
public enum OperationApplyTargetFlag
{
	None = 0,
	Alive = 1,
	Dead = 2,
	Default = 1,
	All = 0xFFFF
}
