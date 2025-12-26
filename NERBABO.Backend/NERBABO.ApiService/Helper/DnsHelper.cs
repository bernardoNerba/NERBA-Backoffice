using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace NERBABO.ApiService.Helper;

public class DnsHelper
{
    // Get the local IP address programmatically
    public static string GetLocalIPAddress()
    {
        try
        {
            // First try to get the IP by connecting to a remote endpoint (doesn't actually connect)
            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                socket.Connect("8.8.8.8", 65530); // Google DNS, dummy port
                var endPoint = socket.LocalEndPoint as IPEndPoint;
                if (endPoint != null)
                {
                    return endPoint.Address.ToString();
                }
            }
        }
        catch
        {
            // Fallback to the original method
        }

        // Fallback method
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(ip))
            {
                return ip.ToString();
            }
        }
        
        return "localhost"; // Ultimate fallback
    }

    // Get all local IP addresses for more flexible CORS configuration
    public static List<string> GetAllLocalIPAddresses()
    {
        var addresses = new List<string>();
        
        try
        {
            var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            
            foreach (var networkInterface in networkInterfaces)
            {
                if (networkInterface.OperationalStatus == OperationalStatus.Up)
                {
                    var ipProperties = networkInterface.GetIPProperties();
                    
                    foreach (var ipInfo in ipProperties.UnicastAddresses)
                    {
                        if (ipInfo.Address.AddressFamily == AddressFamily.InterNetwork && 
                            !IPAddress.IsLoopback(ipInfo.Address))
                        {
                            addresses.Add(ipInfo.Address.ToString());
                        }
                    }
                }
            }
        }
        catch (Exception)
        {
            // If we can't get network interfaces, fall back to basic method
            try
            {
                addresses.Add(GetLocalIPAddress());
            }
            catch
            {
                addresses.Add("localhost");
            }
        }
        
        return addresses.Distinct().ToList();
    }

    // Generate CORS origins based on environment and configuration
    public static List<string> GenerateCorsOrigins(IConfiguration configuration, bool isDevelopment)
    {
        var origins = new List<string>();
        
        if (isDevelopment)
        {
            // Development - allow localhost and local IPs
            origins.AddRange(new[]
            {
                "http://localhost:4200",
                "https://localhost:4200",
                "http://127.0.0.1:4200",
                "https://127.0.0.1:4200"
            });
            
            // Add all local IP addresses
            var localIPs = GetAllLocalIPAddresses();
            foreach (var ip in localIPs)
            {
                origins.Add($"http://{ip}:4200");
                origins.Add($"https://{ip}:4200");
            }
        }
        else
        {
            // Production - use configured origins
            var configuredOrigins = configuration.GetSection("CORS:AllowedOrigins").Get<string[]>();
            if (configuredOrigins != null && configuredOrigins.Length > 0)
            {
                origins.AddRange(configuredOrigins);
            }
            else
            {
                // Fallback for production - try to determine from JWT ClientUrl
                var clientUrl = configuration["JWT:ClientUrl"];
                if (!string.IsNullOrEmpty(clientUrl))
                {
                    origins.Add(clientUrl);
                }
                
                // Add current server IP with standard web ports (for nginx reverse proxy)
                try
                {
                    var serverIP = GetLocalIPAddress();
                    // Add standard web ports (nginx reverse proxy)
                    origins.Add($"http://{serverIP}");
                    origins.Add($"https://{serverIP}");
                    origins.Add($"http://{serverIP}:80");
                    origins.Add($"https://{serverIP}:443");

                    // Add direct Docker ports (when not behind proxy)
                    origins.Add($"http://{serverIP}:4200");
                    origins.Add($"https://{serverIP}:4200");
                }
                catch
                {
                    // Ignore errors in production fallback
                }
            }
        }
        
        return origins.Distinct().ToList();
    }
}