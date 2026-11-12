using System;
using System.Collections.Generic;
using NGame2.NLobby;
using NGame2.NUI.NComponent2.PlayRecord.DisplayValue;
using NShared;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class PlayRecordComponent : MonoBehaviour, NVespa.NPrefab2.InitOnCreate
{
	public const string PrefabPath = "Prefab2/Component/PlayRecord/PlayRecordComponent";

	public static PrefabAuto<PlayRecordComponent> PrefabAuto = new PrefabAuto<PlayRecordComponent>("Prefab2/Component/PlayRecord/PlayRecordComponent");

	public const string UnknownRecordValue = "-";

	private PlayRecordData _playRecordData;

	public UILabel TitleLabel;

	public Transform ValueRoot;

	public UIButton Button;

	private Component _valueComponent;

	void NVespa.NPrefab2.InitOnCreate.OnCreate()
	{
	}

	void NVespa.NPrefab2.InitOnCreate.OnRelease()
	{
		RemoveValueComponent();
	}

	public void Open(PlayRecordData playRecordData, PlayRecordInfo playRecordInfo)
	{
		if (playRecordData != null)
		{
			_playRecordData = playRecordData;
			UIHelper.EnableGameObject(Button, enable: false);
			string stringDirect = LocalizationManager.GetStringDirect(playRecordData.Title);
			UIHelper.SetLabelText(TitleLabel, stringDirect);
			RemoveValueComponent();
			PlayRecordDisplayType displayType = playRecordData.DisplayType;
			PlayRecordDisplayValueType displayValueType = playRecordData.DisplayValueType;
			string recordValue = playRecordInfo?.RecordValue;
			Component component = (_valueComponent = CreateDisplayValueComponent(displayType));
			if (component is IDisplayValueComponent)
			{
				string displayRecordValue = GetDisplayRecordValue(displayValueType, recordValue);
				(component as IDisplayValueComponent).Open(playRecordData, displayRecordValue);
			}
			SetButton();
		}
	}

	private void RemoveValueComponent()
	{
		FactoryBase.SafeReleaseComponent(ref _valueComponent);
	}

	private Component CreateDisplayValueComponent(PlayRecordDisplayType displayType)
	{
		try
		{
			switch (displayType)
			{
			case PlayRecordDisplayType.HeroStar:
				return HeroStarValueComponent.PrefabAuto.Factory.Create(ValueRoot);
			case PlayRecordDisplayType.EquipStar:
				return EquipStarValueComponent.PrefabAuto.Factory.Create(ValueRoot);
			case PlayRecordDisplayType.HeroName:
				return HeroNameValueComponent.PrefabAuto.Factory.Create(ValueRoot);
			case PlayRecordDisplayType.ArenaGrade:
				return ArenaGradeValueComponent.PrefabAuto.Factory.Create(ValueRoot);
			case PlayRecordDisplayType.DifficultyName:
				return DifficultyNameValueComponent.PrefabAuto.Factory.Create(ValueRoot);
			case PlayRecordDisplayType.Chapter:
				return ChapterNameValueComponent.PrefabAuto.Factory.Create(ValueRoot);
			case PlayRecordDisplayType.RaidBossLevel:
				return RaidBossLevelValueComponent.PrefabAuto.Factory.Create(ValueRoot);
			case PlayRecordDisplayType.ChallengeRaidDifficultyName:
				return ChallengeRaidDifficultyNameValueComponent.PrefabAuto.Factory.Create(ValueRoot);
			case PlayRecordDisplayType.Date:
				return DateValueComponent.PrefabAuto.Factory.Create(ValueRoot);
			case PlayRecordDisplayType.Hour:
				return CreateTextValueComponent((string e) => UIHelper.GetRemainHourTextFromSecond(Vespa_String_Extension_Method.TOINT(e, 0)), 0);
			case PlayRecordDisplayType.Day:
				return CreateLocalizedTextValueComponent(NShared.NLocalization.Common.TIME_DAY, 0);
			case PlayRecordDisplayType.Count:
				return CreateLocalizedTextValueComponent(NShared.NLocalization.Common.NUMBER_COUNT2, 0);
			case PlayRecordDisplayType.HeroCount:
				return CreateLocalizedTextValueComponent(Hero.HERO_COUNT, 0);
			case PlayRecordDisplayType.ActionCount:
				return CreateLocalizedTextValueComponent(NShared.NLocalization.Common.NUMBER_COUNT, 0);
			case PlayRecordDisplayType.WinCount:
				return CreateLocalizedTextValueComponent(Match.WIN_COUNT, 0);
			case PlayRecordDisplayType.LoseCount:
				return CreateLocalizedTextValueComponent(Match.LOSE_COUNT, 0);
			case PlayRecordDisplayType.MonsterCount:
				return CreateLocalizedTextValueComponent(Battle.MONSTER_COUNT, 0);
			case PlayRecordDisplayType.HeroLevel:
				return CreateLocalizedTextValueComponent(Hero.HERO_LEVEL, "-");
			case PlayRecordDisplayType.Floor:
				return CreateLocalizedTextValueComponent(Tower.FLOOR, "-");
			case PlayRecordDisplayType.Score:
				return CreateLocalizedTextValueComponent(Match.SCORE, "-");
			case PlayRecordDisplayType.Wave:
				return CreateLocalizedTextValueComponent("CONTENT_WAVE_TEXT", 0);
			}
		}
		catch (Exception ex)
		{
			VespaDebug.LogError("Failed to create value component: " + displayType.ToString() + " error=" + ex);
		}
		return CreateTextValueComponent(null, "-");
	}

	private Component CreateTextValueComponent<T>(Func<string, string> converter, T defaultValue)
	{
		TextValueComponent textValueComponent = TextValueComponent.PrefabAuto.Factory.Create(ValueRoot);
		textValueComponent.SetTextConverter(converter, defaultValue);
		return textValueComponent;
	}

	private Component CreateLocalizedTextValueComponent<T>(Enum e, T defaultValue)
	{
		Func<string, string> converter = (string v) => LocalizationManager.GetString(e, v);
		return CreateTextValueComponent(converter, defaultValue);
	}

	private Component CreateLocalizedTextValueComponent<T>(string key, T defaultValue)
	{
		Func<string, string> converter = (string v) => LocalizationManager.GetStringDirect(key, v);
		return CreateTextValueComponent(converter, defaultValue);
	}

	private string GetDisplayRecordValue(PlayRecordDisplayValueType displayValueType, string recordValue)
	{
		switch (displayValueType)
		{
		case PlayRecordDisplayValueType.KeyCount:
			return __Extension_Method_ICollection__.COUNT(ParseRecordValueList(recordValue)).ToString();
		case PlayRecordDisplayValueType.MaxValue:
		{
			List<Tuple<string, string>> list4 = ParseRecordValueList(recordValue);
			SortRecordValueListByValue(list4, ascending: false);
			if (__Extension_Method_ICollection__.COUNT(list4) <= 0)
			{
				return string.Empty;
			}
			return list4[0].Item2.ToString();
		}
		case PlayRecordDisplayValueType.MinValue:
		{
			List<Tuple<string, string>> list2 = ParseRecordValueList(recordValue);
			SortRecordValueListByValue(list2);
			if (__Extension_Method_ICollection__.COUNT(list2) <= 0)
			{
				return string.Empty;
			}
			return list2[0].Item2.ToString();
		}
		case PlayRecordDisplayValueType.MaxValueKey:
		{
			List<Tuple<string, string>> list3 = ParseRecordValueList(recordValue);
			SortRecordValueListByValue(list3, ascending: false);
			if (__Extension_Method_ICollection__.COUNT(list3) <= 0)
			{
				return string.Empty;
			}
			return list3[0].Item1.ToString();
		}
		case PlayRecordDisplayValueType.MinValueKey:
		{
			List<Tuple<string, string>> list = ParseRecordValueList(recordValue);
			SortRecordValueListByValue(list);
			if (__Extension_Method_ICollection__.COUNT(list) <= 0)
			{
				return string.Empty;
			}
			return list[0].Item1.ToString();
		}
		default:
			return recordValue;
		}
	}

	private static List<Tuple<string, string>> ParseRecordValueList(string recordValue)
	{
		List<Tuple<string, string>> list = new List<Tuple<string, string>>();
		if (string.IsNullOrEmpty(recordValue))
		{
			return list;
		}
		string[] array = recordValue.Split(new char[1] { ',' });
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(array) / 2; i++)
		{
			string i_item = Vespa_String_Extension_Method.TRIM(array[i * 2]);
			string i_item2 = Vespa_String_Extension_Method.TRIM(array[i * 2 + 1]);
			list.Add(new Tuple<string, string>(i_item, i_item2));
		}
		return list;
	}

	private static void SortRecordValueListByValue(List<Tuple<string, string>> list, bool ascending = true)
	{
		if (ascending)
		{
			list.Sort((Tuple<string, string> l, Tuple<string, string> r) => Vespa_String_Extension_Method.TOINT(l.Item2, 0).CompareTo(Vespa_String_Extension_Method.TOINT(r.Item2, 0)));
		}
		else
		{
			list.Sort((Tuple<string, string> l, Tuple<string, string> r) => Vespa_String_Extension_Method.TOINT(r.Item2, 0).CompareTo(Vespa_String_Extension_Method.TOINT(l.Item2, 0)));
		}
	}

	public void SetButton()
	{
		if (!String_Extension_Method.IsNullOrEmpty(_playRecordData.MoveTaskType))
		{
			UIHelper.EnableGameObject(Button, enable: true);
			string stringDirect = LocalizationManager.GetStringDirect(_playRecordData.ButtonLabelText);
			Button.GetComponentInChildren<UILabel>().text = stringDirect;
		}
	}

	public void OnClickMoveTaskButton()
	{
		if (!String_Extension_Method.IsNullOrEmpty(_playRecordData.MoveTaskType))
		{
			SceneAutoBehaviour<ActivityManager>.instance.Run(_playRecordData.MoveTaskType, _playRecordData.MoveTaskValue);
		}
	}
}
