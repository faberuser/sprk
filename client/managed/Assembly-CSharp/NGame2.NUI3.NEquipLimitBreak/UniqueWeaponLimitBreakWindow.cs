using System;
using System.Collections.Generic;
using System.Linq;
using NGame2.NAccount;
using NGame2.NContentsInfo;
using NGame2.NUI;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow;
using NGame2.NUI3.NEmbed;
using NShared;
using NShared.NContentsDefine;
using NShared.SoulWeaponLimitBreak;
using NVespa;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using NVespa.NUtil;
using UnityEngine;

namespace NGame2.NUI3.NEquipLimitBreak;

public class UniqueWeaponLimitBreakWindow : BaseWindow, IUserObserver<NGame2.NAccount.UserEquipItemManager.EquipStatInfo>, IUserObserver
{
	public interface Listener
	{
	}

	private class PendingUpdateInfo
	{
		public float Delay;

		public EquipItemInfo TargetInfo;
	}

	[Serializable]
	public class TopmostUi
	{
		public UIButton Button_OpenMiniGuide;

		public int MiniGuideData_Index;

		public void InitUi()
		{
			GameMiniGuideData data = GameRoot.Instance.GameMiniGuideDataContainer.GetData(MiniGuideData_Index);
			UIHelper.EnableGameObject(Button_OpenMiniGuide, data != null);
		}
	}

	[Serializable]
	public class DevCheatButtonUi
	{
		public UIWidget Group_Cheat;

		public UIButton Button_Cheat;

		public void InitUi(EquipItemInfo targetInfo)
		{
			UIHelper.EnableGameObject(Group_Cheat, enable: false);
		}
	}

	[Serializable]
	public class TargetAndAfterUi : ItemIconComponent.Listener
	{
		public UIWidget Widget_Pivot_Target;

		public UIWidget Widget_Pivot_After;

		private ItemIconComponent _targetComp;

		private ItemIconComponent _afterComp;

		public UniqueWeaponLimitBreakWindow Window { get; private set; }

		public void InitUi(EquipItemInfo targetInfo, EquipItemInfo afterInfo, UniqueWeaponLimitBreakWindow window)
		{
			ResetUi();
			Window = window;
			if (targetInfo != null)
			{
				_targetComp = ItemIconComponent.PrefabAuto.Factory.Create(Widget_Pivot_Target.transform);
				Vespa_Unity_Transform_Extension_Method.ResetLocalTM(_targetComp.transform);
				_targetComp.Open(targetInfo, this);
				_afterComp = ItemIconComponent.PrefabAuto.Factory.Create(Widget_Pivot_After.transform);
				Vespa_Unity_Transform_Extension_Method.ResetLocalTM(_afterComp.transform);
				_afterComp.Open(afterInfo, null);
			}
		}

		public void ResetUi()
		{
			FactoryBase.SafeReleaseComponent(ref _targetComp);
			FactoryBase.SafeReleaseComponent(ref _afterComp);
		}

		void ItemIconComponent.Listener.OnClick(ItemIconComponent itemIconComponent)
		{
			if (_targetComp == itemIconComponent)
			{
				Window.InitUi(null);
			}
		}
	}

	[Serializable]
	public class ItemAsMaterialUi
	{
		public UIWidget Widget_Pivot_MainEquip;

		public Embed_CountAndRequire_Comp CountAndRequire_MainEquip;

		public UIGame3DWidget Effect_Add_MainEquip;

		public UIWidget Widget_Pivot_SubItem_0;

		public Embed_CountAndRequire_Comp CountAndRequire_SubItem_0;

		public UIWidget Widget_Pivot_SubItem_1;

		public Embed_CountAndRequire_Comp CountAndRequire_SubItem_1;

		private ItemIconComponent _mainEquipComp;

		private ItemIconComponent _subItem_0_Comp;

		private ItemIconComponent _subItem_1_Comp;

		public void PlayEffect_Add_MainEquip()
		{
			Effect_Add_MainEquip.Play(null);
		}

		public void InitUi(EquipItemInfo targetInfo, List<EquipItemInfo> mainMaterialList)
		{
			ResetUi();
			if (targetInfo != null)
			{
				InitMainEquip_UniqueWeapon(targetInfo, mainMaterialList);
				InitSubItem_0_SoulStone(targetInfo);
				InitSubItem_1_TransStone(targetInfo);
				InitCountUi(targetInfo, mainMaterialList);
			}
		}

