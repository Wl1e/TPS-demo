
namespace TPSDemo
{

    public interface IFirearmSlot : ICombatSlot
    {
        public float ReloadTime { get; }

        public bool TrySwitchFirearm(int index);

        public void TryReload();
    }
}
