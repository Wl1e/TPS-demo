using UnityEngine;
using UnityEngine.AddressableAssets;

namespace TPSDemo
{
    [CreateAssetMenu(fileName = "WeaponData", menuName = "Config/Weapon/WeaponData")]
    public class WeaponItemData : ItemData
    {
        [Header("Mechanism")]
        [Tooltip("射速")]
        public float FireInternal;

        [Header("Shooting")]
        [Tooltip("子弹预制体")]
        public BulletController BulletPrefab;
        [Tooltip("子弹飞行速度")]
        public float BulletSpeed;
        [Tooltip("子弹碰撞层级")]
        public LayerMask HitLayerMask = -1;
        [Tooltip("单发伤害")]
        public float Damage;
        [Tooltip("每次射击产生的子弹数量，仅 Shotgun 模式")]
        public int BulletsPerShot = 1;
        [Tooltip("霰弹扩散范围")]
        public float SpreadScale = 0.1f;

        [Header("Ammo")]
        [Tooltip("初始弹匣容量")]
        public int DefaultClipSize;
        [Tooltip("换弹时间（秒）")]
        public float ReloadTime;
        [Tooltip("使用的子弹物品ID")]
        public int AmmoId;

        //[Tooltip("手持位置偏移")]
        //public Vector3 HandOffset;
        //[Tooltip("手持旋转")]
        //public Vector3 HandRotation;
        //[Tooltip("背持位置偏移")]
        //public Vector3 BackOffset;
        //[Tooltip("脊柱偏移")]
        //public Vector3 SpineOffset;

        [Header("Recoil")]
        [Tooltip("后坐力大小")]
        public float RecoilForce;
        [Tooltip("后坐力频率")]
        public float RecoilFrequency;
        [Tooltip("后坐力恢复速度")]
        public float RecoilReturnSpeed;

        [Header("Effects & Audio")]
        [Tooltip("枪口特效预制体")]
        public AssetReference MuzzleFlashPrefab;
        [Tooltip("枪口特效持续时间（秒）")]
        public float MuzzleFlashTime = 0.09f;
        [Tooltip("弹壳特效预制体")]
        public AssetReference ShellPrefab;
        [Tooltip("抛弹壳力度")]
        public float ShellEjectForce = 1f;
        [Tooltip("射击音效")]
        public AudioClip ShootSfx;
        [Tooltip("换弹音效")]
        public AudioClip ReloadingSfx;

        [Tooltip("准星")]
        public CrosshairData Crosshair;
        
    }
}
