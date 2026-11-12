using NGame2.NAccount;
using NGame2.NUI.NHelper.NPet;
using NGame2.NUI.NPet;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NPet;

public class PetHouseObjectNameComponent : LinkFactoryComponent
{
	public interface IListener
	{
		void Touch();
	}

	public class PetHouseObjectNameInfo
	{
		public EPetHouseObjectType EPetHouseObjectType { get; set; }

		public Vector3 LocalPositionOffset { get; set; }

		public float Scale { get; set; }

		public IListener IListener { get; set; }

		public string LocalizationKey { get; set; }
	}

	public static string PrefabPath = "Prefab2/Component/Pet/PetHouseObjectNameComponent";

	public static PrefabAuto<PetHouseObjectNameComponent> PrefabAuto = new PrefabAuto<PetHouseObjectNameComponent>(PrefabPath);

	public UILabel NameLabel;

	public UILabel ObjectStateLabel;

	public UISprite NoticeSprite;

	public UIScaleOn3D ScaleOn3D;

	private PetHouseObjectNameInfo _info;

	private long _remainPetFoodCount;

	private long _maxPetFoodCount;

	private bool CheckEnableObjectState
	{
		get
		{
			if (_info == null)
			{
				return false;
			}
			if (_info.EPetHouseObjectType != EPetHouseObjectType.FeedNormal && _info.EPetHouseObjectType != EPetHouseObjectType.FeedEmpty)
			{
				return _info.EPetHouseObjectType == EPetHouseObjectType.FeedMax;
			}
			return true;
		}
	}

	public void Open(PetHouseObjectNameInfo info)
	{
		Init(info);
	}

	private void Init(PetHouseObjectNameInfo info)
	{
		_info = info;
		SetDirection();
		SetPosition();
		SetScale();
		SetNameLabel();
		EnableObjectStateLabel();
		SetPetFoodInfo();
		RefreshNotice();
	}

	private void SetDirection()
	{
		base.transform.rotation = Quaternion.Euler(20f, -90f, 0f);
	}

	private void SetPosition()
	{
		Vector3 vector = Vector3.zero;
		if (_info != null)
		{
			vector = _info.LocalPositionOffset;
		}
		base.transform.localPosition = Vector3.zero;
		base.transform.localPosition += vector;
	}

	private void SetScale()
	{
		if (_info != null && !(ScaleOn3D == null))
		{
			ScaleOn3D.MultiplyScale = _info.Scale;
			ScaleOn3D.enabled = true;
		}
	}

	private void SetNameLabel()
	{
		if (_info != null)
		{
			UIHelper.SetLabelText(NameLabel, LocalizationManager.GetStringDirect(_info.LocalizationKey));
		}
	}

	private void EnableObjectStateLabel()
	{
		UIHelper.EnableGameObject(ObjectStateLabel, CheckEnableObjectState);
	}

	public void SetPetFoodInfo()
	{
		_remainPetFoodCount = GlobalAutoBehaviour<AccountManager>.instance.PetManager.RemainPetFoodCount;
		_maxPetFoodCount = GlobalAutoBehaviour<AccountManager>.instance.PetManager.MaxPetFoodCount;
	}

	public void RefreshNotice()
	{
		if (_info != null && _info.EPetHouseObjectType == EPetHouseObjectType.Management)
		{
			bool enable = GlobalAutoBehaviour<AccountManager>.instance.PetManager.GetAddedPetCount() > 0;
			UIHelper.EnableGameObject(NoticeSprite, enable);
		}
	}

	public void UpdateRemainTimeLabel()
	{
		if (_info != null && CheckEnableObjectState)
		{
			switch (_info.EPetHouseObjectType)
			{
			case EPetHouseObjectType.FeedNormal:
			case EPetHouseObjectType.FeedEmpty:
				_remainPetFoodCount = GlobalAutoBehaviour<AccountManager>.instance.PetManager.RemainPetFoodCount;
				PetHelper.UpdatePetFoodState(ObjectStateLabel, _remainPetFoodCount, _maxPetFoodCount);
				break;
			case EPetHouseObjectType.FeedMax:
				UIHelper.SetLabelText(ObjectStateLabel, LocalizationManager.GetStringDirect("PET_BUTTON_FEED_MAX"));
				break;
			}
		}
	}

	public void OnClick()
	{
		if (_info != null && _info.IListener != null)
		{
			_info.IListener.Touch();
		}
	}
}
