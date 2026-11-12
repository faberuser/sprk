using System;
using System.Collections;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NComponent2.NItemIconSub;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NPayShop;
using NGame2.NUI.NWindow.NPet;
using NGameAsset;
using NShared;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NPet;

public class LegendPetSoulUnionPanel : MonoBehaviour, IPetSoulPanel, ItemIconComponent.Listener
{
	public interface IListener
	{
		void OnClickItemIcon(ItemIconComponent comp);
	}

	public enum EState
	{
		Empty,
		PluggedIn,
		FullPluggedIn,
		Union
	}

	[Serializable]
	public class UnionPetSoulComponet
	{
		public Transform IconRoot;

		public Transform EffectRoot;

		[HideInInspector]
		public ItemIconComponent PetSoulComponet;

		[HideInInspector]
		public int PetSoulClientSlotIndex;

		public bool IsEmpty => PetSoulComponet == null;

		public void SetUnionPetSoulComp(int slotIndex, ItemIconComponent comp)
		{
			PetSoulClientSlotIndex = slotIndex;
			PetSoulComponet = comp;
			UIHelper.EnableGameObject(EffectRoot, enable: true);
		}

		public void Close()
		{
			PetSoulClientSlotIndex = -1;
			FactoryBase.SafeReleaseComponent(ref PetSoulComponet);
			UIHelper.EnableGameObject(EffectRoot, enable: false);
		}
	}

	public UIScrollGrid PetLegendSoulGrid;

	public UnionPetSoulComponet[] UnionPetSoulCompList;

	public UILabel EmptyLegendPetSoulLabel;

	public UIButton UnionButton;

	public Animator Animator;

	private List<ItemIconComponent> _petSoulCompList = new List<ItemIconComponent>();

	private PetSoulInven.IListener _petSoulInvenListener;

	private IListener _listener;

	private PetUpgradeTierData _petUpgradeTierData;

	private EState _prevState;

	private EState _curState;

	private const string EmptyAnim = "LegendPetSoulUnion";

	private const string PluggedInAnim = "LegendPetSoulUnionHit";

	private const string FullPluggedInAnim = "LegendPetSoulUnionMaxHit";

	private const string UnionAnim = "LegendPetSoulUnionGet";

	public bool IsPlayUnionAnim { get; private set; }

	public void Open(IListener listener, PetSoulInven.IListener petSoulInvenListener)
	{
		Reset();
		base.gameObject.SetActive(value: true);
		_listener = listener;
		_petSoulInvenListener = petSoulInvenListener;
		_petUpgradeTierData = GameRoot.Instance.PetUpgradeTierDataContainer.GetData(6);
		Init();
	}

	private void Init()
	{
		SetLegendPetSoulList();
	}

