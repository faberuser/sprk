using System;
using NGame2.NAccount;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NCustomizing;
using NShared;
using NShared.SaveCostumeStorageSlot;
using NShared.SetCostumeStorageSlot;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using NVespa.NWeb;

namespace NGame2.NUI.NManager.NCostume;

public class MySetCostumeRequester : CostumeRequesterBase, MySetCostumeViewWidget.IListener
{
	public MySetCostumeRequester(WebServiceRequest webRequest)
		: base(webRequest)
	{
	}

	void MySetCostumeViewWidget.IListener.OnClickEquip(MySetCostumeViewWidget caller, Action<bool> onComplete)
	{
		if (IsWebRequesterProcessing())
		{
			VespaDebug.LogError("IsWebRequesterProcessing() :: return");
			return;
		}
		NShared.SetCostumeStorageSlot.Request request = new NShared.SetCostumeStorageSlot.Request
		{
			HeroIndex = caller.HeroInfo.HeroIndex,
			CostumeStorageSlotIndex = caller.CurrentComponentOpenData.SlotIndex
		};
		_currentServiceRequest = SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.SetCostumeStorageSlot.Response response)
		{
			if (response.Result != NShared.SetCostumeStorageSlot.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
				Vespa_System_Action_Extension_Method.fire(onComplete, i_argA: false);
			}
			else if (response.CostumeStorageSlotInfo == null)
			{
				VespaDebug.LogError("response.CostumeStorageSlotInfo == null");
				Vespa_System_Action_Extension_Method.fire(onComplete, i_argA: false);
			}
			else
			{
				ApplySlot(response.CostumeStorageSlotInfo);
				Vespa_System_Action_Extension_Method.fire(onComplete, i_argA: true);
			}
		});
	}

	void MySetCostumeViewWidget.IListener.OnClickSave(MySetCostumeViewWidget caller)
	{
		if (IsWebRequesterProcessing())
		{
			VespaDebug.LogError("IsWebRequesterProcessing() :: return");
			return;
		}
		NShared.SaveCostumeStorageSlot.Request request = new NShared.SaveCostumeStorageSlot.Request
		{
			HeroIndex = caller.HeroInfo.HeroIndex,
			CostumeStorageSlotIndex = caller.CurrentComponentOpenData.SlotIndex
		};
		_currentServiceRequest = SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.SaveCostumeStorageSlot.Response response)
		{
			if (response.Result != NShared.SaveCostumeStorageSlot.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else if (response.CostumeStorageSlotInfo == null)
			{
				VespaDebug.LogError("CostumeStorageSlotInfo == null");
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.CostumeStorageManager.Apply(response.CostumeStorageSlotInfo);
				Action<string> action = delegate(string str)
				{
					VespaDebug.LogError(str);
				};
				CostumeStorageSlotInfo costumeStorageSlotInfo = response.CostumeStorageSlotInfo;
				if (request.HeroIndex != costumeStorageSlotInfo.HeroIndex)
				{
					action($"incorrect hero Index  req ({request.HeroIndex}) res ({costumeStorageSlotInfo.HeroIndex})");
				}
			}
		});
	}

	private void ApplySlot(CostumeStorageSlotInfo responseSlot)
	{
		int heroIndex = responseSlot.HeroIndex;
		int costumeIndex = responseSlot.CostumeIndex;
		int hairCostumeIndex = responseSlot.HairCostumeIndex;
		byte hideUniqueWeapon = responseSlot.HideUniqueWeapon;
		int weaponCostumeIndex = responseSlot.WeaponCostumeIndex;
		int accessoryCostumeIndex = responseSlot.AccessoryCostumeIndex1;
		int accessoryCostumeIndex2 = responseSlot.AccessoryCostumeIndex2;
		int accessoryCostumeIndex3 = responseSlot.AccessoryCostumeIndex3;
		int accessoryCostumeIndex4 = responseSlot.AccessoryCostumeIndex4;
		string positionInfo = responseSlot.PositionInfo1;
		string positionInfo2 = responseSlot.PositionInfo2;
		string positionInfo3 = responseSlot.PositionInfo3;
		string positionInfo4 = responseSlot.PositionInfo4;
		AccountManager instance = GlobalAutoBehaviour<AccountManager>.instance;
		instance.HeroManager.SetCostumeIndex(heroIndex, costumeIndex);
		instance.HeroManager.SetHairCostumeIndex(heroIndex, hairCostumeIndex);
		instance.HeroManager.SetHideUniqueWeapon(heroIndex, hideUniqueWeapon);
		instance.HeroManager.SetWeaponCostumeIndex(heroIndex, weaponCostumeIndex);
		PositionInfo positionInfo5 = (String_Extension_Method.IsNullOrEmpty(positionInfo) ? null : PositionInfo.Create().TryAssign(positionInfo));
		instance.HeroManager.SetAccessoryCostume(heroIndex, AccessoryPartType.Head, accessoryCostumeIndex, positionInfo5);
		PositionInfo positionInfo6 = (String_Extension_Method.IsNullOrEmpty(positionInfo2) ? null : PositionInfo.Create().TryAssign(positionInfo2));
		instance.HeroManager.SetAccessoryCostume(heroIndex, AccessoryPartType.Eye, accessoryCostumeIndex2, positionInfo6);
		PositionInfo positionInfo7 = (String_Extension_Method.IsNullOrEmpty(positionInfo3) ? null : PositionInfo.Create().TryAssign(positionInfo3));
		instance.HeroManager.SetAccessoryCostume(heroIndex, AccessoryPartType.Nose, accessoryCostumeIndex3, positionInfo7);
		PositionInfo positionInfo8 = (String_Extension_Method.IsNullOrEmpty(positionInfo4) ? null : PositionInfo.Create().TryAssign(positionInfo4));
		instance.HeroManager.SetAccessoryCostume(heroIndex, AccessoryPartType.Body, accessoryCostumeIndex4, positionInfo8);
	}
}
