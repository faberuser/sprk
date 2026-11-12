using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NWindow;
using NShared;
using NShared.AddHeroStorageSlot;
using NShared.BuyHeroStorageSlot;
using NShared.ChangeHeroStorageSlotName;
using NShared.SetHeroStorageSlot;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NUI.NManager;

public class HeroPresetStorageWebManager : SingletonAuto<HeroPresetStorageWebManager>
{
	public interface IListener
	{
		void OnBuyHeroStorageSlot(HeroPresetStorageInfo heroPresetStorageInfo);

		void OnAddHeroStorageSlot(HeroPresetStorageInfo heroPresetStorageInfo);

		void OnChangeHeroStorageSlot(string staorageKey, string name);

		void OnSetHeroStorageSlot();
	}

	public interface ILoadPresetListener
	{
		void OnBeforeLoadPreset();

		void OnAfterLoadPreset();
	}

	private List<IListener> _listListener = new List<IListener>();

	private List<ILoadPresetListener> _listLoadPresetListener = new List<ILoadPresetListener>();

	public void AddLoadPresetListener(ILoadPresetListener listener)
	{
		_listLoadPresetListener.Remove(listener);
		_listLoadPresetListener.Add(listener);
	}

	public void RemoveLoadPresetListener(ILoadPresetListener listener)
	{
		_listLoadPresetListener.Remove(listener);
	}

	public void AddListener(IListener listener)
	{
		_listListener.Remove(listener);
		_listListener.Add(listener);
	}

	public void RemoveListener(IListener listener)
	{
		_listListener.Remove(listener);
	}

	public void RequestBuyHeroSotrageSlot(string storageKey)
	{
		NShared.BuyHeroStorageSlot.Request request = new NShared.BuyHeroStorageSlot.Request
		{
			StorageKey = storageKey
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.BuyHeroStorageSlot.Response response)
		{
			if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
			}
			else if (response.Result != NShared.BuyHeroStorageSlot.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.CurrencyResults);
				GlobalAutoBehaviour<AccountManager>.instance.HeroPresetStorageManager.Apply(response.HeroPresetStorage);
				_listListener.ForEach(delegate(IListener e)
				{
					e.OnBuyHeroStorageSlot(response.HeroPresetStorage);
				});
			}
		});
	}

	public void RequestAddHeroStorageSlot(string storageKey)
	{
		NShared.AddHeroStorageSlot.Request request = new NShared.AddHeroStorageSlot.Request
		{
			StorageKey = storageKey
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.AddHeroStorageSlot.Response response)
		{
			if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
			}
			else if (response.Result != NShared.AddHeroStorageSlot.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.CurrencyResults);
				GlobalAutoBehaviour<AccountManager>.instance.HeroPresetStorageManager.Apply(response.HeroPresetStorage);
				_listListener.ForEach(delegate(IListener e)
				{
					e.OnAddHeroStorageSlot(response.HeroPresetStorage);
				});
			}
		});
	}

	public void RequestChangeHeroStorageSlot(string storageKey, string name)
	{
		NShared.ChangeHeroStorageSlotName.Request request = new NShared.ChangeHeroStorageSlotName.Request
		{
			StorageKey = storageKey,
			Name = name
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.ChangeHeroStorageSlotName.Response response)
		{
			if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
			}
			else if (response.Result != NShared.ChangeHeroStorageSlotName.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.HeroPresetStorageManager.ApplyName(response.StorageKey, response.NewName);
				_listListener.ForEach(delegate(IListener e)
				{
					e.OnChangeHeroStorageSlot(response.StorageKey, response.NewName);
				});
			}
		});
	}

	public void RequestSetHeroStorageSlot(string storageKey)
	{
		NShared.SetHeroStorageSlot.Request request = new NShared.SetHeroStorageSlot.Request
		{
			StorageKey = storageKey
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.SetHeroStorageSlot.Response response)
		{
			if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
			}
			else if (response.Result != NShared.SetHeroStorageSlot.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				_listLoadPresetListener.ForEach(delegate(ILoadPresetListener e)
				{
					e.OnBeforeLoadPreset();
				});
				GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Apply(response.EquipItems);
				PresetStorageHelper.ApplyUnEquipItems(response.UnEquippedSlotIndices);
				PresetStorageHelper.ApplyHeroPresetStorage(response.Heroes);
				_listLoadPresetListener.ForEach(delegate(ILoadPresetListener e)
				{
					e.OnAfterLoadPreset();
				});
				_listListener.ForEach(delegate(IListener e)
				{
					e.OnSetHeroStorageSlot();
				});
			}
		});
	}
}
