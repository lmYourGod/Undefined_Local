using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy_IA : MonoBehaviour
{
    [SerializeField] protected EnemyScriptStorage _enemyScriptStorage;
    
    [Header("--- NAVMESH ---")]
    [Space(10)]
    [SerializeField] protected NavMeshAgent _navMeshAgent;
    
    [Header("--- WAYPOINTS ---")]
    [Space(10)]
    [SerializeField] private Transform waypointStorage;
    [SerializeField] protected List<Transform> waypointsList;
    [SerializeField] protected int waypointsListIndex;
    
    [Header("--- DETECTION ---")]
    [Space(10)]
    [SerializeField] protected bool isPlayerDetected;
    
    [Header("--- HEALTH ---")]
    [Space(10)]
    [SerializeField] protected int maxHealth;
    [SerializeField] protected int currentHealth;
    
    [Header("--- ANIMATOR ---")]
    [Space(10)]
    [SerializeField] protected Animator _animator;
    
    [Header("--- FPS ---")]
    [Space(10)]
    [SerializeField] protected Transform fps;

    private void Awake()
    {
        _enemyScriptStorage = GetComponent<EnemyScriptStorage>();
        _navMeshAgent = GetComponent<NavMeshAgent>();
        waypointStorage = transform.parent.GetChild(1);
        
        waypointsList.AddRange(waypointStorage.GetComponentsInChildren<Transform>());
        waypointsList.Remove(waypointsList[0]);
        waypointsListIndex = 0;
    }

    public virtual void Start()
    {
        //Si hay waypoints en la lista se setea el destino en el primero de esta;
        if (waypointsList.Count != 0)
        {
            _navMeshAgent.SetDestination(waypointsList[waypointsListIndex].position);
        }
    }

    public virtual void Update()
    {
        Debug.DrawLine(transform.position, _navMeshAgent.destination, Color.red, 0.1f);
        CheckPlayerDetectedStatus();
        FPS_PositionControl();
    }

    private void CheckPlayerDetectedStatus()
    {
        //Solo si el player no es detectado hara la lógica;
        if (!isPlayerDetected)
        {
            //Si el NPC tiene un path asignado y no ve al player hace su path;
            if (_navMeshAgent.hasPath && !_enemyScriptStorage.FieldOfView.canSeePlayer)
            {
                Debug.Log("Player Not Detected");
                UpdatePath();   
            }

            //Si el NPC ve al Player se activa el bool "isPlayerDetected";
            if (_enemyScriptStorage.FieldOfView.canSeePlayer)
            {
                isPlayerDetected = true;
            } 
        }
        else
        {
            _navMeshAgent.stoppingDistance = 0.1f;
        }
    }

    #region - PATH WITH WAYPOINTS -
    
    //Método para comprobar si el NPC tiene que actualizar el waypoint;
    private void UpdatePath()
    {
        if (Vector3.Distance(transform.position, _navMeshAgent.destination) < 0.3f)
        {
            UpdateWaypoint();
        }
    }

    //Método para actualizar el waypoint al que tiene que ir el NPC;
    private void UpdateWaypoint()
    {
        waypointsListIndex = (waypointsListIndex + 1) % waypointsList.Count;
        _navMeshAgent.SetDestination(waypointsList[waypointsListIndex].position);
    }
    
    #endregion

    #region - ACTIVATE ALARM -
    
    //Método para ir a activar la alarma;
    public void GoActivateAlarm()
    {
        //Si la alarma ya ha sido activada no realizara la lógica restante;
        if (Level1Manager.instance.AlarmActivated) return;
        
        Debug.Log("Going Activate Alarm");
        Debug.Log(Vector3.Distance(transform.position, Level1Manager.instance.AlarmWaypoint.position));
        
        _navMeshAgent.SetDestination(Level1Manager.instance.AlarmWaypoint.position);
        _navMeshAgent.speed = 3f;

        //Si la distancia del NPC con la Alarma es menor a 0.1m se activará;
        if (Vector3.Distance(transform.position, Level1Manager.instance.AlarmWaypoint.position) < 0.1f)
        {
            Level1Manager.instance.AlarmActivated = true;
            
            //Una vez activada la alarma todos los NPCs estarán alerta;
            foreach (Enemy_IA enemy in FindObjectsOfType<Enemy_IA>())
            {
                enemy.isPlayerDetected = true;
            }
        }
    }
    
    #endregion

    //Método parar recibir daño
    public void TakeDamage(int damage)
    {
        //Si el NPC tiene vida se la podremos quitar
        if (currentHealth > 0)
        {
            currentHealth -= damage;
        
            //Si la vida llega a 0 muere;
            if (currentHealth <= 0)
            {
                Die();
            } 
        }
    }

    //Método para decidir que hacer cuando el NPC muere;
    private void Die()
    {
        _navMeshAgent.ResetPath();
        _navMeshAgent.enabled = false;
        _animator.enabled = false;
        _enemyScriptStorage.FieldOfView.gameObject.SetActive(false);
    }

    
    private void FPS_PositionControl()
    {
        fps.position = transform.position;
        fps.rotation = transform.rotation;
    }
}
