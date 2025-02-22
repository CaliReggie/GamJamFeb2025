using System;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;
using Random = UnityEngine.Random;

public enum EAIBrainState
{
    Idle,
    Alert,
    Engaging,
    Attacking
}

public enum EAIBodyState
{
    Idle,
    Moving,
    MovingFast,
    Falling,
    Aiming,
    UsingPrimary,
    UsingAlternate,
    UsingSecondary
}
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AttachForAnimEvents))]
[RequireComponent(typeof(ProjectileThrower))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Health))]
public class AIController : MonoBehaviour
{
    #region Variables
    
    [Header("Abilities Info - Set In Inspector")]
    
    [SerializeField]
    private TroopAbilityInfoSO troopAbilityInfoSo;
    
    [Header("Detection Settings")]
    
    [SerializeField]
    private LayerMask layersToDetect = 1;
    
    [Header("Target Change Settings")]
    
    [Tooltip(" After making a target decision, if this is selected the troop will be able to break off periodically" +
             "if a better target presents itself considering troop preferences.")]
    [SerializeField]
    private bool canChangeTargetAfterTargeting = true;
    
    [Range(2.5f,10)]
    [SerializeField]
    private float checkChangeTargetFrequency = 2.5f;
    
    [Header("Target Preference Settings")]
    
    [Tooltip("When deciding enemies to attack, along with other factors like team, distance, heal ability, etc.," +
             "if this is selected, the troop will have a considerably higher preference for a defense position." +
             "Currently, no effect unless team is enemy AI.")]
    [SerializeField]
    private bool prefersDefensePositions;
    
    [Tooltip("If checked, troop is guaranteed to choose priority targets when in range.")]
    [SerializeField]
    private bool guaranteePreferredTargeting;
    
    [Header("Movement Settings")]
    
    [SerializeField]
    private float moveSpeed = 6f;
    
    [SerializeField]
    private float lookSpeed = 15f;
    
    [Header("Ranged Targeting Settings")]
    
    [SerializeField]
    private bool prefersToUseRangedAbilities;
    
    [Tooltip("How spread around the target should ranged attack positions be?")]
    [SerializeField]
    [Range(0,180)]
    private float rangedEngagePositionArc = 35f;
    
    [Tooltip("How much random distance in any direction is added to the ranged attack position")]
    [SerializeField]
    private float rangedAttackPosVariance = 3f;
    
    [Tooltip("How close to the target ranged attack location will this AI attack? Recommended to increase this along" +
             " with variance")]
    [SerializeField]
    private float rangedEngagePosLeniency = 1.5f;
    
    [Header("Melee Targeting Settings")]
    
    [Tooltip("Range to be considered melee. Within this range, the AI will set it's target directly on the target." +
             "Not to be confused with the range of the attack, which is still determined by the attack info.")]
    [SerializeField]
    private float rangeToBeConsideredMelee = 3f;
    
    [Tooltip("How close to the target melee range will this AI attack? Melee range is set in AttackInfoSO")]
    [SerializeField]
    private float meleeEngagePosLeniency = 0.75f;
    
    
    //Dynamic
    
    //State
    
    private EAIBodyState _eCurrentBodyState = EAIBodyState.Idle;
    
    private EAIBrainState _eCurrentBrainState = EAIBrainState.Idle;
    
    private EAIBodyState _eTargetBodyState = EAIBodyState.Idle;
    
    private bool _canChangeBodyState = true;
    
    /*//Info
    private TroopParentInfo troopParentInfo;*/
    
    //attack info
    private AbilityData[] _allAbilitiesData;

    private int _currentAttackIndex;

    //Agent
    private NavMeshAgent _navMeshAgent;
    
    //Animation
    private Animator _anim;
    
    private float _primaryDuration;

    private float _alternateDuration;
    
    private float _secondaryDuration;
    
    private float _primaryCooldown;
    
    private float _alternateCooldown;
    
