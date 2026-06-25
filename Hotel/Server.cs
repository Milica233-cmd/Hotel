using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Hotel
{
    public class Server
    {
        private HttpListener listener;

        public void Start()
        {
            try
            {
                Console.WriteLine("START SERVER...");

                listener = new HttpListener();

                listener.Prefixes.Add("http://localhost:5055/");

                listener.Start();

                Console.WriteLine("SERVER STARTOVAN");

                var localListener = listener;

                Task.Run(() =>
                {
                    while (true)
                    {
                        var context = localListener.GetContext();
                        var response = context.Response;

                        string filePath = @"C:\Temp\GostIzvestaj.pdf";

                        byte[] buffer = File.ReadAllBytes(filePath);

                        response.ContentType = "application/pdf";
                        response.ContentLength64 = buffer.Length;

                        response.OutputStream.Write(buffer, 0, buffer.Length);
                        response.OutputStream.Close();
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("GRESKA: " + ex.Message);
            }
        }
    }
}