		public void RefreshUi(EquipItemInfo targetInfo, List<EquipItemInfo> mainMaterialList)
		{
			ResetMainEquip_UniqueWeapon();
			ResetCountUi();
			InitMainEquip_UniqueWeapon(targetInfo, mainMaterialList);
			InitCountUi(targetInfo, mainMaterialList);
		}

		private void InitCountUi(EquipItemInfo targetInfo, List<EquipItemInfo> mainMaterialList)
		{
			NShared.NContentsDefine.SoulWeaponLimitBreak.LimitBreakData afterDataByCurrent = ContentsDefine.SoulWeaponLimitBreak.GetAfterDataByCurrent(targetInfo);
			if (afterDataByCurrent != null)
			{
				CountAndRequire_MainEquip.InitUi(__Extension_Method_ICollection__.COUNT(mainMaterialList), afterDataByCurrent.WeaponUniqueCount);
				int soulStoneIndex = SingletonAuto<NGame2.NContentsInfo.SoulWeaponLimitBreak>.instance.GetSoulStoneIndex(targetInfo);
				int itemCount = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.GetItemCount(soulStoneIndex);
				CountAndRequire_SubItem_0.InitUi(itemCount, afterDataByCurrent.SoulStoneCount);
				int itemCount2 = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.GetItemCount(afterDataByCurrent.TransStoneIndex);
				CountAndRequire_SubItem_1.InitUi(itemCount2, afterDataByCurrent.TransStoneCount);
			}
		}

		private void InitMainEquip_UniqueWeapon(EquipItemInfo targetInfo, List<EquipItemInfo> mainMaterialList)
		{
			if (__Extension_Method_ICollection__.COUNT(mainMaterialList) == 0)
			{
				_ = targetInfo.EquipItemData;
				_mainEquipComp = ItemIconComponent.PrefabAuto.Factory.Create(Widget_Pivot_MainEquip.transform);
				Vespa_Unity_Transform_Extension_Method.ResetLocalTM(_mainEquipComp.transform);
				_mainEquipComp.Open(targetInfo.ItemData);
				_mainEquipComp.SetIconGrayScale(grayScale: true);
				_mainEquipComp.WidgetThis.alpha = 0.5f;
			}
			else
			{
				EquipItemInfo equipItemInfo = mainMaterialList[__Extension_Method_ICollection__.COUNT(mainMaterialList) - 1];
				_mainEquipComp = ItemIconComponent.PrefabAuto.Factory.Create(Widget_Pivot_MainEquip.transform);
				Vespa_Unity_Transform_Extension_Method.ResetLocalTM(_mainEquipComp.transform);
				_mainEquipComp.Open(equipItemInfo, null);
			}
		}

		private void InitSubItem_0_SoulStone(EquipItemInfo targetInfo)
		{
			int soulStoneIndex = SingletonAuto<NGame2.NContentsInfo.SoulWeaponLimitBreak>.instance.GetSoulStoneIndex(targetInfo);
			ItemData data = GameRoot.Instance.ItemDataContainer.GetData(soulStoneIndex);
			if (data != null)
			{
				_subItem_0_Comp = ItemIconComponent.PrefabAuto.Factory.Create(Widget_Pivot_SubItem_0.transform);
				Vespa_Unity_Transform_Extension_Method.ResetLocalTM(_subItem_0_Comp.transform);
				_subItem_0_Comp.Open(data);
			}
		}

		private void InitSubItem_1_TransStone(EquipItemInfo targetInfo)
		{
			NShared.NContentsDefine.SoulWeaponLimitBreak.LimitBreakData afterDataByCurrent = ContentsDefine.SoulWeaponLimitBreak.GetAfterDataByCurrent(targetInfo);
			if (afterDataByCurrent != null)
			{
				ItemData data = GameRoot.Instance.ItemDataContainer.GetData(afterDataByCurrent.TransStoneIndex);
				if (data != null)
				{
					_subItem_1_Comp = ItemIconComponent.PrefabAuto.Factory.Create(Widget_Pivot_SubItem_1.transform);
					Vespa_Unity_Transform_Extension_Method.ResetLocalTM(_subItem_1_Comp.transform);
					_subItem_1_Comp.Open(data);
				}
			}
		}

