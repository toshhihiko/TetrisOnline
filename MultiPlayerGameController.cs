using Google.Protobuf.WellKnownTypes;
using UnityEngine;
using UnityEngine.UI;

public class MultiPlayerGameController : MonoBehaviour
{
    [SerializeField]InputField _jsonInputField;
    [SerializeField]GameArea gameArea;
    [SerializeField]GameAreaOpponent gameAreaOpponent;

    RealTimeServerConnectionManager _rtClient;
    private int _updateRate = 0;
    

    void Start()
    {
        UnityEngine.Debug.Log("InitGame ");
    }

    void FixedUpdate()
    {
        // Avoid sending messages until this client connect to GameLift RealTimeServer.
        if (_rtClient == null) 
            return;
        if (_rtClient.isConnected == false) 
            return;

        DoReceivedMessage();

        _updateRate++;
        if (_updateRate % 10 == 0)
        {
            if (_rtClient.playerSeverId == -1)
                _rtClient.SendMessageToAll(2, _rtClient.playerSessionId);
            else
                SendMyCharacterPosition();
        }
    }

    // Pull queue messages and process messages based on OpCode defined GameLift
    public void DoReceivedMessage()
    {
        while (_rtClient.receivedMessageQueue.Count > 0)
        {
            var item = _rtClient.receivedMessageQueue.Dequeue();
            UnityEngine.Debug.Log("[RTMessage] OnDataReceived" + $" OpCode = {item.Item1}, senderid = {item.Item2} : {item.Item3}");

            if (item.Item1 == 2 && item.Item3 == _rtClient.playerSessionId)
                _rtClient.playerSeverId = item.Item2;
            if (item.Item1 == 1)
                UpdateField(item.Item2, item.Item3);
        }
    }

    // Connect to GameLift RealTimeServer based on the text of InputField.
    public void Conect()
    {
        _rtClient = new RealTimeServerConnectionManager();
        _rtClient.ConnectToRemoteServer(_jsonInputField.text);
    }

    // Update position and create other character's GameObject when it is not created yet.
    public void UpdateField(int playerid, string message)
    {
        if (_rtClient.playerSeverId == playerid) 
        {
            gameAreaOpponent.field = ConvertStringToIntArray(message);
            return;
        }
    }

    private void SendMyCharacterPosition()
    {
        string message = ConvertIntArrayToString(gameArea.field);
        _rtClient.SendMessageToAll(1, message);
    }

    public static string ConvertIntArrayToString(int[,] intArray)
    {
        int rows = intArray.GetLength(0);
        int cols = intArray.GetLength(1);
        string result = "";

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                result += intArray[i, j].ToString();
                if (j < cols - 1)
                    result += ",";
            }
            if (i < rows - 1)
                result += ";";
        }
        return result;
    }

    public static int[,] ConvertStringToIntArray(string intArrayAsString)
    {
        string[] rows = intArrayAsString.Split(';');
        int rowsCount = rows.Length;
        int colsCount = rows[0].Split(',').Length;
        int[,] result = new int[rowsCount, colsCount];

        for (int i = 0; i < rowsCount; i++)
        {
            string[] rowValues = rows[i].Split(',');
            for (int j = 0; j < colsCount; j++)
            {
                result[i, j] = int.Parse(rowValues[j]);
            }
        }
        return result;
    }
}
