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
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NPet;

public class MinorPetSoulUnionPanel : MonoBehaviour, IPetSoulPanel, ItemIconComponent.Listener
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

	public Transform[] MinorPetSoulsRoot;

	public Transform[] UnionPetSoulsRoot;

	public Transform[] UnionPetSoulEffectsRoot;

	public UIButton UnionButton;

	public Animator Animator;

	public float InsertTime = 0.01f;

	private List<ItemIconComponent> _petSoulCompList = new List<ItemIconComponent>();

	private List<ItemIconComponent> _unionPetSoulCompList = new List<ItemIconComponent>();

	private PetSoulInven.IListener _petSoulInvenListener;

	private IListener _listener;

	private PetUpgradeTierData _petUpgradeTierData;

	private ItemData _petSoulItemData;

	private EState _prevState;

	private EState _curState;

	private const string EmptyAnim = "MinorPetSoulUnion";

	private const string PluggedInAnim = "MinorPetSoulUnion_{0}_Hit";

	private const string FullPluggedInAnim = "MinorPetSoulUnion_{0}_MaxHit";

	private const string UnionAnim = "MinorPetSoulUnion_{0}_Get";

	public bool IsPlayUnionAnim { get; private set; }

	public bool IsInsertPetSoul { get; private set; }

	public void Open(IListener listener, PetSoulInven.IListener petSoulInvenListener)
	{
		Reset();
		base.gameObject.SetActive(value: true);
		_listener = listener;
		_petSoulInvenListener = petSoulInvenListener;
		Init();
	}

	private void Init()
	{
		SetMinorPetSoulList();
	}

	private void SetMinorPetSoulList()
	{
		ClearPetSoulList();
		byte b = 6;
		IList<ItemData> itemDataListByType = GameRoot.Instance.ItemDataContainer.GetItemDataListByType(ItemType.PetSoul);
		int num = 0;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(itemDataListByType); i++)
		{
			ItemData itemData = itemDataListByType[i];
			if (itemData != null && itemData.Grade > 0 && itemData.Grade < b)
			{
				int index = itemData.Index;
				int itemCount = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.GetItemCount(index);
				bool disabled = itemCount == 0;
				Transform parentTm = MinorPetSoulsRoot[num];
				ItemIconComponent itemIconComponent = ItemIconComponent.PrefabAuto.Factory.Create(parentTm);
				bool flag = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.IsNewPetSoul(index);
				itemIconComponent.Open(itemData, this);
				itemIconComponent.SetItemCount_notzero(itemCount);
				itemIconComponent.SetDisabled(disabled);
				itemIconComponent.SetNew(flag);
				Vespa_Unity_Transform_Extension_Method.ResetLocalTM(itemIconComponent.transform);
				num++;
				_petSoulCompList.Add(itemIconComponent);
			}
		}
	}

	private void SetUnionPetSoulList(ItemData itemData)
	{
		if (itemData != null)
		{
			ClearUnionPetSoul();
			_petSoulItemData = itemData;
			_petUpgradeTierData = GameRoot.Instance.PetUpgradeTierDataContainer.GetData(_petSoulItemData.Grade);
			if (_petUpgradeTierData != null)
			{
				VespaCoroutine.Start(coInsertUnionPetSoul(), null, null);
			}
		}
	}

	private IEnumerator coInsertUnionPetSoul(Action onEnd = null)
	{
		IsInsertPetSoul = true;
		int itemCount = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.GetItemCount(_petSoulItemData.Index);
		bool enoughCount = itemCount >= _petUpgradeTierData.ItemCount;
		int insertCount = (enoughCount ? _petUpgradeTierData.ItemCount : itemCount);
		for (int i = 0; i < insertCount; i++)
		{
			Transform parentTm = UnionPetSoulsRoot[i];
			ItemIconComponent itemIconComponent = ItemIconComponent.PrefabAuto.Factory.Create(parentTm);
			itemIconComponent.Open(_petSoulItemData, this);
			Vespa_Unity_Transform_Extension_Method.ResetLocalTM(itemIconComponent.transform);
			itemIconComponent.SetPetSoulInsert(isInsert: true);
			UIHelper.EnableGameObject(Vespa_Generic_IList_Extension_Method.IsValidIndex(UnionPetSoulEffectsRoot, i) ? UnionPetSoulEffectsRoot[i] : null, enable: true);
			PlaySound("Pet.System.TierupgradeMatter");
			_unionPetSoulCompList.Add(itemIconComponent);
			yield return new WaitForSeconds(InsertTime);
		}
		IsInsertPetSoul = false;
		EState state = ((!enoughCount) ? EState.PluggedIn : EState.FullPluggedIn);
		ChangeState(state);
	}

	private void RefreshSelectionMark(ItemIconComponent itemIconComponent = null)
	{
		_petSoulCompList.ForEach(delegate(ItemIconComponent e)
		{
			e.SetSelected(selected: false);
		});
		if (itemIconComponent != null)
		{
			itemIconComponent.SetSelected(selected: true);
		}
	}

	public void RefreshUnion()
	{
		SetMinorPetSoulList();
		ClearUnionPetSoul();
		ChangeState(EState.Empty);
	}

	private void ChangeState(EState state, Action onEnd = null)
	{
		_prevState = _curState;
		_curState = state;
		if (state == EState.Union)
		{
			IsPlayUnionAnim = true;
		}
		CreatureGrade grade = CreatureGrade.None;
		if (_petSoulItemData != null)
		{
			grade = (CreatureGrade)_petSoulItemData.Grade;
		}
		string animName = GetAnimName(state, grade);
		string soundCode = GetSoundCode(state);
		SetAnim(animName, soundCode, onEnd);
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

	private string GetAnimName(EState state, CreatureGrade grade)
	{
		string result = "MinorPetSoulUnion";
		switch (state)
		{
		case EState.PluggedIn:
			result = $"MinorPetSoulUnion_{grade}_Hit";
			break;
		case EState.FullPluggedIn:
			result = $"MinorPetSoulUnion_{grade}_MaxHit";
			break;
		case EState.Union:
			result = $"MinorPetSoulUnion_{grade}_Get";
			break;
		}
		return result;
	}

	private string GetSoundCode(EState state)
	{
		string result = string.Empty;
		if (state == EState.Union)
		{
			result = "Pet.System.Tierupgrade";
		}
		return result;
	}

	private void PlaySound(string soundCode)
	{
		SoundStock stock = new SoundStock
		{
			Code = soundCode
		};
		SingletonAuto<SoundContainer>.instance.PlayOnce(stock, 10f);
	}

	public void Close()
	{
		RemoveMinorNewIndices();
		base.gameObject.SetActive(value: false);
		Reset();
	}

	private void RemoveMinorNewIndices()
	{
		List<ItemInfo> itemsByType = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.GetItemsByType(ItemType.PetSoul);
		byte b = 6;
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
				if (data.Grade < b)
				{
					GlobalAutoBehaviour<AccountManager>.instance.ItemManager.RemoveAddedItemIndex(index);
				}
			}
		}
	}

	public void Reset()
	{
		_listener = null;
		_petSoulInvenListener = null;
		IsPlayUnionAnim = false;
		IsInsertPetSoul = false;
		ClearPetSoulList();
		ClearUnionPetSoul();
		ChangeState(EState.Empty);
	}

	private void ClearPetSoulList()
	{
		FactoryBase.ReleaseAndClearList(_petSoulCompList);
	}

	private void ClearUnionPetSoul()
	{
		_petSoulItemData = null;
		_petUpgradeTierData = null;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(UnionPetSoulEffectsRoot); i++)
		{
			UIHelper.EnableGameObject(UnionPetSoulEffectsRoot[i], enable: false);
		}
		FactoryBase.ReleaseAndClearList(_unionPetSoulCompList);
	}

	void ItemIconComponent.Listener.OnClick(ItemIconComponent itemIconComponent)
	{
		if (IsInsertPetSoul || IsPlayUnionAnim)
		{
			return;
		}
		ItemData itemData = itemIconComponent.ItemData;
		if (itemData == null)
		{
			return;
		}
		if (itemIconComponent.IsDisabledClick())
		{
			CenterMessage.Open(LocalizationManager.GetStringDirect("EQUIP_TIER_UPGRADE.NOT_ENOUGH_MATERIAL_ITEM"));
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
		if (itemIconComponent.IsSelected() || _unionPetSoulCompList.Contains(itemIconComponent))
		{
			RefreshSelectionMark();
			ClearUnionPetSoul();
			ChangeState(EState.Empty);
		}
		else
		{
			RefreshSelectionMark(itemIconComponent);
			SetUnionPetSoulList(itemData);
		}
	}

	public void OnClickUnion()
	{
		if (IsInsertPetSoul || IsPlayUnionAnim)
		{
			return;
		}
		if (_petUpgradeTierData == null || _petSoulItemData == null)
		{
			CenterMessage.Open(LocalizationManager.GetStringDirect("PET_AWAKE_NO_MATERIAL"));
			return;
		}
		int reqCount = _petUpgradeTierData.ItemCount;
		if (__Extension_Method_ICollection__.COUNT(_unionPetSoulCompList) < reqCount)
		{
			CenterMessage.Open(LocalizationManager.GetStringDirect("PET_AWAKE_NO_MATERIAL"));
			return;
		}
		string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
		string stringDirect = LocalizationManager.GetStringDirect("PET_POPUP_DESC_CONFIRM_COMPOSE_NORMAL");
		PayShopMessage3ButtonPopup.Open(title, stringDirect, delegate
		{
			if (_petSoulInvenListener != null)
			{
				ChangeState(EState.Union, delegate
				{
					List<int> list = new List<int>();
					for (int i = 0; i < reqCount; i++)
					{
						list.Add(_petSoulItemData.Index);
					}
					_petSoulInvenListener.OnClickUninon(list.ToArray(), delegate
					{
						IsPlayUnionAnim = false;
						RefreshUnion();
					});
				});
			}
		});
	}
}
