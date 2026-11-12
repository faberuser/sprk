using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NRender3D.NCinema;

public class InteractionComponent : LinkFactoryComponent, LinkFactoryComponent.IReleaseEvent
{
	public const string PrefabPath = "Prefab2/Component/Interaction/InteractionComponent";

	public static PrefabAuto<InteractionComponent> PrefabAuto = new PrefabAuto<InteractionComponent>("Prefab2/Component/Interaction/InteractionComponent");

	public BoxCollider BoxCollider;

	private IInteractionDataProvider _provider;

	private Transform _trans;

	public void OnRelease()
	{
		Close();
	}

	public void Set(IInteractionDataProvider provider)
	{
		_provider = provider;
		_trans = base.transform;
		if (_provider != null)
		{
			Vespa_Unity_Transform_Extension_Method.ResetLocalTM(_trans);
			SetBoxCollider(_provider.BoxColliderCenter, _provider.BoxColliderSize, enable: true);
			AddReleaseEvent(this);
		}
	}

	private void SetBoxCollider(Vector3 center, Vector3 size, bool enable)
	{
		BoxCollider.center = center;
		BoxCollider.size = size;
		BoxCollider.enabled = enable;
	}

	private void Close()
	{
		SetBoxCollider(Vector3.zero, Vector3.zero, enable: false);
		_provider = null;
	}

	public void Click(RaycastHit hit)
	{
		if (CheckRaycastHit(hit))
		{
			Click();
		}
	}

	public void Click()
	{
		if (_provider != null && VespaInput.GetMouseButtonDown(0))
		{
			Vespa_System_Action_Extension_Method.fire(_provider.OnClick);
		}
	}

	public bool CheckRaycastHit(RaycastHit hit)
	{
		if (hit.transform == null)
		{
			return false;
		}
		if (!hit.transform.Equals(_trans))
		{
			return false;
		}
		return true;
	}
}
