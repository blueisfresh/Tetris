using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Tetris;

public sealed class IpManager
{
    private static IpManager instance = null!;
    
    private static readonly object padlock = new object();

    IpManager()
    {
        
    }

    public static IpManager Instance
    {
        get
        {
            lock (padlock)
            {
                if (instance == null)
                {
                    instance = new IpManager();
                }
                
                return instance;
            }
        }
    }
    
    public List<string> IpAdresses = new List<string>()
    {
        "192.168.1.11",
        "192.168.1.9"
    };
    
    // option to display localipadress
    public string GetLocalIpAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        throw new Exception("No network adapters with an IPv4 address in the system!");
    }
    
    public string GetLocalNetworkIpAddress()
    {
        foreach (var networkInterface in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (networkInterface.OperationalStatus == OperationalStatus.Up)
            {
                var properties = networkInterface.GetIPProperties();

                // Check if the interface has a gateway, indicating it's connected to a network
                if (properties.GatewayAddresses.Count > 0)
                {
                    foreach (var address in properties.UnicastAddresses)
                    {
                        if (address.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            return address.Address.ToString(); // Return the IPv4 address as a string
                        }
                    }
                }
            }
        }

        return "No local network IP address found.";
    }
    
    public bool IsValidIpAddress(string ip)
    {
        return System.Net.IPAddress.TryParse(ip, out _);
    }

    public bool CanConnectTo(string ip)
    {
        try
        {
            // Display the Ip address and Loading line in the same loop
            Console.Write($"\n{ip} - Loading");

            Ping ping = new Ping();
            IPAddress address = IPAddress.Parse(ip);

            // Perform pinging 
            PingReply pong = ping.Send(address);
            
            // Update the message directly on the same line
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write($"{ip} - {(pong.Status == IPStatus.Success ? "ok" : "not ok!")}        ");
            
            return pong.Status == IPStatus.Success;
        }
        catch (Exception e)
        {
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write($"{ip} - not ok!        "); // Ensure the line is completely overwritten
            Debug.WriteLine(e);
            return false;
        }
    }
}