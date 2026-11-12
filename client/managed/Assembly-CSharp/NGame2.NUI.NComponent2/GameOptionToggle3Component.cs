using System;
using NShared;
using NVespa.NGlobal;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class GameOptionToggle3Component : GameOptionBaseComponent, InitOnCreate
{
	[Serializable]
	public class Toggle3Info
	{
		public UIToggle Toggle1;

		public UIToggle Toggle2;

		public UIToggle Toggle3;

		private GameOptionValueInfo valueInfo1 = new GameOptionValueInfo();

		private GameOptionValueInfo valueInfo2 = new GameOptionValueInfo();

		private GameOptionValueInfo valueInfo3 = new GameOptionValueInfo();

		private string currentValue;

		private string UIdefaultValue = string.Empty;

		public string Value
		{
			get
			{
				if (Toggle1.value)
				{
					return valueInfo1.Value;
				}
				if (Toggle2.value)
				{
					return valueInfo2.Value;
				}
				if (Toggle3.value)
				{
					return valueInfo3.Value;
				}
				return currentValue;
			}
			set
			{
				if (!(Toggle1 == null) && !(Toggle2 == null) && !(Toggle3 == null))
				{
					if (value == valueInfo1.Value && value == valueInfo2.Value && value == valueInfo3.Value && !String_Extension_Method.IsNullOrEmpty(UIdefaultValue))
					{
						value = UIdefaultValue;
					}
					currentValue = value;
					Toggle1.Set(currentValue == valueInfo1.Value);
					Toggle2.Set(currentValue == valueInfo2.Value);
					Toggle3.Set(currentValue == valueInfo3.Value);
				}
			}
		}

		public bool AllInactive()
		{
			if (Toggle1 == null || Toggle2 == null || Toggle3 == null)
			{
				return true;
			}
			if (!Toggle1.value && !Toggle2.value)
			{
				return !Toggle3.value;
			}
			return false;
		}

		public void Init(NShared.GameOptionData gameOptionData)
		{
			UIdefaultValue = gameOptionData.UIdefaultValue;
			if (!String_Extension_Method.IsNullOrEmpty(gameOptionData.Value1))
			{
				valueInfo1.Value = gameOptionData.Value1;
				valueInfo1.ValueLocalKey = gameOptionData.ValueLocalKey1;
				valueInfo1.DisableGroup = gameOptionData.DisableGroup1;
				Toggle1.group = gameOptionData.Index + 100000;
			}
			else
			{
				UIHelper.EnableGameObject(Toggle1, enable: false);
			}
			if (!String_Extension_Method.IsNullOrEmpty(gameOptionData.Value2))
			{
				valueInfo2.Value = gameOptionData.Value2;
				valueInfo2.ValueLocalKey = gameOptionData.ValueLocalKey2;
				valueInfo2.DisableGroup = gameOptionData.DisableGroup2;
				Toggle2.group = gameOptionData.Index + 100000;
			}
			else
			{
				UIHelper.EnableGameObject(Toggle2, enable: false);
			}
			if (!String_Extension_Method.IsNullOrEmpty(gameOptionData.Value3))
			{
				valueInfo3.Value = gameOptionData.Value3;
				valueInfo3.ValueLocalKey = gameOptionData.ValueLocalKey3;
				valueInfo3.DisableGroup = gameOptionData.DisableGroup3;
				Toggle3.group = gameOptionData.Index + 100000;
			}
			else
			{
				UIHelper.EnableGameObject(Toggle3, enable: false);
			}
		}

		public bool CheckValue(string value)
		{
			if (valueInfo1.Value == value || valueInfo2.Value == value || valueInfo3.Value == value)
			{
				return true;
			}
			return false;
		}

		public bool CheckLocalKeyValue(string value)
		{
			if (valueInfo1.Value == value || valueInfo2.Value == value || valueInfo3.Value == value)
			{
				return true;
			}
			return false;
		}

		public string GetValueWithLocalKey(string localKey)
		{
			if (valueInfo1.ValueLocalKey == localKey)
			{
				return valueInfo1.Value;
			}
			if (valueInfo2.ValueLocalKey == localKey)
			{
				return valueInfo2.Value;
			}
			if (valueInfo3.ValueLocalKey == localKey)
			{
				return valueInfo3.Value;
			}
			return valueInfo1.Value;
		}

		public GameOptionValueInfo GetValueWithDisableGroup(int index)
		{
			if (valueInfo1 != null && __Extension_Method_ICollection__.COUNT(valueInfo1.DisableGroup) > 0)
			{
				int[] disableGroup = valueInfo1.DisableGroup;
				for (int i = 0; i < disableGroup.Length; i++)
				{
					if (disableGroup[i] == index)
					{
						return valueInfo1;
					}
				}
			}
			if (valueInfo2 != null && __Extension_Method_ICollection__.COUNT(valueInfo2.DisableGroup) > 0)
			{
				int[] disableGroup = valueInfo2.DisableGroup;
				for (int i = 0; i < disableGroup.Length; i++)
				{
					if (disableGroup[i] == index)
					{
						return valueInfo2;
					}
				}
			}
			if (valueInfo3 != null && __Extension_Method_ICollection__.COUNT(valueInfo3.DisableGroup) > 0)
			{
				int[] disableGroup = valueInfo3.DisableGroup;
				for (int i = 0; i < disableGroup.Length; i++)
				{
					if (disableGroup[i] == index)
					{
						return valueInfo3;
					}
				}
			}
			return null;
		}

		public string GetCurrentValue()
		{
			return currentValue;
		}

		public GameOptionValueInfo GetCurrentValueInfo()
		{
			if (currentValue == valueInfo1.Value)
			{
				return valueInfo1;
			}
			if (currentValue == valueInfo2.Value)
			{
				return valueInfo2;
			}
			if (currentValue == valueInfo3.Value)
			{
				return valueInfo3;
			}
			return valueInfo1;
		}
	}

	public const string PrefabPath = "Prefab2/Component/GameOption/GameOptionToggle3Component";

	public static PrefabAuto<GameOptionToggle3Component> PrefabAuto = new PrefabAuto<GameOptionToggle3Component>("Prefab2/Component/GameOption/GameOptionToggle3Component");

	public UILabel Label_ToggleLeft;

	public UILabel Label_ToggleMiddle;

	public UILabel Label_ToggleRight;

	public Toggle3Info toggleInfo;

	public void Open(Listener listener, NShared.GameOptionData _gameOptionData)
	{
		_listener = listener;
		gameOptionData = _gameOptionData;
		if (gameOptionData != null)
		{
			SetType();
			SetUIRoot();
			SetLabel();
			SetToggle();
		}
	}

	private void OnDisable()
	{
		Refresh();
	}

	public override GameOptionValueInfo GetCurrentOptionValueInfo()
	{
		return toggleInfo.GetCurrentValueInfo();
	}

	public override GameOptionValueInfo GetDisableGroupOptionValueInfo(int groupIndex)
	{
		return toggleInfo.GetValueWithDisableGroup(groupIndex);
	}

	public override object GetObjectValue(GameOptionValueInfo valueInfo)
	{
		if (valueInfo == null)
		{
			return null;
		}
		return Parse(valueInfo.Value);
	}

	public void SetLabel()
	{
		if (gameOptionData != null)
		{
			UIHelper.SetLabelText(ParentRoot.Label_OptionName, LocalizationManager.GetStringDirect(gameOptionData.OptionName));
			UIHelper.SetLabelText(ChildRoot.Label_OptionName, LocalizationManager.GetStringDirect(gameOptionData.OptionName));
			UIHelper.SetLabelText(Label_ToggleLeft, LocalizationManager.GetStringDirect(gameOptionData.ValueLocalKey1));
			UIHelper.SetLabelText(Label_ToggleMiddle, LocalizationManager.GetStringDirect(gameOptionData.ValueLocalKey2));
			UIHelper.SetLabelText(Label_ToggleRight, LocalizationManager.GetStringDirect(gameOptionData.ValueLocalKey3));
		}
	}

	public void SetToggle()
	{
		if (gameOptionData == null)
		{
			SprkGraphicsOptions.CenterTextureChoices(this);
			return;
		}
		toggleInfo.Init(gameOptionData);
		object optionValue = GetOptionValue(gameOptionData.ClassName, gameOptionData.FieldName);
		if (optionValue == null)
		{
			SprkGraphicsOptions.CenterTextureChoices(this);
			return;
		}
		base.CurrentValue = optionValue;
		string text = CheckValidValue(optionValue.ToString());
		toggleInfo.Value = text;
		ParseAndSetValue(text, setValue: false, callBack: false);
		SprkGraphicsOptions.CenterTextureChoices(this);
	}

	public void SetToggleByParentValue(object value)
	{
		if (gameOptionData != null && value != null)
		{
			string value2 = toggleInfo.Value;
			string text = value.ToString();
			if (!(value2 == text) && !String_Extension_Method.IsNullOrEmpty(text))
			{
				toggleInfo.Value = text;
				ParseAndSetValue(text, setValue: true, __Extension_Method_ICollection__.COUNT(GetChildIndices()) > 0);
			}
		}
	}

	public string CheckValidValue(string value)
	{
		if (!toggleInfo.CheckValue(value) && !String_Extension_Method.IsNullOrEmpty(gameOptionData.UIdefaultValue))
		{
			return gameOptionData.UIdefaultValue;
		}
		return value;
	}

	public void OnClickToggle()
	{
		if (gameOptionData == null)
		{
			return;
		}
		if (!gameOptionData.SaveValue)
		{
			OnClickToggleWithOutSave();
			return;
		}
		object optionValue = GetOptionValue(gameOptionData.ClassName, gameOptionData.FieldName);
		if (optionValue != null)
		{
			string text = optionValue.ToString();
			string value = toggleInfo.Value;
			if (!(text == value) && !String_Extension_Method.IsNullOrEmpty(value))
			{
				toggleInfo.Value = value;
				ParseAndSetValue(value, setValue: true, callBack: true);
			}
		}
	}

	private object Parse(string strValue)
	{
		if (String_Extension_Method.IsNullOrEmpty(strValue))
		{
			return null;
		}
		if (dataType != null)
		{
			if (dataType.IsEnum)
			{
				object obj = Enum.Parse(dataType, strValue);
				if (obj == null)
				{
					return null;
				}
				return obj;
			}
			if (dataType == typeof(bool))
			{
				bool result = false;
				if (bool.TryParse(strValue, out result))
				{
					return result;
				}
			}
		}
		return null;
	}

	private void ParseAndSetValue(string strValue, bool setValue, bool callBack)
	{
		object obj = Parse(strValue);
		if (obj != null)
		{
			if (obj.GetType() == typeof(bool))
			{
				SetUIState((bool)obj);
			}
			else
			{
				SetUIState(state: true);
			}
			if (setValue)
			{
				SetOptionValue(obj, toggleInfo.GetCurrentValueInfo(), callBack);
			}
		}
	}

	public void OnClickToggleWithOutSave()
	{
		if (toggleInfo == null || toggleInfo.GetCurrentValue() == toggleInfo.Value)
		{
			return;
		}
		string value = toggleInfo.Value;
		if (dataType != null && dataType.IsEnum)
		{
			object obj = Enum.Parse(dataType, value);
			if (obj != null)
			{
				RunPostProcessFunction(obj);
			}
		}
	}

	public override void Refresh()
	{
		base.Refresh();
		SetType();
		SetUIRoot();
		SetLabel();
		SetToggle();
	}

	public override void SetValueByChilds(object value, GameOptionValueInfo valueInfo)
	{
		base.SetValueByChilds(value, valueInfo);
		if (value != null)
		{
			toggleInfo.Value = valueInfo.Value;
			ParseAndSetValue(valueInfo.Value, setValue: true, gameOptionData.ParentNum > 0);
		}
	}

	public override void SetValueByParent(object value, GameOptionValueInfo valueInfo)
	{
		base.SetValueByParent(value, valueInfo);
		if (value == null)
		{
			return;
		}
		if (toggleInfo.CheckValue(value.ToString()))
		{
			SetToggleByParentValue(value);
			return;
		}
		string valueWithLocalKey = toggleInfo.GetValueWithLocalKey(valueInfo.ValueLocalKey);
		if (!String_Extension_Method.IsNullOrEmpty(valueWithLocalKey))
		{
			ParseAndSetValue(valueWithLocalKey, setValue: true, __Extension_Method_ICollection__.COUNT(GetChildIndices()) > 0);
		}
	}

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
	}
}
