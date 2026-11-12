using System;
using NGame2.NAccount;
using NGame2.NUI;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI3.NMaxCap;

public class CurrencyMaxCapComponent : LinkFactoryComponent, NVespa.NPrefab2.InitOnCreate
{
	[Serializable]
	public class CurrencyIconUi
	{
		public UIWidget IconRoot;

		private IconProxy _iconProxy;

		public void InitUi(CurrencyType currencyType)
		{
			ResetUi();
			_iconProxy = IconProxy.Create(IconRoot);
			_iconProxy.SetupCurrencyNoBg(currencyType);
		}

		public void ResetUi()
		{
			FactoryBase.SafeReleaseComponent(ref _iconProxy);
		}
	}

	[Serializable]
	public class CurrencyTextUi
	{
		public UITable Table_Info;

		public UILabel Label_Name;

		public UILabel Label_Count;

		public UILabel Label_MaxCap;

		public void InitUi(CurrencyType currencyType)
		{
			string stringDirect = LocalizationManager.GetStringDirect($"TOOLTIP_INFO_NAME_{currencyType.ToString().ToUpper()}");
			long vALUE = Vespa_Generic_IDictionary_Extension_Method.GETVALUE(ContentsDefine.CurrencyMaxCap.MaxCap, currencyType, 0L);
			string text = Vespa_Long_Extension_Method.ToCommaString(GlobalAutoBehaviour<AccountManager>.instance.UserManager.GetUserStat(currencyType.ToString()));
			string text2 = Vespa_Long_Extension_Method.ToCommaString(vALUE);
			UIHelper.SetLabelText(Label_Name, stringDirect);
			UIHelper.SetLabelText(Label_Count, text);
			UIHelper.SetLabelText(Label_MaxCap, text2);
			if (Table_Info != null)
			{
				Table_Info.Reposition();
			}
		}

		public void ResetUi()
		{
		}
	}

	public const string PrefabPath = "Prefab3/MaxCap/CurrencyMaxCapComponent";

	public static PrefabAuto<CurrencyMaxCapComponent> PrefabAuto = new PrefabAuto<CurrencyMaxCapComponent>("Prefab3/MaxCap/CurrencyMaxCapComponent");

	[Header("---[ 아이콘 ]")]
	public CurrencyIconUi CurrencyIcon;

	[Header("---[ 갯수 표기 ]")]
	public CurrencyTextUi CurrencyText;

	[Header("---[ 툴팁 ]")]
	public CurrencyTooltipComponent CurrencyTooltipFunc;

	void NVespa.NPrefab2.InitOnCreate.OnCreate()
	{
	}

	void NVespa.NPrefab2.InitOnCreate.OnRelease()
	{
		CurrencyIcon.ResetUi();
	}

	public void Open(CurrencyType currencyType)
	{
		CurrencyIcon.InitUi(currencyType);
		CurrencyText.InitUi(currencyType);
		if (CurrencyTooltipFunc != null)
		{
			CurrencyTooltipFunc.CurrencyType = currencyType;
		}
	}

	public void OnShowTooltip()
	{
		if (CurrencyTooltipFunc != null)
		{
			CurrencyTooltipFunc.OnShowTooltip();
		}
	}

	public void OnHideTooltip()
	{
		if (CurrencyTooltipFunc != null)
		{
			CurrencyTooltipFunc.OnHideTooltip();
		}
	}
}
