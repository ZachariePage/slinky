using UnityEngine;

public class StateMachine
{
     public State CurrentEnemyState;
    
     public void Init(State startingState)
     {
         CurrentEnemyState = startingState;
         startingState.EnterState();
     }
    
     public void ChangeState(State newState)
     {
         //DebugPrintStateName("entering");
         CurrentEnemyState.ExitState();
         CurrentEnemyState = newState;
         CurrentEnemyState.EnterState();
         //DebugPrintStateName("exiting");
     }
    
     public void DebugPrintStateName(string message)
     {
         Debug.Log($"{message} {CurrentEnemyState}");
     }
}
