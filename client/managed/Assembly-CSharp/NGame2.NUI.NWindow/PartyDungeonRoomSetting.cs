using System;
using System.Collections;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NParty;
using NNguiExtension;
using NShared;
using NShared.NLocalization;
using NShared.NParty;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow;

public class PartyDungeonRoomSetting : BaseWindow
{
	public interface Listener
	{
		void CreatePartyDungeonRoom(PartyDungeonType type, int chapterIndex, int dungeonIndex, PartyDungeonDifficultyType difficulty, bool isPublic);

		void ChangePartyRoomSetting(PartyDungeonType type, int chapterIndex, int dungeonIndex, PartyDungeonDifficultyType difficulty, bool isResetSquard);

		void ReCreatePartyDungeonRoom(PartyDungeonType type, int chapterIndex, int dungeonIndex, PartyDungeonDifficultyType difficulty, bool isPublic);
	}

	public enum WindowType
	{
		Create,
		Setting
	}

	private enum SelectedItemType
	{
		None,
		Type,
		Chapter,
		Stage
	}

	private enum EStage
	{
		None = -1
	}

	public const string PrefabPath = "Resources/Prefab2/Window/PartyDungeon/PartyDungeonRoomSetting";

	public UILabel Label_Confirm;

	public UIButton Button_Confirm;

	public UIToggle PublicRoomToggle;

	private List<PartyDungeonData> _selectedDataList = new List<PartyDungeonData>();

	private Listener _listener;

	private WindowType _windowType;

	private bool _selectItemifOneItem = true;

	public UIComboBox ComboBox_Type;

	public UIComboBox ComboBox_Chapter;

	public UIComboBox ComboBox_Stage;

	private SortedDictionary<PartyDungeonType, bool> Dics_DungeonType = new SortedDictionary<PartyDungeonType, bool>();

	private SortedDictionary<PartyDungeonChapter, bool> Dics_Chapter = new SortedDictionary<PartyDungeonChapter, bool>();

	private SortedDictionary<int, Tuple<int, bool>> _dicStage = new SortedDictionary<int, Tuple<int, bool>>();

	private Dictionary<int, string> _dicStageLocale = new Dictionary<int, string>();

	private PartyDungeonData _partyDungeonData;

	private PartyDungeonDifficultyType _difficulty;

	public bool IsPublic => PublicRoomToggle.value;

	public override void Awake()
	{
		base.Awake();
	}

	private void SetSettingInfo(int chapterIndex, int dungeonIndex, PartyDungeonDifficultyType difficulty)
	{
		_partyDungeonData = GameRoot.Instance.PartyDungeonDataContainer.GetData(chapterIndex, dungeonIndex);
		_difficulty = difficulty;
	}

	public void OpenSetting(Listener listener, int chapterIndex, int dungeonIndex, PartyDungeonDifficultyType difficulty)
	{
		base.Open();
		_listener = listener;
		_windowType = WindowType.Setting;
		PublicRoomToggle.Set(state: true);
		SetUi();
		SetComboBoxCallback();
		UpdateComboBox(SelectedItemType.None);
		SetSettingInfo(chapterIndex, dungeonIndex, difficulty);
		if (_windowType == WindowType.Setting)
		{
			ComboBox_Type.SelectItem((int)_partyDungeonData.DungeonType);
			ComboBox_Chapter.SelectItem((int)_partyDungeonData.Chapter);
			int stageItemIndexByDungeonIndex = GetStageItemIndexByDungeonIndex(_partyDungeonData.DungeonIndex);
			ComboBox_Stage.SelectItem(stageItemIndexByDungeonIndex);
		}
	}

	public void Open(Listener listener, WindowType windowType = WindowType.Create)
	{
		base.Open();
		_listener = listener;
		_windowType = windowType;
		PublicRoomToggle.Set(state: true);
		SetUi();
		SetComboBoxCallback();
		UpdateComboBox(SelectedItemType.None);
		UpdateButtonConfirm();
	}

	private void ClearComboBoxItem()
	{
		ComboBox_Type.Clear();
		ComboBox_Chapter.Clear();
		ComboBox_Stage.Clear();
	}

	private void ClearSettingInfo()
	{
		_partyDungeonData = null;
		_difficulty = PartyDungeonDifficultyType.None;
	}

	public override void Reset()
	{
		base.Reset();
		ClearSettingInfo();
		_selectedDataList.Clear();
		ClearItemDics(SelectedItemType.None);
		ClearComboBoxItem();
	}

	private void UpdateButtonConfirm()
	{
		bool flag = GetCurrentValue(ComboBox_Type, PartyDungeonType.None) != PartyDungeonType.None;
		flag &= GetCurrentValue(ComboBox_Chapter, PartyDungeonChapter.None) != PartyDungeonChapter.None;
		flag &= GetCurrentValue(ComboBox_Stage, 0) != -1;
		UIHelper.EnableButton(Button_Confirm, flag && __Extension_Method_ICollection__.COUNT(_selectedDataList) == 1);
	}

