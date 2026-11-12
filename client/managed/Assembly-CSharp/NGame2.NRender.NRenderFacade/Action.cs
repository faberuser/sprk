using System.Collections.Generic;
using NGameAsset;
using NGameAsset.NCreature;
using NShared;
using NVespa;
using UnityEngine;

namespace NGame2.NRender.NRenderFacade;

public class Action
{
	public bool HasCustomIdle;

	public AnimationKeyData CustomIdleKey;

	private Dictionary<AnimationKeyData, AnimationData.Modifier> _modifierDict = new Dictionary<AnimationKeyData, AnimationData.Modifier>();

	public bool IsBattleMode { get; private set; }

	public bool IsDead { get; private set; }

	public RenderFacade Facade { get; private set; }

	public AnimationData.StockData CurrentMotionStock
	{
		get
		{
			if (Facade.AnimationData.Runtime == null)
			{
				return null;
			}
			return Facade.AnimationData.Runtime.Stock;
		}
	}

	public void AddModifier(AnimationKeyData key, AnimationData.Modifier modifier)
	{
		if (!_modifierDict.TryGetValue(key, out var value))
		{
			_modifierDict[key] = modifier;
			return;
		}
		modifier.Stack += value.Stack;
		_modifierDict[key] = modifier;
	}

	public void RemoveModifier(AnimationKeyData key)
	{
		AnimationData.Modifier value = _modifierDict[key];
		value.Stack--;
		if (value.Stack <= 0)
		{
			_modifierDict.Remove(key);
		}
		else
		{
			_modifierDict[key] = value;
		}
	}

	public bool ApplyModifier(ref AnimationData.StockData stock)
	{
		return ApplyModifier(stock.AnimationKey, ref stock);
	}

	public bool ApplyModifier(AnimationKeyData targetKey, ref AnimationData.StockData stock)
	{
		if (!_modifierDict.ContainsKey(targetKey))
		{
			return false;
		}
		AnimationData.Modifier modifier = _modifierDict[targetKey];
		stock.AnimationKey = modifier.AnimationKey;
		if (modifier.UseCustomTimeValue)
		{
			stock.TimeType = modifier.TimeType;
			stock.TimeValue = modifier.TimeValue;
		}
		return true;
	}

	public bool GetModifier(AnimationKeyData key, out AnimationData.Modifier mod)
	{
		return _modifierDict.TryGetValue(key, out mod);
	}

	public Action(RenderFacade facade)
	{
		Facade = facade;
		if (!(Facade == null) && Facade.AnimationData != null)
		{
			IsBattleMode = true;
			IsDead = false;
			Facade.AnimationData.SetDirection(AdjustDirection(VectorXZ.zero));
		}
	}

	public void SetCustomIdle(AnimationKeyData key)
	{
		HasCustomIdle = true;
		CustomIdleKey = key;
		if (Facade.AnimationData.Runtime.Stock.State == AnimationData.EState.Idle)
		{
			MotionIdle(blendMotion: false);
		}
	}

	public void SetBattleMode(bool isBattleAction, bool blendMotion = true)
	{
		if (IsBattleMode != isBattleAction)
		{
			IsBattleMode = isBattleAction;
			if (Facade.AnimationData.Runtime != null && Facade.AnimationData.Runtime.Stock.State == AnimationData.EState.Idle)
			{
				MotionIdle(blendMotion);
			}
		}
	}

	public void ChainUpdate()
	{
		if (Facade.AnimationData.Runtime.ISEND())
		{
			Idle();
		}
	}

	public AnimationData.EState GetCurActionState()
	{
		if (Facade.AnimationData.Runtime != null)
		{
			return Facade.AnimationData.Runtime.Stock.State;
		}
		return AnimationData.EState.Idle;
	}

	public bool IsIdle()
	{
		if (Facade.AnimationData.Runtime != null)
		{
			return Facade.AnimationData.Runtime.Stock.State == AnimationData.EState.Idle;
		}
		return true;
	}

	public bool IsMove()
	{
		if (Facade.AnimationData.Runtime != null)
		{
			return Facade.AnimationData.Runtime.Stock.State == AnimationData.EState.Run;
		}
		return true;
	}

	public bool IsActionEnd()
	{
		return Facade.AnimationData.Runtime.ISEND();
	}

	public void PauseMotion()
	{
		Facade.AnimationData.Pause();
	}

	public void Idle(bool blendMotion = true)
	{
		if (!IsDead && (Facade.AnimationData.Runtime == null || Facade.AnimationData.Runtime.Stock.State != AnimationData.EState.Idle))
		{
			MotionIdle(blendMotion);
		}
	}

	public void MotionIdle(bool blendMotion)
	{
		if (!IsDead)
		{
			AnimationData.StockData stock = new AnimationData.StockData();
			stock.State = AnimationData.EState.Idle;
			if (HasCustomIdle)
			{
				stock.AnimationKey = CustomIdleKey;
			}
			else if (IsBattleMode)
			{
				stock.AnimationKey = AnimationKeyData.CreateMotionType(DefineCreature.EMotion.BattleIdle);
			}
			else
			{
				stock.AnimationKey = AnimationKeyData.CreateMotionType(DefineCreature.EMotion.Idle);
			}
			stock.MotionEnd = DefineCreature.EMotionEnd.Loop;
			ApplyModifier(ref stock);
			Facade.AnimationData.Play(stock, blendMotion, null);
		}
	}