		public void ResetUi()
		{
			ResetMainEquip_UniqueWeapon();
			ResetCountUi();
			FactoryBase.SafeReleaseComponent(ref _subItem_0_Comp);
			FactoryBase.SafeReleaseComponent(ref _subItem_1_Comp);
			Effect_Add_MainEquip.Stop(immediatelyClear: true);
		}

		private void ResetMainEquip_UniqueWeapon()
		{
			if (_mainEquipComp != null)
			{
				_mainEquipComp.WidgetThis.alpha = 1f;
			}
			FactoryBase.SafeReleaseComponent(ref _mainEquipComp);
		}

		private void ResetCountUi()
		{
			CountAndRequire_MainEquip.Reset();
			CountAndRequire_SubItem_0.Reset();
			CountAndRequire_SubItem_1.Reset();
		}
	}

	[Serializable]
	public class LimitBreakChanceUi
	{
		public UILabel Label_ChancePercent;

		public UILabel Label_FailBonusPercent;

		public void InitUi(EquipItemInfo targetInfo)
		{
			ResetUi();
			if (targetInfo != null)
			{
				NShared.NContentsDefine.SoulWeaponLimitBreak.LimitBreakData afterDataByCurrent = ContentsDefine.SoulWeaponLimitBreak.GetAfterDataByCurrent(targetInfo);
				if (afterDataByCurrent != null)
				{
					UIHelper.SetLabelText(Label_ChancePercent, $"{afterDataByCurrent.SuccessRatio}%");
					UIHelper.SetLabelText(Label_FailBonusPercent, $"{targetInfo.UpgradeStarFailBonus}%");
				}
			}
		}

		public void ResetUi()
		{
			UIHelper.SetLabelText(Label_ChancePercent, "-");
			UIHelper.SetLabelText(Label_FailBonusPercent, "-");
		}
	}

	[Serializable]
	public class ChangingStatUi
	{
		public Embed_ChangingStatGrid_Comp Embed_Prefab;

		public void InitUi(EquipItemInfo targetInfo, EquipItemInfo afterInfo)
		{
			ResetUi();
			if (targetInfo != null)
			{
				StatData equipItemStatData = StatCalculator.GetEquipItemStatData(targetInfo);
				StatData equipItemStatData2 = StatCalculator.GetEquipItemStatData(afterInfo);
				Embed_Prefab.InitUi(targetInfo, equipItemStatData, afterInfo, equipItemStatData2);
			}
		}

		public void ResetUi()
		{
			Embed_Prefab.ResetUi();
		}
	}

	[Serializable]
	public class RequestLimitBreakUi
	{
		public UILabel Label_RequireGold;

		public UIButton Button_RequestLimitBreak;

		public void InitUi(EquipItemInfo targetInfo, List<EquipItemInfo> mainMaterialList)
		{
			ResetUi();
			checked
			{
				if (targetInfo != null)
				{
					int goldCount = ContentsDefine.SoulWeaponLimitBreak.GetAfterDataByCurrent(targetInfo).GoldCount;
					int num = __Extension_Method_ICollection__.COUNT(mainMaterialList);
					int num2 = goldCount;
					long i_value = unchecked((long)num2) * unchecked((long)num);
					UIHelper.SetLabelText(Label_RequireGold, Vespa_Int_Extension_Method.ToCommaString(i_value));
					bool enable = SingletonAuto<NGame2.NContentsInfo.SoulWeaponLimitBreak>.instance.CanRequest_LimitBreak(targetInfo, mainMaterialList);
					UIHelper.EnableButton(Button_RequestLimitBreak, enable);
				}
			}
		}

		public void ResetUi()
		{
			UIHelper.SetLabelText(Label_RequireGold, "-");
			UIHelper.EnableButton(Button_RequestLimitBreak, enable: false);
		}
	}

	[Serializable]
	public class EquipItemGridUi : Embed_ItemGrid_Comp.IListener
	{
		public Embed_ItemGrid_Comp ItemGrid;

		public LocalizeLinkDataInner LocalLink_NoItem;

		private bool _need_Select_Target;

		private UniqueWeaponLimitBreakWindow _window;

