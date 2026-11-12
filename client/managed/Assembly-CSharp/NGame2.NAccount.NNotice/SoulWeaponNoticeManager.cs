using NGame2.NUI.NManager.NLobby;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NAccount.NNotice;

public class SoulWeaponNoticeManager : ICheckerListener, IUserObserver<ChapterDungeonInfo>, IUserObserver
{
	private ISoulWeaponChecker UpgradeChecker;

	private ISoulWeaponChecker RestoreChecker;

	private int _godKingChapterIndex;

	private int _godKingDungeonIndex;

	private bool _godKingComplete;

	private readonly UserObserverManager<SoulWeaponNoticeInfo> _observerManager;

	public IUserObserverManager ObserverManager => _observerManager;

	public SoulWeaponNoticeManager()
	{
		_observerManager = new UserObserverManager<SoulWeaponNoticeInfo>();
		SetGodking();
		UpgradeChecker = new UpgradeChecker();
		RestoreChecker = new RestoreChecker();
		UpgradeChecker.Set(this);
		RestoreChecker.Set(this);
	}

	private void SetGodking()
	{
		_godKingChapterIndex = 65;
		_godKingDungeonIndex = 11;
		_godKingChapterIndex = GameRoot.Instance.ConstantDataContainer.GetValueInt("GodKingTempleReqChapter", _godKingChapterIndex);
		_godKingDungeonIndex = GameRoot.Instance.ConstantDataContainer.GetValueInt("GodKingTempleReqDungeon", _godKingDungeonIndex);
		_godKingComplete = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.IsCompleted(_godKingChapterIndex, _godKingDungeonIndex);
	}

	private bool IsCompleteGodKing(int chapterIndex, int dungeonIndex)
	{
		if (chapterIndex == _godKingChapterIndex)
		{
			return dungeonIndex == _godKingDungeonIndex;
		}
		return false;
	}

	public void Close()
	{
		UpgradeChecker.Close();
		UpgradeChecker = null;
		RestoreChecker.Close();
		RestoreChecker = null;
	}

	public void AddObserver()
	{
		if (AccountManager.IsLogin())
		{
			GlobalAutoBehaviour<AccountManager>.instance.AddUserObserver(this);
		}
		if (UpgradeChecker != null)
		{
			UpgradeChecker.AddObserver();
		}
		if (RestoreChecker != null)
		{
			RestoreChecker.AddObserver();
		}
	}

	public bool IsOpenedContents()
	{
		if (SoulWeaponManagement.IsOpenSoulweapon())
		{
			return _godKingComplete;
		}
		return false;
	}

	public bool AnyNoticeStateOn()
	{
		if (!IsOpenedContents())
		{
			return false;
		}
		if (RestoreChecker != null && RestoreChecker.AnyStateOn())
		{
			return true;
		}
		if (UpgradeChecker != null && UpgradeChecker.AnyStateOn())
		{
			return true;
		}
		return false;
	}

	public bool GetRetoreNoticeState()
	{
		if (!IsOpenedContents())
		{
			return false;
		}
		if (RestoreChecker == null)
		{
			return false;
		}
		return RestoreChecker.AnyStateOn();
	}

	public bool GetLiberationNoticeState()
	{
		if (!IsOpenedContents())
		{
			return false;
		}
		if (UpgradeChecker == null)
		{
			return false;
		}
		return UpgradeChecker.GetNoticeTypeState(ESoulWeaponNoticeType.Liberation);
	}

	public bool GetUpgradeNoticeState()
	{
		if (!IsOpenedContents())
		{
			return false;
		}
		if (UpgradeChecker == null)
		{
			return false;
		}
		return UpgradeChecker.GetNoticeTypeState(ESoulWeaponNoticeType.Upgrade);
	}

	public bool GetSoulWeaponUpgradeNoticeState(int slotIndex)
	{
		if (!IsOpenedContents())
		{
			return false;
		}
		if (UpgradeChecker == null)
		{
			return false;
		}
		return UpgradeChecker.GetNoticeTargetState(slotIndex);
	}

	void ICheckerListener.AnnounceToObservers(ICheckerNoticeInfo noticeInfo)
	{
		if (IsOpenedContents() && noticeInfo != null && _observerManager != null && noticeInfo is SoulWeaponNoticeInfo changedInfo)
		{
			_observerManager.AnnounceToObservers(changedInfo);
		}
	}

	bool IUserObserver.IsGlobal()
	{
		return true;
	}

	void IUserObserver<ChapterDungeonInfo>.OnChanged(ChapterDungeonInfo changedInfo)
	{
		if (IsCompleteGodKing(changedInfo.ChapterIndex, changedInfo.DungeonIndex))
		{
			_godKingComplete = true;
			_observerManager.AnnounceToObservers(new SoulWeaponNoticeInfo
			{
				IsOn = _godKingComplete,
				NoticeType = ESoulWeaponNoticeType.GodKingComplete
			});
		}
	}
}
