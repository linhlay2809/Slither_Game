using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Advertisements;
using MLAPI;
using UnityEngine.Networking;
using MLAPI.Messaging;

public class SnakeMovement : NetworkBehaviour
{
    //	##### added by Yue Chen #####
	private int moveWay ;    // It determines how to control the movement of snake, gained from initial interface
	private int skinID ;     // It determines the skin of the snake, gained from initial interface
	private string nickName;

    public List<Transform> bodyParts = new List<Transform>();   // Records the location information of body parts of the snake

    public Spawns spawn;

    public Camera cameraMain;

    public float snakeWalkSpeed = 3.5f; // Called in SnakeMove()
    private float snakeRunSpeed = 7.0f;  // Called in SnakeRun()
    private bool isRunning; // Called in SnakeRun()
    //public float runBodyPartSmoothTime = 0.1f; // // Called in SnakeRun()
   // private float cameraSmoothTime = 0.13f;  // Called in CameraFollowSnake()

	public float runBodyPartSmoothTime = 0.1f; // // Called in SnakeRun()
	private float cameraSmoothTime = 0.13f;  // Called in CameraFollowSnake()

    public Transform addBodyPart;   // Called in OnCollisionEnter(), it is the thing added behind the snake after eating food

    //Called in SizeUp(), determine after eating how much food the snake will grow its size
    private int[] sizeUpArray = {2, 4, 8, 16, 32, 64, 128, 256, 512, 1024, 2048, 5096, 10192, 20348, 999999};

    public int foodCounter;    // Called in OnCollisionEnter(), the number of food the snake eats so far
    public int curSizeLevel;    // Called in OnCollisionEnter()
    public Vector3 curSize = Vector3.one;  //Called in SizeUp()
    private float growRate = 0.1f;   //Called in OnCollisionEnter(), how much to grow snake size
    private float cameraGrowRate = 0.03f;    // Called in OnCollisionEnter(), when snake gets larger, camera size gets larger as well
    private float bodyPartSmoothTime = 0.2f; //Called in OnCollisionEnter(), the same value as in SnakeBodyActions.cs
    private float bodySmoothTime; // Called in SetBodySizeAndSmoothTime()

    //	##### added by Yue Chen #####

    public int length;
    public int id; // id player

    Vector3 currentPos;
    // use this for initialization
    void Start() {
        spawn = GameObject.FindGameObjectWithTag("Spawn").GetComponent<Spawns>();
        //	##### added by Yue Chen #####
        moveWay = PlayerPrefs.GetInt("moveWayID", 1);    // It determines how to control the movement of snake, gained from initial interface
                                                         // It determines the skin of the snake, gained from initial interface
        skinID = PlayerPrefs.GetInt("skinID", 1);
        nickName = PlayerPrefs.GetString("nickname", "");
        cameraMain = Instantiate(cameraMain, transform.position, transform.rotation);

        AddBodyPartServerRpc();

        AddBodyPartServerRpc();
        if (IsLocalPlayer)
        {
            return;
        }
        cameraMain.enabled = false;
        
    }

    // update is called once per frame
    void Update()
    {
        if (IsLocalPlayer)
        {
            MouseControlSnake();
            ColorSnake(skinID);
            SnakeRun();
            SetScore(length);
            if (snakeWalkSpeed < 4)
            {
                snakeWalkSpeed = 4;
            }
        }
        
    }

    void FixedUpdate()
    {
        if (IsLocalPlayer)
        {
            SnakeMove();
            SetBodySizeAndSmoothTime();
            
            SnakeGlowing(isRunning);
            SnakeMoveAdjust();
            //	##### added by Yue Chen #####

        }
        CameraFollowSnake();
    }