		public void InitUi(EquipItemInfo targetInfo, UniqueWeaponLimitBreakWindow window)
		{
			_window = window;
			ItemGrid.InitUi(this, LocalLink_NoItem.GetLocalize());
			ItemGrid.Clear();
			if (targetInfo != null)
			{
				_need_Select_Target = false;
				FillMaterial_by_Target(targetInfo);
			}
			else
			{
				_need_Select_Target = true;
				FillSoulWeapon_for_LimitBreak();
			}
			UIHelper.RepositionGrid(ItemGrid.ItemGrid);
		}

		public void RefreshUi(EquipItemInfo targetInfo, List<EquipItemInfo> mainMaterialList)
		{
			NShared.NContentsDefine.SoulWeaponLimitBreak.LimitBreakData limitBreakDefine = ContentsDefine.SoulWeaponLimitBreak.GetAfterDataByCurrent(targetInfo);
			if (limitBreakDefine == null)
			{
				return;
			}
			int currentSelectedCount = __Extension_Method_ICollection__.COUNT(mainMaterialList);
			__Extension_Method_ICollection__.COUNT(mainMaterialList);
			_ = limitBreakDefine.WeaponUniqueCount;
			ItemGrid.ForEach(delegate(ItemIconComponent comp)
			{
				if (!comp.IsSelected())
				{
					if (currentSelectedCount < limitBreakDefine.WeaponUniqueCount)
					{
						comp.SetIconGrayScale(grayScale: false);
					}
					else
					{
						comp.SetIconGrayScale(grayScale: true);
					}
				}
			});
		}

		private void FillMaterial_by_Target(EquipItemInfo targetInfo)
		{
			foreach (EquipItemInfo item in GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager)
			{
				if (SingletonAuto<NGame2.NContentsInfo.SoulWeaponLimitBreak>.instance.CanUse_as_Material(item, targetInfo))
				{
					ItemGrid.AddItem(item);
				}
			}
		}

		private void FillSoulWeapon_for_LimitBreak()
		{
			SoulWeaponInfo[] values = GlobalAutoBehaviour<AccountManager>.instance.SoulWeaponManager.GetValues();
			if (__Extension_Method_ICollection__.IsNullOrEmpty(values))
			{
				return;
			}
			SoulWeaponInfo[] array = values;
			for (int i = 0; i < array.Length; i++)
			{
				int equipItemSlotIndex = array[i].EquipItemSlotIndex;
				EquipItemInfo equipItemBySlotIndex = GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.GetEquipItemBySlotIndex(equipItemSlotIndex);
				if (equipItemBySlotIndex != null && SingletonAuto<NGame2.NContentsInfo.SoulWeaponLimitBreak>.instance.CanSelect_LimitBreakTarget(equipItemBySlotIndex))
				{
					ItemGrid.AddItem(equipItemBySlotIndex);
				}
			}
		}

		void Embed_ItemGrid_Comp.IListener.OnSelect(ItemIconComponent itemIconComponent)
		{
			if (itemIconComponent.EquipItemInfo != null)
			{
				if (_need_Select_Target)
				{
					_window.InitUi(itemIconComponent.EquipItemInfo);
				}
				else
				{
					_window.Toggle_MainMaterial_from_EquipItemGridUi(itemIconComponent);
				}
			}
		}
	}

	public const string PrefabPath = "Resources/Prefab3/EquipLimitBreak/UniqueWeaponLimitBreakWindow";

	[Header("[[위쪽]] [Topmost Menu & 도움말]")]
	public TopmostUi Topmost;

	[Header("[[왼쪽]] [개발 전용 치트 버튼]")]
	public DevCheatButtonUi DevCheatButton;

	[Header("[[왼쪽]] [현재 타겟 무기와 돌파 후 결과 무기]")]
	public TargetAndAfterUi TargetAndAfter;

	[Header("[[왼쪽]] [돌파 재료가 되는 아이템들]")]
	public ItemAsMaterialUi ItemAsMaterial;

	[Header("[[왼쪽]] [성공 확율 표기]")]
	public LimitBreakChanceUi LimitBreakChance;

	[Header("[[왼쪽]] [변동 스텟 표기]")]
	public ChangingStatUi ChangingStat;

	[Header("[[왼쪽]] [한계 돌파 요청]")]
	public RequestLimitBreakUi RequestLimitBreak;

