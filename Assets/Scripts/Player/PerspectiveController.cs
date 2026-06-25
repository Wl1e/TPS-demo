using UnityEngine;

using ViewPerspective = Define.ViewPerspective;

namespace Assets.Scripts.Player
{
    public class PerspectiveController : MonoBehaviour
    {

        public Define.ViewPerspective Perspective;

        // TPP
        [SerializeField] GameObject Model;

        // FPP
        [SerializeField] GameObject ArmModel;
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {

        }

        void UpdateViewPerspective(ViewPerspective perspective)
        {
            Perspective = perspective;
            if(Perspective == ViewPerspective.FPP) {
                OnFPP();
            } else {
                OnTPP();
            }
        }

        void OnFPP()
        {
            Model.SetActive(false);
            ArmModel.SetActive(true);
        }

        void OnTPP()
        {
            Model.SetActive(true);
            ArmModel.SetActive(false);
        }
    }
}