    private float _secondaryCooldown;
    
    //Timing
    private float _brainCanThinkTime;
    
    private float _canReTargetTime;
    
    private float _currentAttackTimer;
    
    //ints for hashing _animator states
    
    //bools
    private int _idleHash;
    private int _movingHash;
    // private int _movingFastHash;
    private int _fallingHash;
    private int _aimingHash;
    
    //triggers
    // private int _jumpHash;
    // private int _dashHash;
    private int _primaryHash;
    private int _alternateHash;
    private int _secondaryHash;
    
    //Health
    private Health _health;
    
    //Targeting

    private Health _currentTargetHealth;
    
    private Vector2 _engageMinMax;
    
    private float _alertDistance;
    
    private ETeam[] relevantTeams;
    
    private ETeam[] priorityTeams;
    
    private bool _canHeal;
    
    private bool _canDamage;
    
    private float _currentTargetScore;
    
    #endregion
    
    #region AwakeAndStart
    
    void Awake()
    {
        if (troopAbilityInfoSo == null)
        {
            Debug.LogError("No AITroopInfoSO assigned to " + gameObject.name);
            
            Destroy(gameObject);
            
            return;
        }
        
        //components
        _navMeshAgent = GetComponent<NavMeshAgent>();
        
        _navMeshAgent.speed = moveSpeed;
        
        _anim = GetComponent<Animator>();
        
        _health = GetComponent<Health>();
        
        SetTroopAbilities(troopAbilityInfoSo);
        
        _canChangeBodyState = true;
    }

    void Start()
    {
        /*troopParentInfo = GetComponentInParent<TroopParentInfo>();
        
        if (troopParentInfo == null)
        {
            Debug.LogError("No TroopParentInfo found in parent of " + gameObject.name);
            
            Destroy(gameObject);
            
            return;
        }*/
        
        //anims
        _idleHash = Animator.StringToHash("isIdle");
        
        _movingHash = Animator.StringToHash("isMoving");
        
        // _movingFastHash = Animator.StringToHash("isMovingFast");
        
        // _jumpHash = Animator.StringToHash("jumped");
        // _dashHash = Animator.StringToHash("dashed");
        
        _fallingHash = Animator.StringToHash("isFalling");
        
        _aimingHash = Animator.StringToHash("isAiming");
        
        _primaryHash = Animator.StringToHash("usedPrimary");
        
        _alternateHash = Animator.StringToHash("usedAlternate");
        
        _secondaryHash = Animator.StringToHash("usedSecondary");
    }
    
    #endregion
    
    void Update()
    {
        ManageCooldowns();

        BrainThink();
        
        BodyAct();
    }
    
    #region StatSetting
    
