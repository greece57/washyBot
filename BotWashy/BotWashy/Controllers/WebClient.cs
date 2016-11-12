using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using RestSharp.Portable.HttpClient;
using RestSharp.Portable;

namespace BotWashy
{
    class WebClient
    {
        private const string backendIP = "https://washybackend.azurewebsites.net";
        private const string serverDates = "/times?day=";
        private const string serverHello = "/HelloWorld";
      

        public string getRequest()
        {
            using (var client = new HttpClient())
            {
                string responseString = "";
                try
                {
                    HttpResponseMessage response = client.GetAsync(backendIP + serverHello).Result;                         //GET STATUS RESPONSE
                    string statusCode = response.StatusCode.ToString();                                                     //GET STATUS RESPONSE

                    responseString = client.GetStringAsync(backendIP + serverHello).Result;
                }
                catch (Exception e)
                {
                    responseString = e.Message;
                }
                return responseString;
            }

        }

        public string getDatesRequest(string dateFormat)
        {
            using (var client = new HttpClient())
            {
                string responseString = "";
                try
                {
                    string inputUrl = backendIP + serverDates + dateFormat;
                    responseString = client.GetStringAsync(inputUrl).Result;
                }
                catch (Exception e)
                {
                    responseString = e.Message;
                }

                return responseString;
            }

        }

        public string postRequest(string resource, Dictionary<string,string> parameters)            // resource everything after backendIP ( LINK ) if Errors add "/" to end of "backendIP"
        {

            var client = new RestClient(backendIP);
            var request = new RestRequest(resource, Method.POST);
            /*
            foreach (KeyValuePair<string, string> entry in parameters)
            {
                request.AddParameter(entry.Key, entry.Value);
            }*/

            IRestResponse response;
            try
            {
                response = client.Execute(request).Result;
            }catch (Exception ex)
            {
                return ex.Message;
            }
            return response.Content;

        }
    }
}
