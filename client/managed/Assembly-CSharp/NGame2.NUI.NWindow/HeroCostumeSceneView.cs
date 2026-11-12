using System.Collections.Generic;
using NGame2.NUI.NComponent2.NCostume;
using NGame2.NUI.NWindow.NGlobalTopmost;
using NGame2.NUtil.NCostume;
using NShared;
using NVespa.NTask;

namespace NGame2.NUI.NWindow;

public class HeroCostumeSceneView : HeroFullSceneViewBase
{
	protected IEnumerator<TaskCurrentData> SetupHeroWithSceneChange(CachedHeroInfo heroInfo)
	{
		SceneChanger sceneChanger = GlobalTopmostWindow.instance.CreateSceneChanger(5f, GlobalTopmostWindow.ESceneChangeType.BlackScreen);
		while (!sceneChanger.IsCompleteAnimation())
		{
			yield return TaskCurrentData.RUNNING;
		}
		using (TaskRunner runner = new TaskRunner(SetupHero(heroInfo)))
		{
			while (runner.MoveNextWhileRunning())
			{
				yield return runner.Current;
			}
		}
		sceneChanger.Close();
		yield return TaskCurrentData.SUCCESS;
	}

	public void UpdateCostumeListState(HeroInfo heroInfo, List<HeroCostumeComponent> costumeComponentList)
	{
		if (heroInfo == null)
		{
			return;
		}
		int costumeIndex = heroInfo.CostumeIndex;
		CostumeData equippedCostumeData = ((costumeIndex != 0) ? GameRoot.Instance.CostumeDataContainer.GetData(costumeIndex) : null);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(costumeComponentList); i++)
		{
			HeroCostumeComponent heroCostumeComponent = costumeComponentList[i];
			CostumeData costumeData = heroCostumeComponent.CostumeData;
			if (costumeData != null)
			{
				CostumeState costumeState = HeroCostume.GetCostumeState(equippedCostumeData, costumeData, checkHeroOwn: false);
				if (costumeState == CostumeState.UnEquipSelect)
				{
					costumeState = CostumeState.Equipped;
				}
				heroCostumeComponent.SetState(costumeState);
			}
		}
	}

	public void UpdateCostumeListSelectDeselect(CostumeData selectCostumeData, CostumeData DeselectCostumeData, List<HeroCostumeComponent> costumeComponentList)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(costumeComponentList); i++)
		{
			CostumeData costumeData = costumeComponentList[i].CostumeData;
			if (Vespa_Common_Class_Extension_Method.EQUALS(costumeData, DeselectCostumeData))
			{
				costumeComponentList[i].SetSelected(Selected: false);
			}
			if (Vespa_Common_Class_Extension_Method.EQUALS(costumeData, selectCostumeData))
			{
				costumeComponentList[i].SetSelected(Selected: true);
			}
		}
	}
}