	[Header("[[오른쪽]] [장비 리스트]")]
	public EquipItemGridUi EquipItemGrid;

	private Listener _listener;

	private EquipItemInfo _targetItemInfo;

	private EquipItemInfo _afterItemInfo;

	private List<EquipItemInfo> _mainMaterialList = new List<EquipItemInfo>();

	private PendingUpdateInfo _pendingUpdate;

	public void InitWindow(Listener listener, EquipItemInfo info)
	{
		_listener = listener;
		InitUi(info);
	}

	bool IUserObserver.IsGlobal()
	{
		return false;
	}

	void IUserObserver<NGame2.NAccount.UserEquipItemManager.EquipStatInfo>.OnChanged(NGame2.NAccount.UserEquipItemManager.EquipStatInfo equipStatInfo)
	{
		if (_targetItemInfo != null && _targetItemInfo.SlotIndex == equipStatInfo.SlotIndex)
		{
			EquipItemInfo equipItemBySlotIndex = GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.GetEquipItemBySlotIndex(equipStatInfo.SlotIndex);
			PendingUpdate(equipItemBySlotIndex, 0f);
		}
	}

	private void PendingUpdate(EquipItemInfo info, float delay)
	{
		if (_pendingUpdate == null)
		{
			_pendingUpdate = new PendingUpdateInfo();
		}
		_pendingUpdate.Delay = Mathf.Max(_pendingUpdate.Delay, delay);
		_pendingUpdate.TargetInfo = info;
	}

	private void InitUi(EquipItemInfo info)
	{
		if (SingletonAuto<NGame2.NContentsInfo.SoulWeaponLimitBreak>.instance.CanSelect_LimitBreakTarget(info))
		{
			_targetItemInfo = info;
			_afterItemInfo = _targetItemInfo.Clone();
			_afterItemInfo.Star = _targetItemInfo.Star + 1;
			_afterItemInfo.SoulWeaponInfo = _targetItemInfo.SoulWeaponInfo;
		}
		else
		{
			_targetItemInfo = null;
			_afterItemInfo = null;
		}
		_mainMaterialList.Clear();
		DevCheatButton.InitUi(_targetItemInfo);
		TargetAndAfter.InitUi(_targetItemInfo, _afterItemInfo, this);
		ItemAsMaterial.InitUi(_targetItemInfo, _mainMaterialList);
		LimitBreakChance.InitUi(_targetItemInfo);
		ChangingStat.InitUi(_targetItemInfo, _afterItemInfo);
		EquipItemGrid.InitUi(_targetItemInfo, this);
		RequestLimitBreak.InitUi(_targetItemInfo, _mainMaterialList);
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		if (_pendingUpdate != null)
		{
			_pendingUpdate.Delay -= Time.deltaTime;
			if (_pendingUpdate.Delay <= 0f)
			{
				EquipItemInfo targetInfo = _pendingUpdate.TargetInfo;
				_pendingUpdate = null;
				InitUi(targetInfo);
			}
		}
	}

	public override void Close()
	{
		base.Close();
		ResetEquipLimitBreakWindow();
	}

	public void ResetEquipLimitBreakWindow()
	{
		_listener = null;
	}

