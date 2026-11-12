using System.Collections.Generic;
using NShared;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class KingsPassExpComponent : UIScrollItemBehaviour, InitOnCreate
{
	public new interface IListener
	{
		void OnClickExp();
	}

	public const string PrefabPath = "Prefab2/Component/KingsPass/KingsPassExpComponent";

	public static PrefabAuto<KingsPassExpComponent> PrefabAuto = new PrefabAuto<KingsPassExpComponent>("Prefab2/Component/KingsPass/KingsPassExpComponent");

	public UIWidget IconRoot;

	public UILabel AmountLabel;

	private IconProxy IconProxy;

	private IListener _listener;

	private List<KingsPassItemComponent> _compList = new List<KingsPassItemComponent>();

	private List<KingsPassItemComponent> _additionalCompList = new List<KingsPassItemComponent>();

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		ReleaseComponent();
	}

	public void Open(StaminaType type, int amount, IListener listener)
	{
		_listener = listener;
		if (IconProxy == null)
		{
			IconProxy = IconProxy.Create(IconRoot);
		}
		IconProxy.SetupStamina(type);
		UIHelper.EnableGameObject(AmountLabel, amount > 0);
		string stringDirect = LocalizationManager.GetStringDirect("KINGSPASS_GET_EXP_PER", amount);
		UIHelper.SetLabelText(AmountLabel, stringDirect);
	}

	private void ReleaseComponent()
	{
		if (IconProxy != null)
		{
			FactoryBase.SafeReleaseComponent(ref IconProxy);
		}
		IconProxy = null;
	}

	private void OnClickProduct(int originalIndex, int comboIndex)
	{
		if (_listener != null)
		{
			_listener.OnClickExp();
		}
	}
}
