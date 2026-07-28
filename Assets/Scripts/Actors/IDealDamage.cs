using UnityEngine;

public readonly struct DamageInfo
{
    public readonly float Amount;
    public readonly float StunDuration;
    public readonly bool InstantKill;
    public readonly GameObject Source;
    public readonly Vector3 HitPoint;   
    public readonly Vector3 HitNormal;

    public DamageInfo(float amount, GameObject source, Vector3 inHitPoint, Vector3 inHitNormal,  bool instantKill, float stunDuration)
    {
        Amount = amount;
        Source = source;
        HitPoint = inHitPoint;
        HitNormal = inHitNormal;
        InstantKill = instantKill;
        StunDuration = stunDuration;
    }
}

public struct DamageResult
{
    public float FinalDamage;
    public bool WasBlocked;
    public bool DidStun;
    public bool WasFatal;

    public DamageResult(float finalDamage, bool wasBlocked, bool wasFatal, bool wasStun)
    {
        FinalDamage =  finalDamage;
        WasBlocked =  wasBlocked;
        WasFatal =  wasFatal;
        DidStun = wasStun;
    }
}

public interface IDealDamage
{
    void DealDamage(IDamageable toObj, Collision  collision);
}