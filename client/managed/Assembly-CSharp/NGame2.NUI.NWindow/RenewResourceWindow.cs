using NGame2.NAccount;
using NGame2.NPet;
using NGame2.NUI.NComponent2.NPayShop;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow.NChatting;
using NShared;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow;

public class RenewResourceWindow : BaseWindow, PayShopResourceList.IListener, IUserObserver<UserManager.UserStatInfo>, IUserObserver, IUserObserver<NGame2.NAccount.UserPetManager.ChangedPetInfo>
{
	public PayShopResourceList PayShopResourceList;

	public UIButton MiniGuideButton;

	public UISprite Sprite_PetBuffAlert;

	public bool PreventTouchAnotherButton { get; set; }

	protected void Init()
	{
		PreventTouchAnotherButton = false;
		if (PayShopResourceList != null)
		{
			PayShopResourceList.Open(this);
		}
		if (MiniGuideButton != null)
		{
			SetMiniGuideButton();
		}
		GlobalAutoBehaviour<AccountManager>.instance.PetManager.ObserverManager.AddObserver(this);
		if (Sprite_PetBuffAlert != null)
		{
			CheckPetBuff();
		}
	}

	private void CheckPetBuff()
	{
		bool enable = GlobalAutoBehaviour<PetManager>.instance.IsChangedBuffType();
		UIHelper.EnableGameObject(Sprite_PetBuffAlert, enable);
	}

	private void SetMiniGuideButton()
	{
		UIHelper.EnableGameObject(MiniGuideButton, WindowGuideIndex != 0);
	}

	public override void ReOpen()
	{
		base.ReOpen();
		GlobalAutoBehaviour<AccountManager>.instance.PetManager.ObserverManager.AddObserver(this);
		CheckPetBuff();
	}

	public override void Close()
	{
		base.Close();
		if (PayShopResourceList != null)
		{
			PayShopResourceList.ClearAll();
		}
		GlobalAutoBehaviour<AccountManager>.instance.PetManager.ObserverManager.RemoveObserver(this);
	}

	public void OnclickMail()
	{
		if (!PreventTouchAnotherButton)
		{
			if (!GlobalAutoBehaviour<GlobalManagement>.instance.WindowOpened<MailListPopup>())
			{
				SceneAutoBehaviour<MailManagement>.instance.Open();
			}
			else
			{
				SceneAutoBehaviour<MailManagement>.instance.CloseAllActiveWindow();
			}
		}
	}

	public void OnClickChat()
	{
		if (PreventTouchAnotherButton)
		{
			return;
		}
		string backButtonTitle;
		ETopmostMenuType topmostMenuType = GlobalAutoBehaviour<GlobalManagement>.instance.GetTopmostMenuType(out backButtonTitle);
		if (topmostMenuType == ETopmostMenuType.Tutorial || topmostMenuType == ETopmostMenuType.Tutorial_Guide)
		{
			CenterMessage.Open(LocalizationManager.GetStringDirect("TUTORIAL_RAIDPARTYSETTING_3"));
			return;
		}
		SceneAutoBehaviour<MailManagement>.instance.CloseAllActiveWindow();
		SceneAutoBehaviour<PayShopManagement>.instance.CloseAllActiveWindow();
		if (!GlobalAutoBehaviour<GlobalManagement>.instance.WindowOpened<ChattingWindow>())
		{
			SceneAutoBehaviour<ChattingManagement>.instance.OpenChatting(ChatType.None, 0L);
		}
		else
		{
			SceneAutoBehaviour<ChattingManagement>.instance.CloseAllActiveWindow();
		}
	}

	public void OnClickPetBuff()
	{
		if (!PreventTouchAnotherButton)
		{
			if (GlobalAutoBehaviour<AccountManager>.instance.PetManager.NotHavePet)
			{
				CenterMessage.Open(LocalizationManager.GetStringDirect("PET_USE_FACILITY_CONDITION_TEXT"));
			}
			else
			{
				PetTotalBuffPopup.OpenPopup();
			}
		}
	}

	public void OnClickMiniGuideMenu()
	{
		if (!PreventTouchAnotherButton && SceneAutoBehaviour<LobbyManagement>.instance.GetInstance<LobbyTopmostMenu>().CurrentMiniGuideData != null)
		{
			int[] gameGuideIndex = SceneAutoBehaviour<LobbyManagement>.instance.GetInstance<LobbyTopmostMenu>().CurrentMiniGuideData.GameGuideIndex;
			SceneAutoBehaviour<LobbyManagement>.instance.GetInstance<MiniGuidePopup>().Open(gameGuideIndex);
		}
	}

	void PayShopResourceList.IListener.OnClickStamina()
	{
		SceneAutoBehaviour<PayShopManagement>.instance.OpenNewPayShop(NewPayShopCategoryType.Stamina.ToString());
	}

	void PayShopResourceList.IListener.OnClickRuby()
	{
		SceneAutoBehaviour<PayShopManagement>.instance.OpenNewPayShop(NewPayShopCategoryType.Ruby.ToString());
	}

	void PayShopResourceList.IListener.OnClickGold()
	{
		SceneAutoBehaviour<PayShopManagement>.instance.OpenNewPayShop(NewPayShopCategoryType.Gold.ToString());
	}

	void IUserObserver<UserManager.UserStatInfo>.OnChanged(UserManager.UserStatInfo changedInfo)
	{
		PayShopResourceList.OnChanged(changedInfo);
	}

	bool IUserObserver.IsGlobal()
	{
		return false;
	}

	void IUserObserver<NGame2.NAccount.UserPetManager.ChangedPetInfo>.OnChanged(NGame2.NAccount.UserPetManager.ChangedPetInfo changedInfo)
	{
		if (changedInfo.EChangedPetInfoType == NGame2.NAccount.UserPetManager.EChangedPetInfoType.AddedPet || changedInfo.EChangedPetInfoType == NGame2.NAccount.UserPetManager.EChangedPetInfoType.PetInfoChanged)
		{
			CheckPetBuff();
		}
	}
}
