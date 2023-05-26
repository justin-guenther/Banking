using System.Net;
using System.Net.Sockets;
using System.Text;
using Bank.requests;
using Newtonsoft.Json;

namespace Bank;

public class HttpServer
{
    private Socket serverSocket;

    public Socket StartHttpSocket(string ipAddressString, int port)
    {
        var ipAddress = IPAddress.Parse(ipAddressString);
        var endpoint = new IPEndPoint(ipAddress, port);

        serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        serverSocket.Bind(endpoint);
        serverSocket.Listen(10);

        Console.WriteLine("TCP Server started. Listening for connections...");

        return serverSocket;
    }

    public void ProcessRequest(Socket clientSocket)
    {
        var buffer = new byte[1024];
        var bytesRead = clientSocket.Receive(buffer);
        var request = Encoding.UTF8.GetString(buffer, 0, bytesRead);

        // Extract the HTTP method
        var httpMethod = request.Substring(0, request.IndexOf(' '));
        
        Console.WriteLine("RequestIndexLength: " + request.IndexOf(' ' ));

        Console.WriteLine("Method: " + httpMethod);

        // Find the index of the first space character after the HTTP method
        var firstSpaceIndex = request.IndexOf(' ');

        Console.WriteLine("FirstSpaceIndex: " + firstSpaceIndex);

        // Find the index of the next space character after the HTTP method
        var secondSpaceIndex = request.IndexOf(' ', firstSpaceIndex + 1);

        Console.WriteLine("SecondSpaceIndex: " + secondSpaceIndex);

        // Extract the route based on the positions of the space characters
        var route = request.Substring(firstSpaceIndex + 1, secondSpaceIndex - firstSpaceIndex - 1);

        Console.WriteLine("Route: " + route);

        // Find the index of the empty line separating headers and body
        var emptyLineIndex = request.IndexOf("\r\n\r\n", StringComparison.Ordinal);
        
        Console.WriteLine("EmptyIndex: " + emptyLineIndex);

        // Extract the body by skipping the empty line
        var body = string.Empty;

        if (emptyLineIndex != -1)
        {
            body = request.Substring(emptyLineIndex + 4);
        }

        Console.WriteLine("Body: " + body);

        // Extract the body of the request (if any)
        var response = string.Empty;
        var responseStatusCode = "200 OK";
        var responseOutput = "";

        // Routes
        if (httpMethod.Equals("GET", StringComparison.OrdinalIgnoreCase))
        {
            if (route == "/customers")
            {
                var customersList = JsonConvert.SerializeObject(BankInfo.Customers);

                responseStatusCode = "200 OK";
                responseOutput = customersList;
            }
            else if (route == "/portfolio")
            {
                responseStatusCode = "200 OK";
                responseOutput = BankInfo.Portfolio.ToString();
            }
        }
        else if (httpMethod.Equals("PUT", StringComparison.OrdinalIgnoreCase))
        {
            var convertedBody = JsonConvert.DeserializeObject<WithdrawalBody>(body);

            if (convertedBody.kind == "withdraw")
            {
                var customer = BankInfo.GetCustomerIndex(convertedBody.customerId);

                if (customer.saldo - customer.debt - convertedBody.amount >= 0)
                {
                    customer.saldo -= convertedBody.amount;
                    BankInfo.Portfolio -= convertedBody.amount;

                    responseStatusCode = "200 OK";
                    responseOutput = "Request was made!";
                }
                else
                {
                    responseStatusCode = "405 Method Not Allowed";
                    responseOutput = "The request was declined!";
                }
            }
            else if (convertedBody.kind == "deposit")
            {
                var customer = BankInfo.GetCustomerIndex(convertedBody.customerId);

                customer.saldo += convertedBody.amount;
                BankInfo.Portfolio += convertedBody.amount;
            }
            else if (convertedBody.kind == "debt")
            {
                var customer = BankInfo.GetCustomerIndex(convertedBody.customerId);

                if (customer.debt > 0)
                {
                    if (customer.hasSaldo(convertedBody.amount))
                    {
                        customer.debt -= convertedBody.amount;
                        customer.saldo -= convertedBody.amount;

                        responseStatusCode = "200 OK";
                        responseOutput = "The request was made!";
                    }
                }
                else
                {
                    responseStatusCode = "405 Method Not Allowed";
                    responseOutput = "Customer has no debt!";
                }
            }
        }
        else
        {
            responseStatusCode = "404 Not Found";
            responseOutput = "404 Http request not found!";
        }

        response = "HTTP/1.1 " + responseStatusCode + "\r\n" +
                   "Content-Type: text/html\r\n" +
                   "Date: " + DateTime.UtcNow.ToString("r") + "\r\n" +
                   "\r\n" +
                   responseOutput;


        var responseBytes = Encoding.UTF8.GetBytes(response);
        clientSocket.Send(responseBytes);

        clientSocket.Shutdown(SocketShutdown.Both);
        clientSocket.Close();
    }
}