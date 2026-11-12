using System;

namespace NShared;

[Obsolete("[// ## 미사용] 사용하지 않음 (Anniversary1_XXX, Anniversary1Paid_XXX 전용으로만 사용됨)", false)]
[JsonSerializable(Javascript = true)]
public enum AttendanceGroup
{
	None,
	Anniversary1_Free,
	Anniversary1_Paid
}
