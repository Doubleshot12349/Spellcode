using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using deVoid.Utils;


public class Lightning : MonoBehaviour,ISpell,IGameObjectSource
{
    [SerializeField] private float delayBetweenJumps = 13f;

    public GameObject CurrentTile { get; set; }
    [SerializeField] private GameObject prefab; 


    public GameObject Prefab 
    { 
        get => prefab; 
        set => prefab = value; 
    }
    
    
    public float moveSpeed;
    public float MoveSpeed { get; set; }
    public List<GameObject> path;
    public float Damage { get; set; }
    public float currentDamage;
    public string type = "Lightning";
    public string myPlayer;
    public string Type
    {
        get
        {
            return type;
        }
    }
    public GameObject LastPortal { get; set; }

    public void Cast()
    {
        if (path == null || path.Count == 0)
            return;

        StartCoroutine(ChainRoutine());
    }

    private IEnumerator ChainRoutine()
    {
        for (int i = 0; i < path.Count; i++)
        {
            //finding rotation
            HexTile prev;
            if (i == 0)
            {
                prev = HexGridManager.GetHex(0, 0).GetComponent<HexTile>();//GameObject.Find(myPlayer).GetComponent<PlayerMover>().currentTile;
            }
            else
            {
                prev = path[i - 1].GetComponent<HexTile>();
            }
            GameObject target = path[i];

            HexCoords final = target.GetComponent<HexTile>().coords;
            HexCoords initial = prev.coords;
            if (final != initial)
            {


                GameObject lightning = Instantiate(Prefab, target.transform.position, Quaternion.identity);
                lightning.GetComponent<Renderer>().enabled = true;
                lightning.transform.SetParent(target.transform);
                lightning.transform.position -= new Vector3(0.5f, 0, 0);
                lightning.GetComponent<Animator>().speed = .25f;
                lightning.GetComponent<Lightning>().EnableCollider();

                try
                {
                    ApplyLeyLineEffect(prev, target.GetComponent<HexTile>());
                }
                catch
                {
                    Debug.Log("Error caught in lightning Chain routine");
                }
                    GetVectorAngle(initial, final, lightning.transform);
                    Destroy(lightning, .52f);


                yield return new WaitForSeconds(delayBetweenJumps);
                this.gameObject.GetComponent<Renderer>().enabled = false;
            }
        }

        Debug.Log("Chain complete");
        Destroy(this.gameObject);
    }

    
    public void AddPath(int q, int r)
    {
        //Redefine the path for lightning to follow
        try
        {
            path.Add(HexGridManager.GetHex(q, r));
        }
        catch
        {
            Debug.Log("Unable to add to path");
        }

    }

    //gets the angle between 2 adjacent hexes and sets the apropriate transformation
    public void GetVectorAngle(HexCoords initial, HexCoords final, Transform lightningTrans)
    {

        int dq = final.q - initial.q;
        int dr = final.r - initial.r;
        //each s coord = s=-q-r
        int ds = (-1 * final.q - final.r) - (-1 * initial.q - initial.r);

        Vector3 rotPoint = new Vector3(0, 0, 0);
        rotPoint = lightningTrans.position;
        rotPoint.x += .5f;

        Vector3 axis = new Vector3(0, 0, 1);

        switch (dq, dr, ds)
        {
            case var _ when dq > 0 && true && ds < 0:
                //(1,0,-1)
                //0 degree rotation
                break;
            case var _ when dq > 0 && dr < 0 && true:
                //(1,-1,0)
                if (lightningTrans != null) lightningTrans.RotateAround(rotPoint, axis, 120);
                break;
            case var _ when true && dr < 0 && ds > 0:
                //(0,-1,1)
                if (lightningTrans != null) lightningTrans.RotateAround(rotPoint, axis, 240);
                break;
            case var _ when dq < 0 && true && ds > 0:
                //(-1,0,1)
                if (lightningTrans != null) lightningTrans.RotateAround(rotPoint, axis, 180);
                break;
            case var _ when dq < 0 && dr > 0 && true:
                //(-1,1,0)
                if (lightningTrans != null) lightningTrans.RotateAround(rotPoint, axis, 300);
                break;
            case var _ when true && dr > 0 && ds < 0:
                //(0,1,-1)
                if (lightningTrans != null) lightningTrans.RotateAround(rotPoint, axis, 60);
                break;

            default:
                Debug.Log("Something went wrong in lightning switch statement");
                break;
        }
        return;
    }
    
    private void ApplyLeyLineEffect(HexTile hexA,HexTile hexB){
        LeyLineGen.LeyLine l = GameObject.Find("LeyLineMap").GetComponent<LeyLineGen>().GetLeyLine(hexA, hexB);
        if (l == null) return;
        float mod = l.weight;
        Debug.Log($"The weight of the leyline {hexA},{hexB} is: {mod}");
        this.Damage = Damage * (1f - mod / 200f);
        Debug.Log($"Damage is: {this.Damage}");
    }
    public void Awake()
    {
        //read fields from inspector
        MoveSpeed = moveSpeed;
        Damage = currentDamage;
        Signals.Get<TeleportSignal>().AddListener(OnTeleport);
        
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
    public void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.tag == "Player")
        {
            col.gameObject.GetComponent<PlayerController>().health -= Damage;
        }
        //dissapate
    }

    public void OnTeleport(GameObject target)
    {
        //handle teleport
        if (this.Type != "Portal" && target != this.LastPortal)
        {
            transform.position = target.transform.position;
            CurrentTile = target.GetComponent<ISpell>().CurrentTile;
        }
        Debug.Log($"{gameObject} was teleported to {target}");
    }

    private void OnDestroy()
    {
        Signals.Get<TeleportSignal>().RemoveListener(OnTeleport);
    }

    public bool OverrideMoveSpell(GameObject target)
    {
        //find path
        path = HexGridManager.GetLine(CurrentTile.GetComponent<HexTile>().coords, target.GetComponent<HexTile>().coords);
        Cast();
        
        return false;
    }
}
