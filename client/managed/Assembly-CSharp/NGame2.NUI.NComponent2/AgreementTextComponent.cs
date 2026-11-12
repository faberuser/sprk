using NVespa.NNGUIExtension;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class AgreementTextComponent : UIScrollItemBehaviour, InitOnCreate
{
	public const string PrefabPath = "Prefab2/Component/Login/AgreementTextDoubleComponent";

	public const string PrefabSinglePath = "Prefab2/Component/Login/AgreementTextSingleComponent";

	public static PrefabAuto<AgreementTextComponent> PrefabAuto = new PrefabAuto<AgreementTextComponent>("Prefab2/Component/Login/AgreementTextDoubleComponent");

	public static PrefabAuto<AgreementTextComponent> PrefabSingleAuto = new PrefabAuto<AgreementTextComponent>("Prefab2/Component/Login/AgreementTextSingleComponent");

	public UIWidget WidgetComponent;

	public UILabel LabelAgreement;

	public void Open(string agreementText)
	{
		if (!String_Extension_Method.IsNullOrEmpty(agreementText))
		{
			UIHelper.SetLabelText(LabelAgreement, agreementText);
			WidgetComponent.height = LabelAgreement.height;
		}
	}

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		UIHelper.SetLabelText(LabelAgreement, string.Empty);
	}
}
