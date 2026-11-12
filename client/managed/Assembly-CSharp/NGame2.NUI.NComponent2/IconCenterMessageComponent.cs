using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class IconCenterMessageComponent : LinkFactoryComponent, InitOnCreate
{
	public const string PrefabPath = "Resources/Prefab2/Component/Message/IconCenterMessageComponent";

	public static PrefabAuto<IconCenterMessageComponent> PrefabAuto = new PrefabAuto<IconCenterMessageComponent>("Resources/Prefab2/Component/Message/IconCenterMessageComponent");

	public UILabel Label_Message;

	public UIWidget IconRoot;

	public IconProxy IconProxy;

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		if (IconProxy != null)
		{
			FactoryBase.SafeReleaseComponent(ref IconProxy);
		}
		IconProxy = null;
	}

	public void Open(string message, string IconName, IconProxyType type = IconProxyType.Currency)
	{
		Init(message, IconName, type);
	}

	private void Init(string message, string iconName, IconProxyType type)
	{
		SetIcon(iconName, type);
		SetMessage(message);
	}

	private void SetIcon(string iconName, IconProxyType type)
	{
		if (IconProxy == null)
		{
			IconProxy = IconProxy.Create(IconRoot);
		}
		if (IconProxy != null)
		{
			if (type == IconProxyType.ClassBuff)
			{
				IconProxy.SetClassBuffIcon(iconName);
			}
			else
			{
				IconProxy.SetupCurrencyNoBg(iconName);
			}
		}
	}

	private void SetMessage(string message)
	{
		if (!(Label_Message == null))
		{
			UIHelper.SetLabelText(Label_Message, message);
		}
	}
}
