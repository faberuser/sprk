using System.Collections.Generic;
using NVespa;

namespace NGameAsset.NCreature;

public class CreatureLoad : ICreatureLoad, IAsyncLoad<ICreature>, IAsyncLoadBase
{
	public OpenData OpenData;

	public ICreature Value;

	public bool IsDone;

	public bool IsValid;

	public string LoadMessage = string.Empty;

	private bool _isClosed;

	public CreatureStock Stock { get; private set; }

	string IAsyncLoadBase.KeyDesc
	{
		get
		{
			string text = Stock.Code;
			if (Stock.Star > 0)
			{
				text += $" Star={Stock.Star}";
			}
			if (!String_Extension_Method.IsNullOrEmpty(Stock.BodyPrefix))
			{
				text += $" BodyPrefix={Stock.BodyPrefix}";
			}
			if (!String_Extension_Method.IsNullOrEmpty(Stock.HairPrefix))
			{
				text += $" HairPrefix={Stock.HairPrefix}";
			}
			if (!String_Extension_Method.IsNullOrEmpty(Stock.WeaponStock.WeaponPrefix))
			{
				text += $" WeaponStock.WeaponPrefix={Stock.WeaponStock.WeaponPrefix}";
			}
			if (Stock.WeaponStock.WeaponStar > 0)
			{
				text += $" WeaponStock.WeaponStar={Stock.WeaponStock.WeaponStar}";
			}
			return text;
		}
	}

	bool IAsyncLoadBase.IsDone => IsDone;

	bool IAsyncLoadBase.IsValid => IsValid;

	string IAsyncLoadBase.LoadMessage => LoadMessage;

	ICreature IAsyncLoad<ICreature>.Value => Value;

	Builder ICreatureLoad.Builder
	{
		get
		{
			if (OpenData != null)
			{
				return OpenData.Builder;
			}
			return null;
		}
	}

	CreatureStock ICreatureLoad.Stock
	{
		get
		{
			if (OpenData != null)
			{
				return OpenData.Stock;
			}
			return Stock;
		}
	}

	public void Open(CreatureStock stock)
	{
		_isClosed = false;
		Stock = stock;
		Value = new CreatureProxy();
		Builder builder = SingletonAuto<BuilderContainer>.instance.Get(Stock.Code);
		if (builder == null)
		{
			VespaDebug.LogError($"Creature Load Fail: not found builder in container (Code= {Stock.Code})");
			return;
		}
		OpenData = builder.CreateOpenData(stock);
		if (OpenData == null)
		{
			VespaDebug.LogError($"Creature Load Fail: create open data fail (Code= {Stock.Code})");
			return;
		}
		AsyncManager.IEntry entry = OpenData.CreateAsyncEntry(OnCompleteAsset);
		SingletonUpdater<AsyncManager>.instance.Add(entry);
	}

	public void Close()
	{
		if (Value != null)
		{
			Value.Close();
			Value = null;
		}
		if (OpenData != null)
		{
			OpenData.Close();
		}
		_isClosed = true;
	}

	private void SetInvalid(string message, IAsyncLoadBase childAsyncLoad)
	{
		IsDone = true;
		IsValid = false;
		LoadMessage = message;
		if (childAsyncLoad != null && !String_Extension_Method.IsNullOrEmpty(childAsyncLoad.LoadMessage))
		{
			LoadMessage += $"\n - {childAsyncLoad.LoadMessage}";
		}
		VespaDebug.LogError("Creature Load Fail: " + LoadMessage);
	}