    //public void DetermineBehaviour(TroopAbilityInfoSO troopAbilityInfoSo)
    public void SetTroopAbilities(TroopAbilityInfoSO abilitiesSo)
    { 
        //Preferences and priorities
        _canHeal = abilitiesSo.HasHeals;
        
        _canDamage = abilitiesSo.HasDamage;

        if (_canHeal)
        {
            switch (_health.Team)
            {
                case ETeam.FriendlyAI:
                    if (_canDamage)
                    {
                        relevantTeams = new[]
                        { ETeam.FriendlyAI, ETeam.TroopPlayer, ETeam.EnemyAI };
                        
                        priorityTeams = new[] {ETeam.TroopPlayer, ETeam.FriendlyAI};
                    }
                    else
                    {
                        relevantTeams = new[] {ETeam.FriendlyAI, ETeam.TroopPlayer};
                        
                        priorityTeams = new[] {ETeam.TroopPlayer, ETeam.FriendlyAI};
                    }
                    break;
                case ETeam.EnemyAI:
                    if (_canDamage)
                    {
                        relevantTeams = new[]
                        { ETeam.DefensePosition, ETeam.FriendlyAI, ETeam.TroopPlayer, ETeam.EnemyAI };
                        
                        priorityTeams = new[] { ETeam.EnemyAI };
                    }
                    else
                    {
                        relevantTeams = new[] {ETeam.EnemyAI};
                        
                        priorityTeams = new[] {ETeam.EnemyAI};
                    }
                    
                    break;
                default:
                    Debug.LogError(" Health attached to this AI is configured incorrectly");
                    break;
            }
        }
        else
        {
            switch (_health.Team)
            {
                case ETeam.FriendlyAI:
                    relevantTeams = new[] {ETeam.EnemyAI};
    
                    priorityTeams = Array.Empty<ETeam>();
                    
                    break;
                case ETeam.EnemyAI:

                    relevantTeams = new[] { ETeam.DefensePosition, ETeam.FriendlyAI, ETeam.TroopPlayer };

                    //ability to add prefers player at some point
                    if (prefersDefensePositions)
                    {
                        priorityTeams = new[] {ETeam.DefensePosition};
                    }
                    else
                    {
                        priorityTeams = Array.Empty<ETeam>();
                    }
                    
                    break;
                default:
                    Debug.LogError(" Health attached to this AI is configured incorrectly");
                    break;
            }
        }
        
        //Engagement distances
        _engageMinMax.x = abilitiesSo.MinEngageDistance;
        
        _engageMinMax.y = abilitiesSo.MaxEngageDistance;
        
        _alertDistance = abilitiesSo.alertDistance + _engageMinMax.y;
        
        //Lenghts and cooldowns
        _primaryDuration = abilitiesSo.AttackDuration;
        
        _alternateDuration = abilitiesSo.AlternateDuration;
        
        _secondaryDuration = abilitiesSo.SecondaryDuration;
        
        _primaryCooldown = abilitiesSo.afterPrimaryCooldown;
        
        _alternateCooldown = abilitiesSo.afterAlternateCooldown;
        
        _secondaryCooldown = abilitiesSo.afterSecondaryCooldown;
        
        // Attack info
         _currentAttackIndex = -1;
        
         _allAbilitiesData = new AbilityData[3];
         
         _allAbilitiesData[0] = new AbilityData(abilitiesSo.hasPrimaryAttack, 0, abilitiesSo.primaryRange, 
                abilitiesSo.primaryEffectorType == EHealthEffectorType.Heal);
            
         _allAbilitiesData[1] = new AbilityData(abilitiesSo.hasAlternateAttack, 0, abilitiesSo.alternateRange, 
                abilitiesSo.alternateEffectorType == EHealthEffectorType.Heal);

         _allAbilitiesData[2] = new AbilityData(abilitiesSo.hasSecondaryAttack, 0, abilitiesSo.secondaryRange,
             abilitiesSo.secondaryEffectorType == EHealthEffectorType.Heal);
    }
    
    #endregion

    #region ManageCooldowns
    
    void ManageCooldowns()
    {
        if (_currentAttackTimer > 0)
        {
            _currentAttackTimer -= Time.deltaTime;
            
            if (_currentAttackTimer <= 0)
            {
                switch (_currentAttackIndex)
                {
                    case 0:
                        
                        _allAbilitiesData[_currentAttackIndex].TimeReady = Time.time + _primaryCooldown;
                        
                        PrimaryAnimating = false;
                        
                        break;
                    case 1:
                        
                        _allAbilitiesData[_currentAttackIndex].TimeReady = Time.time + _alternateCooldown;
                        
                        AlternateAnimating = false;
                        
                        break;
                    case 2:
                        
                        _allAbilitiesData[_currentAttackIndex].TimeReady = Time.time + _secondaryCooldown;
                        
                        SecondaryAnimating = false;
                        
                        break;
                    default:
                        
                        Debug.Log("Couldn't update anim but cooldown reached");
                        
                        _currentAttackIndex = -1;
                        
                        break;
                }
                
                _currentAttackIndex = -1;
                        
                SetNewBrainThink(EAIBrainState.Alert);
                
                _canChangeBodyState = true;
            }
        }
    }
    
