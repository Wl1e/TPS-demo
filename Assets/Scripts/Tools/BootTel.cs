
using Unity.Netcode;

namespace TPSDemo
{

    static public class BootTel
    {
        static public void TeleportToHub()
        {
            MapManager.Instance.EnterMap(1);
        }
    }
}
