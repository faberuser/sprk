using NGame2.NAccount;
using NGame2.NUI.NManager;
using NShared;
using NShared.ChangeHeroStorageSlotName;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow;

public class HeroPresetNameChangePopup : BaseWindow
{
	private enum ESlotNameResult
	{
		TooShort,
		TooLong,
		Invalid,
		Success
	}

	public interface IListener
	{
		void OnClickButton(string storageKey, string name);
	}

	public const string PrefabPath = "Prefab2/Window/MessagePopup/HeroPresetNameChangePopup";

	public UILabel LabelDesc;

	public UIInput NameInput;

	private IListener _listener;

	private string _originName;

	private string _storageKey;

	public static void Open(IListener listener, string storageKey, string originName)
	{
		WindowPrefab<HeroPresetNameChangePopup> windowPrefab = new WindowPrefab<HeroPresetNameChangePopup>("Prefab2/Window/MessagePopup/HeroPresetNameChangePopup", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInteral(listener, storageKey, originName);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	private void OpenInteral(IListener listener, string storageKey, string originName)
	{
		_listener = listener;
		_originName = originName;
		_storageKey = storageKey;
		Open(delegate
		{
			SetInputValue(_originName);
		}, null);
	}

	public void SetInputValue(string name)
	{
		if (NameInput != null)
		{
			NameInput.value = name;
			NameInput.UpdateLabel();
		}
	}

	public override void Close()
	{
		base.Close();
		_originName = "";
		_storageKey = "";
		_listener = null;
		SetInputValue("");
	}

	private ResultType Check(string slotName)
	{
		if (string.IsNullOrEmpty(slotName))
		{
			return ResultType.HeroStorageSlotNameTooShort;
		}
		int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("MinNickLength");
		int valueInt2 = GameRoot.Instance.ConstantDataContainer.GetValueInt("MaxNickLength");
		if (slotName.Length < valueInt)
		{
			return ResultType.HeroStorageSlotNameTooShort;
		}
		if (slotName.Length > valueInt2)
		{
			return ResultType.HeroStorageSlotNameTooLong;
		}
		if (!ServerLanguageChecker.CheckValidCharacter(slotName))
		{
			return ResultType.HeroStorageSlotNameInvalidThisServer;
		}
		if (GameRoot.Instance.CensoredWordDataContainer.IsCensoredWord(slotName) != CensoredWordDataContainer.CensoredResult.None)
		{
			return ResultType.HeroStorageSlotNameCensoredWord;
		}
		if (!NickChecker.CheckValidCharacter(slotName))
		{
			return ResultType.HeroStorageSlotNameInvalid;
		}
		if (NickChecker.CheckSimilarWord(slotName))
		{
			return ResultType.HeroStorageSlotNameInvalid;
		}
		return ResultType.Success;
	}

	public void OnClickButton()
	{
		if (_listener != null && !(NameInput == null))
		{
			string value = NameInput.value;
			ResultType resultType = Check(value);
			if (resultType != ResultType.Success)
			{
				string text = LocalizationManager.GetString(resultType);
				UIHelper.SetLabelText(LabelDesc, text);
			}
			else
			{
				_listener.OnClickButton(_storageKey, value);
				Close();
			}
		}
	}
}
