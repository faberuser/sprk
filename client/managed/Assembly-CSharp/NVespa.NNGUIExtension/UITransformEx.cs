using System.Collections.Generic;
using NShared;
using NVespa.NUtil;
using UnityEngine;

namespace NVespa.NNGUIExtension;

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/Transform Ex (Vespa)")]
public class UITransformEx : MonoBehaviour, IInspectorGuiDrawer
{
	[HideInInspector]
	public NVespa.NUtil.TransformData DefaultTransform = new NVespa.NUtil.TransformData();

	[HideInInspector]
	public NVespa.NUtil.TransformData IphoneXTransform = new NVespa.NUtil.TransformData();

	[HideInInspector]
	public NVespa.NUtil.TransformData customTransform;

	[HideInInspector]
	public Transform originTransform;

	[SerializeField]
	public bool enableOnEditor;

	[SerializeField]
	[HideInInspector]
	private List<DeviceStyleType> _phoneStyleTypeList = new List<DeviceStyleType>();

	private string _deviceModel = "";

	private bool _isUpdate;

	private void Awake()
	{
		if (Application.isPlaying)
		{
			_deviceModel = SystemInfo.deviceModel;
			if (!GameRoot.isValidInstance || GameRoot.Instance.DeviceModelDataContainer == null || !GameRoot.Instance.DeviceModelDataContainer.IsLoaded)
			{
				DeviceModelDataContainer.AddObserver(OnChanged);
			}
			SetTransform();
		}
	}

	private void OnDestroy()
	{
		DeviceModelDataContainer.RemoveObserver(OnChanged);
	}

	private void Update()
	{
		if (_isUpdate)
		{
			try
			{
				SetTransform();
				SetPosition();
				SetRotation();
				SetScale();
				_isUpdate = false;
			}
			catch
			{
				_isUpdate = false;
			}
		}
	}

	private void SetTransform()
	{
		if (IsSwitchTransfrom2())
		{
			customTransform = IphoneXTransform;
		}
		else
		{
			customTransform = DefaultTransform;
		}
	}

	private void CopyOriginTransform(NVespa.NUtil.TransformData custom_transform)
	{
		custom_transform.posX = base.transform.localPosition.x;
		custom_transform.posY = base.transform.localPosition.y;
		custom_transform.posZ = base.transform.localPosition.z;
		custom_transform.rotX = base.transform.localRotation.x;
		custom_transform.rotY = base.transform.localRotation.y;
		custom_transform.rotZ = base.transform.localRotation.z;
		custom_transform.scaleX = base.transform.localScale.x;
		custom_transform.scaleY = base.transform.localScale.y;
		custom_transform.scaleZ = base.transform.localScale.z;
	}

	private void RevertOriginTransform()
	{
		if (originTransform != null)
		{
			base.transform.localPosition = originTransform.localPosition;
			base.transform.localRotation = originTransform.localRotation;
			base.transform.localScale = originTransform.localScale;
		}
	}

	private void OnEnable()
	{
		if (Application.isPlaying && customTransform != null)
		{
			SetPosition();
			SetRotation();
			SetScale();
		}
	}

	private void SetPosition()
	{
		if (customTransform.posX != 0f || customTransform.posY != 0f || customTransform.posZ != 0f)
		{
			float posX = customTransform.posX;
			float posY = customTransform.posY;
			float posZ = customTransform.posZ;
			base.transform.localPosition = new Vector3(posX, posY, posZ);
		}
	}

	private void SetRotation()
	{
		if (customTransform.rotX != 0f || customTransform.rotY != 0f || customTransform.rotZ != 0f)
		{
			float rotX = customTransform.rotX;
			float rotY = customTransform.rotY;
			float rotZ = customTransform.rotZ;
			base.transform.localRotation = Quaternion.Euler(rotX, rotY, rotZ);
		}
	}

	private void SetScale()
	{
		if (customTransform.scaleX != 0f || customTransform.scaleY != 0f || customTransform.scaleZ != 0f)
		{
			float scaleX = customTransform.scaleX;
			float scaleY = customTransform.scaleY;
			float scaleZ = customTransform.scaleZ;
			base.transform.localScale = new Vector3(scaleX, scaleY, scaleZ);
		}
	}

	private bool IsSwitchTransfrom()
	{
		if (!GameRoot.isValidInstance || GameRoot.Instance.DeviceModelDataContainer == null)
		{
			return false;
		}
		DeviceModelData data = GameRoot.Instance.DeviceModelDataContainer.GetData(_deviceModel);
		if (data == null)
		{
			return false;
		}
		if (!_phoneStyleTypeList.Contains(data.DeviceStyle))
		{
			return false;
		}
		if (__Extension_Method_ICollection__.COUNT(data.ScreenRate) >= 2)
		{
			float num = data.ScreenRate[0];
			float num2 = data.ScreenRate[1];
			float num3 = 1f;
			float num4 = 1f;
			if (SingletonAuto<ResolutionInfo>.isValidInstance)
			{
				num3 = SingletonAuto<ResolutionInfo>.instance.OriginalWidth;
				num4 = SingletonAuto<ResolutionInfo>.instance.OriginalHeight;
			}
			if (num / num2 != num3 / num4)
			{
				return false;
			}
		}
		return true;
	}

	private bool IsSwitchTransfrom2()
	{
		return false;
	}

	private bool HasNotchStyle()
	{
		if (SingletonAuto<ResolutionInfo>.isValidInstance && (float)SingletonAuto<ResolutionInfo>.instance.RealScreenWidth != Screen.safeArea.width)
		{
			return true;
		}
		return false;
	}

	private void OnChanged(bool changedInfo)
	{
		if (changedInfo)
		{
			_isUpdate = true;
		}
	}
}
