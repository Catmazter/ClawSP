using JetBrains.Annotations;
using UnityEngine;

public class GrabObject : MonoBehaviour
{
    
    
    public float speedFall = 1.0f;
    public int points = 20;
    public float camSpeed = 3;
    public float weight = 1.0f;

    Vector3 home = Vector3.zero;

    [SerializeField] static GameObject[] grabObjs;


    private void Start()
    {
        home = transform.position;
        grabObjs = GameObject.FindGameObjectsWithTag("GrabObj");
    }


    void Update()
    {
        
    }

    public void Reset()
    {
        transform.position = home;
    }

    private void OnCollisionEnter(Collision collision)
    {
        print(collision.relativeVelocity);
        if (collision.relativeVelocity.y > 2)
        {
            for (int i = 0; i < grabObjs.Length; i++)
            {
                grabObjs[i].GetComponent<GrabObject>().Reset();
            }
        }
    }


}
