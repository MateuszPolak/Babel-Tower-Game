using System;
using System.Threading;


namespace TCPserver
{
    public class Program
    {
        static void Main(string[] args)
        {
            Thread t = new Thread(delegate ()
            {
                //Set IP and PORT
                Server myserver = new Server("IP", PORT);
            });
            t.Start();
            
            

            Console.WriteLine("Server Started...!");
        }
    }
}
