namespace Ulys.Runtime.Utilities
{

using UnityEngine;

public class StateMachine
{
	private State _currentState;
	
	public State CurrentState =>  _currentState;
	
	public float StateStartTime { get; private set; }
	public float StateStartFixedTime { get; private set; }
	
	public float StateElapsedTime { get; private set; }
	public float StateFixedElapsedTime { get; private set; }
	
	public void Update()
	{
		_currentState?.OnUpdate();
		StateElapsedTime = Time.time - StateStartTime;
	}

	public void FixedUpdate()
	{
		_currentState?.OnFixedUpdate();
		StateFixedElapsedTime = Time.fixedTime - StateStartFixedTime;
	}

	public void LateUpdate()
	{
		_currentState?.OnLateUpdate();
	}

	public void ChangeState(State newState)
	{
		_currentState?.OnExit();
		_currentState = newState;
		StateStartTime = Time.time;
		StateStartFixedTime = Time.fixedTime;
		StateElapsedTime = 0f;
		StateFixedElapsedTime = 0f;
		_currentState?.OnEnter();
	}
	
	public abstract class State
	{
		protected StateMachine StateMachine;

		protected State(StateMachine stateMachine)
		{
			StateMachine = stateMachine;
		}
		
		public virtual void OnEnter() {}
		public virtual void OnUpdate() {}
		public virtual void OnFixedUpdate() {}
		public virtual void OnLateUpdate() {}
		public virtual void OnExit() {}
	}
}



}
