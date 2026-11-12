using NVespa.NNGUIExtension;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2.ClassBuff;

public class ClassBuffStatComponent : UIScrollItemBehaviour
{
	public const string PrefabPath = "Prefab2/Component/ClassBuff/ClassBuffStatComponent";

	public static PrefabAuto<ClassBuffStatComponent> PrefabAuto = new PrefabAuto<ClassBuffStatComponent>("Prefab2/Component/ClassBuff/ClassBuffStatComponent");

	public UILabel StatDescriptionLabel;

	public void Open(string descripton)
	{
		Init(descripton);
	}

	private void Init(string description)
	{
		SetStatDescription(description);
	}

	private void SetStatDescription(string description)
	{
		if (!(StatDescriptionLabel == null))
		{
			UIHelper.SetLabelText(StatDescriptionLabel, String_Extension_Method.IsNullOrEmpty(description) ? "-" : description);
		}
	}
}
