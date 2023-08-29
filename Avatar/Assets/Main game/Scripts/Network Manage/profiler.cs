using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;
using System.Collections;


public class profiler : MonoBehaviour
{
    private UnityEngine.Ping ping;
    private float startTime;
    private bool testInProgress;

    private async Task StartTest()
    {
        // Set the IP address and port to use for the test
        string ipAddress = "127.0.0.1";
        int port = 9000;

        // Create a new UDP client to send the ping request
        UdpClient client = new UdpClient();

        // Get the current time in ticks
        long sendTime = DateTime.Now.Ticks;

        // Create a new byte array to use as the ping request message
        byte[] data = new byte[8];

        // Send the ping request to the specified IP address and port
        await client.SendAsync(data, data.Length, ipAddress, port);

        // Wait for a response from the server
        UdpReceiveResult result = await client.ReceiveAsync();
        IPEndPoint serverEndPoint = result.RemoteEndPoint;
        byte[] responseData = result.Buffer;

        // Get the current time in ticks again
        long receiveTime = DateTime.Now.Ticks;

        // Calculate the round-trip time and display the results
        long roundTripTime = receiveTime - sendTime;
        Debug.Log("Round-trip time: " + roundTripTime + " ticks");

        // Close the UDP client
        client.Close();
    }

    private IEnumerator TestSpeed()
    {
        while (testInProgress)
        {
            if (ping.isDone)
            {
                float elapsedTime = Time.time - startTime;
                float pingTime = ping.time;
                Debug.Log("Ping time: " + pingTime + "ms");
                Debug.Log("Round-trip time: " + elapsedTime + "s");
                ping.DestroyPing();
                testInProgress = false;
            }
            yield return null;
        }
    }
}
