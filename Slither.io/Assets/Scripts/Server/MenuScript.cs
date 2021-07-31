using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Transports.UNET;
using UnityEngine.UI;
using System;
using System.Text;

public class MenuScript : MonoBehaviour
{
    [SerializeField]
    private GameObject menuPanel;
    public Spawns spawns;
    public InputField inputField;
    [SerializeField] private GameObject leaveButton;
    private void Start()
    {
        spawns = GameObject.FindGameObjectWithTag("Spawn").GetComponent<Spawns>();
        NetworkManager.Singleton.OnServerStarted += HandleServerStarted;
        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnect;
    }
    private void OnDestroy()
    {
        if (NetworkManager.Singleton == null)
        {
            return;
        }

        NetworkManager.Singleton.OnServerStarted -= HandleServerStarted;
        NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnect;
    }
    private void HandleClientConnected(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            menuPanel.SetActive(false);
            leaveButton.SetActive(true);
        }
    }
    private void HandleClientDisconnect(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            menuPanel.SetActive(true);
            leaveButton.SetActive(false);
        }
    }
    private void HandleServerStarted()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            HandleClientConnected(NetworkManager.Singleton.LocalClientId);
        }
    }
    private void ApprovalCheck(byte[] connectionData, ulong clientID, NetworkManager.ConnectionApprovedDelegate callback)
    {
        string passWord = System.Text.Encoding.ASCII.GetString(connectionData);
        bool approve = passWord == inputField.text;
        Debug.Log($"Approval: {approve}");
        callback(true, null, approve, new Vector3(0, 0, 0), Quaternion.identity);
    }

    public void Host()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        NetworkManager.Singleton.StartHost();
        spawns.GenerateFoodBeforeBeginServerRpc();
        spawns.isSpawnFoodAndItem = true;
    }
    public void Join()
    {
        NetworkManager.Singleton.NetworkConfig.ConnectionData = Encoding.ASCII.GetBytes(inputField.text);
        NetworkManager.Singleton.StartClient();
    }
    public void Leave()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.StopHost();
            NetworkManager.Singleton.ConnectionApprovalCallback -= ApprovalCheck;
        }
        else if (NetworkManager.Singleton.IsClient)
        {
            NetworkManager.Singleton.StopClient();
        }
        menuPanel.SetActive(true);
        leaveButton.SetActive(false);
    }
}
