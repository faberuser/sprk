using System;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class TreasureHouseProbabilityComponent : LinkFactoryComponent
{
	public const string PrefabPath = "Prefab2/Component/TreasureHouse/TreasureHouseProbabilityComponent";

	public static PrefabAuto<TreasureHouseProbabilityComponent> PrefabAuto = new PrefabAuto<TreasureHouseProbabilityComponent>("Prefab2/Component/TreasureHouse/TreasureHouseProbabilityComponent");

	public UILabel LabelName;

	public UILabel LabelTotalProbability;

	public UILabel LabelPartialProbablilty;

	public void Open(string name, int totalRatio, int total, int partialRatio, int partial)
	{
		UIHelper.SetLabelText(LabelName, name);
		double num = Math.Truncate((float)totalRatio / (float)total * 10000000f) * 1.0000000116860974E-07;
		string text = $"{num * 100.0:N5}%";
		double num2 = Math.Truncate((float)partialRatio / (float)partial * 10000000f) * 1.0000000116860974E-07;
		string text2 = $"{num2 * 100.0:N5}%";
		UIHelper.SetLabelText(LabelTotalProbability, text);
		UIHelper.SetLabelText(LabelPartialProbablilty, text2);
	}
}
