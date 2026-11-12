using NShared;

namespace NGame2.NAccount;

public class UserEquipGachaManager : NShared.UserEquipGachaManager
{
	public enum ChangeType
	{
		None,
		UpdateOnSale
	}

	public class ChangedGachaInfo
	{
		public ChangeType changeType;

		public int GachaIndex;
	}

	private readonly UserObserverManager<ChangedGachaInfo> _observerManager = new UserObserverManager<ChangedGachaInfo>();

	public IUserObserverManager ObserverManager => _observerManager;

	public void Apply(EquipGachaInfo[] infos)
	{
		if (infos != null)
		{
			for (int i = 0; i < infos.Length; i++)
			{
				Set(infos[i]);
			}
		}
	}

	public void Set(EquipGachaInfo info)
	{
		if (info != null)
		{
			EquipGachaInfo oldInfo = Get(info.GachaIndex);
			Add(info);
			if (NeedUpdate(info, oldInfo))
			{
				_observerManager.AnnounceToObservers(new ChangedGachaInfo
				{
					changeType = ChangeType.UpdateOnSale,
					GachaIndex = info.GachaIndex
				});
			}
		}
	}

	private static bool NeedUpdate(EquipGachaInfo info, EquipGachaInfo oldInfo)
	{
		if (info.OnSale != oldInfo.OnSale)
		{
			return true;
		}
		if (info.RemainNumberOfDiscount != oldInfo.RemainNumberOfDiscount)
		{
			return true;
		}
		if (info.LastDiscountedTime != oldInfo.LastDiscountedTime)
		{
			return true;
		}
		if (info.Discount != oldInfo.Discount)
		{
			return true;
		}
		return false;
	}
}