	private void OnCompleteAsset()
	{
		if (_isClosed)
		{
			return;
		}
		if (OpenData == null)
		{
			SetInvalid("OpenData is null", null);
			return;
		}
		IAsyncLoadBase invalidAsset = OpenData.GetInvalidAsset();
		if (invalidAsset != null)
		{
			SetInvalid("Invalid Asset", invalidAsset);
			return;
		}
		if (!(Value is CreatureProxy creatureProxy))
		{
			SetInvalid("Value is not CreatureProxy", null);
			return;
		}
		CreatureInstance creatureInstance = new CreatureInstance();
		if (!creatureInstance.Open(OpenData))
		{
			creatureInstance.Close();
			SetInvalid("CreatureImpl Open Fail", null);
			return;
		}
		IsDone = true;
		IsValid = true;
		Value = creatureInstance;
		creatureProxy.ApplyToReal(creatureInstance);
		Vespa_Common_Class_Extension_Method.Interface((ICreature)creatureProxy).Close();
	}

	private void OnCompleteChangeStock()
	{
		if (_isClosed)
		{
			return;
		}
		if (OpenData == null)
		{
			SetInvalid("OpenData is null", null);
			return;
		}
		IAsyncLoadBase invalidAsset = OpenData.GetInvalidAsset();
		if (invalidAsset != null)
		{
			SetInvalid("Invalid Asset", invalidAsset);
			return;
		}
		if (!(Value is CreatureInstance creatureInstance))
		{
			SetInvalid("Value is not CreatureInstance", null);
			return;
		}
		CreatureInstance creatureInstance2 = new CreatureInstance();
		if (!creatureInstance2.Open(OpenData))
		{
			creatureInstance2.Close();
			SetInvalid("CreatureImpl Open Fail", null);
			return;
		}
		IsDone = true;
		IsValid = true;
		Value = creatureInstance2;
		creatureInstance.ApplyTo(creatureInstance2);
		Vespa_Common_Class_Extension_Method.Interface((ICreature)creatureInstance).Close();
	}

	void ICreatureLoad.ChangeStock(CreatureStock stock)
	{
		if (OpenData != null)
		{
			Builder builder = OpenData.Builder;
			Stock = stock;
			OpenData.Close();
			OpenData.Open(builder, stock);
			if (IsDone)
			{
				IsDone = false;
				IsValid = false;
				AsyncManager.IEntry entry = OpenData.CreateAsyncEntry(OnCompleteChangeStock);
				SingletonUpdater<AsyncManager>.instance.Add(entry);
			}
		}
	}

	void ICreatureLoad.ChangeCustomizeStock(CustomizeStock itemStock)
	{
		if (OpenData != null)
		{
			if (!itemStock.IsEmptyPart() && !itemStock.IsValid())
			{
				VespaDebug.LogError("itemStock.IsValid() == false");
			}
			if (!(Value is CreatureInstance))
			{
				SetInvalid("Value is not CreatureInstance", null);
				return;
			}
			OpenData.ChangeCustomizeItemInfo(itemStock);
			OnComleteCustomizeItemStock();
		}
	}

	private void OnComleteCustomizeItemStock()
	{
		if (!_isClosed)
		{
			if (OpenData == null)
			{
				SetInvalid("OpenData is null", null);
			}
			else if (!(Value is CreatureInstance creatureInstance))
			{
				SetInvalid("Value is not CreatureInstance", null);
			}
			else
			{
				creatureInstance.ReopenCustomizeItem();
			}
		}
	}

	void ICreatureLoad.ChangeHairPrefix(string hairPrefix)
	{
		if (OpenData != null && !(Stock.HairPrefix == hairPrefix))
		{
			CreatureStock stock = Stock;
			stock.HairPrefix = hairPrefix;
			Stock = stock;
			OpenData.ChangeHairPrefix(hairPrefix);
			if (IsDone)
			{
				AsyncManager.IEntry entry = OpenData.CreateAsyncEntry(OnCompleteChangeHairAsset);
				SingletonUpdater<AsyncManager>.instance.Add(entry);
			}
		}
	}

