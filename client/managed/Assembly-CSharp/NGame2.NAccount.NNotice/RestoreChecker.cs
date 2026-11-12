using System.Collections.Generic;
using NShared;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NAccount.NNotice;

public class RestoreChecker : ISoulWeaponChecker, IChecker, IUserObserver<ItemInfo>, IUserObserver, IUserObserver<UserManager.UserStatInfo>
{
	private ICheckerListener _listener;

	private int _maxSoulWeaponMileage;

	private string _materialItemCode;

	private int _maxMaterialItemCount;

	public Tuple<ESoulWeaponNoticeType, bool> _state = new Tuple<ESoulWeaponNoticeType, bool>(ESoulWeaponNoticeType.SoulStoneRestore, i_item2: false);

	private bool _onMilige;

	private bool _onMaterialItem;

	private void AnnounceToObservers(SoulWeaponNoticeInfo noticeInfo)
	{
		if (_listener != null && noticeInfo != null)
		{
			_listener.AnnounceToObservers(noticeInfo);
		}
	}

	void IChecker.Set(ICheckerListener listener)
	{
		if (!AccountManager.IsLogin())
		{
			VespaDebug.LogError("[SoulWeaponNoticeManager]InValid Set Timing ");
			return;
		}
		_listener = listener;
		IList<SoulStoneRestoreData> allData = GameRoot.Instance.SoulStoneRestoreDataContainer.GetAllData();
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(allData))
		{
			SoulStoneRestoreData soulStoneRestoreData = allData[0];
			if (soulStoneRestoreData != null)
			{
				_materialItemCode = soulStoneRestoreData.MaterialItemCode;
				_maxMaterialItemCount = soulStoneRestoreData.MaterialItemCount;
				_maxSoulWeaponMileage = GameRoot.Instance.ConstantDataContainer.GetValueInt("MaxSoulStoneMileage");
				long soulStoneMileage = GlobalAutoBehaviour<AccountManager>.instance.UserManager.SoulStoneMileage;
				int itemCount = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.GetItemCount(_materialItemCode);
				int maxSoulWeaponMileage = _maxSoulWeaponMileage;
				bool onMilige = soulStoneMileage >= maxSoulWeaponMileage;
				bool onMaterial = itemCount >= _maxMaterialItemCount;
				CreateSoulWeaponNoticeInfo(onMilige, onMaterial);
			}
		}
	}

	void IChecker.AddObserver()
	{
		if (AccountManager.IsLogin())
		{
			GlobalAutoBehaviour<AccountManager>.instance.AddUserObserver(this);
		}
	}

	void IChecker.RemoveObserver()
	{
		if (AccountManager.IsLogin())
		{
			GlobalAutoBehaviour<AccountManager>.instance.RemoveUserObserver(this);
		}
	}

	void IChecker.Close()
	{
		_onMilige = false;
		_onMaterialItem = false;
		_listener = null;
		SetState(enable: false);
	}

	private bool ValidNoticeType(ESoulWeaponNoticeType noticeType)
	{
		return noticeType == ESoulWeaponNoticeType.SoulStoneRestore;
	}

	bool ISoulWeaponChecker.IsNoticeType(ESoulWeaponNoticeType noticeType)
	{
		return ValidNoticeType(noticeType);
	}

	bool ISoulWeaponChecker.AnyStateOn()
	{
		return _state.Item2;
	}

	bool ISoulWeaponChecker.GetNoticeTypeState(ESoulWeaponNoticeType noticeType)
	{
		if (!ValidNoticeType(noticeType))
		{
			return false;
		}
		return _state.Item2;
	}

	bool ISoulWeaponChecker.GetNoticeTargetState(int index)
	{
		return false;
	}

	bool IUserObserver.IsGlobal()
	{
		return true;
	}

	void IUserObserver<ItemInfo>.OnChanged(ItemInfo changedInfo)
	{
		if (changedInfo != null)
		{
			ItemData data = GameRoot.Instance.ItemDataContainer.GetData(changedInfo.ItemIndex);
			if (data != null && !(data.Code != _materialItemCode))
			{
				bool onMaterial = _maxMaterialItemCount <= changedInfo.Count;
				SoulWeaponNoticeInfo noticeInfo = CreateSoulWeaponNoticeInfo(_onMilige, onMaterial);
				AnnounceToObservers(noticeInfo);
			}
		}
	}

	void IUserObserver<UserManager.UserStatInfo>.OnChanged(UserManager.UserStatInfo changedInfo)
	{
		if (changedInfo != null && changedInfo.UserStatType == UserManager.UserStatType.SoulWeaponMilige)
		{
			bool onMilige = _maxSoulWeaponMileage <= changedInfo.AfterValue;
			SoulWeaponNoticeInfo noticeInfo = CreateSoulWeaponNoticeInfo(onMilige, _onMaterialItem);
			AnnounceToObservers(noticeInfo);
		}
	}

	private bool IsChangeState(bool enable)
	{
		return _state.Item2 != enable;
	}

	private void SetState(bool enable)
	{
		_state.Item2 = enable;
	}

	private SoulWeaponNoticeInfo CreateSoulWeaponNoticeInfo(bool onMilige, bool onMaterial)
	{
		bool flag = onMilige || onMaterial;
		if (!IsChangeState(flag))
		{
			return null;
		}
		SetState(flag);
		_onMilige = onMilige;
		_onMaterialItem = onMaterial;
		return new SoulWeaponNoticeInfo
		{
			NoticeType = ESoulWeaponNoticeType.SoulStoneRestore,
			IsOn = flag
		};
	}
}
