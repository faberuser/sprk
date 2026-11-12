using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGameAsset.NEffect;

public class BillboardUpdater : MonoBehaviour
{
	private Camera _camera;

	private CommonData.EBillboardType _billboardType;

	private Quaternion _localRotation;

	public void Open(CommonData.EBillboardType billboardType)
	{
		_billboardType = billboardType;
		_localRotation = base.transform.localRotation;
	}

	private void LateUpdate()
	{
		Camera camera = GetCamera();
		if (!(camera == null))
		{
			if (_billboardType == CommonData.EBillboardType.Billboard)
			{
				base.transform.rotation = camera.transform.rotation * _localRotation;
			}
			else if (_billboardType == CommonData.EBillboardType.VerticalBillboard)
			{
				base.transform.rotation = Math.CalcVerticalBillboard(camera) * _localRotation;
			}
		}
	}

	private Camera GetCamera()
	{
		if (_camera != null && _camera.gameObject.layer == base.gameObject.layer)
		{
			return _camera;
		}
		_camera = GlobalAutoBehaviour<GlobalCameraManager>.instance.GetCameraByLayer(base.gameObject.layer);
		return _camera;
	}
}
