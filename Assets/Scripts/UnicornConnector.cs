using UnityEngine;
// No namespace until we confirm — see note below

public class UnicornConnector : MonoBehaviour
{
    private dynamic device; // Temporarily use dynamic until we confirm exact class

    // Replace this with your actual device name if it's different
    private string deviceName = "Unicorn";

    void Start()
    {
        ConnectToUnicorn();
    }

    void ConnectToUnicorn()
    {
        try
        {
            // Assuming the constructor takes device name directly
            device = new UnicornDevice(deviceName);
            device.StartAcquisition();

            Debug.Log("✅ Connected to Unicorn device: " + deviceName);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("❌ Failed to connect: " + ex.Message);
        }
    }

    void OnApplicationQuit()
    {
        if (device != null)
        {
            device.StopAcquisition();
            device.Dispose();
        }
    }
}
