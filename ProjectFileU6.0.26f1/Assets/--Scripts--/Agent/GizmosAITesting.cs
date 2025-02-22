using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class GizmosAITesting : MonoBehaviour
{
    [Header("Debug and Testing")]
    
    public float testMinEngageDistance = 1f;

    public Color minEngageDistanceCageColor = Color.cyan;
    
    [Space]
    
    public float testMaxEngageDistance = 20f;
    
    public Color maxEngageDistanceCageColor = Color.cyan;
    
    [Space]
    
    public float testExtraAlertDistance = 30f;

    public Color alertDistanceCageColor = Color.green;
    
    [Space]
    
    public Vector3 testEnemyPositionOffsetFromThis = new Vector3(5, 0, 0);
    
    public Color enemyPositionColor = Color.red;
    
    public float enemyPositionSphereRadius = 2f;
    
    [Space]
    
    //on trying to decide to use a ranged attack, imagine an arc from the ENEMY position TO this, expanding in 
    //said direction. Along the edge of this arc is where we will choose a random spot, then we will add a
    //bit of randomness to said position and make that the engage position
    public float testRangedEngagePositionArc = 45f;
    
    public Color rangedEngagePositionArcColor = Color.white;
    
    [Space]
    
    //this won't be show, just used for the decision making, only thinking on horizontal plane as of now too
    public float testEngagePositionDecisionLeniency = 3f;
    
    [Space]
    
    public Vector3 generatedEngagePosition = new Vector3(0, 0, 0);
    
    public float targetEngagePositionSphereRadius = 1.5f;
    
    public Color engagePositionSphereColor = Color.magenta;
    
    [Space]
    
    //this is the radius around the engage position that this will decide it has reached the engage position
    //or is close enough
    public float testEngagePositionActLeniency = 3f;
    
    public Color engagePositionActLeniencyAreaCageColor = Color.yellow;
    
    [Space]
    
    public bool attackWouldBeRanged = false;
    
    public float rangedAttackDistance = 10f;
    
    public bool generateRandomEngagePosition = false;
    
    //This is all for inspector tweaking and debugging not during runtime
    // We don't care about priorities, or healing, or any of that
    // We are just testing where the AI would try to move to to attack the target given the test values
    //Each time the generate button is pressed, we generate a new test engage position and show the information 
    //along with the rest of the debug information
    private void OnValidate()
    {
        if (generateRandomEngagePosition)
        {
            //generating engage position based on how we do it during runtime considering the test values
            
            //it is all self explanatory, the only real edge case we're going to cover is that if the enemy
            //position is not in a targetable position, we will set the engage position to this position
            
            if (testEnemyPositionOffsetFromThis == Vector3.zero 
                || testEnemyPositionOffsetFromThis.magnitude > testMaxEngageDistance + testExtraAlertDistance)
            {
                generatedEngagePosition = transform.position;
            }
            else if (!attackWouldBeRanged)
            {
                generatedEngagePosition = transform.position + testEnemyPositionOffsetFromThis;
            }
            else
            {
                //considering the direction from the enemy to this, we want to mimic encircling the target
                //we will choose between the end of the left and right arc positions, then add a bit of randomness
                //to the position
                
                Vector3 targetDirection = -testEnemyPositionOffsetFromThis;
                
                Vector3 leftArcPosition = Quaternion.Euler(0, -testRangedEngagePositionArc, 0) * targetDirection;
                
                Vector3 rightArcPosition = Quaternion.Euler(0, testRangedEngagePositionArc, 0) * targetDirection;
                
                Vector3 chosenArcPosition = Vector3.Lerp(leftArcPosition, rightArcPosition, Random.value);
                
                generatedEngagePosition = (transform.position + testEnemyPositionOffsetFromThis) + 
                                          chosenArcPosition.normalized * rangedAttackDistance;
                
                //add a bit of randomness to the position
                generatedEngagePosition += new Vector3(Random.Range(-testEngagePositionDecisionLeniency, 
                    testEngagePositionDecisionLeniency), 0, Random.Range(-testEngagePositionDecisionLeniency, 
                    testEngagePositionDecisionLeniency));
            }
            
            generateRandomEngagePosition = false;
        }
        
    }
    void OnDrawGizmosSelected()
    {
        //showing the min engage distance cage
        Gizmos.color = minEngageDistanceCageColor;
        
        Gizmos.DrawWireSphere(transform.position, testMinEngageDistance);
        
        //showing the max engage distance cage
        Gizmos.color = maxEngageDistanceCageColor;
        
        Gizmos.DrawWireSphere(transform.position, testMaxEngageDistance);
        
        //showing the alert distance cage
        Gizmos.color = alertDistanceCageColor;
        
        Gizmos.DrawWireSphere(transform.position,testMaxEngageDistance + testExtraAlertDistance);
        
        //showing the enemy sphere at enemy offset
        Gizmos.color = enemyPositionColor;
        
        Gizmos.DrawSphere(transform.position + testEnemyPositionOffsetFromThis, enemyPositionSphereRadius);
        
        //show the arc that the engage position will be chosen from
        
        //show the left and right lines of the arc considering the angle and enemy
        //position
        
        Gizmos.color = rangedEngagePositionArcColor;
        
        Vector3 targetDirection = -testEnemyPositionOffsetFromThis;
        
        Vector3 leftArcPosition = Quaternion.Euler(0, -testRangedEngagePositionArc, 0) * targetDirection;
        
        Vector3 rightArcPosition = Quaternion.Euler(0, testRangedEngagePositionArc, 0) * targetDirection;
        
        Gizmos.DrawLine((transform.position + testEnemyPositionOffsetFromThis), 
            (transform.position + testEnemyPositionOffsetFromThis) + leftArcPosition.normalized * rangedAttackDistance);
        
        Gizmos.DrawLine((transform.position + testEnemyPositionOffsetFromThis),
            (transform.position + testEnemyPositionOffsetFromThis) + rightArcPosition.normalized *
            rangedAttackDistance);
        
        //engage position will have been calculated, just show it as a sphere
        Gizmos.color = engagePositionSphereColor;

        Gizmos.DrawSphere(generatedEngagePosition, targetEngagePositionSphereRadius);
        
        //show the cage around the engage distance that the AI will consider itself to have reached the engage position
        Gizmos.color = engagePositionActLeniencyAreaCageColor;
        
        Gizmos.DrawWireSphere(generatedEngagePosition, testEngagePositionActLeniency);
    }
}
