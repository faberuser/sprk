using System.Collections.Generic;
using NGame2.NBattleView.NUIOn3d2;
using NGame2.NRender.NRenderFacade;
using NGame2.NUI.NManager.NBattle;
using NGame2.NUI.NWindow.NBattle;
using NGameAsset;
using NGameAsset.NCreature;
using NShared;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NBattleView.NCreature.NHandler;

public class Looting : BaseHandler, IHandler, ILooting
{
	public enum EState
	{
		MoveToLooting,
		Looting,
		End
	}

	public const float MoveSpeed = 5f;

	public const float LootMotionTime = 0.2f;

	private List<DropItem> _lootList = new List<DropItem>();

	private EState _state;

	private float _stateTime;

	private int _lootCount;

	public bool IsEnd => _state == EState.End;

	public bool IsEmpty => __Extension_Method_ICollection__.IsNullOrEmpty(_lootList);

	public float TimeLength { get; private set; }

	public VectorXZ LastPosition
	{
		get
		{
			if (_lootList.Count == 0)
			{
				return Vespa_Unity_Vector3_Extension_Method.VecXZ(Owner.Position);
			}
			return Vespa_Unity_Vector3_Extension_Method.VecXZ(_lootList[_lootList.Count - 1].transform.position);
		}
	}

	public Looting(Creature creature)
		: base(creature)
	{
	}

	public float CalculateIfAdd(DropItem dropItem)
	{
		VectorXZ lastPosition = LastPosition;
		VectorXZ b = Vespa_Unity_Vector3_Extension_Method.VecXZ(dropItem.transform.position);
		return TimeLength + VectorXZ.Distance(lastPosition, b) / 5f + 0.2f;
	}

	public void AddDropItem(DropItem dropItem)
	{
		VectorXZ a = ((_lootList.Count != 0) ? Vespa_Unity_Vector3_Extension_Method.VecXZ(_lootList[_lootList.Count - 1].transform.position) : Vespa_Unity_Vector3_Extension_Method.VecXZ(Owner.Position));
		VectorXZ b = Vespa_Unity_Vector3_Extension_Method.VecXZ(dropItem.transform.position);
		TimeLength += VectorXZ.Distance(a, b) / 5f + 0.2f;
		_lootList.Add(dropItem);
	}

	public bool IsLootingEnd()
	{
		return IsEnd;
	}

	void IHandler.Begin()
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_lootList))
		{
			State_End();
		}
		else
		{
			State_MoveToLooting();
		}
	}

	bool IHandler.IsEnd()
	{
		return IsEnd;
	}

	void IHandler.Cancel()
	{
		_state = EState.End;
	}

	void IHandler.ChainUpdate()
	{
		if (_state == EState.MoveToLooting)
		{
			DropItem dropItem = _lootList[0];
			MoveTo(MainTimer.DefaultLocal, Vespa_Unity_Vector3_Extension_Method.VecXZ(dropItem.transform.position), 5f, idleActionIfEnd: false, out var endMove);
			if (endMove)
			{
				State_Looting();
			}
		}
		else if (_state == EState.Looting && MainTimer.time - _stateTime >= 0.2f)
		{
			if (__Extension_Method_ICollection__.IsNullOrEmpty(_lootList))
			{
				State_End();
			}
			else
			{
				State_MoveToLooting();
			}
		}
	}

	private void State_MoveToLooting()
	{
		_state = EState.MoveToLooting;
		_stateTime = MainTimer.time;
	}

	private void State_Looting()
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_lootList))
		{
			State_End();
			return;
		}
		_state = EState.Looting;
		_stateTime = MainTimer.time;
		DropItem dropItem = _lootList[0];
		_lootList.RemoveAt(0);
		if (dropItem.DropType == DropItem.EDropType.Item)
		{
			SceneAutoBehaviour<BattleManagement>.instance.GetInstance<LootingStatus>().AddBox(1);
			ItemData data = GameRoot.Instance.ItemDataContainer.GetData(dropItem.DropValue);
			if (data != null)
			{
				Manager.instance.OnLootItem(Owner.Uid, data);
			}
		}
		else if (dropItem.DropType == DropItem.EDropType.Gold)
		{
			SceneAutoBehaviour<BattleManagement>.instance.GetInstance<LootingStatus>().AddCoin(dropItem.DropValue);
			Manager.instance.OnLootCoin(Owner.Uid, dropItem.DropValue);
		}
		dropItem.Close();
		VespaObject.DestroyGo(dropItem);
		SoundStock stock = new SoundStock
		{
			Code = "UI.WorldMap.Coin",
			LocalTimer = GlobalTimer.DefaultLocal
		};
		SingletonAuto<SoundContainer>.instance.PlayOnce(stock, 10f);
		NGame2.NRender.NRenderFacade.AnimationData.StockData stockData = new NGame2.NRender.NRenderFacade.AnimationData.StockData();
		stockData.AnimationKey = new AnimationKeyData(DefineCreature.EMotion.Looting, string.Empty);
		stockData.State = NGame2.NRender.NRenderFacade.AnimationData.EState.Action;
		stockData.MotionEnd = DefineCreature.EMotionEnd.Loop;
		Owner.Action.Motion(stockData, null);
	}

	private void State_End()
	{
		_state = EState.End;
		Owner.Action.Idle();
	}
}
