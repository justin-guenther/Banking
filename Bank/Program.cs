using System.Net;
using System.Net.Sockets;
using System.Text;
using Boerse.abstractions;
using Newtonsoft.Json;

namespace Bank;

class Program
{
    private const int BoersePort = 11001;

    private static void Send_Rtt()
    {
        // Get host entry for boerse
        var hostEntry = Dns.GetHostEntry("boerse");
        var bankAddress = hostEntry.AddressList[0].ToString();

        // UDP socket
        Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        IPAddress address = IPAddress.Parse(bankAddress);

        byte[] sendbuf = Encoding.ASCII.GetBytes("RTT_Feedback");
        IPEndPoint ep = new IPEndPoint(address, BoersePort);

        s.SendTo(sendbuf, ep);
    }

    private static void StartUdpListener()
    {
        UdpClient listener = new UdpClient(BoersePort);
        IPEndPoint ep = new IPEndPoint(0, 0);

        Console.WriteLine("UDP Server started. Waiting for requests...");

        try
        {
            while (true)
            {
                byte[] bytes = listener.Receive(ref ep);
                
                var encodedString = Encoding.ASCII.GetString(bytes, 0, bytes.Length);
                IEnumerable<Shares>? deserializedSharesEnumerable = JsonConvert.DeserializeObject<IEnumerable<Shares>>(encodedString);

                if (deserializedSharesEnumerable != null)
                {
                    foreach (var share in deserializedSharesEnumerable) 
                    {
                        BankInfo.Portfolio += share.Price;
                    }

                    BankInfo.Shares = deserializedSharesEnumerable;
                    
                    Console.WriteLine($"Portfolio: {BankInfo.Portfolio}\n");
                    Console.WriteLine($"{encodedString}\n");

                    Send_Rtt();
                }
            }
        }
        catch (SocketException e)
        {
            Console.WriteLine(e);
        }
        finally
        {
            listener.Close();
        }
    }

    private static void StartHttpListener()
    {
        var server = new HttpServer();
        var serverSocket = server.StartHttpSocket("0.0.0.0", 11006);
        
        while (true)
        {
            // Http Socket - Wait for Requests
            var clientSocket = serverSocket.Accept();
            server.ProcessRequest(clientSocket);
        }
    }

    public static void Main()
    {
        Customer customer1 = new Customer(5000, 1000);
        Customer customer2 = new Customer(15000, 2323);
        Customer customer3 = new Customer(123, 4321);

        BankInfo.Customers.Add(customer1);
        BankInfo.Customers.Add(customer2);
        BankInfo.Customers.Add(customer3);
        
        Thread udpThread = new Thread(StartUdpListener);
        Thread httpThread = new Thread(StartHttpListener);

        udpThread.Start();
        httpThread.Start();

        udpThread.Join();
        httpThread.Join();
    }
}