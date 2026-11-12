using NGameAsset;
using NNguiExtension;
using NShared;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class CreatureWidgetComponent : LinkFactoryComponent, InitOnCreate
{
	public const string PrefabPath = "Prefab2/Component/TreasureHouse/CreatureWidgetComponent";

	public static PrefabAuto<CreatureWidgetComponent> PrefabAuto = new PrefabAuto<CreatureWidgetComponent>("Prefab2/Component/TreasureHouse/CreatureWidgetComponent");

	public UICreatureWidget UICreatureWidget;

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		if (UICreatureWidget != null)
		{
			UICreatureWidget.Close();
		}
	}

	public void Open(int creatureIndex, int[] localPos, int[] localRotation, int localScale)
	{
		if (!(UICreatureWidget == null) && creatureIndex != 0)
		{
			CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(creatureIndex);
			UICreatureWidget.Open(data, 3, 0, CreatureStock.ELoadLevel.Minimum, 0f);
			UICreatureWidget.SetLocalPosition(localPos);
			UICreatureWidget.SetLocalRotation(localRotation);
			UICreatureWidget.SetLocalScale(localScale);
			SetAlpha(0f);
		}
	}

	public void SetAlpha(float alpha)
	{
		if (UICreatureWidget != null)
		{
			UICreatureWidget.SetAlpha(alpha);
		}
	}

	public void SetDepth(int depth)
	{
		UIDepthHelper.RearrangeWidgetDepth(UICreatureWidget.gameObject, depth);
	}

	public void SetCustomIdle(DefineCreature.EMotion motionType, string name)
	{
		UICreatureWidget.SetCustomIdle(motionType, name);
	}
}
