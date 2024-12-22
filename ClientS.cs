using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class Client : MonoBehaviour
{
    public string ID = "zyzz";
    private TcpClient client;  // Declare it only once
    private Thread receiveThread;
    public string message;
    public void ReceiveServerMessages()
    {
        NetworkStream stream = client.GetStream();
        byte[] buffer = new byte[1024];

        while (true)
        {
            try
            {
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead > 0)
                {
                    message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Debug.Log("Received message: " + message);
                }
                else
                {
                    Debug.Log("Server closed the connection");
                    break;
                }
            }
            catch (Exception)
            {
                Debug.Log("Connection lost");
                break;
            }
        }
    }

    // Method to send messages to the server
    public void ClientSendMessage(string message)
    {
        NetworkStream stream = client.GetStream();

        try
        {
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            stream.Write(messageBytes, 0, messageBytes.Length);
            Debug.Log("Sent message: " + message);
        }
        catch (Exception)
        {
            Debug.Log("Connection lost while sending message");
        }
    }

    // Connect to the server
    void ConnectToServer()
    {
        try
        {
            client = new TcpClient("127.0.0.1", 5555);  // Connect to the server
            Debug.Log("Connected to the server");

            ClientSendMessage($"Connected,{ID}");

            // Start the receive thread
            receiveThread = new Thread(ReceiveServerMessages);
            receiveThread.IsBackground = true;
            receiveThread.Start();
        }
        catch (Exception)
        {
            Debug.Log("Unable to connect to the server");
        }
    }

    // Unity's Start method for initialization
    void Start()
    {
        ConnectToServer();
    }

    // Unity's Update method to handle sending messages
    void Update()
    {
        if (client != null && client.Connected)
        {
            if (Input.GetKeyDown(KeyCode.Space))  
            {
                ClientSendMessage("Ping");
            }
        }
    }

    // Cleanup on destroying the object
    void OnApplicationQuit()
    {
        try
        {
            if (client != null && client.Connected)
            {
                // Send the Disconnected message before closing
                ClientSendMessage($"Disconnected,{ID}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error while sending disconnect message: {ex.Message}");
        }
        finally
        {
            // Clean up resources
            if (receiveThread != null && receiveThread.IsAlive)
            {
                receiveThread.Abort();
            }
            if (client != null)
            {
                client.Close();
            }
        }
    }
}