	private void SetUi()
	{
		string empty = string.Empty;
		if (_windowType == WindowType.Setting)
		{
			PartyInfo partyInfo = GlobalAutoBehaviour<PartyManager>.instance.PartyInfo;
			bool flag = false;
			if (partyInfo != null)
			{
				flag = partyInfo.IsPublicRoom();
				PublicRoomToggle.Set(flag);
			}
			empty = LocalizationManager.GetStringDirect("PARTY_CHANGE");
		}
		else
		{
			empty = LocalizationManager.GetStringDirect("PARTY_ENTER");
		}
		UIHelper.SetLabelText(Label_Confirm, empty);
		bool enable = _windowType == WindowType.Create;
		UIHelper.EnableToggle(PublicRoomToggle, enable, grayscale: true);
		if ((bool)PublicRoomToggle)
		{
			PublicRoomToggle.gameObject.SetActive(value: false);
			PublicRoomToggle.gameObject.SetActive(value: true);
		}
	}

	public void OnClickCreatePartyRoom()
	{
		if (__Extension_Method_ICollection__.COUNT(_selectedDataList) == 0)
		{
			VespaDebug.LogError("_selectedDataList  COUNT() == 0");
			return;
		}
		if (__Extension_Method_ICollection__.COUNT(_selectedDataList) > 1)
		{
			VespaDebug.LogError("_selectedDataList.COUNT() > 1");
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_selectedDataList); i++)
			{
				VespaDebug.LogError("_selectedDataList[" + i + "] : " + _selectedDataList[i].ChapterIndex + " , " + _selectedDataList[i].DungeonIndex);
			}
		}
		PartyDungeonType dungeonType = _selectedDataList[0].DungeonType;
		int chapterIndex = _selectedDataList[0].ChapterIndex;
		int dungeonIndex = _selectedDataList[0].DungeonIndex;
		NShared.DungeonDifficultyType difficulty = (NShared.DungeonDifficultyType)Helper.GetDifficulty(chapterIndex, dungeonIndex);
		PartyDungeonDifficultyType difficulty2 = UIHelper.GetPartyDungeonDifficultyType(difficulty);
		if (_listener != null)
		{
			if (_windowType == WindowType.Create)
			{
				_listener.CreatePartyDungeonRoom(dungeonType, chapterIndex, dungeonIndex, difficulty2, IsPublic);
			}
			else
			{
				PartyDungeonData data = GameRoot.Instance.PartyDungeonDataContainer.GetData(chapterIndex, dungeonIndex);
				if (data == null)
				{
					VespaDebug.LogError($"ChatperIndex : {chapterIndex} ,DungoenIndex : {dungeonIndex} PartyDungeonData Is Null");
					return;
				}
				if (data.PlayerCount < _partyDungeonData.PlayerCount)
				{
					string text = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
					string message = LocalizationManager.GetString(MULTIBATTLE.MULTIBATTLE_ROOM_CHANGE_LEADER);
					MessagePopup3Button.Open(text, message, delegate
					{
						_listener.ReCreatePartyDungeonRoom(dungeonType, chapterIndex, dungeonIndex, difficulty2, IsPublic);
					}, null, NShared.NLocalization.Common.COMMON_CONFIRM, NShared.NLocalization.Common.COMMON_CANCEL);
					return;
				}
				bool isResetSquard = data.DeckCount != _partyDungeonData.DeckCount || data.DeckCountPerPlayer != _partyDungeonData.DeckCountPerPlayer;
				_listener.ChangePartyRoomSetting(dungeonType, chapterIndex, dungeonIndex, difficulty2, isResetSquard);
			}
		}
		Close();
	}

	public void OnTogglePublicRoom()
	{
	}

	private void SetComboBoxCallback()
	{
		ComboBox_Type.OnChanged = DungeonTypeSelect;
		ComboBox_Chapter.OnChanged = ChapterSelect;
		ComboBox_Stage.OnChanged = StageSelect;
	}

	private void ClearIf(IDictionary dics, bool condition)
	{
		if (condition)
		{
			dics.Clear();
		}
	}

	private void ClearItemDics(SelectedItemType selectedType)
	{
		ClearIf(Dics_DungeonType, selectedType < SelectedItemType.Type);
		ClearIf(Dics_Chapter, selectedType < SelectedItemType.Chapter);
		ClearIf(_dicStage, selectedType < SelectedItemType.Stage);
	}

	private void SetStageComboBoxItem()
	{
		ComboBox_Stage.Clear();
		foreach (KeyValuePair<int, Tuple<int, bool>> item in _dicStage)
		{
			if (item.Value.Item2)
			{
				string stageLocale = GetStageLocale(item.Value.Item1);
				ComboBox_Stage.AddItem(item.Key, stageLocale);
			}
		}
		ComboBox_Stage.Reposition();
	}

	private void SetComboBoxItem<T>(UIComboBox comboBox, IDictionary<T, bool> list, Func<T, string> localeFunc)
	{
		comboBox.Clear();
		foreach (KeyValuePair<T, bool> item in list)
		{
			if (item.Value)
			{
				string text = ((localeFunc != null) ? localeFunc(item.Key) : string.Empty);
				comboBox.AddItem((int)(object)item.Key, text);
			}
		}
		comboBox.Reposition();
	}

	private T GetCurrentValue<T>(UIComboBox comboBox, T defaultValue = default(T))
	{
		if (comboBox.GetItemCount() == 0)
		{
			return defaultValue;
		}
		if (comboBox.CurrentKey != -1)
		{
			return (T)(object)comboBox.CurrentKey;
		}
		return defaultValue;
	}

	private void SelectItemIf(UIComboBox comboBox, int value, bool condition)
	{
		if (condition)
		{
			comboBox.SelectItem(value, isNotify: false);
		}
	}

	private void UpdateItemDics(SelectedItemType selectedType)
	{
		ClearItemDics(selectedType);
		PartyDungeonType currentValue = GetCurrentValue(ComboBox_Type, PartyDungeonType.None);
		PartyDungeonChapter partyDungeonChapter = ((selectedType >= SelectedItemType.Chapter) ? GetCurrentValue(ComboBox_Chapter, PartyDungeonChapter.None) : PartyDungeonChapter.None);
		int requireDungeonIndex = ((selectedType >= SelectedItemType.Stage) ? GetDungeonIndexByCurrentComboBoxKey() : 0);
		PartyDungeonDifficultyType difficulty = _difficulty;
		if (selectedType < SelectedItemType.Stage)
		{
			_dicStageLocale.Clear();
		}
		_selectedDataList.Clear();
		List<PartyDungeonData> filteredData = GameRoot.Instance.PartyDungeonDataContainer.GetFilteredData(currentValue, partyDungeonChapter, difficulty, requireDungeonIndex, delegate(PartyDungeonData data)
		{
			if (data == null)
			{
				return false;
			}
			return GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.CanGoToChapter(data.ChapterIndex) && GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.CanVisit(data.ChapterIndex, data.DungeonIndex);
		});
		for (int num = 0; num < __Extension_Method_ICollection__.COUNT(filteredData); num++)
		{
			Dics_DungeonType[filteredData[num].DungeonType] = true;
			Dics_Chapter[filteredData[num].Chapter] = true;
			if (partyDungeonChapter != PartyDungeonChapter.None)
			{
				if (!_dicStage.ContainsKey(num))
				{
					_dicStage[num] = new Tuple<int, bool>(filteredData[num].DungeonIndex, i_item2: true);
				}
				_dicStageLocale[filteredData[num].DungeonIndex] = UIHelper.GetNodeName(filteredData[num].ChapterIndex, filteredData[num].DungeonIndex);
			}
			Helper.GetMaxAvailableDifficulty(filteredData[num].ChapterIndex, filteredData[num].DungeonIndex);
			_selectedDataList.Add(filteredData[num]);
		}
	}

	private void SelectItem(UIComboBox combo, bool noti = false)
	{
		if (!combo.SelectItem(combo.CurrentKey, noti))
		{
			combo.SelectFirstItem();
		}
	}

	private string GetStageLocale(int stage)
	{
		if (stage == 0)
		{
			return UIHelper.PartyStageLocale(stage);
		}
		if (_dicStageLocale.ContainsKey(stage))
		{
			return _dicStageLocale[stage];
		}
		return stage.ToString();
	}

	private void UpdateComboBox(SelectedItemType selectedItemType)
	{
		UpdateItemDics(selectedItemType);
		SetComboBoxItem(ComboBox_Type, Dics_DungeonType, UIHelper.PartyDungeonTypeLocale);
		SetComboBoxItem(ComboBox_Chapter, Dics_Chapter, UIHelper.PartyChapterLocale);
		SetStageComboBoxItem();
		SelectItem(ComboBox_Type);
		SelectItem(ComboBox_Chapter, selectedItemType < SelectedItemType.Chapter);
		SelectItem(ComboBox_Stage, selectedItemType < SelectedItemType.Stage);
	}

	private int GetStageItemIndexByDungeonIndex(int dungeonIndex)
	{
		foreach (KeyValuePair<int, Tuple<int, bool>> item in _dicStage)
		{
			Tuple<int, bool> value = item.Value;
			if (value != null && value.Item1 == dungeonIndex)
			{
				return item.Key;
			}
		}
		return 0;
	}

	private int GetDungeonIndexByCurrentComboBoxKey()
	{
		int currentKey = ComboBox_Stage.CurrentKey;
		if (_dicStage.ContainsKey(currentKey))
		{
			Tuple<int, bool> tuple = _dicStage[currentKey];
			if (tuple != null)
			{
				return tuple.Item1;
			}
		}
		return 0;
	}

	private void DungeonTypeSelect(int value)
	{
		UpdateComboBox(SelectedItemType.Type);
		UpdateButtonConfirm();
	}

	private void ChapterSelect(int value)
	{
		UpdateComboBox(SelectedItemType.Chapter);
		UpdateButtonConfirm();
	}

	private void StageSelect(int value)
	{
		UpdateComboBox(SelectedItemType.Stage);
		UpdateButtonConfirm();
	}
}
