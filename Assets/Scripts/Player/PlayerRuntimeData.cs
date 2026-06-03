
using System;
using UnityEngine;

namespace TPSDemo
{
    public class PlayerRuntimeData
    {
        public bool IsAiming;
        public PlayerMovementState State;
        public Transform CameraRoot;

        public Combat.Slot ActiveSlot;

        public Action<bool> OnAim;
        public Action<PlayerMovementState> OnStateChanged;

        // animator
        public AnimatorParameter AniParameter = new AnimatorParameter();

        public Define.ViewPerspective AimType;
    }
}
