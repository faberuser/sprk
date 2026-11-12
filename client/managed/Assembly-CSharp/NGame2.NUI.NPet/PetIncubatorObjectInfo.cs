using System;
using NGame2.NAccount;
using NGame2.NRender;
using NGame2.NRender.NRenderFacade;
using NGame2.NUI.NHelper.NPet;
using NGameAsset;
using NGameAsset.NCreature;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NUI.NPet;

public class PetIncubatorObjectInfo
{
	public enum IncubatorStateType
	{
		None,
		Phase0,
		Phase1Start,
		Phase1,
		Phase2Start,
		Phase2,
		Phase3Start,
		Phase3,
		Open
	}

	public const float Phase1PlayTime = 1.7f;

	public const float Phase2PlayTime = 2.3f;

	public const float Phase3PlayTime = 1.233f;

	public const float OpenPlayTime = 2f;

	public IncubatorStateType CurrentState;

	private System.Action _onEnd;

	public int SlotIndex { get; set; }

	public PetIncubatorSlotInfo SlotInfo { get; set; }

	public RenderFacade RenderFacade { get; set; }

	public DateTime StateChangeTime { get; set; }

	public void UpdateIncubatorState()
	{
		if (CheckChangingIncubatorState(CurrentState))
		{
			return;
		}
		if (_onEnd != null)
		{
			Vespa_System_Action_Extension_Method.fire(_onEnd);
			_onEnd = null;
		}
		if (SlotInfo.SetItemIndex == 0)
		{
			ChangeIncubatorStateAuto(IncubatorStateType.Phase0);
			return;
		}
		TimeSpan? incubatorRemainTime = PetHelper.GetIncubatorRemainTime(SlotInfo);
		if (!incubatorRemainTime.HasValue)
		{
			return;
		}
		if (incubatorRemainTime.Value.TotalSeconds <= 0.0)
		{
			if (CurrentState == IncubatorStateType.Phase2)
			{
				ChangeIncubatorStateAuto(IncubatorStateType.Phase3Start);
			}
			else if (CurrentState == IncubatorStateType.Phase3Start)
			{
				ChangeIncubatorStateAuto(IncubatorStateType.Phase3);
			}
		}
		else if (PetHelper.GetIncubatorRemainTimePercent(SlotInfo) >= 0.5f)
		{
			if (CurrentState == IncubatorStateType.Phase1)
			{
				ChangeIncubatorStateAuto(IncubatorStateType.Phase2Start);
			}
			else
			{
				ChangeIncubatorStateAuto(IncubatorStateType.Phase2);
			}
		}
		else if (CurrentState == IncubatorStateType.Phase1Start)
		{
			ChangeIncubatorStateAuto(IncubatorStateType.Phase1);
		}
	}

	private void ChangeIncubatorStateAuto(IncubatorStateType state)
	{
		if (CurrentState != state && state != IncubatorStateType.None)
		{
			PlayIncubatorActionByState(state);
		}
	}

	private bool CheckChangingIncubatorState(IncubatorStateType state)
	{
		float playTimebyState = GetPlayTimebyState(state);
		if (playTimebyState == 0f)
		{
			return false;
		}
		return (double)playTimebyState - (GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime() - StateChangeTime).TotalSeconds > 0.0;
	}

	private float GetPlayTimebyState(IncubatorStateType state)
	{
		return state switch
		{
			IncubatorStateType.Phase1Start => 1.7f, 
			IncubatorStateType.Phase2Start => 2.3f, 
			IncubatorStateType.Phase3Start => 1.233f, 
			IncubatorStateType.Open => 2f, 
			_ => 0f, 
		};
	}

	private bool CheckLoopIncubatorState(IncubatorStateType state)
	{
		if (state == IncubatorStateType.Phase1Start || state == IncubatorStateType.Phase2Start || state == IncubatorStateType.Phase3Start || state == IncubatorStateType.Open)
		{
			return false;
		}
		return true;
	}

	public void PlayIncubatorActionByState(IncubatorStateType state, System.Action onEnd = null, bool needInfoUpdate = false)
	{
		if (needInfoUpdate)
		{
			SlotInfo = GlobalAutoBehaviour<AccountManager>.instance.PetIncubatorSlotManager.Get(SlotIndex);
		}
		bool flag = CheckLoopIncubatorState(state);
		if (!flag)
		{
			_onEnd = onEnd;
		}
		CurrentState = state;
		StartActionAnimation(state, flag);
	}

	public static string GetIncubatorAnimKey(IncubatorStateType incubatorStateType, PetIncubatorSlotInfo slotInfo)
	{
		string result = incubatorStateType.ToString();
		if (incubatorStateType == IncubatorStateType.Phase1Start)
		{
			int setItemIndex = slotInfo.SetItemIndex;
			ItemData data = GameRoot.Instance.ItemDataContainer.GetData(setItemIndex);
			if (data == null)
			{
				return null;
			}
			switch (data.Grade)
			{
			case 3:
				result = "Phase1Start01";
				break;
			case 4:
				result = "Phase1Start02";
				break;
			case 5:
				result = "Phase1Start03";
				break;
			case 6:
				result = "Phase1Start04";
				break;
			default:
				return null;
			}
		}
		return result;
	}

	private void StartActionAnimation(IncubatorStateType incubatorStateType, bool isLoop, bool blendMotion = true)
	{
		if (!(RenderFacade == null) && incubatorStateType != IncubatorStateType.None)
		{
			string incubatorAnimKey = GetIncubatorAnimKey(incubatorStateType, SlotInfo);
			if (RenderFacade.AssetCreature is CreatureInstance creatureInstance)
			{
				creatureInstance.CloseAllNonPermanentEffects();
			}
			NGame2.NRender.NRenderFacade.AnimationData.StockData stock = new NGame2.NRender.NRenderFacade.AnimationData.StockData();
			stock.State = NGame2.NRender.NRenderFacade.AnimationData.EState.Action;
			stock.AnimationKey = AnimationKeyData.CreateEmotiMotion(incubatorAnimKey);
			stock.MotionEnd = (isLoop ? DefineCreature.EMotionEnd.Loop : DefineCreature.EMotionEnd.End);
			if (!isLoop)
			{
				stock.TimeLength = GetPlayTimebyState(incubatorStateType);
			}
			RenderFacade.Action.ApplyModifier(ref stock);
			RenderFacade.AnimationData.Play(stock, blendMotion, null);
			StateChangeTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime();
		}
	}
}
