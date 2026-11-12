using System;
using NGame2.NUI;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NAccount;

public class UserStarterGachaManager : NShared.UserStarterGachaManager
{
	public class ChangedGachaInfo
	{
		public int GachaIndex;

		public int PrevCount;

		public int CurrCount;

		public int PrevTicket;

		public int CurrTicket;
	}

	private readonly UserObserverManager<ChangedGachaInfo> _observerManager = new UserObserverManager<ChangedGachaInfo>();

	public IUserObserverManager ObserverManager => _observerManager;

	public void Apply(StarterPickupEquipGachaInfo[] infos)
	{
		if (infos != null)
		{
			for (int i = 0; i < infos.Length; i++)
			{
				Set(infos[i]);
			}
		}
	}

	public void Set(StarterPickupEquipGachaInfo info)
	{
		if (info != null)
		{
			StarterPickupEquipGachaInfo starterPickupEquipGachaInfo = Get(info.GachaIndex);
			Add(info);
			if (starterPickupEquipGachaInfo != null && NeedUpdate(info, starterPickupEquipGachaInfo))
			{
				_observerManager.AnnounceToObservers(new ChangedGachaInfo
				{
					GachaIndex = info.GachaIndex,
					PrevCount = starterPickupEquipGachaInfo.GachaCount,
					CurrCount = info.GachaCount
				});
			}
		}
	}

	private static bool NeedUpdate(StarterPickupEquipGachaInfo info, StarterPickupEquipGachaInfo oldInfo)
	{
		if (info == null || oldInfo == null)
		{
			return false;
		}
		if (info.GachaCount != oldInfo.GachaCount)
		{
			return true;
		}
		if (info.IsTakeLastReward != oldInfo.IsTakeLastReward)
		{
			return true;
		}
		return false;
	}

	public bool IsRemainGachaCount(int index)
	{
		StarterPickupEquipGachaInfo starterPickupEquipGachaInfo = Get(index);
		NewEquipGachaData data = GameRoot.Instance.NewEquipGachaDataContainer.GetData(index);
		if (data == null)
		{
			return false;
		}
		if (starterPickupEquipGachaInfo == null)
		{
			return false;
		}
		int starterMileage = data.StarterMileage;
		int gachaCount = starterPickupEquipGachaInfo.GachaCount;
		int num = starterMileage - gachaCount;
		if (!starterPickupEquipGachaInfo.IsTakeLastReward && num == 0)
		{
			return true;
		}
		if (starterMileage - gachaCount > 0)
		{
			return true;
		}
		if (!starterPickupEquipGachaInfo.IsTakeLastReward)
		{
			return true;
		}
		return false;
	}

	[Obsolete("NAccount.HelperItem.GetFreeGachaItemCount 를 사용하세요")]
	public int GetRemainFreeTicketCount(int index)
	{
		NewEquipGachaData data = GameRoot.Instance.NewEquipGachaDataContainer.GetData(index);
		if (data == null)
		{
			return 0;
		}
		int freeGachaItemCount = HelperItem.GetFreeGachaItemCount(data.Index);
		if (freeGachaItemCount <= 0)
		{
			return 0;
		}
		return freeGachaItemCount;
	}

	public int GetRemainLimitChance(int index)
	{
		StarterPickupEquipGachaInfo starterPickupEquipGachaInfo = Get(index);
		NewEquipGachaData data = GameRoot.Instance.NewEquipGachaDataContainer.GetData(index);
		EquipGachaInfo equipGachaInfo = GlobalAutoBehaviour<AccountManager>.instance.EquipGachaManager.Get(index);
		int starterMileage = data.StarterMileage;
		if (equipGachaInfo.BeginTime != null && equipGachaInfo.EndTime != null)
		{
			if (UIHelper.IsEnableDate(equipGachaInfo.BeginTime, equipGachaInfo.EndTime, isUTC: true))
			{
				if (starterPickupEquipGachaInfo == null)
				{
					return 0;
				}
				int gachaCount = starterPickupEquipGachaInfo.GachaCount;
				int num = starterMileage - gachaCount;
				if (num < 0)
				{
					num = 0;
				}
				return num;
			}
			return 0;
		}
		if (starterPickupEquipGachaInfo == null)
		{
			return 0;
		}
		int gachaCount2 = starterPickupEquipGachaInfo.GachaCount;
		int num2 = starterMileage - gachaCount2;
		if (num2 < 0)
		{
			num2 = 0;
		}
		return num2;
	}
}
