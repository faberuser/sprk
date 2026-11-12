using System;
using NShared.NUtil;

namespace NShared.NArea;

public class BaseArea
{
	public int MinMz { get; private set; }

	public int MaxMz { get; private set; }

	public int MinMx { get; private set; }

	public int MaxMx { get; private set; }

	public int MzCenter { get; set; }

	public int MxCenter { get; set; }

	public void Init(int minMx, int maxMx, int minMz, int maxMz)
	{
		MinMx = minMx;
		MaxMx = maxMx;
		MinMz = minMz;
		MaxMz = maxMz;
		CalculationCenter();
	}

	public virtual void InitMx(int minMx, int maxMx)
	{
		MinMx = minMx;
		MaxMx = maxMx;
		CalculationCenter();
	}

	public virtual void Validate(ref Vector3 position)
	{
		position.Mx = Math.Max(position.Mx, MinMx);
		position.Mx = Math.Min(position.Mx, MaxMx);
		position.Mz = Math.Max(position.Mz, MinMz);
		position.Mz = Math.Min(position.Mz, MaxMz);
	}

	protected virtual void CalculationCenter()
	{
		MxCenter = CalCenter(MinMx, MaxMx);
		MzCenter = CalCenter(MinMz, MaxMz);
	}

	protected int CalCenter(int min, int max)
	{
		return (min + max) / 2;
	}
}
