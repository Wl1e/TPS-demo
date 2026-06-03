using UnityEngine;
using System.Collections;
using TMPro;

namespace TPSDemo
{
	public class DebugLayer: MonoBehaviour
	{
        public PlayerController Player;
        public TextMeshProUGUI StatusText;

        public bool ShowState = true;
        public bool ShowVelocity = true;
        public bool ShowClimbState = true;
        public bool ShowCombatState = true;


        private void LateUpdate()
        {
            string text = "";
            if (ShowState) {
                text += $"State: {Player.RuntimeData.State}\n";
            }
            if (ShowVelocity) {
                text += $"Velocity: {Player.Movement.Velocity}\n";
                text += $"IsGrounded: {Player.Movement.IsGrounded}\n";
            }
            if(ShowClimbState) {
                text += $"CanClimb: {Player.ClimbController.CanClimb}\nCanLedge: {Player.ClimbController.CanLedge}\n";
            }
            if (ShowCombatState) {
                text += $"CombatState: {Player.CombatController.CurrentActiveSlot.ToString()}";
            }
            StatusText.text = text;
        }
    }
}
