using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IState
{

    void Enter();

    void ExecuteInUpdate();

    void ExecuteInFixedUpdate();

    void Exit();
}
