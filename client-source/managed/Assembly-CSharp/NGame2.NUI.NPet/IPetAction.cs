using UnityEngine;

namespace NGame2.NUI.NPet;

public interface IPetAction
{
	EPetActionType EPetActionType { get; }

	void Idle(float seconds);

	void Walk(Vector3 targetPosition);

	void Run(Vector3 targetPosition);

	void Walk();

	void Run();

	void HappyForInteraction();

	void Happy();

	void Sleep();

	void Slide(Vector3 startPosition);

	void Special();

	void Stop();
}
