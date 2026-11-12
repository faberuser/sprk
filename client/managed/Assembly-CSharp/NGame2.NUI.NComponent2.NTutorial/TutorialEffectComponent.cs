using NVespa;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2.NTutorial;

public class TutorialEffectComponent : LinkFactoryComponent, InitOnCreate
{
	public const string PrefabPath = "Prefab2/Component/Tutorial/TutorialEffectComponent";

	public static PrefabAuto<TutorialEffectComponent> PrefabAuto = new PrefabAuto<TutorialEffectComponent>("Prefab2/Component/Tutorial/TutorialEffectComponent");

	public UIGame3DWidget EffectWidget;

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		if (EffectWidget != null)
		{
			EffectWidget.Stop(immediatelyClear: true);
		}
	}

	public void SetAssetEffect(string code, TransformData extraTransform)
	{
		if (EffectWidget != null)
		{
			EffectWidget.PlayInstantEffect(code, extraTransform, null);
		}
	}
}
