using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NDropBonusEvent;
using NGame2.NUI.NWindow;
using NShared;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI;

public class CurrencyTooltipComponent : MonoBehaviour
{
	public enum PositionType
	{
		Icon,
		Top
	}

	private class TooltipTextInfoProxy
	{
		private CurrencyTooltipTextInfo _tooltipTextInfo;

		public CurrencyTooltipTextInfo GetInfo(CurrencyType currency_Type, PositionType positionType, string eventCurrencyType = null)
		{
			SetupTooltipText(currency_Type, positionType, eventCurrencyType);
			return _tooltipTextInfo;
		}

		private void SetupTooltipText(CurrencyType currency_Type, PositionType positionType, string eventCurrencyType)
		{
			if (_tooltipTextInfo == null)
			{
				_tooltipTextInfo = new CurrencyTooltipTextInfo();
			}
			_tooltipTextInfo.Reset();
			_tooltipTextInfo.Name.Value = currency_Type.ToString();
			SetupItemTooltipText(currency_Type, positionType, eventCurrencyType);
		}

		private void SetupItemTooltipText(CurrencyType currency_Type, PositionType positionType, string eventCurrencyType)
		{
			if (currency_Type == CurrencyType.None)
			{
				_tooltipTextInfo.TypeText.Reset();
				return;
			}
			_tooltipTextInfo.Name.Reset();
			_tooltipTextInfo.TypeText.Reset();
			_tooltipTextInfo.Desc.Reset();
			string key = $"TOOLTIP_INFO_NAME_{currency_Type.ToString().ToUpper()}";
			string key2 = $"TOOLTIP_INFO_TEXT_{currency_Type.ToString().ToUpper()}";
			if (currency_Type == CurrencyType.EventDungeonPoint || currency_Type == CurrencyType.EventDungeonPoint2)
			{
				List<DropBonusEvent> allEffectiveEvent = GlobalAutoBehaviour<DropBonusEventManager>.instance.GetAllEffectiveEvent(EventBonusType.EventDungeon);
				int result = -1;
				if (allEffectiveEvent != null)
				{
					for (int i = 0; i < allEffectiveEvent.Count; i++)
					{
						DropBonusEvent dropBonusEvent = allEffectiveEvent[i];
						if (!String_Extension_Method.IsNullOrEmpty(dropBonusEvent.Condition))
						{
							int.TryParse(dropBonusEvent.Condition, out result);
							break;
						}
					}
				}
				if (result != -1)
				{
					CampaignChapterData data = GameRoot.Instance.CampaignChapterDataContainer.GetData(result);
					if (data != null)
					{
						string resultInfoIconSprite = data.GetResultInfoIconSprite(currency_Type.ToString());
						if (!String_Extension_Method.IsNullOrEmpty(resultInfoIconSprite))
						{
							key = $"TOOLTIP_INFO_NAME_{resultInfoIconSprite.ToString().ToUpper()}";
							key2 = $"TOOLTIP_INFO_TEXT_{resultInfoIconSprite.ToString().ToUpper()}";
						}
					}
				}
			}
			string name = LocalizationManager.GetStringDirect(key);
			string desc = LocalizationManager.GetStringDirect(key2);
			if (positionType == PositionType.Top)
			{
				string myCurrencyValue = GetMyCurrencyValue(currency_Type);
				if (!String_Extension_Method.IsNullOrEmpty(myCurrencyValue))
				{
					desc = myCurrencyValue + "\n" + desc;
				}
			}
			ReassignTextInfo(currency_Type, ref name, ref desc);
			_tooltipTextInfo.CurrencyType = currency_Type;
			_tooltipTextInfo.Name.Value = name;
			_tooltipTextInfo.Desc1.Value = desc;
		}

		private void ReassignTextInfo(CurrencyType currencyType, ref string name, ref string desc)
		{
			switch (UIHelper.GetCurrencyTooltipType(currencyType))
			{
			case CurrencyTooltipType.NameOnly:
				desc = string.Empty;
				break;
			case CurrencyTooltipType.DescOnly:
				name = string.Empty;
				break;
			}
		}

		private string GetMyCurrencyValue(CurrencyType currencyType)
		{
			if (!GlobalAutoBehaviour<AccountManager>.isValidInstance || GlobalAutoBehaviour<AccountManager>.instance.UserManager == null)
			{
				return "";
			}
			string text = "";
			if (currencyType == CurrencyType.Gold)
			{
				text = Vespa_Long_Extension_Method.ToCommaString(GlobalAutoBehaviour<AccountManager>.instance.UserManager.Gold);
			}
			if (String_Extension_Method.IsNullOrEmpty(text))
			{
				return "";
			}
			return LocalizationManager.GetStringDirect("TOOLTIP_INFO_TEXT_TOTAL.AMOUNT", text);
		}
	}

	public CurrencyType CurrencyType;

	public float ScalePercent = 1f;

	public PositionType Position;

	private TooltipTextInfoProxy _tooltipTextProxy = new TooltipTextInfoProxy();

	public void OnShowTooltip()
	{
		CurrencyTooltipTextInfo info = _tooltipTextProxy.GetInfo(CurrencyType, Position);
		CurrencyIconTooltip.SetScale(ScalePercent);
		CurrencyIconTooltip.Open(GetComponent<UIWidget>(), info);
	}

	public void OnShowTooltip_CurrencyTypeComponent(CurrencyType currency_Type, PositionType positionType, string eventCurrency)
	{
		CurrencyTooltipTextInfo info = _tooltipTextProxy.GetInfo(currency_Type, positionType, eventCurrency);
		CurrencyIconTooltip.SetScale(ScalePercent);
		CurrencyIconTooltip.Open(GetComponent<UIWidget>(), info);
	}

	public void OnHideTooltip()
	{
		CurrencyIconTooltip.Hide(GetComponent<UIWidget>());
	}
}
