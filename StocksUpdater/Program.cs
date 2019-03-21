using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace StocksUpdater
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                GetAllEventData();
            }
            catch (Exception ex)
            {

            }
        }


        public static void GetAllEventData() //Get All Events Records  
        {
            using (var client = new WebClient()) //WebClient  
            {
                client.Headers.Add("Content-Type:application/json"); //Content-Type  
                client.Headers.Add("Accept:application/json");
                string uri = ConfigurationManager.AppSettings["StocksApiUri"];
                var result = client.DownloadString(uri); //URI  
                Console.WriteLine(Environment.NewLine + result);
            }
        }
    }
}