    #endregion
    
    #region BrainThinking
    
    void BrainThink()
    {
        if (Time.time < _brainCanThinkTime) return;
        
        switch (_eCurrentBrainState)
        {
            case EAIBrainState.Idle:
                
                if (FindTarget())
                {
                    SetNewBrainThink(EAIBrainState.Alert);
                }
                else 
                {
                    RequestBodyStateChange(EAIBodyState.Idle);
                    
                    SetNewBrainThink(EAIBrainState.Idle , Random.Range(1f, 2f));
                }
                
                break;
            case EAIBrainState.Alert:
                
                if (ChooseAbilityAndEngageLocation())
                {
                    SetNewBrainThink(EAIBrainState.Engaging);
                    
                }
                else
                {
                    SetNewBrainThink(EAIBrainState.Idle);
                }
                
                break;
            case EAIBrainState.Engaging:
                
                //if we can change target after targeting, we check if we can re target
                //doesn't need update to _canRe.. because find target sets it
                if (canChangeTargetAfterTargeting && Time.time >= _canReTargetTime)
                {
                    Health firstTarget = _currentTargetHealth;
                    
                    if (FindTarget())
                    {
                        if (_currentTargetHealth != firstTarget)
                        {
                            SetNewBrainThink(EAIBrainState.Alert);
                            
                            break;
                        }
                    }
                }
                
                if (_eCurrentBodyState != EAIBodyState.Moving)
                {
                    RequestBodyStateChange(EAIBodyState.Moving);
                    
                    break;
                }
                
                if (LostTarget || _currentAttackIndex == -1)
                {
                    SetNewBrainThink(EAIBrainState.Idle);
                    
                    break;
                }

                if (LocInvalid)
                {
                    
                    if (!ChooseEngageLocation())
                    {
                        SetNewBrainThink(EAIBrainState.Idle);
                    }
                    
                    break;
                }
                
                if (LocHit)
                {
                    SetNewBrainThink(EAIBrainState.Attacking);
                    
                    switch (_currentAttackIndex)
                    {
                        case 0:
                            RequestBodyStateChange(EAIBodyState.UsingPrimary);
                            break;
                        case 1:
                            RequestBodyStateChange(EAIBodyState.UsingAlternate);
                            break;
                        case 2:
                            RequestBodyStateChange(EAIBodyState.UsingSecondary);
                            break;
                        //means we have no attack to use, come back later
                        default:
                            break;
                    }
                }
                
                break;
            case EAIBrainState.Attacking:
                
                //possible error handling needed
                
                UpdateLookRotToTarget();
                break;
        }
    }
    
    void SetNewBrainThink(EAIBrainState thoughtType, float waitTime = 0f)
    {
        _brainCanThinkTime = Time.time + waitTime;
        
        _eCurrentBrainState = thoughtType;
    }
    
    #endregion
    