	public void Idle(VectorXZ direction)
	{
		Facade.AnimationData.BlendDirection(AdjustDirection(direction));
		Idle();
	}

	public void Stun(ILocalTimer localTimer)
	{
		if (!IsDead)
		{
			AnimationData.StockData stock = new AnimationData.StockData();
			stock.State = AnimationData.EState.Stun;
			stock.AnimationKey = AnimationKeyData.CreateMotionType(DefineCreature.EMotion.Stun);
			stock.MotionEnd = DefineCreature.EMotionEnd.Loop;
			ApplyModifier(ref stock);
			if (stock.State != CurrentMotionStock.State || stock.AnimationKey.MotionType != CurrentMotionStock.AnimationKey.MotionType || stock.MotionEnd != CurrentMotionStock.MotionEnd)
			{
				Facade.AnimationData.Play(stock, localTimer);
			}
		}
	}

	public void Down(ILocalTimer localTimer)
	{
		if (!IsDead)
		{
			AnimationData.StockData stock = new AnimationData.StockData();
			stock.State = AnimationData.EState.Action;
			stock.AnimationKey = AnimationKeyData.CreateMotionType(DefineCreature.EMotion.Down);
			stock.MotionEnd = DefineCreature.EMotionEnd.End;
			ApplyModifier(ref stock);
			if (stock.State != CurrentMotionStock.State || stock.AnimationKey.MotionType != CurrentMotionStock.AnimationKey.MotionType || stock.MotionEnd != CurrentMotionStock.MotionEnd)
			{
				Facade.AnimationData.Play(stock, localTimer);
			}
		}
	}

	public void Run(VectorXZ direction, float speed)
	{
		if (!IsDead)
		{
			Facade.AnimationData.BlendDirection(AdjustDirection(direction));
			float timeMultiply = 5f / speed;
			if (Facade.AnimationData.Runtime != null && Facade.AnimationData.Runtime.Stock.State == AnimationData.EState.Run)
			{
				Facade.AnimationData.Runtime.Stock.TimeMultiply = timeMultiply;
				ApplyModifier(AnimationKeyData.CreateMotionType(DefineCreature.EMotion.Run), ref Facade.AnimationData.Runtime.Stock);
				return;
			}
			AnimationData.StockData stock = new AnimationData.StockData();
			stock.State = AnimationData.EState.Run;
			stock.AnimationKey = AnimationKeyData.CreateMotionType(DefineCreature.EMotion.Run);
			stock.MotionEnd = DefineCreature.EMotionEnd.Loop;
			stock.TimeMultiply = timeMultiply;
			ApplyModifier(ref stock);
			Facade.AnimationData.Play(stock, null);
		}
	}

	public void Die(ILocalTimer localTimer)
	{
		IsDead = true;
		if (Facade.AnimationData.Runtime == null || Facade.AnimationData.Runtime.Stock.State != AnimationData.EState.Die)
		{
			AnimationData.StockData stock = new AnimationData.StockData();
			stock.State = AnimationData.EState.Die;
			stock.AnimationKey = AnimationKeyData.CreateMotionType(DefineCreature.EMotion.Die);
			stock.MotionEnd = DefineCreature.EMotionEnd.Infinite;
			ApplyModifier(ref stock);
			Facade.AnimationData.Play(stock, localTimer);
		}
	}

	public void Die(VectorXZ direction, ILocalTimer localTimer)
	{
		Facade.AnimationData.BlendDirection(AdjustDirection(direction));
		Die(localTimer);
	}

	public VectorXZ GetDirection()
	{
		return Facade.AnimationData.RealDirection;
	}

	public void SetDirection(VectorXZ direction, bool blendMotion)
	{
		if (!IsDead)
		{
			if (blendMotion)
			{
				Facade.AnimationData.BlendDirection(AdjustDirection(direction));
			}
			else
			{
				Facade.AnimationData.SetDirection(AdjustDirection(direction));
			}
		}
	}

	public void Motion(AnimationData.StockData stock, VectorXZ direction, ILocalTimer localTimer, FacialAnimationData facialAnimation = null)
	{
		Motion(stock, direction, blendMotion: true, localTimer, facialAnimation);
	}

	public void Motion(AnimationData.StockData stock, VectorXZ direction, bool blendMotion, ILocalTimer localTimer, FacialAnimationData facialAnimation = null)
	{
		SetDirection(direction, blendMotion);
		Motion(stock, blendMotion, localTimer, facialAnimation);
	}

	public void Motion(AnimationData.StockData stock, ILocalTimer localTimer, FacialAnimationData facialAnimation = null, bool unUseBlenMotion = false)
	{
		if (!IsDead)
		{
			Facade.AnimationData.Play(stock, !unUseBlenMotion, localTimer, facialAnimation);
		}
	}

	public void Motion(AnimationData.StockData stock, bool blendMotion, ILocalTimer localTimer, FacialAnimationData facialAnimation = null)
	{
		if (!IsDead)
		{
			Facade.AnimationData.Play(stock, blendMotion, localTimer, facialAnimation);
		}
	}

	private VectorXZ AdjustDirection(VectorXZ direction)
	{
		if (direction.magnitude < Mathf.Epsilon)
		{
			if (Facade.TeamType != TeamType.Left)
			{
				return -VectorXZ.right;
			}
			return VectorXZ.right;
		}
		return direction;
	}

	public void Revive()
	{
		IsDead = false;
		Idle(blendMotion: false);
	}
}
