using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
class Server
{
    TcpListener server = null;
    static List<TcpClient> ClientsList = new List<TcpClient>();
    static List<string> ClientsNames = new List<string>();
    public Server(string ip, int port)
    {
        IPAddress localAddr = IPAddress.Parse(ip);
        server = new TcpListener(localAddr, port);
        server.Start();
        StartListener();
    }

    public void StartListener()
    {
        try
        {
            while (true)
            {
                Console.WriteLine("Waiting for a connection...");
                TcpClient client = server.AcceptTcpClient();
                Console.WriteLine("Connected!");
                Thread t = new Thread(new ParameterizedThreadStart(HandleDevice));
                t.Start(client);
                ClientsList.Add(client);
                wypisz();
            }
        }
        catch (SocketException e)
        {
            Console.WriteLine("SocketException: {0}", e);
            server.Stop();
        }
    }
    public void HandleDevice(Object obj)
    {
        TcpClient client = (TcpClient)obj;
        var stream = client.GetStream();
        string imei = String.Empty;
        string data = null;
        Byte[] bytes = new Byte[256];
        int i;
        try
        {
            while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
            {
                string hex = BitConverter.ToString(bytes);
                data = Encoding.ASCII.GetString(bytes, 0, i);
                Console.WriteLine("Received: {0}", data);
                if (data == "7")
                {
                    Console.WriteLine("Game");
                    int idx = ClientsList.IndexOf(client);
                    Console.WriteLine("ID:" + idx);
                }
                else if (data.Contains("DROP") == true)
                {
                    int idx = ClientsList.IndexOf(client);
                    Console.WriteLine("ID DROP:" + idx);
                    string name = data.Remove(0, 5);
                    int idxx = idx - 1;
                    name = ClientsNames[idxx];
                    STG("drop_" + idx + name);
                    Console.WriteLine("TEST drop by " + name);
                }
                else if (data.Contains("Name_") == true)
                {
                    int idx = ClientsList.IndexOf(client);
                    Console.WriteLine("ID:" + idx);
                    string name = data.Remove(0, 5);
                    ClientsNames.Add(name);
                    STG("player"+idx + name);
                }
                else
                {
                    Console.WriteLine(data);
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Exception: {0}", e.ToString());
            client.Close();
        }
    }
    public static void ShowActiveTcpConnections()
    {
        Console.WriteLine("Active TCP Connections");
        IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
        TcpConnectionInformation[] connections = properties.GetActiveTcpConnections();
        foreach (TcpConnectionInformation c in connections)
        {
            Console.WriteLine("{0} <==> {1}",
                              c.LocalEndPoint.ToString(),
                              c.RemoteEndPoint.ToString());
        }
    }
    public void wypisz()
    {
        foreach (TcpClient c in ClientsList)
        {
            int idx = ClientsList.IndexOf(c);
            Console.WriteLine("ID:"+idx);
        }
    }
    public void STG(string txt)
    {
        var serv = ClientsList[0];
        var stream = serv.GetStream();
        Byte[] reply_txt = System.Text.Encoding.ASCII.GetBytes(txt);
        stream.Write(reply_txt, 0, reply_txt.Length);
        Console.WriteLine("Send to game: " + txt);
    }
}