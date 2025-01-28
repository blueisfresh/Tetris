using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Tetris;

public class SocketManager
{
    
    // TODO: Add the socket communication between client and server
    private static IPAddress ipAddress;
    
    IPEndPoint ipEndPoint = new(ipAddress, 500);
    
    public static async Task RunClientAsync(IPEndPoint ipEndPoint)
    {
        using Socket client = new(
            ipEndPoint.AddressFamily,
            SocketType.Stream,
            ProtocolType.Tcp);

        try
        {
            // Connect to the server
            await client.ConnectAsync(ipEndPoint);
            Console.WriteLine($"Connected to {ipEndPoint}");

            while (true)
            {
                // Send message
                var message = "Hi friends 👋!<|EOM|>";
                var messageBytes = Encoding.UTF8.GetBytes(message);
                _ = await client.SendAsync(messageBytes, SocketFlags.None);
                Console.WriteLine($"Socket client sent message: \"{message}\"");

                // Receive acknowledgment
                var buffer = new byte[1024];
                var received = await client.ReceiveAsync(buffer, SocketFlags.None);
                var response = Encoding.UTF8.GetString(buffer, 0, received);

                if (response == "<|ACK|>")
                {
                    Console.WriteLine($"Socket client received acknowledgment: \"{response}\"");
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            // Clean up
            client.Shutdown(SocketShutdown.Both);
            client.Close();
            Console.WriteLine("Connection closed.");
        }
    }

    public static async Task RunServerAsync(IPEndPoint ipEndPoint)
    {
        using Socket listener = new(
            ipEndPoint.AddressFamily,
            SocketType.Stream,
            ProtocolType.Tcp);

        try
        {
            // Bind the listener to the IP endpoint and start listening
            listener.Bind(ipEndPoint);
            listener.Listen(100);
            Console.WriteLine($"Server is listening on {ipEndPoint}");

            // Accept an incoming connection
            using Socket handler = await listener.AcceptAsync();
            Console.WriteLine("Client connected.");

            while (true)
            {
                // Receive message from client
                var buffer = new byte[1024];
                var received = await handler.ReceiveAsync(buffer, SocketFlags.None);
                var message = Encoding.UTF8.GetString(buffer, 0, received);

                const string eom = "<|EOM|>";
                if (message.IndexOf(eom) > -1) // End of message detected
                {
                    Console.WriteLine($"Socket server received message: \"{message.Replace(eom, "")}\"");

                    // Send acknowledgment back to client
                    var ackMessage = "<|ACK|>";
                    var ackBytes = Encoding.UTF8.GetBytes(ackMessage);
                    await handler.SendAsync(ackBytes, SocketFlags.None);
                    Console.WriteLine($"Socket server sent acknowledgment: \"{ackMessage}\"");
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            Console.WriteLine("Server shutting down.");
            listener.Close();
        }
    }
}