	private void Toggle_MainMaterial_from_EquipItemGridUi(ItemIconComponent itemIconComponent)
	{
		if (_targetItemInfo == null)
		{
			return;
		}
		NShared.NContentsDefine.SoulWeaponLimitBreak.LimitBreakData limitBreakDefine = ContentsDefine.SoulWeaponLimitBreak.GetAfterDataByCurrent(_targetItemInfo);
		if (limitBreakDefine == null)
		{
			return;
		}
		int selectedSlotIndex = itemIconComponent.EquipItemInfo.SlotIndex;
		int num = _mainMaterialList.FindIndex((EquipItemInfo v) => v.SlotIndex == selectedSlotIndex);
		if (num < 0)
		{
			if (__Extension_Method_ICollection__.COUNT(_mainMaterialList) >= limitBreakDefine.WeaponUniqueCount)
			{
				return;
			}
			_mainMaterialList.Add(itemIconComponent.EquipItemInfo);
			itemIconComponent.SetSelected(selected: true);
			ItemAsMaterial.PlayEffect_Add_MainEquip();
		}
		else
		{
			_mainMaterialList.RemoveAt(num);
			itemIconComponent.SetSelected(selected: false);
		}
		RequestLimitBreak.InitUi(_targetItemInfo, _mainMaterialList);
		ItemAsMaterial.RefreshUi(_targetItemInfo, _mainMaterialList);
		EquipItemGrid.RefreshUi(_targetItemInfo, _mainMaterialList);
		int currentSelectedCount = __Extension_Method_ICollection__.COUNT(_mainMaterialList);
		__Extension_Method_ICollection__.COUNT(_mainMaterialList);
		_ = limitBreakDefine.WeaponUniqueCount;
		EquipItemGrid.ItemGrid.ForEach(delegate(ItemIconComponent comp)
		{
			if (!comp.IsSelected())
			{
				if (currentSelectedCount < limitBreakDefine.WeaponUniqueCount)
				{
					comp.SetIconGrayScale(grayScale: false);
				}
				else
				{
					comp.SetIconGrayScale(grayScale: true);
				}
			}
		});
	}

	public void OnClick_BackButton()
	{
		Close();
	}

	public void OnClick_OpenMiniGuide()
	{
		GameMiniGuideData data = GameRoot.Instance.GameMiniGuideDataContainer.GetData(Topmost.MiniGuideData_Index);
		if (data != null)
		{
			SceneAutoBehaviour<LobbyManagement>.instance.GetInstance<MiniGuidePopup>().Open(data.GameGuideIndex);
		}
	}

	public void OnClick_CheatNoResource()
	{
	}

	public void OnClick_RequestLimitBreak()
	{
		Request_to_Server_LimitBreak(cheat: false);
	}

	private void Request_to_Server_LimitBreak(bool cheat)
	{
		if (_targetItemInfo == null)
		{
			return;
		}
		NShared.NContentsDefine.SoulWeaponLimitBreak.LimitBreakData afterDataByCurrent = ContentsDefine.SoulWeaponLimitBreak.GetAfterDataByCurrent(_targetItemInfo);
		if (afterDataByCurrent == null)
		{
			return;
		}
		int soulStoneIndex = SingletonAuto<NGame2.NContentsInfo.SoulWeaponLimitBreak>.instance.GetSoulStoneIndex(_targetItemInfo);
		int[] materialItemInfors = new int[4] { soulStoneIndex, afterDataByCurrent.SoulStoneCount, afterDataByCurrent.TransStoneIndex, afterDataByCurrent.TransStoneCount };
		int[] materialSlotIndices = _mainMaterialList.Select((EquipItemInfo v) => v.SlotIndex).ToArray();
		NShared.SoulWeaponLimitBreak.Request request = new NShared.SoulWeaponLimitBreak.Request
		{
			EquipItemSlotIndex = _targetItemInfo.SlotIndex,
			MaterialSlotIndices = materialSlotIndices,
			MaterialItemInfors = materialItemInfors
		};
		EquipItemInfo oldTargetInfo = _targetItemInfo.Clone();
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.SoulWeaponLimitBreak.Response response)
		{
			if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
			}
			else if (response.Result != ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Apply(response.ResultEquipItem.SlotIndex, response.ResultEquipItem);
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.CurrencyResult);
				GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.ItemResults);
				for (int i = 0; i < __Extension_Method_ICollection__.COUNT(response.RemovedEquipItemSlotIndices); i++)
				{
					int slotIndex = response.RemovedEquipItemSlotIndices[i];
					GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Apply(slotIndex, null);
				}
				GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.RemovedItemResultInfos);
				OpenResultScene(response, oldTargetInfo);
			}
		});
	}

	private void OpenResultScene(NShared.SoulWeaponLimitBreak.Response response, EquipItemInfo oldTargetInfo)
	{
		PendingUpdate(response.ResultEquipItem, 1f);
		LimitBreakResultScene successScene = SceneAutoBehaviour<BlacksmithManagement>.instance.GetInstance<LimitBreakResultScene>();
		successScene.Open(delegate
		{
			successScene.InitScene(response.Success, oldTargetInfo, response.ResultEquipItem, delegate
			{
				if (_pendingUpdate != null)
				{
					_pendingUpdate.Delay = 0f;
				}
			});
		});
	}
}
