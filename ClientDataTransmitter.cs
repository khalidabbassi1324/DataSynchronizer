using UnityEngine;
using System.Threading.Tasks;

public class AsyncPrintData : MonoBehaviour
{
    public Client clientRef;  // The reference to the Client script, now public for inspector assignment
    public int tickrate = 400;

    void Start()
    {
        // Check if clientRef is assigned in the Inspector
        if (clientRef == null)
        {
            Debug.LogError("Client Script not assigned in the Inspector!");
            return;  // Exit early to avoid further issues
        }

        RunAsync(); // Start the async task properly
    }

    private async void RunAsync()
    {
        // Await the AsyncTransform method to prevent the CS4014 warning
        await AsyncTransform();
    }

    // Asynchronous method that continuously sends transform data
    private async Task AsyncTransform()
    {
        while (clientRef != null)
        {
            try
            {
                // Check if clientRef and its properties are valid
                if (clientRef == null)
                {
                    Debug.LogError("clientRef is null.");
                    break; // Exit the loop to avoid spamming errors
                }

                if (string.IsNullOrEmpty(clientRef.ID))
                {
                    Debug.LogError("clientRef.ID is not set.");
                    break; // Exit the loop to avoid sending invalid messages
                }

                // Send the transform data asynchronously using the clientRef instance
                clientRef.ClientSendMessage($"PTransform,{clientRef.ID},{transform.position.x},{transform.position.y},{transform.position.z},{transform.rotation.eulerAngles.x},{transform.rotation.eulerAngles.y},{transform.rotation.eulerAngles.z}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error in AsyncTransform: {ex.Message}");
            }

            // Await for the next iteration with a delay (non-blocking)
            await Task.Delay(tickrate); // Delay in milliseconds (400ms = 0.4s)
        }
    }
}
