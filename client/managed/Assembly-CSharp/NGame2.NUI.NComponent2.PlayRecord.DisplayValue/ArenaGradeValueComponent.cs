using NNguiExtension;
using NShared;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2.PlayRecord.DisplayValue;

public class ArenaGradeValueComponent : MonoBehaviour, IDisplayValueComponent
{
	public const string PrefabPath = "Prefab2/Component/PlayRecord/DisplayValue/ArenaGradeValueComponent";

	public static PrefabAuto<ArenaGradeValueComponent> PrefabAuto = new PrefabAuto<ArenaGradeValueComponent>("Prefab2/Component/PlayRecord/DisplayValue/ArenaGradeValueComponent");

	public Transform ChampionRoot;

	public Transform Root;

	public UISpriteSelector NameSelector;

	public GameObject[] BronzeLevel;

	public GameObject[] SilverLevel;

	public GameObject[] GoldLevel;

	public GameObject[] PlatinumLevel;

	public GameObject[] DiamondLevel;

	public GameObject[] MasterLevel;

	public GameObject[] RoyalMasterLevel;

	void IDisplayValueComponent.Open(PlayRecordData playRecordData, string recordValue)
	{
		UIHelper.EnableGameObject(Root, enable: false);
		UIHelper.EnableGameObject(ChampionRoot, enable: false);
		int num = Vespa_String_Extension_Method.TOINT(recordValue, 0);
		if (num == GameRoot.Instance.ConstantDataContainer.GetValueInt("ChampionTierIndex"))
		{
			UIHelper.EnableGameObject(ChampionRoot, enable: true);
			return;
		}
		MatchTierData data = GameRoot.Instance.MatchTierDataContainer.GetData(num);
		if (data != null)
		{
			UIHelper.EnableGameObject(Root, enable: true);
			MatchTierType tier = data.Tier;
			int level = data.Level;
			NameSelector.SelectEnum(tier);
			for (int i = 0; i < BronzeLevel.Length; i++)
			{
				UIHelper.EnableGameObject(BronzeLevel[i], tier == MatchTierType.Bronze && i == level);
			}
			for (int j = 0; j < SilverLevel.Length; j++)
			{
				UIHelper.EnableGameObject(SilverLevel[j], tier == MatchTierType.Silver && j == level);
			}
			for (int k = 0; k < GoldLevel.Length; k++)
			{
				UIHelper.EnableGameObject(GoldLevel[k], tier == MatchTierType.Gold && k == level);
			}
			for (int l = 0; l < PlatinumLevel.Length; l++)
			{
				UIHelper.EnableGameObject(PlatinumLevel[l], tier == MatchTierType.Platinum && l == level);
			}
			for (int m = 0; m < DiamondLevel.Length; m++)
			{
				UIHelper.EnableGameObject(DiamondLevel[m], tier == MatchTierType.Diamond && m == level);
			}
			for (int n = 0; n < MasterLevel.Length; n++)
			{
				UIHelper.EnableGameObject(MasterLevel[n], tier == MatchTierType.Master && n == level);
			}
			for (int num2 = 0; num2 < RoyalMasterLevel.Length; num2++)
			{
				UIHelper.EnableGameObject(RoyalMasterLevel[num2], tier == MatchTierType.RoyalMaster && num2 == level);
			}
		}
	}
}
