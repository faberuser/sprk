using UnityEngine;

namespace NGame2.NUI.NAtlasAssignComponent;

public interface IAssinger
{
	void SetTarget(Component targetComponent);

	void Save();

	void Assign();
}
