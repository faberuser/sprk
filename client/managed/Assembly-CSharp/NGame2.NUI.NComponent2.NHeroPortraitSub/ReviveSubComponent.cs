using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2.NHeroPortraitSub;

public class ReviveSubComponent : BaseSubComponent<HeroPortraitComponent>
{
	public static string PrefabPath = "Prefab2/Component/HeroPortraitSub/ReviveSubComponent";

	public static PrefabAuto<ReviveSubComponent> PrefabAuto = new PrefabAuto<ReviveSubComponent>(PrefabPath);

	public UILabel LabelRemainTime;

	public UILabel LabelReqGemCount;

	public override void Open(HeroPortraitComponent owner)
	{
		base.Open(owner);
		SetRootAnchor(base.Owner.GetRootGO(HeroPortraitComponent.ESubComponent.Revive));
	}

	public void Show(bool show)
	{
		UIHelper.EnableGameObject(Root, show);
	}

	public void SetReqGemLabel(int reqGemCount)
	{
		UIHelper.SetLabelText(LabelReqGemCount, Vespa_Int_Extension_Method.ToCommaString(reqGemCount));
	}

	public void SetRemainTime(string text)
	{
		UIHelper.SetLabelText(LabelRemainTime, text);
	}
}
