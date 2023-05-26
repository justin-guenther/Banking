using System.Net;
using System.Net.Sockets;
using System.Text;
using Boerse.abstractions;
using Newtonsoft.Json;
using Tynamix.ObjectFiller;

class Program
{
    static readonly string ListenPort = Environment.GetEnvironmentVariable("port");
    //static readonly string ListenPort = "11000";
    
    private static DateTime StartTime;
    private static readonly int Iterations = 1;
    private static double SumTime = 0.0;

    private static void Rtt_Test()
    {
        UdpClient listener = new UdpClient(Int32.Parse(ListenPort));
        IPEndPoint groupEp = new IPEndPoint(IPAddress.Any, Int32.Parse(ListenPort));

        try
        {
            var waitForResponse = true;

            while (waitForResponse)
            {
                byte[] bytes = listener.Receive(ref groupEp);

                var encodedString = Encoding.ASCII.GetString(bytes, 0, bytes.Length);

                if (encodedString == "RTT_Feedback")
                {
                    DateTime endTime = DateTime.Now;
                    SumTime += (endTime - StartTime).TotalMilliseconds;

                    // Received response and stop 'for'-loop
                    waitForResponse = false;
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

    static void Main(string[] args)
    {
        for (int i = 0; i < Iterations; i++)
        {
            // Mock data
            var filler = new Filler<Shares>();
            var random = new Random();
            filler.Setup().OnProperty(x => x.Price).Use(random.Next(500));
            filler.Setup().OnProperty(x => x.Amount).Use(random.Next(5000));
            IEnumerable<Shares> sharesEnumerable = filler.Create(3);
            
            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            s.EnableBroadcast = true;
            IPAddress broadcast = IPAddress.Parse("255.255.255.255");
            
            var shareJson = JsonConvert.SerializeObject(sharesEnumerable);

            byte[] sendbuf = Encoding.ASCII.GetBytes(shareJson);
            IPEndPoint ep = new IPEndPoint(broadcast, 11001);

            StartTime = DateTime.Now;
            s.SendTo(sendbuf, ep);

            // RTT test
            Rtt_Test();
        }

        Console.WriteLine("Test Iterations: {0}", Iterations);
        Console.WriteLine("Average RTT: {0}", SumTime / Iterations);
    }
}
