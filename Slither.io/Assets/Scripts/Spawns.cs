using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Messaging;

public class Spawns : MonoBehaviour
{
    public List<GameObject> Robots = new List<GameObject>();    // Records the information of Robots
    public int curAmountOfRobot, maxAmountOfRobot = 30;  // The max amount of robots in the map
    public GameObject[] robotGenerateTarget;     // Store the objects of robot snakes

    /* Generate food points every few seconds until there are enough points on the map*/
    public int curAmountOfFood, maxAmountOfFood = 600;  // The max amount of food in the map
    public int curAmountOfItem, maxAmountOfItem = 60;  // The max amount of item in the map
    private float foodGenerateEveryXSecond = 0.1f;   // Generate a food point every 3 seconds
    public NetworkObject[] foodGenerateTarget;     // Store the objects of food points
    public NetworkObject[] itemGenerateTarget;     // Store the objects of item
    public bool isSpawnFoodAndItem = false;

    

    // Start is called before the first frame update
    void Start()
    {
        GenerateRobotBeforeBegin();
    }

    // Update is called once per frame
    void Update()
    {
        GenerateFoodAndItem();
    }
    void GenerateRobotBeforeBegin()
    {
        int i = 0;
        while (i < 0)
        {
            int r = Random.Range(0, 2);
            Vector3 robotPos;
            if (r == 0)
            {
                robotPos = new Vector3(Random.Range(-30, 30), Random.Range(-30, 30), 0);
            }
            else
            {
                robotPos = new Vector3(Random.Range(-100, 100), Random.Range(-100, 100), 0);
            }
            GameObject newRobot = Instantiate(robotGenerateTarget[Random.Range(0, robotGenerateTarget.Length)],
                robotPos, Quaternion.identity) as GameObject;
            newRobot.name = "Robot" + i;
            Robots.Add(newRobot);
            newRobot.GetComponent<RobotAction>().SkinId = Random.Range(1, 4);
            newRobot.transform.parent = GameObject.Find("Robots").transform;
            curAmountOfRobot++;
            i++;
        }
    }

    /* Gernate 200 food points before game start*/
    [ServerRpc]
    public void GenerateFoodBeforeBeginServerRpc()
    {
        int i = 0;
        while (i < 200)
        {
            int r = Random.Range(0, 2);
            Vector3 foodPos;
            if (r == 0)
            {
                foodPos = new Vector3(Random.Range(-30, 30), Random.Range(-30, 30), 0);
            }
            else
            {
                foodPos = new Vector3(Random.Range(-60, 60), Random.Range(-60, 60), 0);
            }
            NetworkObject newFood = Instantiate(foodGenerateTarget[Random.Range(0, foodGenerateTarget.Length)], foodPos, Quaternion.identity);
            newFood.transform.parent = GameObject.Find("Foods").transform;
            newFood.GetComponent<NetworkObject>().Spawn();
            curAmountOfFood++;
            i++;
        }
    }




    
    private void GenerateFoodAndItem()
    {
        StartCoroutine("RunGenerateFoodAndItemServerRpc", foodGenerateEveryXSecond);
    }
    [ServerRpc]
    IEnumerator RunGenerateFoodAndItemServerRpc(float time)
    {
        yield return new WaitForSeconds(time);
        StopCoroutine("RunGenerateFoodAndItemServerRpc");
        if (isSpawnFoodAndItem == true)
        {
            if (curAmountOfFood < maxAmountOfFood)
            {
                int r = Random.Range(0, 4);
                Vector3 foodPos;
                if (r == 0)
                {
                    foodPos = new Vector3(Random.Range(-30, 30), Random.Range(-30, 30), 0);
                }
                else if (r <= 1)
                {
                    foodPos = new Vector3(Random.Range(-60, 60), Random.Range(-60, 60), 0);
                }
                else if (r <= 2)
                {
                    foodPos = new Vector3(Random.Range(-90, 90), Random.Range(-90, 90), 0);
                }
                else
                {
                    foodPos = new Vector3(Random.Range(-120, 120), Random.Range(-120, 120), 0);
                }
                NetworkObject newFood = Instantiate(foodGenerateTarget[Random.Range(0, foodGenerateTarget.Length)], foodPos, Quaternion.identity);
                newFood.transform.parent = GameObject.Find("Foods").transform;
                newFood.GetComponent<NetworkObject>().Spawn();
                curAmountOfFood++;
            }
            if (curAmountOfItem < maxAmountOfItem)
            {
                int r = Random.Range(0, 4);
                Vector3 itemPos;
                if (r == 0)
                {
                    itemPos = new Vector3(Random.Range(-30, 30), Random.Range(-30, 30), 0);
                }
                else if (r <= 1)
                {
                    itemPos = new Vector3(Random.Range(-60, 60), Random.Range(-60, 60), 0);
                }
                else if (r <= 2)
                {
                    itemPos = new Vector3(Random.Range(-90, 90), Random.Range(-90, 90), 0);
                }
                else
                {
                    itemPos = new Vector3(Random.Range(-120, 120), Random.Range(-120, 120), 0);
                }
                NetworkObject newItem = Instantiate(itemGenerateTarget[Random.Range(0, itemGenerateTarget.Length)], itemPos, Quaternion.identity);
                newItem.transform.parent = GameObject.Find("Items").transform;
                newItem.GetComponent<NetworkObject>().Spawn();
                curAmountOfItem++;
            }
        }
        
    }
}
