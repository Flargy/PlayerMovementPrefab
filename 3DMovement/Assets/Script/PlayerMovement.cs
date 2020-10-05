using UnityEngine;

[RequireComponent( typeof(CapsuleCollider))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private LayerMask collisionMask = 0;

    [SerializeField] private Vector3 Velocity = Vector3.zero;
    private Vector3 Direction = Vector3.zero;


    private CapsuleCollider coll = null;


    [SerializeField] private float horizontalDirection = 0.0f;
    [SerializeField] private float verticalDirection = 0.0f;

    private Vector3 pointUp;
    private Vector3 pointDown;
    private PhysicsComponent otherPhysics;
    private RaycastHit capsuleRaycast;
    [SerializeField] private float frictionCoefficient = 0.95f;
    private float airFriction = 0.95f;

    private float gravity = 9.81f;
    private float skinWidth = 0.05f;
    private float maxSpeed = 6.0f;


    private void Awake()
    {
        coll = GetComponent<CapsuleCollider>();
    }

    public void Update()
    {
        if (Grounded())
        {
            MovementInput();

            CameraDirectionChanges();
            ProjectToPlaneNormal();
            ControlDirection();
            GroundDistanceCheck();
            Accelerate(Direction);
        }



        CollisionCheck(Velocity * Time.deltaTime);
        ApplyGravity();

    }

    /// <summary>
    /// Checks for collision using <see cref="capsuleRaycast"/> and recursive calls.
    /// </summary>
    /// <param name="frameMovement"></param>
    public void CollisionCheck(Vector3 frameMovement)
    {

        pointUp = transform.position + (coll.center + Vector3.up * (coll.height / 2 - coll.radius));
        pointDown = transform.position + (coll.center + Vector3.down * (coll.height / 2 - coll.radius));
        if (Physics.CapsuleCast(pointUp, pointDown, coll.radius, frameMovement.normalized, out capsuleRaycast, Mathf.Infinity, collisionMask))
        {
            Debug.DrawRay(transform.position, frameMovement.normalized, Color.red);
            
            float angle = (Vector3.Angle(capsuleRaycast.normal, frameMovement.normalized) - 90) * Mathf.Deg2Rad;
            float snapDistanceFromHit = skinWidth / Mathf.Sin(angle);

            Vector3 snapMovementVector = frameMovement.normalized * (capsuleRaycast.distance - snapDistanceFromHit);
            snapMovementVector = Vector3.ClampMagnitude(snapMovementVector, frameMovement.magnitude);
            frameMovement -= snapMovementVector;

            Vector3 frameMovementNormalForce = HelpClass.NormalizeForce(frameMovement, capsuleRaycast.normal);
            frameMovement += frameMovementNormalForce;

            transform.position += snapMovementVector;

            if (frameMovementNormalForce.magnitude > 0.001f)
            {
                Vector3 velocityNormalForce = HelpClass.NormalizeForce(Velocity, capsuleRaycast.normal);
                Velocity += velocityNormalForce;

            }

            if (frameMovement.magnitude > 0.001f)
            {
                CollisionCheck(frameMovement);
            }
            
            return;
        }

        else
        {
            transform.position += frameMovement;
        }
    }

    /// <summary>
    /// Lowers the players speed by a set amount each update.
    /// </summary>
    public void Decelerate()
    {
        pointUp = transform.position + (coll.center + Vector3.up * (coll.height / 2 - coll.radius));
        pointDown = transform.position + (coll.center + Vector3.down * (coll.height / 2 - coll.radius));
        Physics.CapsuleCast(pointUp, pointDown, coll.radius, Velocity.normalized, out capsuleRaycast, maxSpeed, collisionMask);

        Vector3 velocityOnGround = Vector3.ProjectOnPlane(Velocity, capsuleRaycast.normal);
        Vector3 decelerationVector = velocityOnGround * frictionCoefficient;

        if (decelerationVector.magnitude > velocityOnGround.magnitude)
        {
            Velocity = Vector3.zero;
        }
        else
        {
            Velocity -= decelerationVector;
        }


    }

    /// <summary>
    /// Applies the velocity of a moving object onto the player if the player is standing on it.
    /// </summary>
    /// <param name="collideObject"></param>
    /// <param name="normalForce"></param>
    private void InheritVelocity(Transform collideObject, ref Vector3 normalForce)
    {
        otherPhysics = collideObject.GetComponent<PhysicsComponent>();
        if (otherPhysics == null)
            return;
        normalForce = normalForce.normalized * (normalForce.magnitude + Vector3.Project(otherPhysics.GetVelocity(), normalForce.normalized).magnitude);
        Vector3 forceInDirection = Vector3.ProjectOnPlane(Velocity - otherPhysics.GetVelocity(), normalForce.normalized);
        Vector3 friction = -forceInDirection.normalized * normalForce.magnitude;

        if (friction.magnitude > forceInDirection.magnitude)
            friction = friction.normalized * forceInDirection.magnitude;
        Velocity += friction;
    }

    /// <summary>
    /// Applies a constant force of gravity on the player.
    /// </summary>
    public void ApplyGravity()
    {
        Velocity += Vector3.down * gravity * Time.deltaTime;

    }

    /// <summary>
    /// Gradually increases the players velocity.
    /// </summary>
    /// <param name="direction"></param>
    public void Accelerate(Vector3 direction)
    {
        Velocity = direction.normalized * maxSpeed;
    }

    /// <summary>
    /// Uses a <see cref="capsuleRaycast"/> to see if the player has made contact with the cround
    /// </summary>
    /// <returns></returns>
    public bool Grounded()
    {
        Vector3 pointUp = transform.position + coll.center + Vector3.up * (coll.height / 2 - coll.radius);
        Vector3 pointDown = transform.position + coll.center + Vector3.down * (coll.height / 2 - coll.radius);
        if (Physics.CapsuleCast(pointUp, pointDown, coll.radius, Vector3.down, out capsuleRaycast, (0.05f + skinWidth), collisionMask)) // ändrade 0,8 till 0,6
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void GroundDistanceCheck()
    {

        if (capsuleRaycast.collider != null)
        {
            if (capsuleRaycast.distance > 0.4f)
            {
                Velocity += new Vector3(0, -capsuleRaycast.distance * 5, 0);
            }
        }

    }

    /// <summary>
    /// Returns the normal angle of the object below the player.
    /// </summary>
    /// <returns></returns>
    public Vector3 GroundedNormal()
    {
        Vector3 pointUp = transform.position + coll.center + Vector3.up * (coll.height / 2 - coll.radius);
        Vector3 pointDown = transform.position + coll.center + Vector3.down * (coll.height / 2 - coll.radius);
        if (Physics.CapsuleCast(pointUp, pointDown, coll.radius, Vector3.down, out capsuleRaycast, (0.5f + skinWidth), collisionMask))
        {
            return capsuleRaycast.normal;
        }
        else
        {
            return Vector3.zero;
        }
    }



    /// <summary>
    /// Alters the direction input to match the cameras direction.
    /// </summary>
    public void CameraDirectionChanges()
    {
        Direction = Camera.main.transform.rotation * new Vector3(horizontalDirection, 0, verticalDirection).normalized;
    }

    /// <summary>
    /// Gives the movement variables values from input
    /// </summary>
    public void MovementInput()
    {
        verticalDirection = Input.GetAxisRaw("Vertical");
        horizontalDirection = Input.GetAxisRaw("Horizontal");

    }

    /// <summary>
    /// Updates the players direction to match the terrains normal.
    /// </summary>
    public void ProjectToPlaneNormal()
    {

        RaycastHit collision;
        Vector3 point1 = transform.position + coll.center + Vector3.up * (coll.height / 2 - coll.radius);
        Vector3 point2 = transform.position + coll.center + Vector3.down * (coll.height / 2 - coll.radius);

        Physics.CapsuleCast(point1, point2, coll.radius, Vector3.down, out collision, maxSpeed, collisionMask);

        Direction = Vector3.ProjectOnPlane(Direction, collision.normal).normalized;

    }

    /// <summary>
    /// Stops the player from sliding down hills
    /// </summary>
    public void ControlDirection()
    {
        Vector3 projectedDirection = Vector3.ProjectOnPlane(Direction, capsuleRaycast.normal);
        if (Vector3.Dot(projectedDirection, Velocity) != 1)
        {
            Velocity = projectedDirection.normalized * Velocity.magnitude;
        }
    }

}