    #region Detection
    bool FindTarget()
    {
        //used to use non alloc, but it was causing issue. If performance issues, look at managing detection?
        Collider[] hits = Physics.OverlapSphere(transform.position, _alertDistance, layersToDetect);
        
        for ( int i = 0; i < hits.Length; i++)
        {
            float currentScore;
            
            Health otherHealth = hits[i].GetComponent<Health>();
            
            if (otherHealth == _health || otherHealth == null) continue;
            
            if (priorityTeams.Contains(otherHealth.Team))
            {
                if (guaranteePreferredTargeting)
                {
                    _currentTargetHealth = otherHealth;
                    
                    return true;
                }
                
                currentScore = 2f;
                
                if (_canHeal)
                {
                    //if we're a healer and the target is invisible to healers, skip
                    if (otherHealth.InvisibleToHealers)
                    {
                        continue;
                    }
                    
                    //if we're a healer and the target has full health, cut weight
                    if (otherHealth.CurrentHealth == otherHealth.MaxHealth)
                    {
                        currentScore *= 0.25f;
                    }
                }
            }
            else if (relevantTeams.Contains(otherHealth.Team))
            {
                currentScore = 1;
            }
            else
            {
                continue;
            }
            
            // rating based on distance between x and y of min and max engage distance
            float distance = Vector3.Distance(transform.position, hits[i].transform.position);
            
            if (distance > _alertDistance)
            {
                currentScore = 0;
            }
            else
            {
                //find percentage distance between min and alert distance
                float percentage = (distance - _engageMinMax.x) / (_alertDistance - _engageMinMax.x); 
                
                //we want closer to be higher so we subtract from 1
                percentage = 1 - percentage;
                
                //square it to make it more exponential
                percentage *= percentage;
                
                //multiply by current score
                currentScore *= percentage;
            }
            
            if (currentScore > _currentTargetScore)
            { 
                _currentTargetScore = currentScore;
                
                _currentTargetHealth = otherHealth;
            }
        }
        
        //set the cooldown for retargeting during engagement if we were to be in that state next
        _canReTargetTime = Time.time + checkChangeTargetFrequency;
        
        if (_currentTargetHealth != null) return true;
        
        _currentTargetScore = 0;
        
        return false;
    }
    #endregion
    
    #region AttackPlanning
    
    bool ChooseAbilityToUse()
    {
        if (LostTarget) return false;
        
        //we look for heal abilities if healer
        if (_canHeal && priorityTeams.Contains(_currentTargetHealth.Team))
        {
            //if in melee range, we iterate forwards, because the ability SO's are set up (not hard coded),
            //to have melee abilities first
            if (InMeleeRange)
            {
                for (int i = 0; i < _allAbilitiesData.Length; i++)
                {
                    //if has it, is ready, and can heal
                    if (_allAbilitiesData[i].HasAbility && 
                        Time.time >= _allAbilitiesData[i].TimeReady && 
                        _allAbilitiesData[i].CanHeal)
                    {
                        _currentAttackIndex = i;
                        return true;
                    }
                }
            }
            else
            {
                for (int i = _allAbilitiesData.Length - 1; i >= 0; i--)
                {
                    //if has it, is ready, and can heal
                    if (_allAbilitiesData[i].HasAbility && 
                        Time.time >= _allAbilitiesData[i].TimeReady && 
                        _allAbilitiesData[i].CanHeal)
                    {
                        _currentAttackIndex = i;
                        return true;
                    }
                }
            }
            
            
        }
        //we look for damage abilities if we can damage
        else if (_canDamage && relevantTeams.Contains(_currentTargetHealth.Team))
        {
            //same iteration logic as above
            if (InMeleeRange)
            {
                for (int i = 0; i < _allAbilitiesData.Length; i++)
                {
                    //if has it, is ready, and does damage
                    if (_allAbilitiesData[i].HasAbility && 
                        Time.time >= _allAbilitiesData[i].TimeReady && 
                        !_allAbilitiesData[i].CanHeal)
                    {
                        _currentAttackIndex = i;
                        return true;
                    }
                }
            }
            else
            {
                for (int i = _allAbilitiesData.Length - 1; i >= 0; i--)
                {
                    //if has it, is ready, and does damage
                    if (_allAbilitiesData[i].HasAbility && 
                        Time.time >= _allAbilitiesData[i].TimeReady && 
                        !_allAbilitiesData[i].CanHeal)
                    {
                        _currentAttackIndex = i;
                        return true;
                    }
                }
            }
        }
        
        _currentAttackIndex = -1;
        
        return false;
    }
    
