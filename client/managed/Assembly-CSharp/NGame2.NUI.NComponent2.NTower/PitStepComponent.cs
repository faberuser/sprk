using NGameAsset;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NTower;

public class PitStepComponent : LinkFactoryComponent, InitOnCreate
{
	public interface Listener
	{
		void OnClickStep(PitStepComponent component);
	}

	public const string PrefabPath = "Prefab2/Component/Tower/PitStepComponent";

	public static PrefabAuto<PitStepComponent> PrefabAuto = new PrefabAuto<PitStepComponent>("Prefab2/Component/Tower/PitStepComponent");

	public UILabel StepLabel;

	public UIWidget CompletedRoot;

	public UIWidget VisibleRoot;

	public UIWidget LockedRoot;

	public UITexture BossTexture;

	public UILabel NameLabel;

	public UILabel DescLabel;

	public UILabel LevelLabel;

	public GameObject[] ClosedObject;

	public UIWidget HpWidget;

	public UISprite HpSprite;

	public UILabel HpPercentageLabel;

	public UIGame3DWidget CompleteEffect;

	public UIGame3DWidget[] OpenEffect;

	private Listener _listener;

	public TowerData TowerData { get; private set; }

	public TowerFloorData FloorData { get; private set; }

	public int CompletedFloor { get; private set; }

	public bool IsCompleted => CompletedFloor >= FloorData.Floor;

	public int HpPercentage { get; private set; }

	public bool TextureLoaded { get; private set; }

	public void Open(Listener listener, TowerData towerData, TowerFloorData floorData, int completedFloor, int hpPercentage)
	{
		_listener = listener;
		TowerData = towerData;
		FloorData = floorData;
		CompletedFloor = completedFloor;
		HpPercentage = hpPercentage;
		TextureLoaded = false;
		UIHelper.SetLabelText(StepLabel, floorData.Floor);
		InitPortrait();
	}

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
	}

	private bool IsLocked()
	{
		int num = CompletedFloor + 1;
		return FloorData.Floor > num;
	}

	public void SetOpened(bool opened)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(ClosedObject); i++)
		{
			UIHelper.EnableGameObject(ClosedObject[i], i == FloorData.Floor && !opened);
		}
	}

	public void DisplayClearEffect()
	{
		if (CompleteEffect != null)
		{
			CompleteEffect.Play(null);
			CompleteEffect.PlayPhase("End");
		}
	}

	public void PlayClearEffect()
	{
		if ((bool)CompleteEffect)
		{
			UIHelper.EnableGameObject(CompleteEffect, enable: true);
			CompleteEffect.Play(null);
		}
	}

	public void PlayOpenEffect()
	{
		int num = ((FloorData != null) ? FloorData.Floor : (-1));
		if (num >= 0 && num < __Extension_Method_ICollection__.COUNT(OpenEffect))
		{
			UIGame3DWidget uIGame3DWidget = OpenEffect[num];
			if ((bool)uIGame3DWidget)
			{
				UIHelper.EnableGameObject(uIGame3DWidget, enable: true);
				uIGame3DWidget.Play(null);
			}
		}
	}

	private void InitPortrait()
	{
		UIHelper.EnableGameObject(CompletedRoot, enable: false);
		UIHelper.EnableGameObject(VisibleRoot, enable: false);
		UIHelper.EnableGameObject(LockedRoot, enable: false);
		TextureLoaded = true;
		if (IsLocked())
		{
			SetOpened(opened: false);
			UIHelper.EnableGameObject(LockedRoot, enable: true);
			return;
		}
		bool flag = FloorData.Floor <= CompletedFloor;
		UIHelper.EnableGameObject(VisibleRoot, enable: true);
		UIHelper.EnableGameObject(CompletedRoot, flag);
		UIHelper.SetWidgetGrayScale(VisibleRoot, flag);
		SetOpened(opened: true);
		int num = ((!flag) ? HpPercentage : 0);
		UIHelper.SetWidgetGrayScale(HpWidget, flag);
		UIHelper.SetSpriteFillAmount(HpSprite, (float)num / 100f);
		UIHelper.SetLabelText(HpPercentageLabel, num + "%");
		int chapterIndex = FloorData.ChapterIndex;
		int dungeonIndex = FloorData.DungeonIndex;
		CampaignWaveData bossWaveData;
		CreatureData bossCreatureData = GameRoot.Instance.CampaignDungeonDataContainer.GetBossCreatureData(chapterIndex, dungeonIndex, DungeonDifficultyType.Easy, scenario: false, out bossWaveData);
		if (bossCreatureData == null)
		{
			VespaDebug.LogError("Boss creature not found : chapterIndex=" + chapterIndex + " dungeonIndex=" + dungeonIndex);
			return;
		}
		if ((bool)BossTexture)
		{
			TextureLoaded = false;
			AssetTexture heroLargeIcon = UIHelper.GetHeroLargeIcon2(bossCreatureData.CodeName, 0);
			UIHelper.ResetPlaneAssetTextureAsync(this, BossTexture, heroLargeIcon, UIHelper.EAssetTextureAssign.None, delegate
			{
				TextureLoaded = true;
			});
		}
		string creatureName = string.Empty;
		string creatureNamePrefix = string.Empty;
		GameRoot.Instance.CreatureVisualDataContainer.GetCreatureName(bossCreatureData, bossWaveData?.BossVisualInfo, LocalizationManager.GetStringDirect, out creatureName, out creatureNamePrefix);
		UIHelper.SetLabelText(NameLabel, creatureName);
		UIHelper.SetWidgetGrayScale(NameLabel, flag);
		UIHelper.SetLabelText(DescLabel, creatureNamePrefix);
		UIHelper.SetWidgetGrayScale(DescLabel, flag);
		UIHelper.EnableGameObject(LevelLabel, bossWaveData != null);
		if (bossWaveData != null)
		{
			UIHelper.SetLabelText(LevelLabel, bossWaveData.Level);
			UIHelper.SetWidgetGrayScale(LevelLabel, flag);
		}
	}

	public void OnClickStep()
	{
		if (_listener != null)
		{
			_listener.OnClickStep(this);
		}
	}
}
