using NShared;
using NVespa.NGlobal;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class SubEntryMenuComponent : EntryMenuButtonComponent
{
	public new static string PrefabPath = "Prefab2/Component/EntryMenu/SubEntryMenuComponent";

	public new static PrefabAuto<SubEntryMenuComponent> PrefabAuto = new PrefabAuto<SubEntryMenuComponent>(PrefabPath);

	public UIBundleLoader BundleLoader;

	public UILabel DescLabel;

	protected override void Init()
	{
		base.Init();
		string valueString = ArgParser.GetValueString(Menu.EntryMenuButton, 2);
		string valueString2 = ArgParser.GetValueString(Menu.EntryMenuButton, 3);
		UIHelper.SetBundleTexture(BundleLoader, valueString);
		UIHelper.SetLabelText(DescLabel, LocalizationManager.GetStringDirect(valueString2));
	}
}