	private void OnCompleteChangeHairAsset()
	{
		if (_isClosed)
		{
			return;
		}
		if (OpenData == null)
		{
			SetInvalid("OpenData is null", null);
			return;
		}
		IAsyncLoadBase invalidAsset = OpenData.GetInvalidAsset();
		if (invalidAsset != null)
		{
			SetInvalid("Invalid Asset", invalidAsset);
			return;
		}
		if (!(Value is CreatureInstance creatureInstance))
		{
			SetInvalid("Value is not CreatureInstance", null);
			return;
		}
		CreatureInstance creatureInstance2 = new CreatureInstance();
		if (!creatureInstance2.Open(OpenData))
		{
			creatureInstance2.Close();
			SetInvalid("CreatureImpl Open Fail", null);
		}
		else
		{
			creatureInstance.ApplyTo(creatureInstance2);
			creatureInstance.Close();
			Value = creatureInstance2;
		}
	}

	void ICreatureLoad.ChangeWeaponStock(WeaponStock weaponStock)
	{
		if (OpenData != null && (!(Stock.WeaponStock.WeaponPrefix == weaponStock.WeaponPrefix) || Stock.WeaponStock.WeaponStar != weaponStock.WeaponStar || Stock.WeaponStock.IsEquipSoulWeapon != weaponStock.IsEquipSoulWeapon || Stock.WeaponStock.SoulWeaponGrade != weaponStock.SoulWeaponGrade))
		{
			CreatureStock stock = Stock;
			stock.WeaponStock = weaponStock;
			Stock = stock;
			OpenData.ChangeWeaponPrefix(weaponStock);
			if (IsDone)
			{
				AsyncManager.IEntry entry = OpenData.CreateAsyncEntry(OnCompleteChangeWeaponAsset);
				SingletonUpdater<AsyncManager>.instance.Add(entry);
			}
		}
	}

	private void OnCompleteChangeWeaponAsset()
	{
		if (_isClosed)
		{
			return;
		}
		if (OpenData == null)
		{
			SetInvalid("OpenData is null", null);
			return;
		}
		IAsyncLoadBase invalidAsset = OpenData.GetInvalidAsset();
		if (invalidAsset != null)
		{
			SetInvalid("Invalid Asset", invalidAsset);
		}
		else if (!(Value is CreatureInstance creatureInstance))
		{
			SetInvalid("Value is not CreatureInstance", null);
		}
		else
		{
			creatureInstance.ReOpenWeapon();
		}
	}

	private void OnCompleteLoadExtraAnimation()
	{
		if (_isClosed)
		{
			return;
		}
		if (OpenData == null)
		{
			SetInvalid("OpenData is null", null);
			return;
		}
		IAsyncLoadBase invalidAsset = OpenData.GetInvalidAsset();
		if (invalidAsset != null)
		{
			SetInvalid("Invalid Asset", invalidAsset);
		}
		else if (!(Value is CreatureInstance creatureInstance))
		{
			SetInvalid("Value is not CreatureInstance", null);
		}
		else
		{
			creatureInstance.RefreshAnimationData();
		}
	}

	IAsyncLoadBase ICreatureLoad.LoadFacialModelAsync()
	{
		if (OpenData == null)
		{
			return null;
		}
		OpenData.LoadFacialModelAsync();
		return OpenData;
	}

	void ICreatureLoad.OpenFacialModel()
	{
		if (Value is CreatureInstance creatureInstance)
		{
			creatureInstance.OpenFacialModel();
		}
	}

	IAsyncLoadBase ICreatureLoad.LoadExtraAnimation(DefineCreature.EMotion motionType)
	{
		if (OpenData == null)
		{
			return null;
		}
		OpenData.LoadExtraAnimation(motionType);
		if (!IsDone)
		{
			return OpenData;
		}
		if (Value is CreatureInstance creatureInstance)
		{
			creatureInstance.RefreshAnimationData();
		}
		return OpenData;
	}

	IAsyncLoadBase ICreatureLoad.LoadExtraAnimation(List<AnimationKeyData> keyList)
	{
		if (OpenData == null)
		{
			return null;
		}
		OpenData.LoadExtraAnimation(keyList);
		if (!IsDone)
		{
			return OpenData;
		}
		if (Value is CreatureInstance creatureInstance)
		{
			creatureInstance.RefreshAnimationData();
		}
		return OpenData;
	}
}
