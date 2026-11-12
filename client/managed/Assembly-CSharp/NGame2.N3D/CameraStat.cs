using System;
using NGame2.NBattleContext;
using NGame2.NBattleView;
using NGameAsset.NBattleFieldMap2;
using NShared;
using NVespa;
using UnityEngine;

namespace NGame2.N3D;

public class CameraStat
{
	public struct CameraStockOffset
	{
		public static CameraStockOffset Empty;

		public float LookAngle;

		public float HorizonAngle;

		public float FieldOfView;

		public float MinHeight;

		public Vector3 BoundsMargin;

		public Vector3 BoundsOffset;
	}

	public class CameraStock
	{
		private FieldCameraData _cameraData;

		private CameraStockOffset _offset;

		public string NextCameraName => _cameraData.NextCameraName;

		public float LookAngle => _cameraData.LookAngle + _offset.LookAngle;

		public float HorizonAngle => _cameraData.HorizonAngle + _offset.HorizonAngle;

		public float FieldOfView => _cameraData.FieldOfView + _offset.FieldOfView;

		public float MinHeight => _cameraData.MinHeight + _offset.MinHeight;

		public bool FitToField => _cameraData.FitToField;

		public Vector3 BoundsMargin => _cameraData.BoundsMargin + _offset.BoundsMargin;

		public Vector3 BoundsOffset => _cameraData.BoundsOffset + _offset.BoundsOffset;

		public Vector3 CameraForward { get; private set; }

		public Quaternion CameraRotation { get; private set; }

		public Quaternion CameraRotationInverse { get; private set; }

		public float FovH { get; private set; }

		public CameraStock(FieldCameraData cameraData)
		{
			_cameraData = cameraData;
			CameraRotation = Quaternion.AngleAxis(HorizonAngle, Vector3.up) * Quaternion.AngleAxis(LookAngle, Vector3.right);
			CameraRotationInverse = Quaternion.Inverse(CameraRotation);
			CameraForward = CameraRotation * Vector3.forward;
			float f = Mathf.Tan(FieldOfView * 0.5f * (MathF.PI / 180f)) * (float)SingletonAuto<ResolutionInfo>.instance.RealScreenWidth / (float)SingletonAuto<ResolutionInfo>.instance.RealScreenHeight;
			FovH = Mathf.Atan(f) * 2f * 57.29578f;
		}

		public void ChangeOffset(CameraStockOffset offset)
		{
			_offset = offset;
			if (offset.HorizonAngle != 0f || offset.LookAngle != 0f || offset.FieldOfView != 0f)
			{
				ResetRotation();
			}
		}

		public void ResetCameraValues(Quaternion cameraRotation)
		{
			CameraRotation = cameraRotation;
			CameraRotationInverse = Quaternion.Inverse(CameraRotation);
			CameraForward = CameraRotation * Vector3.forward;
			float f = Mathf.Tan(FieldOfView * 0.5f * (MathF.PI / 180f)) * (float)SingletonAuto<ResolutionInfo>.instance.RealScreenWidth / (float)SingletonAuto<ResolutionInfo>.instance.RealScreenHeight;
			FovH = Mathf.Atan(f) * 2f * 57.29578f;
		}

		public void ResetRotation()
		{
			CameraRotation = Quaternion.AngleAxis(HorizonAngle, Vector3.up) * Quaternion.AngleAxis(LookAngle, Vector3.right);
			CameraRotationInverse = Quaternion.Inverse(CameraRotation);
			CameraForward = CameraRotation * Vector3.forward;
			float f = Mathf.Tan(FieldOfView * 0.5f * (MathF.PI / 180f)) * (float)SingletonAuto<ResolutionInfo>.instance.RealScreenWidth / (float)SingletonAuto<ResolutionInfo>.instance.RealScreenHeight;
			FovH = Mathf.Atan(f) * 2f * 57.29578f;
		}
	}

	private static CameraStock _basic;

	private Vector2 _h;

	private Vector2 _p;

	public static bool BasicIsValidInstance => _basic != null;

	public static CameraStock Basic
	{
		get
		{
			if (_basic == null)
			{
				string name = string.Empty;
				if (SingletonAuto<BattleContext>.isValidInstance && SingletonAuto<BattleContext>.instance.DataProvider != null)
				{
					BattleDefineData battleDefine = SingletonAuto<BattleContext>.instance.DataProvider.GetBattleDefine();
					if (battleDefine != null)
					{
						name = battleDefine.BattleCameraName;
					}
				}
				ResetCameraStock(GameSetting.data.BattleView.GetBattleCamera(name));
			}
			return _basic;
		}
	}

	public Vector3 PosOnCameraSpace => Vespa_Unity_Vector2_Extension_Method.ToVec3(_p, 0f - Mathf.Max(_h.x, _h.y));

	public void Reset(Vector3 posOnCameraSpace)
	{
		_h = new Vector2(0f - posOnCameraSpace.z, 0f - posOnCameraSpace.z);
		_p = new Vector2(posOnCameraSpace.x, posOnCameraSpace.y);
	}

