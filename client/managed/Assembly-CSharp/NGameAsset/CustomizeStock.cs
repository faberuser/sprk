using NShared;
using NVespa;
using UnityEngine;

namespace NGameAsset;

public struct CustomizeStock
{
	public AccessoryPartType Part;

	public int AccessoryIndex;

	public string Code;

	public string Pivot;

	public string TransformName1;

	public string TransformName2;

	public Vector3 InitPosition;

	public Vector3 InitRotation;

	public float InitScale;

	public Vector3 Position;

	public Vector3 Rotation;

	public float Scale;

	public static CustomizeStock CreateEmptyPart(AccessoryPartType partType)
	{
		return new CustomizeStock
		{
			Part = partType
		};
	}

	public bool IsEmptyPart()
	{
		if (Part != AccessoryPartType.None && AccessoryIndex == 0)
		{
			return String_Extension_Method.IsNullOrEmpty(Code);
		}
		return false;
	}

	public bool IsEqualItem(CustomizeStock stock)
	{
		if (Part == stock.Part && AccessoryIndex == stock.AccessoryIndex)
		{
			return Code == stock.Code;
		}
		return false;
	}

	public bool IsEqualTransform(CustomizeStock stock)
	{
		if (Customizing_Extension_Method.Approximately(Position, stock.Position) && Customizing_Extension_Method.Approximately(Rotation, stock.Rotation))
		{
			return Customizing_Extension_Method.Approximately(Scale, stock.Scale);
		}
		return false;
	}

	public bool Assign(AccessoryCostumeData accessoryData)
	{
		if (accessoryData == null)
		{
			VespaDebug.LogError("Assign Fail (accessoryData is null)");
			return false;
		}
		AccessoryIndex = accessoryData.Index;
		Code = accessoryData.Code;
		Part = accessoryData.PartType;
		return true;
	}

	public bool Assign(ClientCustomizingPivotData pivotData)
	{
		if (pivotData == null)
		{
			VespaDebug.LogError("Assign Fail (pivotData is null)");
			return false;
		}
		InitPosition = pivotData.GetInitPosition();
		InitRotation = pivotData.GetInitRotation();
		InitScale = pivotData.InitScale;
		TransformName1 = pivotData.TransformName1;
		TransformName2 = pivotData.TransformName2;
		return true;
	}

	public bool Assign(PositionInfo positionInfo)
	{
		if (positionInfo == null)
		{
			return false;
		}
		Position = positionInfo.GetPosition();
		Rotation = positionInfo.GetRotation();
		Scale = positionInfo.GetScale();
		return true;
	}

	public bool IsValid()
	{
		if (Part == AccessoryPartType.None)
		{
			VespaDebug.LogError("Part == NShared.AccessoryPartType.None");
			return false;
		}
		if (String_Extension_Method.IsNullOrEmpty(Code))
		{
			VespaDebug.LogError("Code.IsNullOrEmpty()");
			return false;
		}
		if (String_Extension_Method.IsNullOrEmpty(TransformName1) && String_Extension_Method.IsNullOrEmpty(TransformName2) && String_Extension_Method.IsNullOrEmpty(Pivot))
		{
			VespaDebug.LogError("TransformName1.IsNullOrEmpty() && TransformName2.IsNullOrEmpty() && Pivot.IsNullOrEmpty()");
			return false;
		}
		return true;
	}
}
