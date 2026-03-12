using deVoid.Utils;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Fire : MonoBehaviour,ISpell,IGameObjectSource
{
    public GameObject CurrentTile { get; set; }
    public float moveSpeed;
    public float MoveSpeed { get; set; }
    public float Damage { get; set; }
    public float startDamage = 25;
    public string type = "Fire";
    public string Type
    {
        get
        {
            return type;
        }
    }
    public GameObject LastPortal { get; set; }

    public List<GameObject> path;
    [SerializeField] private float delayBetweenHexes = .2f;
    
    [SerializeField] private GameObject prefab; 
    public GameObject Prefab
    {
        get => prefab;
        set => prefab = value;
    }
    
    [SerializeField] private LeyLineGen leyLineMap; 


    private Animator fireAnimator;
    private bool hasExploded = false;
    [SerializeField] private float explodeLifetime = 1.0f;
    
    public void Awake()
    {
        //read fields from inspector
        MoveSpeed = moveSpeed;
        Damage = startDamage;
        path = new List<GameObject>();

        fireAnimator = GetComponentInChildren<Animator>();
        
        // Disable collider initially to prevent collisions during instantiation/setup
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = false;
        }
    }
    
    public void EnableCollider()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = true;
        }
    }

    public void Cast()
    {
        if (path == null || path.Count == 0)
            return;

        StartCoroutine(ChainRoutine());
    }

    private IEnumerator ChainRoutine()
    {
        HexTile prev = this.CurrentTile.GetComponent<HexTile>();
        for (int i = 0; i < path.Count && !hasExploded; i++)
        {
            
            GameObject target = path[i];
            // store the target HexTile
            HexTile targetHex = target.GetComponent<HexTile>();
            // Rotate fireball to face direction of next move
            GetVectorAngle(prev.coords, targetHex.coords, transform);

            // Move fire to hex
            transform.position = target.transform.position;
            CurrentTile = target;
            try
            {
                ApplyLeyLineEffect(prev, target.GetComponent<HexTile>());
            }
            catch
            {
                Debug.Log("Error caught in fire Chain Routine");
            }
            prev = target.GetComponent<HexTile>();

            yield return new WaitForSeconds(delayBetweenHexes);
        }

        Destroy(this.gameObject);
    }
    
    private void ApplyLeyLineEffect(HexTile hexA,HexTile hexB)
    {
        LeyLineGen.LeyLine l = leyLineMap.GetLeyLine(hexA, hexB);
        if (l == null) return;
        float mod = l.weight;
        Debug.Log($"The weight of the leyline {hexA},{hexB} is: {mod}");
        this.Damage = Damage * (1f - mod / 100f);
        Debug.Log($"Damage is: {this.Damage}");
        
    }

    public void AddPath(int q, int r)
    {
        // Add hex to path for fire to follow
        try
        {
            path.Add(HexGridManager.GetHex(q, r));
        }
        catch
        {
            Debug.Log("Unable to add to path");
        }
    }

    public void GetVectorAngle(HexCoords initial, HexCoords final, Transform fireballTrans)
    {
        int dq = final.q - initial.q;
        int dr = final.r - initial.r;
        // each s coord = s = -q - r
        int ds = (-1 * final.q - final.r) - (-1 * initial.q - initial.r);

        if (fireballTrans == null)
            return;

        switch (dq, dr, ds)
        {
            case var _ when dq > 0 && true && ds < 0:
                //(1,0,-1)
                // 0 degree rotation
                fireballTrans.rotation = Quaternion.Euler(0f, 0f, 0f);
                break;

            case var _ when dq > 0 && dr < 0 && true:
                //(1,-1,0)
                fireballTrans.rotation = Quaternion.Euler(0f, 0f, 120f);
                break;

            case var _ when true && dr < 0 && ds > 0:
                //(0,-1,1)
                fireballTrans.rotation = Quaternion.Euler(0f, 0f, 240f);
                break;

            case var _ when dq < 0 && true && ds > 0:
                //(-1,0,1)
                fireballTrans.rotation = Quaternion.Euler(0f, 0f, 180f);
                break;

            case var _ when dq < 0 && dr > 0 && true:
                //(-1,1,0)
                fireballTrans.rotation = Quaternion.Euler(0f, 0f, 300f);
                break;

            case var _ when true && dr > 0 && ds < 0:
                //(0,1,-1)
                fireballTrans.rotation = Quaternion.Euler(0f, 0f, 60f);
                break;

            default:
                Debug.Log("Something went wrong in fireball switch statement");
                break;
        }
    }

    public bool OverrideMoveSpell(GameObject target)
    {
        // Generate hex-by-hex path from Fire's current position to target
        List<GameObject> hexPath = HexGridManager.GetLine(CurrentTile.GetComponent<HexTile>().coords,target.GetComponent<HexTile>().coords);
        
        if (hexPath.Count > 0)
        {
            path = hexPath;
            StartCoroutine(ChainRoutine());
            return false; // Don't use default MoveRoutine
        }     
        return true; // Fall back to default if path generation fails
    }

    private void OnTriggerEnter2D(Collider2D col)
    {

        if (hasExploded)
            return;

        if (col.gameObject.CompareTag("Tile"))
            return;
        
        Debug.Log($"Fire triggered by {col.gameObject.name}");
        
        // Handle ice spell collision
        Ice ice = col.GetComponent<Ice>();
        if (ice != null)
        {
            ice.TakeDamage(Damage, "Fire");
            StartCoroutine(ExplodeAndDestroy());
            return;
        }
        
        // Handle player collision
        PlayerController player = col.GetComponent<PlayerController>();
        if (player != null)
        {
            player.health -= Damage;
            StartCoroutine(ExplodeAndDestroy());
            return;
        }
        
        StartCoroutine(ExplodeAndDestroy());
    }

    public void OnTeleport(GameObject target)
    {
        //handle teleport
        if (this.Type != "Portal" && target != this.LastPortal)
        {
            transform.position = target.transform.position;
            CurrentTile = target.GetComponent<ISpell>().CurrentTile;
            this.LastPortal = target;
        }
        Debug.Log($"{gameObject} was teleported to {target}");
    }

    private IEnumerator ExplodeAndDestroy()
    {
        if (hasExploded)
            yield break;

        hasExploded = true;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = false;
        }

        if (fireAnimator != null)
        {
            fireAnimator.SetTrigger("Explode");
            yield return null;
            while (!fireAnimator.GetCurrentAnimatorStateInfo(0).IsName("Explode"))
            {
                yield return null;
            }
            while (fireAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
            {
                yield return null;
            }
        } else
        {
            yield return new WaitForSeconds(0.5f);
        }

        //yield return new WaitForSeconds(explodeLifetime);

        Destroy(gameObject);
    }
}
