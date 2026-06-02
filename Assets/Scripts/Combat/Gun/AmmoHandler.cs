
using System;
using UnityEngine;

[Serializable]
public class AmmoHandler: MonoBehaviour
{
    [SerializeField] int m_ClipSize;
    [SerializeField] float m_ReloadTime;
    [SerializeField] int m_AmmoId;

    IWeapon m_Weapon;

    int m_CurrentAmmo;
	public int CurrentAmmo => m_CurrentAmmo;
	public float ReloadTime => m_ReloadTime;
    public int ClipSize => m_ClipSize;
    public int AmmoId => m_AmmoId;

    public void Awake()
	{
		m_CurrentAmmo = m_ClipSize;
    }

    public void Initialize(IWeapon weapon)
    {
        m_Weapon = weapon;
    }

	public bool ValidReload()
	{
		return m_CurrentAmmo < m_ClipSize;
	}

	public void StartReload()
	{
		if(!ValidReload()) {
            return;
		}
        m_CurrentAmmo = 0;
	}

	public void EndReload(int ammo)
	{
        m_CurrentAmmo = ammo;
    }

	public bool ComsumeAmmo(int ammo = 1)
	{
		if(m_CurrentAmmo < ammo) {
			return false;
		}
		m_CurrentAmmo -= ammo;
		return true;
	}
}
