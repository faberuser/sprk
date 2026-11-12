using System;

namespace NShared;

[JsonSerializable(Javascript = true)]
public class PositionInfo
{
	public float PositionX { get; set; }

	public float PositionY { get; set; }

	public float PositionZ { get; set; }

	public float RotationX { get; set; }

	public float RotationY { get; set; }

	public float RotationZ { get; set; }

	public float Scale { get; set; }

	public string Log()
	{
		return $"P({PositionX},{PositionY},{PositionZ}) R({RotationX},{RotationY},{RotationZ}) S({Scale}) ";
	}

	public static PositionInfo Create()
	{
		return new PositionInfo
		{
			PositionX = 0f,
			PositionY = 0f,
			PositionZ = 0f,
			RotationX = 0f,
			RotationY = 0f,
			RotationZ = 0f,
			Scale = 1f
		};
	}

	public PositionInfo Clone()
	{
		return new PositionInfo
		{
			PositionX = PositionX,
			PositionY = PositionY,
			PositionZ = PositionZ,
			RotationX = RotationX,
			RotationY = RotationY,
			RotationZ = RotationZ,
			Scale = Scale
		};
	}

	public void Round()
	{
		int digits = 3;
		PositionX = (float)Math.Round(PositionX, digits);
		PositionY = (float)Math.Round(PositionY, digits);
		PositionZ = (float)Math.Round(PositionZ, digits);
		RotationX = (float)Math.Round(RotationX, digits);
		RotationY = (float)Math.Round(RotationY, digits);
		RotationZ = (float)Math.Round(RotationZ, digits);
		Scale = (float)Math.Round(Scale, digits);
	}
}
