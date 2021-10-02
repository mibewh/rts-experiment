using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : Selectable
{
    public GameObject spawnable;
    public float spawnDistance = 10;


    
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        
    }

    public void SpawnUnit()
    {
        Vector3 rallyPoint = transform.Find("RallyPoint").position;
        Vector3 dir = (rallyPoint - transform.position).normalized;
        Vector3 spawnPosition = transform.position + spawnDistance * dir;
        Quaternion spawnRotation = Quaternion.LookRotation(dir, Vector3.up);
        
        GameObject unitObj = Instantiate(spawnable, spawnPosition, spawnRotation);
        Unit unit = unitObj.GetComponent<Unit>();
        
        unit.MoveToward(rallyPoint);
    }
}
