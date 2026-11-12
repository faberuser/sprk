using System;
using NGame2.N3D;
using NGame2.NAccount;
using NGame2.NBattleField;
using NShared;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NBattleView.NCamera;

public abstract class CameraBase
{
	public class WeightData
	{
		public float Frame { get; private set; }

		public float Value { get; private set; }

		public void SetByFrame(float frame)
		{
			Frame = Mathf.Clamp01(frame);
			Value = Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(GameSetting.data.BattleView.BattleCamera_WeightCurve, Frame, Frame);
		}

		public void Set(float weight)
		{
			Value = weight;
		}
	}

	public class BoundsInfo
	{
		public int Uid;

		public Bounds Bounds;

		public float Weight;

		public float LeaveDelay;

		public int LastUpdateFrame;

		public BoundsInfo(int uid)
		{
			Uid = uid;
		}
	}

	private static Comparison<BoundsInfo> _comparison;

	public WeightData Weight = new WeightData();

	public IncreaseDic<int, BoundsInfo> Dic = new IncreaseDic<int, BoundsInfo>();

	public virtual ILocalTimer Timer => GlobalTimer.DefaultLocal;

	public virtual float CameraBlendSpeed => 1f;

	public virtual float EnterWeightPerSec => 2f;

	public virtual float LeaveWeightPerSec => 0.5f;

	public virtual float LeaveDelay => 0f;

	static CameraBase()
	{
		_comparison = (BoundsInfo lhs, BoundsInfo rhs) => Mathf.Approximately(lhs.Weight, rhs.Weight) ? lhs.Uid.CompareTo(rhs.Uid) : rhs.Weight.CompareTo(lhs.Weight);
	}

	public static TeamType GetMyTeam()
	{
		long id = GlobalAutoBehaviour<AccountManager>.instance.UserManager.Id;
		return SingletonAuto<BattleFacade>.instance.GetPlayerCache(id).TeamType;
	}

	public virtual void Reset()
	{
		int frameCount = GlobalTimer.frameCount;
		Dic.Clear();
		for (int i = 0; i < BattleViewGenericSingleton<CreatureManager>.instance.GetCreatureCount(); i++)
		{
			Creature creatureByIndex = BattleViewGenericSingleton<CreatureManager>.instance.GetCreatureByIndex(i);
			if (!(creatureByIndex == null) && IsAcceptable(creatureByIndex))
			{
				BoundsInfo boundsInfo = Dic.GETVALUE(creatureByIndex.Uid, null);
				if (boundsInfo == null)
				{
					boundsInfo = new BoundsInfo(creatureByIndex.Uid);
					Dic.Add(creatureByIndex.Uid, boundsInfo);
				}
				boundsInfo.Bounds = creatureByIndex.ShapeBounds;
				boundsInfo.Bounds.center += creatureByIndex.Position;
				ModifyCreatureBounds(creatureByIndex, ref boundsInfo.Bounds);
				boundsInfo.Weight = 1f;
				boundsInfo.LeaveDelay = LeaveDelay;
				boundsInfo.LastUpdateFrame = frameCount;
			}
		}
	}

	public virtual void ChainUpdate()
	{
		int frameCount = GlobalTimer.frameCount;
		float num = (float)Timer.deltaTick * 0.001f;
		for (int i = 0; i < BattleViewGenericSingleton<CreatureManager>.instance.GetCreatureCount(); i++)
		{
			Creature creatureByIndex = BattleViewGenericSingleton<CreatureManager>.instance.GetCreatureByIndex(i);
			if (!(creatureByIndex == null) && IsAcceptable(creatureByIndex))
			{
				Bounds bounds = creatureByIndex.ShapeBounds;
				bounds.center += creatureByIndex.Position;
				BoundsInfo boundsInfo = Dic.GETVALUE(creatureByIndex.Uid, null);
				if (boundsInfo == null)
				{
					boundsInfo = new BoundsInfo(creatureByIndex.Uid);
					boundsInfo.Bounds = bounds;
					Dic.Add(creatureByIndex.Uid, boundsInfo);
				}
				Bounds bounds2 = boundsInfo.Bounds;
				ModifyCreatureBounds(creatureByIndex, ref bounds);
				float maxDistanceDelta = Vector3.Distance(bounds2.center, bounds.center) * num * GameSetting.data.BattleView.BattleCamera_BoundsBlendSpeed;
				bounds.center = Vector3.MoveTowards(bounds2.center, bounds.center, maxDistanceDelta);
				boundsInfo.Bounds = bounds;
				boundsInfo.Weight = Mathf.Clamp01(boundsInfo.Weight + EnterWeightPerSec * num);
				boundsInfo.LeaveDelay = LeaveDelay;
				boundsInfo.LastUpdateFrame = frameCount;
			}
		}
		for (int j = 0; j < Dic.Count; j++)
		{
			BoundsInfo valueByIndex = Dic.GetValueByIndex(j);
			if (valueByIndex.LastUpdateFrame != frameCount)
			{
				if (valueByIndex.LeaveDelay > 0f)
				{
					valueByIndex.LeaveDelay -= num;
				}
				else
				{
					valueByIndex.Weight -= LeaveWeightPerSec * num;
				}
				if (valueByIndex.Weight < Mathf.Epsilon)
				{
					Dic.RemoveByIndex(j);
					j--;
				}
			}
		}
		Dic.SortByValue(_comparison);
		if (Dic.Count > 0)
		{
			Dic.GetValueByIndex(0).Weight = 1f;
		}
	}

	public CameraStat GetCameraInfo(CameraStat.CameraStock cameraStock)
	{
		if (Dic.IsNullOrEmpty())
		{
			return null;
		}
		BoundsInfo valueByIndex = Dic.GetValueByIndex(0);
		CameraStat cameraStat = CameraStat.ExtractBounds(cameraStock, valueByIndex.Bounds);
		for (int i = 1; i < Dic.Count; i++)
		{
			valueByIndex = Dic.GetValueByIndex(i);
			if (!(valueByIndex.Weight < Mathf.Epsilon))
			{
				CameraStat.MergeBounds(cameraStock, valueByIndex.Bounds, valueByIndex.Weight, cameraStat);
			}
		}
		return cameraStat;
	}

	protected abstract bool IsAcceptable(Creature creature);

	protected virtual void ModifyCreatureBounds(Creature creature, ref Bounds bounds)
	{
	}
}
