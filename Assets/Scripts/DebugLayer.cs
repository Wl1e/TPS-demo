using UnityEngine;
using System.Collections;
using TMPro;

namespace Assets.Scripts
{
	public class DebugLayer: MonoBehaviour
	{
        public PlayerController Player;
        public TextMeshProUGUI StatusText;

        public bool ShowState = true;
        public bool ShowVelocity = true;
        public bool ShowClimbState = true;


        private void LateUpdate()
        {
            string text = "";
            if (ShowState) {
                text += $"State: {Player.RuntimeData.State}\n";
            }
            if (ShowVelocity) {
                text += $"Velocity: {Player.Movement.Velocity}\n";
            }
            text += $"IsGrounded: {Player.Movement.IsGrounded}\n";
            if(ShowClimbState) {
                text += $"CanClimb: {Player.ClimbController.CanClimb}\nCanLedge: {Player.ClimbController.CanLedge}\n";
            }
            text += $"WantSprint: {Player.StateMachine.WantSprint}";
            StatusText.text = text;
        }
    }
}
