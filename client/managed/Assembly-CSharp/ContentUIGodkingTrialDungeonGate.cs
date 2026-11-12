using System.Collections;
using UnityEngine;

public class ContentUIGodkingTrialDungeonGate : MonoBehaviour
{
	public enum State
	{
		None,
		Opened,
		Closed,
		Locked,
		Opening,
		Closing,
		Locking,
		Unlocking
	}

	private const string ANIM_TRIGGER_OPENED = "Opened";

	private const string ANIM_TRIGGER_CLOSED = "Closed";

	private const string ANIM_TRIGGER_LOCKED = "Locked";

	private const string ANIM_TRIGGER_OPENING = "Opening";

	private const string ANIM_TRIGGER_CLOSING = "Closing";

	private const string ANIM_TRIGGER_LOCKING = "Locking";

	private const string ANIM_TRIGGER_UNLOCKING = "Unlocking";

	private const float CLOSING_LOCKING_DELAY_TIME = 2f;

	public Animator AnimStateController;

	private State _prevState;

	private State _curState;

	public State CurState => _curState;

	public void OpenGate()
	{
		ChangeState(State.Opening);
	}

	public void CloseGate()
	{
		ChangeState(State.Closing);
	}

	public void LockGate()
	{
		ChangeState(State.Locking);
	}

	public void UnlockGate()
	{
		ChangeState(State.Unlocking);
	}

	public void SetOpened()
	{
		ChangeState(State.Opened);
	}

	public void SetClosed()
	{
		ChangeState(State.Closed);
	}

	public void SetLocked()
	{
		ChangeState(State.Locked);
	}

	public void SetClosingLock()
	{
		VespaCoroutine.Start(coClosingLock());
	}

	public void OnChangeAniState(State state)
	{
		_prevState = _curState;
		_curState = state;
	}

	private void Reset()
	{
		ChangeState(State.Closed);
	}

	private void SetAnimTrigger(string trigger)
	{
		if (!(AnimStateController == null))
		{
			AnimStateController.SetTrigger(trigger);
		}
	}

	private void ChangeState(State state)
	{
		_prevState = _curState;
		_curState = state;
		switch (state)
		{
		case State.Opened:
			SetAnimTrigger("Opened");
			break;
		case State.Closed:
			SetAnimTrigger("Closed");
			break;
		case State.Locked:
			SetAnimTrigger("Locked");
			break;
		case State.Opening:
			SetAnimTrigger("Opening");
			break;
		case State.Closing:
			SetAnimTrigger("Closing");
			break;
		case State.Locking:
			SetAnimTrigger("Locking");
			break;
		case State.Unlocking:
			SetAnimTrigger("Unlocking");
			break;
		}
	}

	private IEnumerator coClosingLock()
	{
		ChangeState(State.Closing);
		yield return new WaitForSeconds(2f);
		ChangeState(State.Locking);
	}
}
