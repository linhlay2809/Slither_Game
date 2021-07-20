using UnityEngine;
using System.Collections;

public class SnakeBodyActions : MonoBehaviour {

    private int myOrder;    // The order of this part in the whole snake
    public GameObject[] head;     // The location of snake head
    public GameObject owner;
    private Vector3 movementVelocity;   // The velocity of current part
    [Range(0.0f, 1.0f)]
    //public float smoothTime = 0.2f;    // The smooth time when a body part follows head
    public float smoothTime = 0.05f;
    //public float smoothTime = 0.3f;
    public int idPlayer; // get IdPlayer

    void Start() {
        head = GameObject.FindGameObjectsWithTag("Player");
        foreach(GameObject a in head)
        {
            if (a.GetComponent<SnakeMovement>().id == idPlayer)
            {
                owner = a;
                for (int i = 0; i < a.GetComponent<SnakeMovement>().bodyParts.Count; i++)
                {
                    if (gameObject == a.GetComponent<SnakeMovement>().bodyParts[i].gameObject)
                    {
                        myOrder = i;
                        break;
                    }
                }
                break;
            }
        }
        




    }

    void FixedUpdate() {
        // If the body part is the first one, then it follows the head
        if (myOrder == 0) {
            transform.position = Vector3.SmoothDamp(transform.position, owner.transform.position, ref movementVelocity, smoothTime);
            // Rotates the transform so the forward vector points at target's current position
            transform.LookAt(owner.transform.position);
        }
        // If not, then it follows previous body part
        else {
            transform.position = Vector3.SmoothDamp(transform.position,
                owner.GetComponent<SnakeMovement>().bodyParts[myOrder - 1].position, ref movementVelocity, smoothTime);
            transform.LookAt(owner.GetComponent<SnakeMovement>().bodyParts[myOrder - 1].position);
        }

    }

}
