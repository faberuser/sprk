using System;
using System.Collections.Generic;
using NGameAsset;
using NShared;
using NShared.NLocalization;
using NShared.NWorldMap.NEventSubData;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NWorldMap.NMapLayout;

public class QuestDungeonInfo : MonoBehaviour, InitOnCreate
{
	[Serializable]
	public class MonsterGradeObject
	{
		public MonsterGradeType Grade;

		public GameObject GameObject;
	}

	public static PrefabAuto<QuestDungeonInfo> PrefabAuto = new PrefabAuto<QuestDungeonInfo>("Prefab2/WorldMap/QuestDungeonInfo");

	public GameObject Root;

	public UILabel ProgressLabel;

	public Transform RemainTimeRoot;

	public UILabel RemainTimeLabel;

	public UISprite RemainTimeGaugeSprite;

	public UITexture PortraitTexture;

	public UILabel Label_Objective;

	public GameObject CompletedObject;

	public List<MonsterGradeObject> MonsterGradeObjs;

	public QuestDungeon QuestDungeonData { get; set; }

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
	}

	public void Open(QuestDungeon questDungeon)
	{
		if (questDungeon != null)
		{
			QuestDungeonData = questDungeon;
			int monsterIndex = questDungeon.MonsterIndex;
			int requireCount = questDungeon.RequireCount;
			int num = Mathf.Min(questDungeon.ProgressCount, requireCount);
			Show(show: true);
			InitPortraitTexture(monsterIndex);
			UIHelper.SetLabelText(ProgressLabel, num + " / " + requireCount);
			UIHelper.EnableGameObject(CompletedObject, questDungeon.Completed);
			UIHelper.EnableGameObject(RemainTimeRoot, !questDungeon.Completed);
			if (!questDungeon.Completed)
			{
				UpdateRemainTime();
			}
		}
	}

	public void Show(bool show)
	{
		if ((bool)Root && Root.activeInHierarchy != show)
		{
			Root.SetActive(show);
		}
	}

	public int GetRemainTime()
	{
		return QuestDungeonData.GetRemainTime();
	}

	private void UpdateRemainTime()
	{
		if (QuestDungeonData != null)
		{
			int duration = QuestDungeonData.Duration;
			int remainTime = GetRemainTime();
			UpdateRemainTime(duration, remainTime, RemainTimeLabel, RemainTimeGaugeSprite);
		}
	}

	public static void UpdateRemainTime(int duration, int remainTime, UILabel remainTimeLabel, UISprite remainTimeGaugeSprite)
	{
		int num = remainTime / 3600;
		int num2 = remainTime / 60 % 60;
		int num3 = remainTime % 60;
		string text = "";
		if (num > 0)
		{
			text = text + num + ":";
		}
		text += $"{num2:D2}:{num3:D2}";
		UIHelper.SetLabelText(remainTimeLabel, text);
		UIHelper.SetSpriteFillAmount(remainTimeGaugeSprite, (duration > 0) ? ((float)remainTime / (float)duration) : 0f);
	}

	private void InitPortraitTexture(int creatureIndex)
	{
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(creatureIndex);
		if (data != null)
		{
			AssetTexture heroIcon = UIHelper.GetHeroIcon2(creatureIndex, ignoreAccount: true);
			UIHelper.ResetPlaneAssetTextureAsync(this, PortraitTexture, heroIcon);
			string text = data.Name;
			string text2 = LocalizationManager.GetString(WorldMapEvent.OBJECTIVE_KILL_MONSTER, text);
			UIHelper.SetLabelText(Label_Objective, text2);
		}
	}

	private void Update()
	{
		UpdateRemainTime();
	}
}
