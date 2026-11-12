using NGameAsset;
using NGameAsset.NCreature;
using NVespa;
using UnityEngine;

namespace NGame2.NRender.NRenderFacade;

public class ActionSubCreature
{
	private SubCreatureContoller _animationData;

	public bool IsBattleMode { get; private set; }

	public bool IsDead { get; private set; }

	public AnimationData.StockData CurrentMotionStock
	{
		get
		{
			if (_animationData.Runtime == null)
			{
				return null;
			}
			return _animationData.Runtime.Stock;
		}
	}

	public ActionSubCreature(SubCreatureContoller animationData)
	{
		_animationData = animationData;
		if (_animationData != null)
		{
			IsBattleMode = true;
			IsDead = false;
		}
	}

	public void ChainUpdate()
	{
		if (_animationData.Runtime.ISEND())
		{
			Idle();
		}
	}

	public AnimationData.EState GetCurActionState()
	{
		if (_animationData.Runtime != null)
		{
			return _animationData.Runtime.Stock.State;
		}
		return AnimationData.EState.Idle;
	}

	public bool IsIdle()
	{
		if (_animationData.Runtime != null)
		{
			return _animationData.Runtime.Stock.State == AnimationData.EState.Idle;
		}
		return true;
	}

	public bool IsMove()
	{
		if (_animationData.Runtime != null)
		{
			return _animationData.Runtime.Stock.State == AnimationData.EState.Run;
		}
		return true;
	}

	public bool IsActionEnd()
	{
		return _animationData.Runtime.ISEND();
	}

	public void PauseMotion()
	{
		_animationData.Pause();
	}

	public void Idle(bool blendMotion = true)
	{
		if (!IsDead && (_animationData.Runtime == null || _animationData.Runtime.Stock.State != AnimationData.EState.Idle))
		{
			MotionIdle(blendMotion);
		}
	}

	public void MotionIdle(bool blendMotion)
	{
		if (!IsDead)
		{
			AnimationData.StockData stockData = new AnimationData.StockData();
			stockData.State = AnimationData.EState.Idle;
			if (IsBattleMode)
			{
				stockData.AnimationKey = AnimationKeyData.CreateMotionType(DefineCreature.EMotion.BattleIdle);
			}
			else
			{
				stockData.AnimationKey = AnimationKeyData.CreateMotionType(DefineCreature.EMotion.Idle);
			}
			stockData.MotionEnd = DefineCreature.EMotionEnd.Loop;
			_animationData.Play(stockData, blendMotion, null);
		}
	}

	public void Stun(ILocalTimer localTimer)
	{
		if (!IsDead)
		{
			AnimationData.StockData stockData = new AnimationData.StockData();
			stockData.State = AnimationData.EState.Stun;
			stockData.AnimationKey = AnimationKeyData.CreateMotionType(DefineCreature.EMotion.Stun);
			stockData.MotionEnd = DefineCreature.EMotionEnd.Loop;
			if (stockData.State != CurrentMotionStock.State || stockData.AnimationKey.MotionType != CurrentMotionStock.AnimationKey.MotionType || stockData.MotionEnd != CurrentMotionStock.MotionEnd)
			{
				_animationData.Play(stockData, localTimer);
			}
		}
	}

	public void Down(ILocalTimer localTimer)
	{
		if (!IsDead)
		{
			AnimationData.StockData stockData = new AnimationData.StockData();
			stockData.State = AnimationData.EState.Action;
			stockData.AnimationKey = AnimationKeyData.CreateMotionType(DefineCreature.EMotion.Down);
			stockData.MotionEnd = DefineCreature.EMotionEnd.End;
			if (stockData.State != CurrentMotionStock.State || stockData.AnimationKey.MotionType != CurrentMotionStock.AnimationKey.MotionType || stockData.MotionEnd != CurrentMotionStock.MotionEnd)
			{
				_animationData.Play(stockData, localTimer);
			}
		}
	}

	public void Run(VectorXZ direction, float speed)
	{
		if (!IsDead)
		{
			float timeMultiply = 5f / speed;
			if (_animationData.Runtime != null && _animationData.Runtime.Stock.State == AnimationData.EState.Run)
			{
				_animationData.Runtime.Stock.TimeMultiply = timeMultiply;
				return;
			}
			AnimationData.StockData stockData = new AnimationData.StockData();
			stockData.State = AnimationData.EState.Run;
			stockData.AnimationKey = AnimationKeyData.CreateMotionType(DefineCreature.EMotion.Run);
			stockData.MotionEnd = DefineCreature.EMotionEnd.Loop;
			stockData.TimeMultiply = timeMultiply;
			_animationData.Play(stockData, null);
		}
	}

	public void Die(ILocalTimer localTimer)
	{
		IsDead = true;
		if (_animationData.Runtime == null || _animationData.Runtime.Stock.State != AnimationData.EState.Die)
		{
			AnimationData.StockData stockData = new AnimationData.StockData();
			stockData.State = AnimationData.EState.Die;
			stockData.AnimationKey = AnimationKeyData.CreateMotionType(DefineCreature.EMotion.Die);
			stockData.MotionEnd = DefineCreature.EMotionEnd.Infinite;
			_animationData.Play(stockData, localTimer);
		}
	}

	public void Motion(AnimationData.StockData stock, ILocalTimer localTimer)
	{
		if (!IsDead)
		{
			_animationData.Play(stock, localTimer);
		}
	}

	public void Motion(AnimationData.StockData stock, bool blendMotion, ILocalTimer localTimer)
	{
		if (!IsDead)
		{
			_animationData.Play(stock, blendMotion, localTimer);
		}
	}
}
