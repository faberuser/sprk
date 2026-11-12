using System.Collections.Generic;
using NGame2.NAccount;
using NNguiExtension;
using NShared;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NWorldMap.NMapLayout;

public class DecoratorBattle : LinkFactoryComponent
{
	public enum ELabelBgType
	{
		Visitable,
		CompleteEasy,
		CompleteNormal,
		CompleteHard,
		CompleteHell
	}

	public const string PrefabPath = "Prefab2/WorldMap/Node/Decorator/DecoratorBattle";

	public static PrefabAuto<DecoratorBattle> PrefabAuto = new PrefabAuto<DecoratorBattle>("Prefab2/WorldMap/Node/Decorator/DecoratorBattle");

	public UISpriteSelector LabelBgSelector;

	public UILabel NameLabel;

	public UIWidget WidgetRoot;

	public UIWidget ScenarioWidget;

	public List<UISpriteSelector> StarSelectorList = new List<UISpriteSelector>();

	private DungeonDifficultyType _difficulty;

	private int _star;

	public void Open(int chapterIndex, int dungeonIndex, bool canVisit)
	{
		GameRoot.Instance.CampaignDungeonDataContainer.GetData(chapterIndex, dungeonIndex);
		GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.GetCompleteStar(chapterIndex, dungeonIndex, out _difficulty, out _star);
		ELabelBgType value = ELabelBgType.Visitable;
		if (_difficulty == DungeonDifficultyType.Easy)
		{
			value = ((_star >= 3) ? ELabelBgType.CompleteEasy : ELabelBgType.Visitable);
		}
		else if (_difficulty == DungeonDifficultyType.Normal)
		{
			value = ((_star < 3) ? ELabelBgType.CompleteEasy : ELabelBgType.CompleteNormal);
		}
		else if (_difficulty == DungeonDifficultyType.Hard)
		{
			value = ((_star < 3) ? ELabelBgType.CompleteNormal : ELabelBgType.CompleteHard);
		}
		else if (_difficulty == DungeonDifficultyType.Hell)
		{
			value = ((_star < 3) ? ELabelBgType.CompleteHard : ELabelBgType.CompleteHell);
		}
		CampaignNodeData data = GameRoot.Instance.CampaignNodeDataContainer.GetData(chapterIndex, dungeonIndex);
		if (data != null)
		{
			if (__Extension_Method_ICollection__.COUNT(data.Code) == 0)
			{
				UIHelper.SetLabelText(NameLabel, $"{chapterIndex}-{dungeonIndex}");
			}
			else
			{
				UIHelper.SetLabelText(NameLabel, LocalizationManager.GetStringDirect(data.Code[0]));
			}
		}
		LabelBgSelector.SelectEnum(value);
		for (int i = 0; i < StarSelectorList.Count; i++)
		{
			UIHelper.EnableGameObject(StarSelectorList[i], canVisit);
		}
		if (!canVisit)
		{
			UIHelper.EnableGameObject(ScenarioWidget, enable: false);
			return;
		}
		bool flag = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.CanPlayScenario(chapterIndex, dungeonIndex);
		bool flag2 = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.IsCompleteScenario(chapterIndex, dungeonIndex);
		UIHelper.EnableGameObject(ScenarioWidget, flag && !flag2);
		for (int j = 0; j < StarSelectorList.Count; j++)
		{
			UISpriteSelector uISpriteSelector = StarSelectorList[j];
			if (!(uISpriteSelector == null))
			{
				if (j < _star)
				{
					uISpriteSelector.SelectEnum(_difficulty);
				}
				else
				{
					uISpriteSelector.Select("None");
				}
			}
		}
	}
}
