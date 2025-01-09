using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ItemMove : MonoBehaviour
{
    [SerializeField,Header("ターゲットとなるタグ")]
    private string m_TargetTag;
    [SerializeField,Header("速度")]
    private float m_Speed;

    private Transform target;

    private bool canMove = false;

    void Start()
    {
        Invoke("InitializeMovement", 3f); 
    }
    void InitializeMovement()
    {
        GameObject targetObj = GameObject.FindGameObjectWithTag(m_TargetTag);
        if (targetObj != null)
        {
            target = targetObj.transform;
            canMove = true;
        }

    }
    void Update()
    {
        if( target != null &&canMove)
        {
            Vector3 direction=target.position - transform.position;
            transform.Translate(direction.normalized*m_Speed*Time.deltaTime);
        }
    }
}
    