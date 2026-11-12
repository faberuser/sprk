using System;
using NGame2.NDev;
using NGame2.NTutorial;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow;
using NShared;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NManager;

public class AchievementClearedPopupQueue : SceneUpdateBehaviour<AchievementClearedPopupQueue>
{
	private class AchievementPopupQueue : PopupQueue
	{
		protected override void AddPopup(IQueuedPopup popup)
		{
			if (popup != null && (!SingletonUpdater<TutorialManager>.isValidInstance || (!SingletonUpdater<TutorialManager>.instance.IsProgressing && !SingletonUpdater<TutorialManager>.instance.HasNextTurorial)))
			{
				_queuedPopupList.Add(popup);
			}
		}

		protected override bool CanProcessOpen()
		{
			if (!base.CanProcessOpen())
			{
				return false;
			}
			if (DevParam.GetDevParamBool("LegacyAchievementPopupCheck"))
			{
				if (SceneAutoBehaviour<MyHeroManagement>.isValidInstance)
				{
					HeroEquipUpgradeResultPopup instance = SceneAutoBehaviour<MyHeroManagement>.instance.GetInstance<HeroEquipUpgradeResultPopup>();
					if (instance != null && instance.IsOpen())
					{
						return false;
					}
				}
				if (SceneAutoBehaviour<MailManagement>.isValidInstance)
				{
					BaseWindow rewardPopup = SceneAutoBehaviour<MailManagement>.instance.rewardPopup;
					if (rewardPopup != null && rewardPopup.IsOpen())
					{
						return false;
					}
				}
				if (SceneAutoBehaviour<LobbyManagement>.isValidInstance)
				{
					GetNewHero2 instance2 = SceneAutoBehaviour<LobbyManagement>.instance.GetInstance<GetNewHero2>();
					if (instance2 != null && instance2.IsOpen())
					{
						SceneAutoBehaviour<AchievementClearedPopupQueue>.instance.BlockNotiPopup = false;
						return false;
					}
				}
				if (SceneAutoBehaviour<LobbyPopupQueue>.isValidInstance && !SceneAutoBehaviour<LobbyPopupQueue>.instance.Empty())
				{
					return false;
				}
				if (SceneAutoBehaviour<AchievementClearedPopupQueue>.instance.BlockNotiPopup)
				{
					return false;
				}
			}
			else
			{
				if (SceneAutoBehaviour<LobbyPopupQueue>.isValidInstance && !SceneAutoBehaviour<LobbyPopupQueue>.instance.Empty())
				{
					return false;
				}
				if (SceneAutoBehaviour<AchievementClearedPopupQueue>.instance.BlockNotiPopup)
				{
					return false;
				}
				if (!CheckFlag(PopupFlag.HideOnTopmostMenuOpen))
				{
					return false;
				}
			}
			return true;
		}
	}

	private const float blockDuration = 10f;

	private bool blockNotiPopup;

	private float blockStartTime;

	private PopupQueue _popupQueue = new AchievementPopupQueue();

	private bool _opened;

	public bool BlockNotiPopup
	{
		get
		{
			if (blockNotiPopup && Time.time - blockStartTime > 10f)
			{
				blockNotiPopup = false;
			}
			return blockNotiPopup;
		}
		set
		{
			blockNotiPopup = value;
			if (blockNotiPopup)
			{
				blockStartTime = Time.time;
			}
		}
	}

	public static bool Empty()
	{
		if (!SceneAutoBehaviour<AchievementClearedPopupQueue>.isValidInstance)
		{
			return true;
		}
		return SceneAutoBehaviour<AchievementClearedPopupQueue>.instance._popupQueue.Empty();
	}

	public static bool IsProgressing()
	{
		if (!SceneAutoBehaviour<AchievementClearedPopupQueue>.isValidInstance)
		{
			return false;
		}
		return SceneAutoBehaviour<AchievementClearedPopupQueue>.instance._popupQueue.IsProgressing();
	}

	public void Open()
	{
		_opened = true;
	}

	public void AddPopup<T>(T popupWindow, Action<T> openAction, PopupQueue.PopupFlag flag = PopupQueue.PopupFlag.None, string groupName = "") where T : BaseWindow
	{
		_popupQueue.AddPopup(popupWindow, openAction, flag, groupName);
	}

	protected override void ChainUpdate()
	{
		UpdatePopup();
	}

	public void UpdatePopup()
	{
		if (_opened)
		{
			_popupQueue.UpdatePopup();
		}
	}

	public void ReserveAchievementClearedPopUp(AchievementData achievementData, bool isReward)
	{
	}

	public void RemoveAchievementClearedPopup()
	{
		_popupQueue.RemovePopup<AchievementClearedPopup>();
	}

	public void RemoveAllClearedPopup(string groupName)
	{
		_popupQueue.RemoveAll(groupName);
	}
}
