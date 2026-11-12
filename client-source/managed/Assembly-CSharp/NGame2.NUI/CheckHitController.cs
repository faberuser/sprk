using UnityEngine;

namespace NGame2.NUI;

public class CheckHitController : MonoBehaviour
{
	private Transform _transform;

	private Vector3? _direction;

	private Vector3? _targetPosition;

	private Vector3? _radiusSize;

	private bool _isHit = true;

	public bool CheckHit => _isHit;

	public void Init(Transform transform)
	{
		_transform = transform;
		_isHit = true;
		if (_transform != null)
		{
			SetRadiusSize(_transform.lossyScale);
		}
	}

	public void SetRadiusSize(Vector3 radiusSize)
	{
		_radiusSize = radiusSize;
	}

	public void SetInfo(Vector3? direction, Vector3? targetPosition)
	{
		_direction = direction;
		_targetPosition = targetPosition;
	}

	private void OnDrawGizmos()
	{
		_isHit = false;
		if (_transform == null || !_radiusSize.HasValue)
		{
			return;
		}
		Vector3 position = _transform.position;
		position.y += 0.2f;
		float radius = _radiusSize.Value.x / 2f;
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(position, radius);
		if (_direction.HasValue && _targetPosition.HasValue)
		{
			float num = Vector3.Distance(_transform.position, _targetPosition.Value);
			Vector3 value = _direction.Value;
			_isHit = Physics.Raycast(position, value, out var hitInfo, num);
			if (_isHit)
			{
				Gizmos.DrawRay(position, value * hitInfo.distance);
			}
			else
			{
				Gizmos.DrawRay(position, value * num);
			}
		}
	}
}
