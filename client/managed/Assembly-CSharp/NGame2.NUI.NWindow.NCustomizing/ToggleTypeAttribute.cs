using System;
using NShared;

namespace NGame2.NUI.NWindow.NCustomizing;

public class ToggleTypeAttribute : Attribute
{
	public AccessoryPartType customizingCameraSettingType;

	public AccessoryPartType accessoryPartType;

	public ToggleTypeAttribute(AccessoryPartType accessoryType)
	{
		accessoryPartType = accessoryType;
		customizingCameraSettingType = accessoryType;
	}

	public ToggleTypeAttribute(AccessoryPartType accessoryType, AccessoryPartType cameraType)
	{
		accessoryPartType = accessoryType;
		customizingCameraSettingType = cameraType;
	}
}
