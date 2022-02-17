using UnityEngine;
using Random = UnityEngine.Random;
using Unity.Mathematics;

public class EnemyController : MonoBehaviour
{
    public EnemyPreset preset = null;
    public Vector3 anchorPoint;    //spawned position
    private TDTile anchorTile;
    private SpriteRenderer sr;
    private PathFinding pf;
    private Rigidbody2D rb;
    private CharacterAnimationController animationController;
    private Map mapRef;
    //wander
    private bool combat = false;
    private bool dead = false;
    public bool IsDead  //dead getsetter
    {
        set{dead = value;}
        get{return dead;}
    }
    public bool InCombat    //combat getsetter
    {
        set{combat = value;}
        get{return combat;}
    }
    public float observeTime = 2;
    public float wanderTime = 5; //time before changes direction
    public float movementSpeed = 3f;    //speed same as player
    private Vector3 moveDir;    //movement direction
    private int wanderRadius = 5;     //movement circle around spawned point
    private int OnEnableCount = 0; //skip first onEnable(that after awake)
    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        pf = GetComponent<PathFinding>();
        rb = GetComponent<Rigidbody2D>();
        animationController = GetComponent<CharacterAnimationController>();
        rb.freezeRotation = true;
        var m = GameObject.FindGameObjectWithTag("Map");
        try
        {
            mapRef = m.GetComponent<Map>();
        }
        catch
        {
            Debug.Log("Map reference not found in Enemy controller.");
        }
    }

    /// <summary>
    /// Performs on activation
    /// </summary>
    private void OnEnable() {
        //when creating object pool this is performed, resulting in null
        if (preset != null)
        {
            sr.sprite = preset.sprite;
        }
        //start wandering right after enable
        if (OnEnableCount != 0)
        {
            //ChangeDirection();
            observeTime = GetRandomObserveTime();
        }
        OnEnableCount++;
    }

    private void Start() {
        //get anchor tile
        int2 coords = new int2((int)anchorPoint.x, (int)anchorPoint.y);
        anchorTile =  mapRef.GetTile(mapRef.TileRelativePos(coords), mapRef.TileChunkPos(coords));
    }

    /// <summary>
    /// Performs on deactivation
    /// </summary>
    private void OnDisable() {
        //Debug.Log("Deactivated");    
    }

    private void Update() {
        if (!IsDead)
        {
            Wander();
        }
        
    }

    private void FixedUpdate() {
        rb.velocity = moveDir * movementSpeed;
    }

    private void Wander(){
        if (wanderTime > 0)
        {
            //change direction from non walkable tile
            TileController();
            wanderTime -= Time.deltaTime;
        }else{
            //stop and observer after wander
            if (observeTime > 0)
            {
                //animate idle
                animationController.CharacterDirection(moveDir);
                moveDir = Vector3.zero; //stop
                observeTime -= Time.deltaTime;
            //then find new direction towards anchor point and move
            }else{
                observeTime = GetRandomObserveTime();
                wanderTime = Random.Range(1.0f, 6.0f);
                ChangeDirection();
            }
        }
    }

    //TODO doc
    private void ChangeDirection(){
        Vector3 targetPos = RandomPointInRadius();
        moveDir = new Vector3(targetPos.x-this.gameObject.transform.position.x, targetPos.y - this.gameObject.transform.position.y, 0f).normalized;
        //animate movement
        animationController.CharacterMovement(moveDir);
    }

    //TODO doc
    private Vector3 RandomPointInRadius(){
        return new Vector3(anchorPoint.x + Random.Range(-wanderRadius,wanderRadius), anchorPoint.y + Random.Range(-wanderRadius,wanderRadius));
    }

    //TODO doc
    private float GetRandomObserveTime(){
        return Random.Range(0.0f, 5.0f);
    }

    //TODO description
    private void TileController(){
        int2 coords = new int2((int)this.gameObject.transform.position.x,(int)this.gameObject.transform.position.y);
        TDTile tile = mapRef.GetTile(mapRef.TileRelativePos(coords), mapRef.TileChunkPos(coords));
        if (!tile.IsWalkable || tile == null)
        {
            ChangeDirection();
        }   
        return;
    }
}
