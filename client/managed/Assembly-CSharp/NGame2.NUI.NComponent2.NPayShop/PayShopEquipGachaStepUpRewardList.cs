using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NShared;
using NVespa;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NPayShop;

public class PayShopEquipGachaStepUpRewardList : MonoBehaviour, PayShopStepUpGachaRewardItemComponent.IListener
{
	public enum EAllowDirection
	{
		None,
		Up,
		Down,
		Left,
		Right
	}

	public interface IListener
	{
		void OnClickRewardItem(StepUpGachaRewardData data);
	}

	[Serializable]
	public class StepUpGachaRewardSlotData
	{
		public int Index;

		public EAllowDirection AllowDirection;

		public Transform Root;
	}

	public GameObject Root;

	public GameObject SlotRoot;

	public int StartIndex;

	public Vector3 OriginPos;

	public Vector3 SingleLinePos;

	public StepUpGachaRewardSlotData[] SlotDatas;

	private List<PayShopStepUpGachaRewardItemComponent> _listStepUpItemComponent = new List<PayShopStepUpGachaRewardItemComponent>();

	private Dictionary<int, StepUpGachaRewardSlotData> _dicSlotData = new Dictionary<int, StepUpGachaRewardSlotData>();

	private IListener _listener;

	public void Setup(NewEquipGachaData data, StepUpEquipGachaInfo info, IListener listener)
	{
		_listener = listener;
		if (!IsShow(data))
		{
			Close();
			return;
		}
		if (info == null)
		{
			info = new StepUpEquipGachaInfo
			{
				GachaIndex = data.Index,
				CompletedCount = 0,
				CompletedStep = 0
			};
		}
		InitSlotData();
		InitSlotRoot(info);
		CreateStepUpComponents(info);
	}

