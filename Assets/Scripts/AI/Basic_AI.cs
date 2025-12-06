using UnityEngine;
using UnityEngine.AI;

public class Basic_AI : MonoBehaviour
{
    private NavMeshAgent agent;
    public Transform pointB;

    public string playerTag = "Player";
    private Transform player;

    [Header("AI Color Change")]
    public Renderer playerRendererOverride;   

    private Renderer[] aiRenderers;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.SetDestination(pointB.position);

        aiRenderers = GetComponentsInChildren<Renderer>(true);
    }

    void Update()
    {
        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            Debug.Log("Destination reached");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        player = other.transform;

        var src = playerRendererOverride ? playerRendererOverride : other.GetComponentInChildren<Renderer>();
        if (src != null) ApplyColorToAI(GetColor(src));

        agent.SetDestination(player.position);
    }

    static Color GetColor(Renderer r)
    {
        var m = r.material; 
        if (m == null) return Color.white;

        if (m.HasProperty("_BaseColor")) return m.GetColor("_BaseColor"); 
        if (m.HasProperty("_Color")) return m.GetColor("_Color");    
        return Color.white;
    }

    void ApplyColorToAI(Color c)
    {
        foreach (var r in aiRenderers)
        {
            foreach (var m in r.materials) 
            {
                if (m == null) continue;
                if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
                if (m.HasProperty("_Color")) m.SetColor("_Color", c);
            }
        }
    }
}
