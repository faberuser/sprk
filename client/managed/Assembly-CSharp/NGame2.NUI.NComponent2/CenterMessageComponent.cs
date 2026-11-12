using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class CenterMessageComponent : LinkFactoryComponent
{
	public const string PrefabPath = "Resources/Prefab2/Component/Message/CenterMessageComponent";

	public static PrefabAuto<CenterMessageComponent> PrefabAuto = new PrefabAuto<CenterMessageComponent>("Resources/Prefab2/Component/Message/CenterMessageComponent");

	public UILabel Label_Message;

	public void Open(string message)
	{
		UIHelper.SetLabelText(Label_Message, message);
	}
}
