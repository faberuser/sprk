using System;
using UnityEngine;

namespace NGame2.NUI.NRender3D.NCinema;

public interface IInteractionDataProvider
{
	Vector3 BoxColliderCenter { get; }

	Vector3 BoxColliderSize { get; }

	Action OnClick { get; }
}
