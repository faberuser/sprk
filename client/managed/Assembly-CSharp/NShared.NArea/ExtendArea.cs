using System;
using System.Collections.Generic;
using NShared.NUtil;

namespace NShared.NArea;

public class ExtendArea : BaseArea
{
	private int _cosInt;

	private int _sinInt;

	private int _mCosInt;

	private int _mSinInt;

	private Vector3 _cachedPos;

	public int YAxisAngle { get; private set; }

	public Vector3 YAxisPosition { get; private set; }

	private int _yAxisRaianInt { get; set; }

	private double _yAxisRadian { get; set; }

	public void Init(int minMx, int maxMx, int minMz, int maxMz, int yAxisAngle)
	{
		YAxisAngle = yAxisAngle;
		_yAxisRaianInt = (int)((double)yAxisAngle * Math.PI * 1000.0) / 180;
		_cosInt = (int)(Math.Cos((double)_yAxisRaianInt / 1000.0) * 1000.0);
		_sinInt = (int)(Math.Sin((double)_yAxisRaianInt / 1000.0) * 1000.0);
		_mCosInt = (int)(Math.Cos((double)(-_yAxisRaianInt) / 1000.0) * 1000.0);
		_mSinInt = (int)(Math.Sin((double)(-_yAxisRaianInt) / 1000.0) * 1000.0);
		YAxisPosition = new Vector3(maxMx, 0L, CalCenter(minMz, maxMz));
		Init(minMx, maxMx, minMz, maxMz);
	}

	public void YawPoint(ref Vector3 position, bool inverse)
	{
		position.mx -= YAxisPosition.mx;
		position.mz -= YAxisPosition.mz;
		long mx;
		long mz;
		if (inverse)
		{
			mx = (position.mz * _mSinInt + position.mx * _mCosInt) / 1000 + YAxisPosition.mx;
			mz = (position.mz * _mCosInt - position.mx * _mSinInt) / 1000 + YAxisPosition.mz;
		}
		else
		{
			mx = (position.mz * _sinInt + position.mx * _cosInt) / 1000 + YAxisPosition.mx;
			mz = (position.mz * _cosInt - position.mx * _sinInt) / 1000 + YAxisPosition.mz;
		}
		position.mx = mx;
		position.mz = mz;
	}

	public override void InitMx(int minMx, int maxMx)
	{
		YAxisPosition = new Vector3(maxMx, 0L, CalCenter(base.MinMz, base.MaxMz));
		base.InitMx(minMx, maxMx);
	}

	public override void Validate(ref Vector3 position)
	{
		if (YAxisAngle == 0)
		{
			base.Validate(ref position);
		}
		else if (!IsInnerArea(position))
		{
			YawPoint(ref position, inverse: true);
			base.Validate(ref position);
			YawPoint(ref position, inverse: false);
		}
	}

	private bool IsInnerArea(Vector3 position)
	{
		_cachedPos = position;
		YawPoint(ref _cachedPos, inverse: true);
		if (base.MinMx > _cachedPos.Mx || base.MaxMx < _cachedPos.mx || base.MinMz > _cachedPos.Mz || base.MaxMz < _cachedPos.Mz)
		{
			return false;
		}
		return true;
	}

	public void YawCreaturePositionList(List<Vector3> creaturePositionList)
	{
		if (creaturePositionList != null && creaturePositionList.Count != 0 && YAxisAngle != 0)
		{
			for (int i = 0; i < creaturePositionList.Count; i++)
			{
				Vector3 position = creaturePositionList[i];
				YawPoint(ref position, inverse: false);
				creaturePositionList[i] = position;
			}
		}
	}
}