	private void SetLegendPetSoulList()
	{
		ClearPetSoulList();
		byte b = 6;
		List<ItemInfo> itemsByType = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.GetItemsByType(ItemType.PetSoul);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(itemsByType); i++)
		{
			ItemInfo itemInfo = itemsByType[i];
			if (itemInfo == null)
			{
				continue;
			}
			ItemData data = GameRoot.Instance.ItemDataContainer.GetData(itemInfo.ItemIndex);
			if (data != null && data.Grade >= b)
			{
				int index = data.Index;
				bool num = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.IsNewPetSoul(index);
				int newCount = 0;
				if (num)
				{
					newCount = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.GetNewPetSoulCount(index);
				}
				DuplicatePetSoulComponent(itemInfo, data, newCount);
			}
		}
		UIHelper.ResetGridPosition(PetLegendSoulGrid);
		UIHelper.EnableGameObject(EmptyLegendPetSoulLabel, __Extension_Method_ICollection__.COUNT(_petSoulCompList) == 0);
	}

	private void DuplicatePetSoulComponent(ItemInfo itemInfo, ItemData itemData, int newCount = 0)
	{
		for (int i = 0; i < itemInfo.Count; i++)
		{
			ItemIconComponent itemIconComponent = UIHelper.AddGridItem(PetLegendSoulGrid, ItemIconComponent.PrefabAuto);
			itemIconComponent.Open(itemData, this);
			if (newCount > 0)
			{
				bool flag = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.IsNewPetSoul(itemInfo.ItemIndex);
				itemIconComponent.SetNew(flag);
				newCount--;
			}
			_petSoulCompList.Add(itemIconComponent);
		}
	}

	private void SelectPetSoul(int slotIndex, ItemIconComponent comp)
	{
		int num = 0;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(UnionPetSoulCompList); i++)
		{
			if (!UnionPetSoulCompList[i].IsEmpty)
			{
				num++;
			}
		}
		if (((_petUpgradeTierData != null) ? _petUpgradeTierData.ItemCount : 0) <= num)
		{
			CenterMessage.Open(LocalizationManager.GetStringDirect("PET_TIER_UPGRADE_ALREADY_FULL"));
			return;
		}
		SetPetSoulSelection(slotIndex, isSelect: true);
		AddUnionPetSoulComp(slotIndex, comp);
		EState state = ((num == 0) ? EState.PluggedIn : EState.FullPluggedIn);
		ChangeState(state);
	}

	private void DeselectPetSoul(int slotIndex)
	{
		SetPetSoulSelection(slotIndex, isSelect: false);
		RemoveUnionPetSoulComp(slotIndex);
		int num = 0;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(UnionPetSoulCompList); i++)
		{
			if (!UnionPetSoulCompList[i].IsEmpty)
			{
				num++;
			}
		}
		EState state = EState.Empty;
		ChangeState(state);
	}

	private void SetPetSoulSelection(int slotIndex, bool isSelect)
	{
		if (Vespa_Generic_IList_Extension_Method.IsValidIndex(_petSoulCompList, slotIndex))
		{
			ItemIconComponent itemIconComponent = _petSoulCompList[slotIndex];
			if (!(itemIconComponent == null))
			{
				itemIconComponent.SetSelected(isSelect);
			}
		}
	}

	private void AddUnionPetSoulComp(int slotIndex, ItemIconComponent itemIconComp)
	{
		if (itemIconComp == null || itemIconComp.ItemData == null)
		{
			return;
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(UnionPetSoulCompList); i++)
		{
			UnionPetSoulComponet unionPetSoulComponet = UnionPetSoulCompList[i];
			if (unionPetSoulComponet.IsEmpty)
			{
				ItemIconComponent itemIconComponent = ItemIconComponent.PrefabAuto.Factory.Create(unionPetSoulComponet.IconRoot);
				itemIconComponent.Open(itemIconComp.ItemData, this);
				Vespa_Unity_Transform_Extension_Method.ResetLocalTM(itemIconComponent.transform);
				itemIconComponent.SetPetSoulInsert(isInsert: true);
				unionPetSoulComponet.SetUnionPetSoulComp(slotIndex, itemIconComponent);
				PlaySound("Pet.System.TierupgradeMatter");
				break;
			}
		}
	}

	private void RemoveUnionPetSoulComp(int slotIndex)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(UnionPetSoulCompList); i++)
		{
			UnionPetSoulComponet unionPetSoulComponet = UnionPetSoulCompList[i];
			if (unionPetSoulComponet != null && !unionPetSoulComponet.IsEmpty && unionPetSoulComponet.PetSoulClientSlotIndex == slotIndex)
			{
				unionPetSoulComponet.Close();
				break;
			}
		}
	}

	private void ChangeState(EState state, Action onEnd = null)
	{
		_prevState = _curState;
		_curState = state;
		if (state == EState.Union)
		{
			IsPlayUnionAnim = true;
		}
		string aniName = "LegendPetSoulUnion";
		string soundCode = string.Empty;
		switch (state)
		{
		case EState.PluggedIn:
			aniName = "LegendPetSoulUnionHit";
			break;
		case EState.FullPluggedIn:
			aniName = "LegendPetSoulUnionMaxHit";
			break;
		case EState.Union:
			aniName = "LegendPetSoulUnionGet";
			soundCode = "Pet.System.TierupgradeLegend";
			break;
		}
		SetAnim(aniName, soundCode, onEnd);
	}

	private void SetAnim(string aniName, string soundCode, Action onEnd = null)
	{
		VespaCoroutine.Start(coPlayAnim(aniName, soundCode, onEnd), null, null);
	}

	private IEnumerator coPlayAnim(string animName, string soundCode, Action onEnd)
	{
		if (Animator == null)
		{
			yield break;
		}
		Animator.StopPlayback();
		Animator.Play(animName);
		if (!String_Extension_Method.IsNullOrEmpty(soundCode))
		{
			PlaySound(soundCode);
		}
		yield return new WaitForSeconds(0.1f);
		AnimatorClipInfo[] currentAnimatorClipInfo = Animator.GetCurrentAnimatorClipInfo(0);
		if (__Extension_Method_ICollection__.COUNT(currentAnimatorClipInfo) != 0)
		{
			float seconds = 0f;
			if (!__Extension_Method_ICollection__.IsNullOrEmpty(currentAnimatorClipInfo))
			{
				seconds = currentAnimatorClipInfo[0].clip.length;
			}
			yield return new WaitForSeconds(seconds);
			Vespa_System_Action_Extension_Method.fire(onEnd);
		}
	}

	private void PlaySound(string soundCode)
	{
		SoundStock stock = new SoundStock
		{
			Code = soundCode
		};
		SingletonAuto<SoundContainer>.instance.PlayOnce(stock, 10f);
	}

	public void RefreshUnion()
	{
		SetLegendPetSoulList();
		ClearUnionPetSoulList();
		ChangeState(EState.Empty);
	}

	public void Close()
	{
		RemoveLegendNewIndices();
		base.gameObject.SetActive(value: false);
		Reset();
	}

	private void RemoveLegendNewIndices()
	{
		List<ItemInfo> itemsByType = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.GetItemsByType(ItemType.PetSoul);
		byte b = 6;
		List<int> list = new List<int>();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(itemsByType); i++)
		{
			ItemInfo itemInfo = itemsByType[i];
			if (itemInfo == null)
			{
				continue;
			}
			ItemData data = GameRoot.Instance.ItemDataContainer.GetData(itemInfo.ItemIndex);
			if (data != null)
			{
				int index = data.Index;
				if (!list.Contains(index) && data.Grade >= b)
				{
					list.Add(index);
					GlobalAutoBehaviour<AccountManager>.instance.ItemManager.RemoveAddedItemIndex(index);
				}
			}
		}
	}

	public void Reset()
	{
		_listener = null;
		_petSoulInvenListener = null;
		_petUpgradeTierData = null;
		IsPlayUnionAnim = false;
		ClearPetSoulList();
		ClearUnionPetSoulList();
		ChangeState(EState.Empty);
	}

	private void ClearPetSoulList()
	{
		FactoryBase.ReleaseAndClearList(_petSoulCompList);
	}

	private void ClearUnionPetSoulList()
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(UnionPetSoulCompList); i++)
		{
			UnionPetSoulCompList[i]?.Close();
		}
	}

	void ItemIconComponent.Listener.OnClick(ItemIconComponent itemIconComponent)
	{
		if (IsPlayUnionAnim || itemIconComponent.ItemData == null)
		{
			return;
		}
		if (itemIconComponent.IsSubComponentActive<NewMarkSubComponent.ComponentProxy>())
		{
			itemIconComponent.SetNew(isNew: false);
			GlobalAutoBehaviour<AccountManager>.instance.ItemManager.RemoveAddedItemIndex(itemIconComponent.ItemIndex);
			if (_listener != null)
			{
				_listener.OnClickItemIcon(itemIconComponent);
			}
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(UnionPetSoulCompList); i++)
		{
			UnionPetSoulComponet unionPetSoulComponet = UnionPetSoulCompList[i];
			if (!unionPetSoulComponet.IsEmpty && unionPetSoulComponet.PetSoulComponet == itemIconComponent)
			{
				DeselectPetSoul(unionPetSoulComponet.PetSoulClientSlotIndex);
				return;
			}
		}
		int num = _petSoulCompList.IndexOf(itemIconComponent);
		for (int j = 0; j < __Extension_Method_ICollection__.COUNT(UnionPetSoulCompList); j++)
		{
			UnionPetSoulComponet unionPetSoulComponet2 = UnionPetSoulCompList[j];
			if (!unionPetSoulComponet2.IsEmpty && unionPetSoulComponet2.PetSoulClientSlotIndex == num)
			{
				DeselectPetSoul(num);
				return;
			}
		}
		SelectPetSoul(num, itemIconComponent);
	}

	public void OnClickUnion()
	{
		if (IsPlayUnionAnim || _petUpgradeTierData == null)
		{
			return;
		}
		int num = 0;
		int num2 = ((_petUpgradeTierData != null) ? _petUpgradeTierData.ItemCount : 0);
		List<int> petSouls = new List<int>();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(UnionPetSoulCompList); i++)
		{
			UnionPetSoulComponet unionPetSoulComponet = UnionPetSoulCompList[i];
			if (unionPetSoulComponet != null && !unionPetSoulComponet.IsEmpty)
			{
				num++;
				petSouls.Add(unionPetSoulComponet.PetSoulComponet.ItemIndex);
			}
		}
		if (num2 > num)
		{
			CenterMessage.Open(LocalizationManager.GetStringDirect("PET_AWAKE_NO_MATERIAL"));
			return;
		}
		string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
		string stringDirect = LocalizationManager.GetStringDirect("PET_POPUP_DESC_CONFIRM_COMPOSE_LEGENDARY");
		PayShopMessage3ButtonPopup.Open(title, stringDirect, delegate
		{
			if (_petSoulInvenListener != null)
			{
				ChangeState(EState.Union, delegate
				{
					_petSoulInvenListener.OnClickUninon(petSouls.ToArray(), delegate
					{
						IsPlayUnionAnim = false;
						RefreshUnion();
					});
				});
			}
		});
	}
}
