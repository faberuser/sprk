using System;
using NGame2.NUI.NWindow;
using NNguiExtension;
using NShared;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class MailListComponent : UIScrollItemBehaviour
{
	public interface Listener
	{
		void OnClick(MailListComponent caller);
	}

	public const string PrefabPath = "Prefab2/Component/Mail/MailListComponent";

	public static PrefabAuto<MailListComponent> PrefabAuto = new PrefabAuto<MailListComponent>("Prefab2/Component/Mail/MailListComponent");

	public UITexture PortraitTexture;

	public UILabel SenderNameLabel;

	public UILabel TitleLabel;

	public UILabel DueLabel;

	public UILabel DotLabel;

	public GameObject NewObject;

	public UISpriteSelector MailTypeSelector;

	private Listener _listener;

	private static LocalAccountBoolArray _readMailIndices = new LocalAccountBoolArray(i_default: false, "MailList_Read_Mail_Indices");

	public NShared.MailInfo MailInfo { get; private set; }

	public bool isNormal()
	{
		if (__Extension_Method_ICollection__.COUNT(MailInfo.ItemInfos) > 0)
		{
			return false;
		}
		if (__Extension_Method_ICollection__.COUNT(MailInfo.EquipItemInfos) > 0)
		{
			return false;
		}
		if (__Extension_Method_ICollection__.COUNT(MailInfo.CurrencyInfos) > 0)
		{
			return false;
		}
		if (__Extension_Method_ICollection__.COUNT(MailInfo.StaminaInfos) > 0)
		{
			return false;
		}
		if (MailInfo.SendGold > 0)
		{
			return false;
		}
		if (MailInfo.SendGem > 0)
		{
			return false;
		}
		if (MailInfo.PvpCoin > 0)
		{
			return false;
		}
		if (MailInfo.Mileage > 0)
		{
			return false;
		}
		if (MailInfo.LuaPoint > 0)
		{
			return false;
		}
		if (MailInfo.GuildPoint > 0)
		{
			return false;
		}
		if (MailInfo.WorldBossPoint > 0)
		{
			return false;
		}
		if (MailInfo.EventDungeonPoint > 0)
		{
			return false;
		}
		if (MailInfo.EventDungeonPoint2 > 0)
		{
			return false;
		}
		if (MailInfo.GloryPoint > 0)
		{
			return false;
		}
		if (MailInfo.SendChicken > 0)
		{
			return false;
		}
		if (MailInfo.SendSword > 0)
		{
			return false;
		}
		if (MailInfo.SendSword2 > 0)
		{
			return false;
		}
		if (MailInfo.SendUndergroundPrisonKey > 0)
		{
			return false;
		}
		if (MailInfo.SendUndergroundLabyrinthKey > 0)
		{
			return false;
		}
		if (MailInfo.SendHideoutKey > 0)
		{
			return false;
		}
		if (MailInfo.SendChallengeTowerKey > 0)
		{
			return false;
		}
		if (MailInfo.SendMazeKey > 0)
		{
			return false;
		}
		return true;
	}

	public bool Open(NShared.MailInfo mailInfo, bool isNew, Listener listener)
	{
		MailInfo = mailInfo;
		_listener = listener;
		UIHelper.SetLabelText(SenderNameLabel, LocalizationManager.LocalizeCode(mailInfo.SenderName));
		string text = LocalizationManager.LocalizeCode(mailInfo.Title);
		UIHelper.SetLabelText(TitleLabel, text);
		DotLabel.gameObject.SetActive(TitleLabel.CalculateOffsetToFit(text) > 0);
		if (!string.IsNullOrEmpty(mailInfo.ExpireTime))
		{
			UIHelper.EnableGameObject(DueLabel, enable: true);
			UIHelper.SetLabelText(DueLabel, GetRemainTimeText(mailInfo.ExpireTime));
		}
		else
		{
			UIHelper.EnableGameObject(DueLabel, enable: false);
		}
		UIHelper.EnableGameObject(NewObject, isNew);
		if (isNormal())
		{
			MailTypeSelector.SelectEnum(MailType.Normal);
		}
		else
		{
			MailTypeSelector.SelectEnum(MailType.Attached);
		}
		return true;
	}

	public void UpdateReadState()
	{
		if (MailInfo != null)
		{
			bool enable = IsNewMail(MailInfo.MailIndex);
			UIHelper.EnableGameObject(NewObject, enable);
		}
	}

	public static bool IsNewMail(long mailIndex)
	{
		return !_readMailIndices[mailIndex];
	}

	public static void MakeRead(long mailIndex)
	{
		_readMailIndices[mailIndex] = true;
	}

	private string GetRemainTimeText(string expireTimeText)
	{
		if (!Vespa_String_Extension_Method2.TryParseNetworkTime2(expireTimeText, out var o_dateTime))
		{
			return string.Empty;
		}
		TimeSpan timeSpan = o_dateTime - NGame2.NUI.NWindow.Common.GetUTCNow();
		if ((int)timeSpan.TotalDays >= 1)
		{
			return LocalizationManager.GetString(Mail.MAIL_DELETE_DAYS_AFTER, (int)timeSpan.TotalDays);
		}
		if ((int)timeSpan.TotalHours >= 1)
		{
			return LocalizationManager.GetString(Mail.MAIL_DELETE_HOURS_AFTER, (int)timeSpan.TotalHours);
		}
		return LocalizationManager.GetString(Mail.MAIL_DELETE_MINUTES_AFTER, (int)timeSpan.TotalMinutes);
	}

	public void OnClickThis()
	{
		if (_listener != null)
		{
			_listener.OnClick(this);
		}
	}
}