	public Vector3 GetPosOnCameraSpace(CameraStock stock)
	{
		Vector3 vector = PosOnCameraSpace;
		Vector3 vector2 = stock.CameraRotation * vector;
		if (vector2.y < stock.MinHeight && Mathf.Abs(stock.CameraForward.y) > 0.01f)
		{
			float num = (stock.MinHeight - vector2.y) / stock.CameraForward.y;
			vector2 += stock.CameraForward * num;
			vector = stock.CameraRotationInverse * vector2;
		}
		if (BattleViewGenericSingleton<FieldMapManager2>.IsValidInstance)
		{
			vector = BattleViewGenericSingleton<FieldMapManager2>.instance.AdjustCameraPos(stock, vector);
		}
		return vector;
	}

	public CameraStat Clone()
	{
		return new CameraStat
		{
			_h = _h,
			_p = _p
		};
	}

	public void WeightAdd(CameraStat stat, float weight)
	{
		_h += stat._h * weight;
		_p += stat._p * weight;
	}

	public CameraStat Lerp(CameraStat rhs, float t)
	{
		return new CameraStat
		{
			_h = Vector2.Lerp(_h, rhs._h, t),
			_p = Vector2.Lerp(_p, rhs._p, t)
		};
	}

	private static void CalculateDepth(float h1, float h2, float tan, float p1, float p2, out float h, out float p)
	{
		float num = Mathf.Abs(p1 - p2);
		if (num < Mathf.Epsilon)
		{
			if (h1 > h2)
			{
				h = h1;
				p = p1;
			}
			else
			{
				h = h2;
				p = p2;
			}
			return;
		}
		h = 0.5f * h1 + 0.5f * h2 + 0.5f * num / tan;
		if (h <= h1 || h <= h2)
		{
			if (h1 > h2)
			{
				h = h1;
				p = p1;
			}
			else
			{
				h = h2;
				p = p2;
			}
		}
		else
		{
			float i_frame = 0.5f * (h2 - h1) * tan / num + 0.5f;
			p = NVespa.Math.LerpOver(p1, p2, i_frame);
		}
	}

	private void MergeStat(CameraStock stock, CameraStat stat)
	{
		float num = Mathf.Tan(stock.FieldOfView * 0.5f * (MathF.PI / 180f));
		float tan = num * (float)Screen.width / (float)Screen.height;
		CalculateDepth(_h.x, stat._h.x, tan, _p.x, stat._p.x, out var h, out var p);
		CalculateDepth(_h.y, stat._h.y, num, _p.y, stat._p.y, out var h2, out var p2);
		_h = new Vector2(h, h2);
		_p = new Vector2(p, p2);
	}

	private void MergePoint(CameraStock stock, Vector3 posOnCameraSpace)
	{
		float num = Mathf.Tan(stock.FieldOfView * 0.5f * (MathF.PI / 180f));
		float tan = num * (float)Screen.width / (float)Screen.height;
		CalculateDepth(_h.x, 0f - posOnCameraSpace.z, tan, _p.x, posOnCameraSpace.x, out var h, out var p);
		CalculateDepth(_h.y, 0f - posOnCameraSpace.z, num, _p.y, posOnCameraSpace.y, out var h2, out var p2);
		_h = new Vector2(h, h2);
		_p = new Vector2(p, p2);
	}

	private static Vector3 CalcPosOnCameraSpace(CameraStock stock, Vector3 pos)
	{
		return stock.CameraRotationInverse * pos;
	}

	public static CameraStat ExtractBounds(CameraStock stock, Bounds bounds)
	{
		bounds.size += stock.BoundsMargin;
		bounds.center += stock.BoundsOffset;
		CameraStat cameraStat = new CameraStat();
		cameraStat.Reset(CalcPosOnCameraSpace(stock, new Vector3(bounds.min.x, bounds.min.y, bounds.min.z)));
		cameraStat.MergePoint(stock, CalcPosOnCameraSpace(stock, new Vector3(bounds.min.x, bounds.min.y, bounds.max.z)));
		cameraStat.MergePoint(stock, CalcPosOnCameraSpace(stock, new Vector3(bounds.max.x, bounds.min.y, bounds.min.z)));
		cameraStat.MergePoint(stock, CalcPosOnCameraSpace(stock, new Vector3(bounds.max.x, bounds.min.y, bounds.max.z)));
		cameraStat.MergePoint(stock, CalcPosOnCameraSpace(stock, new Vector3(bounds.center.x, bounds.max.y, bounds.center.z)));
		return cameraStat;
	}

	public static void MergeBounds(CameraStock stock, Bounds bounds, float weight, CameraStat cameraStat)
	{
		CameraStat rhs = ExtractBounds(stock, bounds);
		CameraStat stat = cameraStat.Lerp(rhs, weight);
		cameraStat.MergeStat(stock, stat);
	}

	public static void ResetCameraStock(FieldCameraData cameraData = null)
	{
		if (cameraData == null)
		{
			_basic = null;
		}
		else
		{
			_basic = new CameraStock(cameraData);
		}
	}
}
