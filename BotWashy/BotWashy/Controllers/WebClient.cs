using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using RestSharp.Portable.HttpClient;
using RestSharp.Portable;
using System.IO;

namespace BotWashy
{
    class WebClient
    {
        private const string backendIP = "https://washybackend.azurewebsites.net";
        private const string serverDates = "/times?day=";
        private const string serverHello = "/HelloWorld";
        private const string serverUser = "/user";
        private const string serverConversationId = "/conversation";


        public string getRequest()
        {
            using (var client = new HttpClient())
            {
                string responseString = "";
                try
                {
                    HttpResponseMessage response = client.GetAsync(backendIP + serverHello).Result;                         //GET STATUS RESPONSE
                    string statusCode = response.StatusCode.ToString();                                                     //GET STATUS RESPONSE
                    string data = response.Content.ReadAsStringAsync().Result;

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

        public HttpResponseMessage getUserRequest(string channel, string userId)
        {
            using (var client = new HttpClient())
            {
                HttpResponseMessage response = new HttpResponseMessage();
                try
                {
                    string inputUrl = backendIP + serverUser + "?" + channel + "=" + userId;
                    response = client.GetAsync(inputUrl).Result;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }

                return response;
            }

        }

        public HttpResponseMessage getConversationRequest(string conversationId)
        {
            using (var client = new HttpClient())
            {
                HttpResponseMessage response = new HttpResponseMessage();

                try
                {
                    string inputUrl = backendIP + serverConversationId + "/" + conversationId + "/state";
                    response = client.GetAsync(inputUrl).Result;

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }

                return response;
            }

        }

        public async System.Threading.Tasks.Task<string> putRequest(string conversationId, string state)            // resource everything after backendIP ( LINK ) if Errors add "/" to end of "backendIP"
        {
            string inputUrl = serverConversationId + "/" + conversationId + "/state?state=" + state;
            var client = new RestClient(backendIP);
            var request = new RestRequest(inputUrl, Method.PUT);
            /*
            foreach (KeyValuePair<string, string> entry in parameters)
            {
                request.AddParameter(entry.Key, entry.Value);
            }*/

            IRestResponse response;
            try
            {
                response = await client.Execute(request);

            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return response.Content;

        }
    }
}