    [System.Obsolete]
    void OnCollisionEnter(Collision obj)
    {
        if (obj.gameObject.CompareTag("Food"))
        {
            NetworkObject.Destroy(obj.gameObject);
            spawn.curAmountOfFood--;
            if (SizeUp(foodCounter) == false)
            {
                length++;
                foodCounter++;
                // The contents in 'if' shouldn't be exectued in logic as we always have several body parts
                

                AddBodyPartServerRpc();
            }
            else
            {
                length++;
                foodCounter++;
                // The contents in 'if' shouldn't be exectued in logic as we always have several body parts
                
                
                AddBodyPartServerRpc();
                curSize += Vector3.one * growRate;
                bodyPartSmoothTime += 0.01f;
                transform.localScale = curSize;
                // Scale up camera
                cameraMain.orthographicSize += cameraMain.orthographicSize * cameraGrowRate;
            }
        }
        //	##### added by Morgan #####
        else if (obj.transform.tag == "Item")
        {
            NetworkObject.Destroy(obj.gameObject);
            if (obj.transform.GetComponent<ParticleSystem>().startColor == new Color32(255, 0, 255, 255))
            {
                spawn.curAmountOfItem--;
                snakeWalkSpeed += 3.5f;
                StartCoroutine("speedUpTime");
            }
            if (obj.transform.GetComponent<ParticleSystem>().startColor == new Color32(0, 255, 0, 255))
            {
                spawn.curAmountOfItem--;
                if (bodyParts.Count > 4)
                {
                    isRunning = true;
                    StartCoroutine("punishTime");
                }
            }
        }
    }
    [ServerRpc(Delivery = RpcDelivery.Unreliable)]
    void AddBodyPartServerRpc()
    {
        AddBodyPartClientRpc();
    }
    [ClientRpc(Delivery = RpcDelivery.Unreliable)]
    void AddBodyPartClientRpc()
    {
        if (this.bodyParts.Count == 0)
        {
            currentPos = this.transform.position;
        }
        else
        {
            currentPos = this.bodyParts[bodyParts.Count - 1].position;
        }
        Transform newPart = Instantiate(addBodyPart, this.currentPos, Quaternion.identity) as Transform;
        newPart.parent = GameObject.Find("SnakeBodies").transform;
        newPart.GetComponent<SnakeBodyActions>().owner = this.gameObject;
        bodyParts.Add(newPart);
    }

    /* When the head encounters an object, figure out what to do*/
    void OnTriggerEnter(Collider obj)
    {
        if (obj.transform.tag == "Snake")
        {
            bool isMyself = false;
            Transform myself = obj.gameObject.transform;
            foreach (Transform part in bodyParts)
            {
                if (part.Equals(myself))
                    isMyself = true;
            }
            if (isMyself == false)
            {
                DeadServerRpc();

            }
        }
        else if (obj.CompareTag("Boundary"))
        {
            DeadServerRpc();

        }
    }
    [ServerRpc]
    private void DeadServerRpc()
    {
        DeadClientRpc();
    }
    [ClientRpc]
    private void DeadClientRpc()
    {
        while (this.bodyParts.Count > 0)
        {
            int lastIndex = this.bodyParts.Count - 1;
            Transform lastBodyPart = this.bodyParts[lastIndex].transform;
            Destroy(lastBodyPart.gameObject);
            this.bodyParts.RemoveAt(lastIndex); 
            NetworkObject newFood = Instantiate(spawn.foodGenerateTarget[Random.Range(0, spawn.foodGenerateTarget.Length)], lastBodyPart.position, Quaternion.identity);
            newFood.transform.parent = GameObject.Find("Foods").transform;
            
        }
        if (!IsLocalPlayer)
        {
            return;
        }
        NetworkObject.Destroy(this.gameObject);
    }
    //	##### added by Morgan #####
    IEnumerator speedUpTime()
    {
        yield return new WaitForSeconds(2);
        snakeWalkSpeed -= 3.5f;
    }
    IEnumerator punishTime()
    {
        yield return new WaitForSeconds(2);
        isRunning = false;
		snakeWalkSpeed = 4f;
     
    }


    /* When losing body parts, snake size down*/

    void SnakeScaleChange() {
        if (curSizeLevel > 0 && foodCounter <= sizeUpArray[curSizeLevel - 1]) {
            curSizeLevel--;
            curSize -= Vector3.one * growRate;
            bodyPartSmoothTime -= 0.01f;
            transform.localScale = curSize;
            // Scale down camera
            cameraMain.orthographicSize -= cameraMain.orthographicSize * cameraGrowRate;
        }
    }

    /* Figure out whether snake size increases after eating*/
    bool SizeUp(int x)
    {
        if (x == sizeUpArray[curSizeLevel]) {
            curSizeLevel++;
            return true;
        }
        return false;
    }

    /* Set the size and smooth time of snake body parts every frame*/
    void SetBodySizeAndSmoothTime()
    {
        transform.localScale = curSize;
        if (snakeWalkSpeed >= snakeRunSpeed)
        {
            bodySmoothTime = runBodyPartSmoothTime;
        }
        else
        {
            bodySmoothTime = bodyPartSmoothTime;
        }
        foreach (Transform part in bodyParts)
        {
            part.localScale = curSize;
            part.GetComponent<SnakeBodyActions>().smoothTime = bodySmoothTime;
        }
    }
    

