using NShared;

namespace NGame2.NUI.NWindow.NCustomizing;

public enum ToggleType
{
	None,
	[ToggleType(AccessoryPartType.None, AccessoryPartType.Body)]
	MySet,
	[ToggleType(AccessoryPartType.None, AccessoryPartType.Body)]
	Costume,
	[ToggleType(AccessoryPartType.None, AccessoryPartType.Body)]
	Weapon,
	[ToggleType(AccessoryPartType.Head)]
	Head,
	[ToggleType(AccessoryPartType.Eye)]
	Eye,
	[ToggleType(AccessoryPartType.Nose)]
	Nose,
	[ToggleType(AccessoryPartType.Body)]
	Body,
	[ToggleType(AccessoryPartType.None, AccessoryPartType.Head)]
	Hair,
	_MAX_
}
