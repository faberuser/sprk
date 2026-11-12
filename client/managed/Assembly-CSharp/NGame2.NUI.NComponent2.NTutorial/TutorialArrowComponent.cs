using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NTutorial;

public class TutorialArrowComponent : LinkFactoryComponent
{
	public const string PrefabPath = "Prefab2/Component/Tutorial/TutorialArrowComponent";

	public static PrefabAuto<TutorialArrowComponent> PrefabAuto = new PrefabAuto<TutorialArrowComponent>("Prefab2/Component/Tutorial/TutorialArrowComponent");

	public UIWidget TooltipArrowWidget;

	public void SetTooltipArrow(Vector3 fromPosition, Vector3 toPosition)
	{
		UIHelper.EnableGameObject(TooltipArrowWidget, enable: true);
		int h = (int)Vector2.Distance(fromPosition, toPosition);
		Vector3 vector = toPosition - fromPosition;
		float magnitude = vector.magnitude;
		Vector3 upwards = vector / magnitude;
		TooltipArrowWidget.transform.localRotation = Quaternion.LookRotation(Vector3.back, upwards);
		TooltipArrowWidget.transform.localPosition = (fromPosition + toPosition) / 2f;
		TooltipArrowWidget.SetDimensions((int)TooltipArrowWidget.localSize.x, h);
	}
}