    bool  ChooseEngageLocation()
    {
        if (LostTarget || _currentAttackIndex == -1) return false;
        
        Vector3 targetPos = _currentTargetHealth.transform.position;
        
        
        Vector3 targetDirection = targetPos - transform.position;
        
        
        //making teams engage from directions that make sense (not in enemy lines)
        switch (_health.Team)
            {
                case ETeam.FriendlyAI:
                    
                    targetDirection = GameManager.Instance.FriendlySpawnDirection;
                    
                    break;
                case ETeam.EnemyAI:
                    
                    targetDirection = GameManager.Instance.EnemySpawnDirection;
                    
                    break;
            }
        
        if (_allAbilitiesData[_currentAttackIndex].Range >= rangeToBeConsideredMelee)
        {
            
            targetDirection.y = 0;
        
            Vector3 leftArcPosition = Quaternion.Euler(0, -rangedEngagePositionArc, 0) * targetDirection;
        
            Vector3 rightArcPosition = Quaternion.Euler(0, rangedEngagePositionArc, 0) * targetDirection;
        
            Vector3 chosenArcPosition = Vector3.Lerp(leftArcPosition, rightArcPosition, Random.value);
        
            targetPos += chosenArcPosition.normalized * _allAbilitiesData[_currentAttackIndex].Range;
        
            targetPos += new Vector3(Random.Range(-rangedAttackPosVariance,
                    rangedAttackPosVariance), 0, Random.Range(-rangedAttackPosVariance,
                    rangedAttackPosVariance));
        }
        
        if (LostTarget) return false;
        
        SetAgentMovement(true, targetPos);
        
        return true;
    }
    
    bool ChooseAbilityAndEngageLocation()
    {
        if (ChooseAbilityToUse())
        {
            if (ChooseEngageLocation())
            {
                if (LostTarget) return false;
                
                return true;
            }
            
        }
        return false;
    }

    // bool ChooseDamageAbility()
    // {
    //      if (_canDamage)
    //      {
    //          for (int i = 0; i < _allAbilitiesData.Length; i++)
    //          {
    //              //if has it, is ready, and does damage
    //              if (_allAbilitiesData[i].HasAbility &&
    //                  Time.time >= _allAbilitiesData[i].TimeReady &&
    //                  !_allAbilitiesData[i].CanHeal)
    //              {
    //                  _currentAttackIndex = i;
    //                  return true;
    //              }
    //          }
    //      }
    //     
    //      _currentAttackIndex = -1;
    //     
    //      return false;
    // }
    //
    // // bool ChooseHealAbility()
    // // {
    // //     if (_canHeal)
    // //     {
    // //         for (int i = 0; i < _allAbilitiesData.Length; i++)
    // //         {
    // //             //if has it, is ready, and can heal
    // //             if (_allAbilitiesData[i].HasAbility &&
    // //                 Time.time >= _allAbilitiesData[i].TimeReady &&
    // //                 _allAbilitiesData[i].CanHeal)
    // //             {
    // //                 _currentAttackIndex = i;
    // //                 return true;
    //             }
    //         }
    //     }
    //     
    //     _currentAttackIndex = -1;
    //     
    //     return false;
    // }
    //
    // bool ForceChooseAbilityAndEngageTarget(Health target)
    // {
    //     if (target == null) return false;
    //     
    //     _currentTargetHealth = target;
    //     
    //     if (Choose____Ability())
    //     {
    //         if (ChooseEngageLocation())
    //         {
    //             if (LostTarget) return false;
    //             
    //             return true;
    //         }
    //     }
    //     return false;
    // }
    
    #endregion

    #region BodyState
    
