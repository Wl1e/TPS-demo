
public interface IFirearmSlot: ICombatSlot
{
    public float ReloadTime { get; }

    public void TrySwitchFirearm(int index);

    public void TryChangeFirearmIndex(int value);

    public void TryReload();
}
