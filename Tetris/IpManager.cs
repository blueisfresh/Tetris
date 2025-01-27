using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Tetris;

public class IpManager
{
    public static List<string> IpAdresses = new List<string>();
    
    /*
    public static string GetLocalIpAddress()
    {
        UnicastIPAddressInformation mostSuitableIp = null;

        var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

        foreach (var network in networkInterfaces)
        {
            if (network.OperationalStatus != OperationalStatus.Up)
                continue;

            var properties = network.GetIPProperties();

            if (properties.GatewayAddresses.Count == 0)
                continue;

            foreach (var address in properties.UnicastAddresses)
            {
                if (address.Address.AddressFamily != AddressFamily.InterNetwork)
                    continue;

                if (IPAddress.IsLoopback(address.Address))
                    continue;

                if (!address.IsDnsEligible)
                {
                    if (mostSuitableIp == null)
                        mostSuitableIp = address;
                    continue;
                }

                // The best IP is the IP got from DHCP server
                if (address.PrefixOrigin != PrefixOrigin.Dhcp)
                {
                    if (mostSuitableIp == null || !mostSuitableIp.IsDnsEligible)
                        mostSuitableIp = address;
                    continue;
                }

                return address.Address.ToString();
            }
        }

        return mostSuitableIp != null 
            ? mostSuitableIp.Address.ToString()
            : "";
    }
    */
    
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