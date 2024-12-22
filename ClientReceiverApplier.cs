using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;

public class ClientReceiverApplier : MonoBehaviour
{
    public Client clientRef;  // Reference to the Client script
    public List<ClientData> clients = new List<ClientData>();  // List to hold client data
    public GameObject model;  // Reference to the model for new clients

    private string lastMessage = string.Empty;  // To store the last message

    private async void Start()
    {
        // Start the message handling loop
        await MessageHandlingLoopAsync();
    }

    // Async method to handle the message processing in a loop
    private async Task MessageHandlingLoopAsync()
    {
        while (true)
        {
            // Get the current message from clientRef
            string currentMessage = clientRef?.message;

            // Check if the message is different from the last one
            if (!string.IsNullOrEmpty(currentMessage) && currentMessage != lastMessage)
            {
                // Update last message to the current one
                lastMessage = currentMessage;

                // Handle the new message
                HandleMessage(currentMessage);
            }

            // Delay for a short duration to avoid locking up the game loop
            await Task.Delay(100); // Adjust the delay as needed (e.g., 100ms)
        }
    }


    // Handle incoming messages
    public void HandleMessage(string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            Debug.LogError("Received an empty or null message.");
        }

        string[] messageParts = message.Split(',');

        // Ensure the message contains valid data
        if (messageParts.Length < 2)
        {
            Debug.LogError("Invalid message format.");
            return;
        }

        string messageType = messageParts[0];  // Extract the message type (e.g., "PTransform", "Connected")

        // Ignore messages from the same client (avoid processing your own messages)
        if (messageParts[1] == clientRef?.ID)
        {
            return;
        }

        // Process based on the message type
        switch (messageType)
        {
            case "PTransform":
                HandlePTransform(messageParts);
                break;

            case "Connected":
                HandleConnected(messageParts);
                break;

            case "Disconnected":
                HandleDisconnected(messageParts);
                break;

            default:
                Debug.LogError($"Invalid message type: {messageType}");
                break;
        }
    }

    private void HandlePTransform(string[] messageParts)
    {

        string clientID = messageParts[1];
        Vector3 position = new Vector3(float.Parse(messageParts[2]), float.Parse(messageParts[3]), float.Parse(messageParts[4]));
        Quaternion rotation = Quaternion.Euler(float.Parse(messageParts[5]), float.Parse(messageParts[6]), float.Parse(messageParts[7]));

        ClientData client = clients.Find(c => c.ID == clientID);
        if (client != null)
        {
            client.clientObject.transform.position = position;
            client.clientObject.transform.rotation = rotation;
            Debug.Log($"Updated transform for client {clientID}.");
        }
        else
        {
            Debug.LogError($"Client with ID {clientID} not found.");
        }
    }

    private void HandleConnected(string[] messageParts)
    {
        if (messageParts.Length != 2)
        {
            Debug.LogError("Invalid Connected message format.");
            return;
        }

        string clientID = messageParts[1];
        if (model == null)
        {
            Debug.LogError("Model prefab is not assigned.");
            return;
        }

        GameObject newClientObject = Instantiate(model, Vector3.zero, Quaternion.identity);
        newClientObject.name = clientID;

        ClientData newClient = new ClientData
        {
            ID = clientID,
            clientObject = newClientObject
        };
        clients.Add(newClient);

        Debug.Log($"Client {clientID} connected and instantiated.");
    }

    private void HandleDisconnected(string[] messageParts)
    {
        if (messageParts.Length != 2)
        {
            Debug.LogError("Invalid Disconnected message format.");
            return;
        }

        string clientID = messageParts[1];
        ClientData clientToRemove = clients.Find(c => c.ID == clientID);

        if (clientToRemove != null)
        {
            Destroy(clientToRemove.clientObject);
            clients.Remove(clientToRemove);
            Debug.Log($"Client {clientID} disconnected and removed.");
        }
        else
        {
            Debug.LogError($"Client with ID {clientID} not found.");
        }
    }
}

// Class to hold client data
public class ClientData
{
    public string ID;
    public GameObject clientObject;
}
