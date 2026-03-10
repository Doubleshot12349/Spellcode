using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;

public enum SyscallResult {
    Halt, SleepTurn, Nothing
}

public class StackMachine: MonoBehaviour {
    public long id;
    public SysCallManager manager;
    public bool halted = false;
    public string program;
    public LeyLineGen leyLineMap;

    public void Start()
    {
        manager = gameObject.GetComponent<SysCallManager>();
    }

    public void Awake()
    {
        Compiler.compile(program, out CompileResult res);
        id = res.id;
        Debug.Log(res.error);
    }

    public async void RunTurn()
    {
        Debug.Log("Running turn");
        if (halted) return;

        int total_allowance = 10000;
        int num_executed = 0;
        bool running = true;
        while (running)
        {
            int syscall = Compiler.run_to_syscall_or_n(id, total_allowance - num_executed, ref num_executed);
            Debug.Log($"syscall = {syscall}");
            if (syscall < 0) return;

            SyscallResult res = await SyscallHandler(syscall);
            switch (res) {
                case SyscallResult.Nothing:
                    break;
                case SyscallResult.SleepTurn:
                    running = false;
                    break;
                case SyscallResult.Halt:
                    running = false;
                    halted = true;
                    break;
            }


            if (num_executed >= total_allowance)
            {
                break;
            }
        }
    }

    public async Task<SyscallResult> SyscallHandler(int code)
    {
        switch (code)
        {
            case 0:
                //nop
               
                return SyscallResult.Nothing;
            case 1:
                //push mana
                Compiler.push_int(id, manager.GetPlayerMana());
                return SyscallResult.Nothing;
            case 2:
                //push env ID (water,fire, field, space, etc.)
                Compiler.push_int(id, manager.GetEnv());
                return SyscallResult.Nothing;
            case 3:
                //spawn effect (fireball, lightning, etc.)
                Compiler.pop_int(id, out int val);
                int eff = manager.Effect(val);
                Compiler.push_int(id, eff);
                //need to put instance ID on stack
                return SyscallResult.Nothing;
            case 4:
                //get player location (q,r)
                int q, r;
                (q, r) = manager.GetPlayerLocation();
                Compiler.push_int(id, r);
                Compiler.push_int(id, q);
                return SyscallResult.Nothing;
            case 5:
                //get clicked location
                (int, int) location = await manager.GetClickedLocation();
                //Debug.Log("pushing array");
                Compiler.PushIntArray(id, new int[] { location.Item1, location.Item2 });
                Debug.Log("pushing array");
                return SyscallResult.Nothing;
            case 6:
                //sleep a turn
                return SyscallResult.SleepTurn;
            case 7:
                //print ascii character
                
                Compiler.pop_int(id, out int c1);
                manager.Print((char)c1);
                return SyscallResult.Nothing;
            case 8:
                // halt
                return SyscallResult.Halt;
            case 9:
                // runtime exception
                return SyscallResult.Halt;
            case 10:
                // move effect
                Compiler.pop_int(id, out int q2);
                Compiler.pop_int(id, out int r2);
                Compiler.pop_int(id, out int instance_id);
                Debug.Log($"moving spell {instance_id} to {r2} {q2}");
                await manager.MoveSpell(instance_id, q2, r2);
                //temporary fix for beta testing
                return SyscallResult.SleepTurn;
            //return SyscallResult.Nothing;
            case 11:
                // GetNeighbors
                var neighbors = new List<int>();
                Compiler.pop_int(id, out int q3);
                Compiler.pop_int(id, out int r3);
                HexTile initHex = HexGridManager.GetHex(q3, r3).GetComponent<HexTile>();
                foreach (var dir in HexGridManager.NeighborDirs)
                {
                    HexTile neighborHex = HexGridManager.GetHex(q3 + dir.q, r3 + dir.r).GetComponent<HexTile>();
                    
                    if (neighborHex == null)
                    {
                        neighbors.Add(1234);
                        neighbors.Add(1234);
                        neighbors.Add(1234);
                    }
                    else
                    {
                        neighbors.Add(q3 + dir.q);
                        neighbors.Add(r3 + dir.r);
                        //Debug.Log(transform);
                        //Debug.Log(transform.GetComponentInChildren<Fire>());
                        //Debug.Log(transform.GetComponentInChildren<Fire>().leyLineMapObj);
                        //Debug.Log(transform.GetComponentInChildren<Fire>().leyLineMapObj.GetComponent<LeyLineGen>());
                        //Debug.Log(transform.GetComponentInChildren<Fire>().leyLineMapObj.GetComponent<LeyLineGen>().GetLeyLine(initHex, neighborHex));
                        neighbors.Add(Mathf.RoundToInt(leyLineMap.GetLeyLine(initHex, neighborHex).weight));
                    }
                }

                Compiler.GetNeighbors(id,neighbors.ToArray());
                return SyscallResult.Nothing;
                
            default:
                Debug.Log("Invalid opcode\n");
                return SyscallResult.Halt;
        }
    }
}