    void BodyAct()
    {
        //MAYBE DO MORE WITH AGENT ANOTHER TIME
        if (!_canChangeBodyState) { return; }
        
        if (!StateChangeRequested) { return; }
        
        switch (_eTargetBodyState)
        {
            case EAIBodyState.Idle:
                SetAgentMovement( false);
                break;
            case EAIBodyState.Moving:
                //Comeback later
                break;
            case EAIBodyState.MovingFast:
                //Comeback later
                break;
            case EAIBodyState.Falling:
                //Comeback later
                break;
            case EAIBodyState.UsingPrimary:
                _canChangeBodyState = false;
                
                SetAgentMovement(false);
                //Comeback later
                break;
            case EAIBodyState.UsingAlternate:
                _canChangeBodyState = false;
                
                SetAgentMovement(false);
                //Comeback later
                break;
            case EAIBodyState.Aiming:
                _canChangeBodyState = false;
                
                SetAgentMovement(false);
                //Comeback later
                break;
            case EAIBodyState.UsingSecondary:
                _canChangeBodyState = false;
                
                SetAgentMovement(false);
                //Comeback later
                break;
        }
        
        _eCurrentBodyState = _eTargetBodyState;

        ManageAnimation();
        
        ClearStateChangeRequest();
    }
    
    void ManageAnimation()
    {
        switch (_eCurrentBodyState)
        {
            case EAIBodyState.Idle:
                _anim.SetBool(_idleHash, true);
                _anim.SetBool(_movingHash, false);
                _anim.SetBool(_fallingHash, false);
                _anim.SetBool(_aimingHash, false);
                break;
            case EAIBodyState.Moving:
                _anim.SetBool(_idleHash, false);
                _anim.SetBool(_movingHash, true);
                _anim.SetBool(_fallingHash, false);
                _anim.SetBool(_aimingHash, false);
                break;
            case EAIBodyState.MovingFast:
                _anim.SetBool(_idleHash, false);
                _anim.SetBool(_movingHash, true);
                _anim.SetBool(_fallingHash, false);
                _anim.SetBool(_aimingHash, false);
                break;
            case EAIBodyState.Falling:
                _anim.SetBool(_idleHash, false);
                _anim.SetBool(_movingHash, false);
                _anim.SetBool(_fallingHash, true);
                _anim.SetBool(_aimingHash, false);
                break;
            case EAIBodyState.UsingPrimary:
                _anim.SetBool(_idleHash, false);
                _anim.SetBool(_movingHash, false);
                _anim.SetBool(_fallingHash, false);
                _anim.SetBool(_aimingHash, false);
                
                if (!PrimaryAnimating)
                {
                    if (_allAbilitiesData[_currentAttackIndex].Range > rangeToBeConsideredMelee)
                    {
                        TrySnapAim();
                    }
                    
                    _anim.SetTrigger(_primaryHash);
                    
                    _currentAttackTimer = _primaryDuration;
                    
                    PrimaryAnimating = true;
                }
                break;
            case EAIBodyState.UsingAlternate:
                _anim.SetBool(_idleHash, false);
                _anim.SetBool(_movingHash, false);
                _anim.SetBool(_fallingHash, false);
                _anim.SetBool(_aimingHash, false);
                
                if (!AlternateAnimating)
                {
                    if (_allAbilitiesData[_currentAttackIndex].Range > rangeToBeConsideredMelee)
                    {
                        TrySnapAim();
                    }
                    
                    _anim.SetTrigger(_alternateHash);
                    
                    _currentAttackTimer = _alternateDuration;
                    
                    AlternateAnimating = true;
                }
                break;
            case EAIBodyState.Aiming:
                _anim.SetBool(_idleHash, false);
                _anim.SetBool(_movingHash, false);
                _anim.SetBool(_fallingHash, false);
                _anim.SetBool(_aimingHash, true);
                
                break;
            case EAIBodyState.UsingSecondary:
                _anim.SetBool(_idleHash, false);
                _anim.SetBool(_movingHash, false);
                _anim.SetBool(_fallingHash, false);
                _anim.SetBool(_aimingHash, false);
                
                if (!SecondaryAnimating)
                {
                    if (_allAbilitiesData[_currentAttackIndex].Range > rangeToBeConsideredMelee)
                    {
                        TrySnapAim();
                    }
                    
                    _anim.SetTrigger(_secondaryHash);
                    
                    _currentAttackTimer = _secondaryDuration;
                    
                    SecondaryAnimating = true;
                }
                break;
        }
    }

