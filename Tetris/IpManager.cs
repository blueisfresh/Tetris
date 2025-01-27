using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Tetris;

public class IpManager
{
    public static List<string> IpAdresses = new List<string>()
    {
        "192.168.1.11",
        "192.168.1.9"
    };
    
    // option to display localipadress
    public static string GetLocalIpAddress()
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
    
    public static string GetLocalNetworkIpAddress()
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
    
    public static bool IsValidIpAddress(string ip)
    {
        return System.Net.IPAddress.TryParse(ip, out _);
    }

    public static bool CanConnectTo(string ip)
    {
        Ping ping = new Ping();

        IPAddress address = IPAddress.Parse(ip); 
        
        PingReply pong = ping.Send(address);
        if (pong.Status == IPStatus.Success)
        {
            return true;
        } 
        
        return false;
    }
}