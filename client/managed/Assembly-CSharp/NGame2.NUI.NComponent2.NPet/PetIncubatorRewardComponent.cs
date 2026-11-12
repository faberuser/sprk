using System.Collections.Generic;
using NGame2.NUI.NHelper.NPet;
using NShared;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2.NPet;

public class PetIncubatorRewardComponent : LinkFactoryComponent, InitOnCreate
{
	public interface IIncubatorRewardDataProvider
	{
		List<int> GetRewardPetIndexList(CreatureGrade grade);
	}

	public static string PrefabPath = "Prefab2/Component/Pet/PetIncubatorRewardComponent";

	public static PrefabAuto<PetIncubatorRewardComponent> PrefabAuto = new PrefabAuto<PetIncubatorRewardComponent>(PrefabPath);

	private IIncubatorRewardDataProvider _dataProvider;

	public UIWidget Widget_Root;

	public UILabel Label_Grade;

	public UISimpleGrid Grid_PetPortrait;

	public int LabelHeight = 70;

	public int BottomHeight = 10;

	private int _selectedEggIndex;

	private CreatureGrade _grade;

	private List<PetPortraitComponent> _petPortraitCompList = new List<PetPortraitComponent>();

	public void Open(IIncubatorRewardDataProvider dataProvider, CreatureGrade grade)
	{
		_dataProvider = dataProvider;
		_grade = grade;
		Init();
	}

	private void Init()
	{
		FactoryBase.ReleaseAndClearList(_petPortraitCompList);
		if (_dataProvider == null)
		{
			return;
		}
		string creatureGradeColorLocalizationKey = PetHelper.GetCreatureGradeColorLocalizationKey(_grade);
		if (String_Extension_Method.IsNullOrEmpty(creatureGradeColorLocalizationKey))
		{
			return;
		}
		UIHelper.SetLabelText(Label_Grade, LocalizationManager.GetStringDirect(creatureGradeColorLocalizationKey));
		foreach (int rewardPetIndex in _dataProvider.GetRewardPetIndexList(_grade))
		{
			PetPortraitComponent petPortraitComponent = UIHelper.AddGridItem(Grid_PetPortrait, PetPortraitComponent.SmallPrefabAuto);
			if (petPortraitComponent == null)
			{
				return;
			}
			petPortraitComponent.OpenByIndex(rewardPetIndex, null);
			petPortraitComponent.SetStarMark(show: false);
			_petPortraitCompList.Add(petPortraitComponent);
		}
		UIHelper.RepositionGrid(Grid_PetPortrait);
		int maxPerLine = Grid_PetPortrait.Grid.maxPerLine;
		float cellHeight = Grid_PetPortrait.Grid.cellHeight;
		int num = (_petPortraitCompList.Count - 1) / maxPerLine + 1;
		float num2 = cellHeight * (float)num;
		Widget_Root.height = LabelHeight + (int)num2 + BottomHeight;
	}

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		FactoryBase.ReleaseAndClearList(_petPortraitCompList);
	}
}