	public bool IsPlayAnimation(NewEquipGachaData data, StepUpEquipGachaInfo newInfo)
	{
		if (data == null || newInfo == null)
		{
			return false;
		}
		StepUpGachaRewardData data2 = GameRoot.Instance.StepUpGachaRewardDataContainer.GetData(newInfo.GachaIndex, newInfo.CompletedStep);
		StepUpGachaRewardData nexData = GameRoot.Instance.StepUpGachaRewardDataContainer.GetNexData(data2);
		if (data2.Round != nexData.Round || data2.Step > nexData.Step)
		{
			return true;
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_listStepUpItemComponent); i++)
		{
			PayShopStepUpGachaRewardItemComponent payShopStepUpGachaRewardItemComponent = _listStepUpItemComponent[i];
			if (!(payShopStepUpGachaRewardItemComponent == null))
			{
				StepUpGachaRewardSlotData slotData = GetSlotData(i);
				payShopStepUpGachaRewardItemComponent.Refresh(newInfo, slotData);
			}
		}
		return false;
	}

	public IEnumerator CoPlayAnimation(NewEquipGachaData data, StepUpEquipGachaInfo newInfo)
	{
		int gachaIndex = data.Index;
		int stepupRound = GetStepupRound(newInfo);
		IList<StepUpGachaRewardData> stepupGachaRoundList = GameRoot.Instance.StepUpGachaRewardDataContainer.GetStepupGachaRoundList(newInfo.GachaIndex, stepupRound);
		int nextStartIndex = stepupGachaRoundList.FirstOrDefault().Step;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_listStepUpItemComponent); i++)
		{
			PayShopStepUpGachaRewardItemComponent comp = _listStepUpItemComponent[i];
			if (!(comp == null))
			{
				StepUpGachaRewardData data2 = GameRoot.Instance.StepUpGachaRewardDataContainer.GetData(gachaIndex, nextStartIndex + i);
				StepUpGachaRewardSlotData slotData = GetSlotData(i);
				comp.PlayAnimation(data2);
				yield return new WaitForSeconds(0.2f);
				comp.Refresh(newInfo, slotData);
			}
		}
	}

	private void InitSlotRoot(StepUpEquipGachaInfo info)
	{
		int stepupRound = GetStepupRound(info);
		if (GetStepupCount(info, stepupRound) <= 5)
		{
			SlotRoot.transform.localPosition = SingleLinePos;
			StartIndex = 5;
		}
		else
		{
			SlotRoot.transform.localPosition = OriginPos;
			StartIndex = 0;
		}
	}

	private void InitSlotData()
	{
		_dicSlotData.Clear();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(SlotDatas); i++)
		{
			StepUpGachaRewardSlotData stepUpGachaRewardSlotData = SlotDatas[i];
			if (stepUpGachaRewardSlotData == null)
			{
				VespaDebug.LogError("[StepUpRewardPanel] SlotData Is Null Index : " + i);
			}
			else if (_dicSlotData.ContainsKey(stepUpGachaRewardSlotData.Index))
			{
				VespaDebug.LogError("[StepUpRewardPanel]Already Contains SlotData Index :" + stepUpGachaRewardSlotData.Index);
			}
			else
			{
				_dicSlotData.Add(stepUpGachaRewardSlotData.Index, stepUpGachaRewardSlotData);
			}
		}
	}

	public void CreateStepUpComponents(StepUpEquipGachaInfo info)
	{
		FactoryBase.ReleaseAndClearList(_listStepUpItemComponent);
		int stepupRound = GetStepupRound(info);
		int stepupCount = GetStepupCount(info, stepupRound);
		IList<StepUpGachaRewardData> stepupGachaListByIndex = GameRoot.Instance.StepUpGachaRewardDataContainer.GetStepupGachaListByIndex(info.GachaIndex);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(stepupGachaListByIndex); i++)
		{
			StepUpGachaRewardData data = stepupGachaListByIndex[i];
			int index = i % stepupCount;
			StepUpGachaRewardSlotData slotData = GetSlotData(index);
			PayShopStepUpGachaRewardItemComponent payShopStepUpGachaRewardItemComponent = CreateStepUpComponent(data, slotData, stepupRound);
			if (!(payShopStepUpGachaRewardItemComponent == null))
			{
				payShopStepUpGachaRewardItemComponent.Open(data, slotData, info, this);
				_listStepUpItemComponent.Add(payShopStepUpGachaRewardItemComponent);
			}
		}
	}

	private void ReleaseStepUpRewardComponents()
	{
		if (__Extension_Method_ICollection__.COUNT(_listStepUpItemComponent) > 0)
		{
			FactoryBase.ReleaseAndClearList(_listStepUpItemComponent);
		}
	}

	private int GetStepupRound(StepUpEquipGachaInfo info)
	{
		int result = 1;
		GameRoot.Instance.StepUpGachaRewardDataContainer.GetStepupGachaListByIndex(info.GachaIndex);
		if (info.CompletedStep != 0)
		{
			StepUpGachaRewardData data = GameRoot.Instance.StepUpGachaRewardDataContainer.GetData(info.GachaIndex, info.CompletedStep);
			StepUpGachaRewardData nexData = GameRoot.Instance.StepUpGachaRewardDataContainer.GetNexData(data);
			result = ((data.Round == nexData.Round) ? data.Round : nexData.Round);
		}
		return result;
	}

	private int GetStepupCount(StepUpEquipGachaInfo info, int round)
	{
		return GameRoot.Instance.StepUpGachaRewardDataContainer.GetStepupGachaCount(info.GachaIndex, round);
	}

	private StepUpGachaRewardSlotData GetSlotData(int index)
	{
		StepUpGachaRewardSlotData value = null;
		int key = StartIndex + index;
		_dicSlotData.TryGetValue(key, out value);
		return value;
	}

	private PayShopStepUpGachaRewardItemComponent CreateStepUpComponent(StepUpGachaRewardData data, StepUpGachaRewardSlotData slotData, int round)
	{
		if (data == null)
		{
			return null;
		}
		if (slotData != null)
		{
			_ = slotData.Root == null;
		}
		if (data.Round != round)
		{
			return null;
		}
		Transform root = slotData.Root;
		return PayShopStepUpGachaRewardItemComponent.PrefabAuto.Factory.Create(root);
	}

	public void Close()
	{
		StartIndex = 0;
		ReleaseStepUpRewardComponents();
	}

	private bool IsShow(NewEquipGachaData data)
	{
		if (data != null)
		{
			return data.Type == EquipGachaType.StepUp;
		}
		return false;
	}

	public void ShowUi(bool show)
	{
		UIHelper.EnableGameObject(Root, show);
	}

	void PayShopStepUpGachaRewardItemComponent.IListener.OnClickRewardItem(StepUpGachaRewardData data)
	{
		if (_listener != null)
		{
			_listener.OnClickRewardItem(data);
		}
	}
}
