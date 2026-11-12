using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NContents;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.ClassBuff;
using NShared;
using NShared.InitClassBuff;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NUI.NManager.ClassBuff;

public class ClassBuffManagement : BaseManagement<ClassBuffManagement>
{
	private ClassBuffWindowManager _windowManager = new ClassBuffWindowManager();

	private ClassBuffWebManager _webManager = new ClassBuffWebManager();

	private ClassBuffWrapper _wrapper = new ClassBuffWrapper();

	public ClassBuffWindowManager ClassBuffWindowManager => _windowManager;

	public ClassBuffWebManager ClassBuffWebManager => _webManager;

	public ClassBuffWrapper ClassBuffWrapper => _wrapper;

	protected override void Awake()
	{
		base.Awake();
		InitPrefab();
		InitManager();
	}

	private void InitPrefab()
	{
		AddInstance<ClassBuffMain>("Resources/Prefab2/Window/ClassBuff/ClassBuffMain");
		AddInstance<ClassBuffLevelUpPopup>("Prefab2/Window/ClassBuff/ClassBuffLevelUpPopup");
	}

	private void InitManager()
	{
		_windowManager.Init(this);
		_webManager.Init(this);
		_wrapper.Init(this);
	}

	public void OpenClassBuffMain(CreatureTagType creatureType)
	{
		ClassBuffWindowManager.OpenClassBuffMain(creatureType);
	}

	public void AddClassBuffPoint(ItemData itemData)
	{
		if (ClassBuffWrapper.CheckPossibleGetClassBuffPointByItemData(itemData))
		{
			AddClassBuffPointByItemData(itemData);
		}
	}

	public void AddClassBuffPoint(HeroExpResultInfo info)
	{
		if (info != null && info.OldLevel < info.NewLevel)
		{
			AddClassBuffPoint();
		}
	}

	private void AddClassBuffPointByItemData(ItemData itemData)
	{
		ClassBuffWebManager.RequestGetClassBuffInfo(delegate
		{
			ResponseCompleteGetClassBuffInfos(ClassBuffWrapper.GetCreatureTypeListByItemData(itemData));
		});
	}

	public void AddClassBuffPoint()
	{
		ClassBuffWebManager.RequestGetClassBuffInfo(delegate
		{
			List<CreatureTagType> creatureTypeListByGetClassBuffPoint = ClassBuffWrapper.GetCreatureTypeListByGetClassBuffPoint();
			ResponseCompleteGetClassBuffInfos(creatureTypeListByGetClassBuffPoint);
		});
	}

	private void ResponseCompleteGetClassBuffInfos(List<CreatureTagType> list)
	{
		bool flag = SingletonAuto<MenuOpenCheckManager>.instance.IsOpen(ERightMenuButton.ClassBuff);
		if (!ClassBuffWrapper.IsFirstOpenStorage)
		{
			if (flag)
			{
				OpenContentsPopupClassBuff();
			}
		}
		else if (flag)
		{
			OpenCenterPopupGetClassBuffPoint(list);
			ClassBuffWrapper.SetClassBuffPointDIc(list);
		}
	}

	private void OpenContentsPopupClassBuff()
	{
		if (SceneAutoBehaviour<LobbyManagement>.isValidInstance)
		{
			SceneAutoBehaviour<LobbyManagement>.instance.OpenClassBuffPopup();
		}
	}

	private void OpenCenterPopupGetClassBuffPoint(List<CreatureTagType> list)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(list))
		{
			return;
		}
		NGame2.NAccount.UserClassBuffManager classBuffManager = GlobalAutoBehaviour<AccountManager>.instance.ClassBuffManager;
		if (classBuffManager == null)
		{
			return;
		}
		MultiCenterMessages multiCenterMessages = MultiCenterMessages.Open();
		if (multiCenterMessages == null)
		{
			return;
		}
		multiCenterMessages.Reset();
		long num = 0L;
		string text = "";
		int num2 = -1;
		List<CreatureTagType> sortCreatureTypeList = FilterHelper.GetListOrderCreatureTagType();
		int index = 0;
		while (index < sortCreatureTypeList.Count)
		{
			num2 = list.FindIndex((CreatureTagType creatureTagType) => creatureTagType == sortCreatureTypeList[index]);
			if (num2 >= 0)
			{
				CreatureTagType type = list[num2];
				num = classBuffManager.GetClassBuffPointByCreatureType(type);
				if (num > 0)
				{
					text = LocalizationManager.GetStringDirect(ClassBuffWrapper.LocalizeClassBuffPointCenterMessage, ClassBuffWrapper.LocalizeClassBuffPointName(list[num2]), num);
					multiCenterMessages.AddClassBuffIconMessage(text, list[num2].ToString());
				}
			}
			int num3 = index + 1;
			index = num3;
		}
		multiCenterMessages.RepositionMessage();
	}

	private void OpenPopupNotEnoughtCurrency()
	{
		string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
		string message = LocalizationManager.GetString(NShared.NLocalization.System.MOVE_GEM_SHOP_CONFIRM);
		MessagePopup.Open(title, message, delegate
		{
			SceneAutoBehaviour<PayShopManagement>.instance.OpenNewPayShop(NewPayShopCategoryType.Ruby.ToString());
		}, null);
	}

	public void LevelUpClassBuff(ClassBuffData data)
	{
		ClassBuffWebManager.RequestLevelUpClassBuff(data, delegate
		{
			ResponseCompleteLevelUpClassBuff(data.ClassBuffIndex);
		});
	}

	private void ResponseCompleteLevelUpClassBuff(int levelUpIndex)
	{
		ClassBuffWindowManager.ResetClassBuffMain();
		ClassBuffWindowManager.PlayLevelUpEffect(levelUpIndex);
	}

	public void ResetClassBuff(CreatureTagType typeCategory)
	{
		if (ClassBuffWrapper == null)
		{
			return;
		}
		int needGemResetClassBuff = ClassBuffWrapper.GetNeedGemResetClassBuff();
		long gem = GlobalAutoBehaviour<AccountManager>.instance.UserManager.Gem;
		int num = needGemResetClassBuff;
		if (gem < num)
		{
			OpenPopupNotEnoughtCurrency();
			return;
		}
		string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
		string stringDirect = LocalizationManager.GetStringDirect(ClassBuffWrapper.LocalizeClassBuffResetPopupDesc, ClassBuffWrapper.LocalizeClassBuffName(typeCategory));
		string stringDirect2 = LocalizationManager.GetStringDirect(ClassBuffWrapper.LocalizeClassBuffResetBtn);
		PricedMessagePopup.Open(title, stringDirect, stringDirect2, PricedMessagePopup.EPriceType.Gem, needGemResetClassBuff, delegate
		{
			ClassBuffWebManager.RequestResetClassBuff(typeCategory, ResponseCompleteResetClassBuff, ResponseError);
		});
	}

	private void ResponseCompleteResetClassBuff()
	{
		ClassBuffWindowManager.ResetClassBuffMain();
	}

	private void ResponseError(ResultType resultType)
	{
		if (resultType == ResultType.NotEnoughCurrency)
		{
			OpenPopupNotEnoughtCurrency();
		}
		else
		{
			MessagePopup.OpenError(resultType);
		}
	}
}
