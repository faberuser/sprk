using NGame2.NAccount;
using NShared;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NMultipleDungeon;

public class MultipleDungeonComponent : LinkFactoryComponent, NVespa.NPrefab2.InitOnCreate
{
	public interface Listener
	{
		void EnterDungeon(UnderPrisonData underPrionData);
	}

	public static string PrefabPath = "Prefab2/Component/MultipleDungeon/MultipleDungeonComponent";

	public static PrefabAuto<MultipleDungeonComponent> PrefabAuto = new PrefabAuto<MultipleDungeonComponent>(PrefabPath);

	public UILabel DungeonNameLabel;

	public Transform OpenTransform;

	public Transform CloseTransform;

	public UIButton EnterDungeonButton;

	public UIBundleTexSelector BgTextureSelector;

	public MultipleDungeonEnterCount MultipleDungeonEnterCount;

	private Listener _listener;

	private UnderPrisonData _underPrisonData;

	void NVespa.NPrefab2.InitOnCreate.OnCreate()
	{
	}

	void NVespa.NPrefab2.InitOnCreate.OnRelease()
	{
	}

	public void Update()
	{
	}

	public void Open(Listener listener, UnderPrisonData underPrisonData)
	{
		Init(listener, underPrisonData);
	}

	private void Init(Listener listener, UnderPrisonData underPrisonData)
	{
		_listener = listener;
		_underPrisonData = underPrisonData;
		SetDungeon();
		SetDungeonState();
		if (_underPrisonData != null)
		{
			MultipleDungeonEnterCount.Init(_underPrisonData.ChapterIndex);
		}
	}

	private void SetDungeonState()
	{
		if (_underPrisonData != null)
		{
			int remainEnterCountByChapterIndex = GlobalAutoBehaviour<AccountManager>.instance.UnderPrisonManager.GetRemainEnterCountByChapterIndex(_underPrisonData.ChapterIndex);
			EnableDugeonComponent(remainEnterCountByChapterIndex > 0);
		}
	}

	private void EnableDugeonComponent(bool enable)
	{
		UIHelper.EnableGameObject(OpenTransform, enable);
		UIHelper.EnableGameObject(CloseTransform, !enable);
		UIHelper.EnableButton(EnterDungeonButton, enable);
	}

	private void SetDungeon()
	{
		if (_underPrisonData != null)
		{
			UIHelper.SetLabelText(DungeonNameLabel, LocalizationManager.GetStringDirect(_underPrisonData.Name));
			if (BgTextureSelector != null)
			{
				BgTextureSelector.SelectEnum(_underPrisonData.Type);
			}
		}
	}

	public void OnClickEnterDungeon()
	{
		if (_listener != null && _underPrisonData != null)
		{
			_listener.EnterDungeon(_underPrisonData);
		}
	}
}