    /* Make the snake head move forward all the time*/
    void SnakeMove()
    {
        transform.position += transform.forward * snakeWalkSpeed * Time.deltaTime;
    }
    /* Make the camera follow the snake when it moves*/
    void CameraFollowSnake()
    {
        Vector3 velocity = Vector3.zero;
        // Reach from current position to target position smoothly
        cameraMain.transform.position = Vector3.SmoothDamp(cameraMain.transform.position,
            new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, -10)
            , ref velocity, cameraSmoothTime);
    }

    /* Make the snake run when it should run, and lose parts*/
    private float t1;
    private float t2;
    void SnakeRun() {
        if (bodyParts.Count > 2)
        {
            if (snakeWalkSpeed <= 4)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    t2 = Time.realtimeSinceStartup;
                    if (t2 - t1 < 0.2)
                    {
                        isRunning = true;
                        snakeWalkSpeed = snakeRunSpeed;
                    }
                    t1 = t2;
                }
            }
                if (Input.GetMouseButtonUp(0) && isRunning == true)
                {
                    isRunning = false;
                    snakeWalkSpeed = 4f;

                }
            
            
        }
        else {
            isRunning = false;         
			snakeWalkSpeed = 4f;
        }
        if (isRunning == true)
        {
            LosingBodyServerRpc();
        }
        
    }
    [ServerRpc]
    private void LosingBodyServerRpc()
    {
        LosingBodyClientRpc();
    }
    [ClientRpc]
    private void LosingBodyClientRpc()
    {
        StartCoroutine("LosingBodyParts");
    }
    IEnumerator LosingBodyParts() {
        yield return new WaitForSeconds(1f);  // Every 0.8 second lose one body part
        StopCoroutine("LosingBodyParts");
        int lastIndex = this.bodyParts.Count - 1;
        Transform lastBodyPart = this.bodyParts[lastIndex].transform;
        this.bodyParts.RemoveAt(lastIndex);
        Instantiate(spawn.foodGenerateTarget[Random.Range(0, spawn.foodGenerateTarget.Length)], lastBodyPart.position, Quaternion.identity);
        NetworkObject.Destroy(lastBodyPart.gameObject);
        spawn.curAmountOfFood++;
        foodCounter--;
		length--;
        SnakeScaleChange();
    }
    /* If snake is running, then glowing*/
    void SnakeGlowing(bool isRunning) {
        foreach (Transform part in this.bodyParts) {
            part.Find("Glowing").gameObject.SetActive(isRunning);
        }
    }


    /* Sanke moves toward finger*/
    private Vector3 pointInWorld, mousePosition, direction;
    private float radius = 20.0f;
    void MouseControlSnake()
    {
        Ray ray = cameraMain.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
        RaycastHit hit; // Store the first obj touched by ray
        Physics.Raycast(ray, out hit, 50.0f); // The third parameter is the max distance
        mousePosition = new Vector3(hit.point.x, hit.point.y, 0);
        direction = Vector3.Slerp(direction, mousePosition - transform.position, Time.deltaTime * 3);
        direction.z = 0;
        pointInWorld = direction.normalized * radius + transform.position;
        transform.LookAt(pointInWorld);
    }


    void SnakeMoveAdjust()
    {
        Vector3 temp = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, 0);
        gameObject.transform.position = temp;
    }

    /* Choose the skin of snake*/
    public Material blue, red, orange;
    void ColorSnake(int id) {
        switch (id)
        {
            case 1: BlueAndWhite(); break;
            case 2: RedAndWhite(); break;
            case 3: OrangeAndWhite();break;
        }
    }
    void BlueAndWhite()
    {
        int n = 2;
        int m = 3;
        for (int i = 0; i < bodyParts.Count; i++)
        {
            if (i == n)
            {
                n = n + 4;
                bodyParts[i].GetComponent<Renderer>().material = blue;
            }
            else if (i == m)
            {
                m = m + 4;
                bodyParts[i].GetComponent<Renderer>().material = blue;
            }
        }
    }
    void RedAndWhite()
    {
        for (int i = 0; i < bodyParts.Count; i++)
        {
            for (int j = 0; j < bodyParts.Count; j++)
            {

            }
            if (i % 2 == 0)
            {
                bodyParts[i].GetComponent<Renderer>().material = red;
            }
        }
    }
    void OrangeAndWhite()
    {
        for (int i = 0; i < bodyParts.Count; i++)
        {
            if (i % 2 == 0)
            {
                bodyParts[i].GetComponent<Renderer>().material = orange;
            }
        }
    }



	public void SetScore(int curScore){
		int bestScore;
		PlayerPrefs.SetInt ("FinalScore",curScore );
		bestScore = PlayerPrefs.GetInt ("BestScore", 0);
		if (bestScore < curScore) {
			PlayerPrefs.SetInt ("BestScore", curScore);
		}
	}


}
