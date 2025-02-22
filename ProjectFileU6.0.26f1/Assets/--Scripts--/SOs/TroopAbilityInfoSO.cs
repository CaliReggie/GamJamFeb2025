using UnityEngine;


public struct AbilityData
{
    public bool HasAbility;
    
    public float TimeReady;
    
    public float Range;
    
    public bool CanHeal;
    
    public AbilityData(bool hasAbility,float timeReady, float range, bool canHeal)
    {
        HasAbility = hasAbility;
        
        TimeReady = timeReady;
        
        Range = range;
        
        CanHeal = canHeal;
    }
    
    public AbilityData(bool hasAbility, float timeReady)
    {
        HasAbility = hasAbility;
        
        TimeReady = timeReady;
        
        Range = 0f;
        
        CanHeal = false;
    }
}

[CreateAssetMenu(fileName = "TroopAbilityInfoSO", menuName = "ScriptableObjects/TroopAbilityInfoSO")]
public class TroopAbilityInfoSO : ScriptableObject
{
    [Header("Detection Settings")]
    
    [Tooltip("At what extra distance from the furthest attack does this AI become alert?")]
    public float alertDistance = 25f;

    [Header("Primary")]
    
    public bool hasPrimaryAttack;
    
    public AnimationClip primaryClip;
    
    public float primaryClipSpeed = 1f;
    
    public EHealthEffectorType primaryEffectorType = EHealthEffectorType.Damage;
    
    public float primaryRange = 1f;
    
    public float afterPrimaryCooldown = 1f;
    
    [Header("Alternate")]
    
    public bool hasAlternateAttack;
    
    public AnimationClip alternateClip;
    
    public float alternateClipSpeed = 1f;
    
    public EHealthEffectorType alternateEffectorType = EHealthEffectorType.Damage;
    
    public float alternateRange = 1f;
    
    public float afterAlternateCooldown = 1f;
    
    [Header("Secondary")]
    
    public bool hasSecondaryAttack;
    
    public AnimationClip secondaryClip;
    
    public float secondaryClipSpeed = 1f;
    
    public EHealthEffectorType secondaryEffectorType = EHealthEffectorType.Damage;
    
    public float secondaryRange = 1f;
    
    public float afterSecondaryCooldown = 1f;
    
    public float AttackDuration { get { return ( primaryClip.length / primaryClipSpeed); } }
    
    public float AlternateDuration { get { return (alternateClip.length / alternateClipSpeed); } }
    
    public float SecondaryDuration { get { return (secondaryClip.length / secondaryClipSpeed); } }
    
    
    public float MinEngageDistance 
    {
        get { return Mathf.Min(primaryRange, alternateRange, secondaryRange); }
    }
    
    public float MaxEngageDistance
    {
        get { return Mathf.Max(primaryRange, alternateRange, secondaryRange); }
    }
    
    public bool HasHeals
    {
        get { return (hasPrimaryAttack && primaryEffectorType == EHealthEffectorType.Heal) 
                     || (hasAlternateAttack && alternateEffectorType == EHealthEffectorType.Heal) 
                     || (hasSecondaryAttack && secondaryEffectorType == EHealthEffectorType.Heal); }
    }

    public bool HasDamage
    {
        get { return (hasPrimaryAttack && primaryEffectorType == EHealthEffectorType.Damage) 
                     || (hasAlternateAttack && alternateEffectorType == EHealthEffectorType.Damage) 
                     || (hasSecondaryAttack && secondaryEffectorType == EHealthEffectorType.Damage); }
    }
}
