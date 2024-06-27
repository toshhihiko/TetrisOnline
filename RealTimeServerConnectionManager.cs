using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using Aws.GameLift.Realtime;
using Aws.GameLift.Realtime.Types;
using UnityEngine;

[System.Serializable]
public class CreatePlayerSessionResult
{
    public PlayerSessionResult PlayerSession;
}

[System.Serializable]
public class PlayerSessionResult
{
    public string PlayerId;
    public string PlayerSessionId;
    public string DnsName;
    public string Port;
}

public class RealTimeServerConnectionManager
{
    public string playerSessionId { get; private set; }
    public int playerSeverId = -1;
    public bool isConnected { get; private set; } = false;
    public Queue<Tuple<int, int, string>>receivedMessageQueue = new Queue<Tuple<int, int, string>>();
    Aws.GameLift.Realtime.Client _realtimeClient;


    public void ConnectToRemoteServer(string playsersession)
    {
        // Set values into each variables from session string with converting JSON format.
        CreatePlayerSessionResult playserSessionResult = JsonUtility.FromJson<CreatePlayerSessionResult>(playsersession);
        playerSessionId = playserSessionResult.PlayerSession.PlayerSessionId;
        int clientPort = SearchAvailableUdpPort(7777, 65000);
        int remotePort = int.Parse(playserSessionResult.PlayerSession.Port);
        string dnsName = playserSessionResult.PlayerSession.DnsName;

        if (clientPort == -1)
        {
            UnityEngine.Debug.Log("Available port is not found.");
            return;
        }
        
        _realtimeClient = new Client(new ClientConfiguration
        {
            ConnectionType = ConnectionType.RT_OVER_WS_UDP_UNSECURED
        });
        _realtimeClient.DataReceived += OnDataReceived;
        _realtimeClient.ConnectionOpen += OnConnect;
        _realtimeClient.ConnectionError += OnError;

        // Connect to GameLift RealTimeServer
        ConnectionStatus res =_realtimeClient.Connect(
            dnsName, 
            remotePort, 
            clientPort, 
            new ConnectionToken(playerSessionId, null));
        UnityEngine.Debug.Log($"[RTMessage] Status {res}");
    }

    // Enqueue messages when this callback is invoked.
    void OnDataReceived(object sender, Aws.GameLift.Realtime.Event.DataReceivedEventArgs e)
    {
        receivedMessageQueue
            .Enqueue(new Tuple<int, int, string>(
                (int) e.OpCode,
                e.Sender,
                Encoding.Default.GetString(e.Data)));
    }

    // Update connection status to connected.
    void OnConnect(object sender, EventArgs e)
    {
        UnityEngine.Debug.Log("[RTMessage] OnConnect to Server");
        isConnected = true;
    }

    void OnError(object sender, Aws.GameLift.Realtime.Event.ErrorEventArgs e)
    {
        UnityEngine.Debug.Log($"[RTMessage] Connection Error! : {e.Exception.Message}");
    }

    // Send messages to all clients in same gamesession. 
    public void SendMessageToAll(int opCode, string payload)
    {
        _realtimeClient
            .SendMessage(_realtimeClient
                .NewMessage(opCode)
                .WithDeliveryIntent(DeliveryIntent.Fast)
                .WithTargetPlayer(Constants.PLAYER_ID_SERVER)
                .WithPayload(Encoding.UTF8.GetBytes(payload)));
    }

    // Search available upd port with each way based on platform.
    int SearchAvailableUdpPort(int from = 1024, int to = 65535)
    {
        from = Mathf.Clamp(from, 1, 65535);
        to = Mathf.Clamp(to, from, 65535);

#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
        var set = GetAvailableLsofUdpPorts(from, to);
#else
        var set = GetAvailableUdpPorts ();
#endif

        for (int port = from; port <= to; port++)
            if (!set.Contains(port)) 
                return port;
        
        return -1;
    }

    HashSet<int> GetAvailableLsofUdpPorts(int from, int to)
    {
        var set = new HashSet<int>();
        
        string command = string.Join(
            " | ",
            $"lsof -nP -iUDP:{from.ToString()}-{to.ToString()}",
            "sed -E 's/->[0-9.:]+$//g'",
            @"grep -Eo '\d+$'");

        var listportProcess =
            Process.Start(new ProcessStartInfo {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{command}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false
            });

        if (listportProcess != null)
        {
            listportProcess.WaitForExit();
            var stream = listportProcess.StandardOutput;
            while (!stream.EndOfStream)
                if (int.TryParse(stream.ReadLine(), out int port)) 
                    set.Add(port);
        }
        return set;
    }

    HashSet<int> GetAvailableUdpPorts()
    {
        return new HashSet<int>(IPGlobalProperties
                .GetIPGlobalProperties()
                .GetActiveUdpListeners()
                .Select(listener => listener.Port));
    }
}
