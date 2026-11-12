using System;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow;

public class IconCenterMessage : CenterMessage
{
	public const string PrefabPathIcon = "Resources/Prefab2/Window/MessagePopup/IconCenterMessage";

	public UIWidget IconRoot;

	public IconProxy IconProxy;

	public new void Reset()
	{
		base.Reset();
		if (IconProxy != null)
		{
			FactoryBase.SafeReleaseComponent(ref IconProxy);
		}
		IconProxy = null;
	}

	public static void Open(string message, Enum value)
	{
		Open(message, value.ToString());
	}

	public static void Open(string message, string iconName)
	{
		if (!String_Extension_Method.IsNullOrEmpty(message))
		{
			CenterMessage centerMessage = GlobalAutoBehaviour<PopupManegement>.instance.PopUpExist<CenterMessage>();
			if (centerMessage != null)
			{
				centerMessage.Close2(immediately: true);
			}
			IconCenterMessage iconCenterMessage = GlobalAutoBehaviour<PopupManegement>.instance.PopUpExist<IconCenterMessage>();
			if ((bool)iconCenterMessage)
			{
				iconCenterMessage.CompleteAnimationWithoutCallback();
				iconCenterMessage.SetMessage(message, iconName);
				iconCenterMessage.PlayWindowOpenAnimation();
			}
			else
			{
				WindowPrefab<IconCenterMessage> windowPrefab = new WindowPrefab<IconCenterMessage>("Resources/Prefab2/Window/MessagePopup/IconCenterMessage", GameUIRoot.ELayerType.UI);
				windowPrefab.instance.OpenInternal(message, iconName);
				GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
			}
		}
	}

	protected void SetMessage(string message, string iconName)
	{
		SetMessage(message);
		SetupIconProxy(iconName);
	}

	protected void OpenInternal(string message, string iconName)
	{
		OpenInternal(message);
		SetupIconProxy(iconName);
	}

	private void SetupIconProxy(string currencyName)
	{
		if (IconProxy == null)
		{
			IconProxy = IconProxy.Create(IconRoot);
		}
		if (IconProxy != null)
		{
			IconProxy.SetupCurrencyNoBg(currencyName);
		}
	}
}
