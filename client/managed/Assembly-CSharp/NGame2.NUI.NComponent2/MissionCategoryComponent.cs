using NGame2.NScene;
using NGame2.NUI.NManager.Mission;
using NGame2.NUI.NWindow;
using NNguiExtension;
using NShared;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

[RequireComponent(typeof(UISprite))]
[RequireComponent(typeof(UIToggle))]
[RequireComponent(typeof(UIEventTrigger))]
public sealed class MissionCategoryComponent : MonoBehaviour
{
	public enum ECategoryState
	{
		Normal,
		Disable,
		Selected,
		Lock
	}

	public enum EStateIconType
	{
		None,
		Reward,
		New
	}

	public interface IListner
	{
		void OnClick(MissionCategoryComponent comp);
	}

	public const string PrefabPath = "Prefab2/Component/MissionCategory/MissionCategoryComponent";

	public const string PrefabPath2 = "Prefab2/Component/MissionCategory/MissionListSubCategoryComponent";

	public static PrefabAuto<MissionCategoryComponent> PrefabAuto = new PrefabAuto<MissionCategoryComponent>("Prefab2/Component/MissionCategory/MissionCategoryComponent");

	public static PrefabAuto<MissionCategoryComponent> PrefabAuto2 = new PrefabAuto<MissionCategoryComponent>("Prefab2/Component/MissionCategory/MissionListSubCategoryComponent");

	[Header("[CommonMissionCategory]")]
	public UILabel TitleLabel;

	public GameObject SelectedObject;

	public GameObject LockObject;

	[Header("[MissionCategoryComponent]")]
	public UISpriteSelector SpriteSelectorNotiImage;

	public bool isSubCategory;

	public UILabel LockLabel;

	private ECategoryState eCategoryState;

	private IListner _listner;

	private bool DestroyGameObj;

	private UIToggle _toggle;

	private int Index;

	public MissionCategoryData MissionCategoryData { get; set; }

	public void OpenComponent(int index, MissionCategoryData data, IListner listener)
	{
		Index = index;
		_listner = listener;
		MissionCategoryData = data;
		InitToggle();
		SetDrawInfos();
	}

	private void SetState(ECategoryState state)
	{
		if (eCategoryState != state)
		{
			eCategoryState = state;
			DoState();
		}
	}

	private void DoState()
	{
		switch (eCategoryState)
		{
		case ECategoryState.Normal:
			NormalState();
			break;
		case ECategoryState.Lock:
			LockedState();
			break;
		case ECategoryState.Disable:
		case ECategoryState.Selected:
			break;
		}
	}

	public void Refresh()
	{
		SetDrawInfos();
	}

	private void OnDestroy()
	{
		DestroyGameObj = true;
	}

	public void ToggleState(bool state)
	{
		_toggle = GetComponent<UIToggle>();
		if (!(_toggle == null))
		{
			_toggle.Set(state);
		}
	}

	private void InitToggle()
	{
		_toggle = GetComponent<UIToggle>();
		if (!(_toggle == null))
		{
			_toggle.startsActive = false;
			int num = _toggle.group;
			if (Index == 0)
			{
				num++;
			}
			_toggle.group = ((Index == 0) ? num : (num + 1));
		}
	}

	private void SetDrawInfos()
	{
		if (MissionCategoryData == null)
		{
			return;
		}
		if (isSubCategory)
		{
			UIHelper.SetLabelText(TitleLabel, LocalizationManager.GetStringDirect(MissionCategoryData.SubCategoryLocalKey));
			UIHelper.SetLabelText(LockLabel, LocalizationManager.GetStringDirect(MissionCategoryData.SubCategoryLocalKey));
			SetState(ECategoryState.Normal);
			SetIsNew(isSubCategory);
			return;
		}
		UIHelper.SetLabelText(TitleLabel, LocalizationManager.GetStringDirect(MissionCategoryData.MainCategoryLocalKey));
		UIHelper.SetLabelText(LockLabel, LocalizationManager.GetStringDirect(MissionCategoryData.MainCategoryLocalKey));
		if (MissionManagement.IsOpenMainCategory(MissionCategoryData.MainCategoryIndex))
		{
			SetState(ECategoryState.Normal);
			SetIsNew();
		}
		else
		{
			SetState(ECategoryState.Lock);
		}
	}

	private void SetIsNew(bool SubCategory = false)
	{
		if (MissionCategoryData == null || LoadingManager.ApplicationQuit || DestroyGameObj || base.gameObject == null)
		{
			return;
		}
		if (!SubCategory && MissionManagement.IsNewMainCategory(MissionCategoryData.MainCategoryIndex))
		{
			EnableStateIcon(isEnable: true);
			SetNotiImage(EStateIconType.New, isGrayScale: false);
			return;
		}
		bool flag = false;
		flag = (SubCategory ? (flag | MissionManagement.IsMissionSubCategoryRewardCheck(MissionCategoryData.MainCategoryIndex, MissionCategoryData.SubCategoryIndex)) : (flag | MissionManagement.IsMissionCategoryRewardCheck(MissionCategoryData.MainCategoryIndex)));
		EnableStateIcon(flag);
		if (flag)
		{
			SetNotiImage(EStateIconType.Reward, isGrayScale: false);
		}
	}

	private void LockedState()
	{
		UIHelper.EnableGameObject(LockObject, enable: true);
		UIHelper.EnableGameObject(SelectedObject, enable: false);
	}

	private void NormalState()
	{
		UIHelper.EnableGameObject(LockObject, enable: false);
		UIHelper.EnableGameObject(SelectedObject, enable: false);
	}

	private void SetNotiImage(EStateIconType type, bool isGrayScale)
	{
		if (SpriteSelectorNotiImage != null)
		{
			SpriteSelectorNotiImage.SelectEnum(type);
			SpriteSelectorNotiImage.GetTargetSprite().isGrayscale = isGrayScale;
		}
	}

	private void EnableStateIcon(bool isEnable)
	{
		if (SpriteSelectorNotiImage != null)
		{
			UIHelper.EnableGameObject(SpriteSelectorNotiImage, isEnable);
		}
	}

	public void OnClick()
	{
		if (eCategoryState == ECategoryState.Lock)
		{
			CenterMessage.Open(LocalizationManager.GetStringDirect(MissionCategoryData.OpenConditionLocalKey));
			return;
		}
		if (isSubCategory)
		{
			MissionManagement.CheckNewMark(MissionCategoryData.MainCategoryIndex, MissionCategoryData.SubCategoryIndex);
		}
		else if (GameRoot.Instance.MissionCategoryDataContainer.GetSubCategoryCount(MissionCategoryData.MainCategoryIndex) <= 1)
		{
			MissionManagement.CheckNewMark(MissionCategoryData.MainCategoryIndex);
		}
		if (_listner != null)
		{
			_listner.OnClick(this);
		}
	}
}
