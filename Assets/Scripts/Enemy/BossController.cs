using System;
using System.Collections;
using Unity.Behavior;
using Unity.Netcode;
using UnityEngine;

namespace TPSDemo
{
	public class BossController: EnemyController
	{
        public void SetStandPos(Vector3 pos)
        {
            if(m_BehaviorTree.GetVariable<Vector3>("StandPos", out var standPos)) {
                standPos.Value = pos;
            }
        }
    }
}