    void TrySnapAim()
    {
        if (LostTarget) return;
        
        //try and snap our aim by rotating along world up axis
        Vector3 targetDir = _currentTargetHealth.transform.position - transform.position;
        
        targetDir.y = 0;
        
        if (targetDir != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(targetDir);
            
            transform.rotation = targetRotation;
        }
        
    }
    
    void UpdateLookRotToTarget()
    {
        if (LostTarget) return;
        
        //currently same, might add aiming or trailing later
        
        if (_allAbilitiesData[_currentAttackIndex].Range <= rangeToBeConsideredMelee)
        {
            var targetMeleeDir = _currentTargetHealth.transform.position - transform.position;
        
            targetMeleeDir.y = 0;
        
            if (targetMeleeDir != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(targetMeleeDir);
        
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * lookSpeed);
            }
        }
        else
        {
            var targetRangedDir = _currentTargetHealth.transform.position - transform.position;
        
            targetRangedDir.y = 0;
        
            if (targetRangedDir != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(targetRangedDir);
        
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * lookSpeed);
            }
        }
    }
    
    void SetAgentMovement(bool shouldMove , Vector3 destination = default)
    {
        if (shouldMove)
        {
            _navMeshAgent.isStopped = false;
            
            _navMeshAgent.SetDestination(destination);
        }
        else
        {
            _navMeshAgent.isStopped = true;
        }
    }
    
    void RequestBodyStateChange(EAIBodyState targetState)
    {
        _eTargetBodyState = targetState;
        
        StateChangeRequested = true;
    }
    
    void ClearStateChangeRequest()
    {
        StateChangeRequested = false;
        
    }
    
    #endregion
    
    bool LocInvalid
    {
        get
        {
            //if melee, true if distance from target and destination is greater than melee range and leniency
            if (_allAbilitiesData[_currentAttackIndex].Range <= rangeToBeConsideredMelee)
            {
                return (Vector3.Distance(_currentTargetHealth.transform.position, _navMeshAgent.destination) >
                        _allAbilitiesData[_currentAttackIndex].Range + meleeEngagePosLeniency);
            }
            //if ranged, true if distance from target and destination is greater than ranged range and leniency,
            // or if distance from this troop and destination is greater than the distance
            // from this troop and target (to prevent running past target)
            else
            {
                if ( Vector3.Distance(_currentTargetHealth.transform.position, _navMeshAgent.destination) >
                         _allAbilitiesData[_currentAttackIndex].Range + rangedEngagePosLeniency ||
                     Vector3 .Distance(transform.position, _navMeshAgent.destination) >
                         Vector3.Distance(transform.position, _currentTargetHealth.transform.position))
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }
        }
    }
    
    bool LocHit
    {
        get
        {
            //if melee, true if distance from this troop and destination is less or equal to 
            //melee range and leniency
            if (_allAbilitiesData[_currentAttackIndex].Range <= rangeToBeConsideredMelee)
            {
                return (Vector3.Distance(transform.position, _navMeshAgent.destination) <=
                        meleeEngagePosLeniency);
            }
            //if ranged, true if distance from this troop and destination is less than ranged engage pos leniency
            else
            {
                return (Vector3.Distance(transform.position, _navMeshAgent.destination) <= 
                        rangedEngagePosLeniency);
            }
        }
    }
    
    bool InMeleeRange
    {
        get
        {
            return (Vector3.Distance(transform.position, _currentTargetHealth.transform.position) <=
                    rangeToBeConsideredMelee);
        }
    }

    bool LostTarget { get { return _currentTargetHealth == null; } }
    
    public bool StateChangeRequested { get; private set; }
    
    // public bool SpecialMoveAnimating { get; private set; }
    
    public bool PrimaryAnimating { get; private set; }
    
    public bool AlternateAnimating { get; private set; }
    
    public bool SecondaryAnimating { get; private set; }
}