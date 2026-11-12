using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2.NHeroPortraitSub;

public class DeckSlotSmallSubComponent : BaseSubComponent<HeroPortraitComponent>
{
	public static string PrefabPath = "Prefab2/Component/HeroPortraitSub/DeckSlotSmallSubComponent";

	public static PrefabAuto<DeckSlotSmallSubComponent> PrefabAuto = new PrefabAuto<DeckSlotSmallSubComponent>(PrefabPath);

	public UILabel Label_SlotIndex;

	public override void Open(HeroPortraitComponent owner)
	{
		base.Open(owner);
		SetRootAnchor(base.Owner.GetRootGO(HeroPortraitComponent.ESubComponent.DeckSlot));
	}

	public void SetSlotIndex(int slotIndex)
	{
		UIHelper.SetLabelText(Label_SlotIndex, slotIndex);
	}

	public void Close()
	{
	}
}
