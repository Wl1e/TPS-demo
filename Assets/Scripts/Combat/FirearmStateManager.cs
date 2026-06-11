using System;
using TPSDemo;
using Unity.Netcode;
using UnityEngine;

public class FirearmStateManager : MonoBehaviour
{
    public enum FirearmState
    {
        None,
        Equiping,
        Unequiping,
        ReadyToFire,
        Reloading
    }

    private float m_Duration = 0f;

    private FirearmState m_CurrentState = FirearmState.None;
    public FirearmState CurrentState => m_CurrentState;

    private void Update()
    {
        if(m_Duration > 0) {
            m_Duration -= Time.deltaTime;
            if(m_Duration <= 0f) {
                
                HandleStateTimeEnd();
            }
        }
    }

    private void HandleStateTimeEnd()
    {
        m_Duration = 0f;
        switch (CurrentState) {
            case FirearmState.None:
                break;
            case FirearmState.Equiping:
                m_CurrentState = FirearmState.ReadyToFire;
                break;
            case FirearmState.Unequiping:
                m_CurrentState = FirearmState.None;
                break;
            case FirearmState.Reloading:
                m_CurrentState = FirearmState.ReadyToFire;
                //m_Weapon.EndReload();
                break;
        }
    }

    public void ChangeState(FirearmState state, float duration)
    {
        m_CurrentState = state;
        if (duration > 0f) {
            m_Duration = duration;
        }
    }

}
