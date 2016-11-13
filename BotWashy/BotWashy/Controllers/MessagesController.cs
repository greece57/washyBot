using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using System.Globalization;

namespace BotWashy
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {

            if (activity.Type == ActivityTypes.Message)
            {
                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                // calculate something for us to return
                int length = (activity.Text ?? string.Empty).Length;
                Activity reply = null;

                int state = 5555;
            
                //HttpResponseMessage response = client.GetAsync(backendIP + serverHello).Result;                         //GET STATUS RESPONSE
                //string statusCode = response.StatusCode.ToString();   
                if (activity.Text.Contains("test"))
                {
                    WebClient com = new WebClient();
                    string result = com.getRequest();
                    reply = activity.CreateReply($"{result}");

                }
                else if (activity.Text.ToLower().Contains("wash"))
                {
                    //Check State
                    WebClient stateCom = new WebClient();
                    HttpResponseMessage stateResponse = stateCom.getConversationRequest(activity.Conversation.Id);
                    string statusCode = stateResponse.StatusCode.ToString();                                                     //GET STATUS RESPONSE

                    if (statusCode != "OK")
                    {
                        state = 5555;
                        // check for user existence
                        WebClient userCom = new WebClient();
                        HttpResponseMessage userResponse = userCom.getUserRequest(activity.ChannelId,activity.From.Id);
                        string statusCodeUser = userResponse.StatusCode.ToString();                                                     //GET STATUS RESPONSE

                        if (statusCodeUser != "OK")
                        {
                            state = 5555;

                            WebClient editState = new WebClient();
                            string editresponse = await editState.putRequest(activity.Conversation.Id, "0");
                            string statusPost = userResponse.StatusCode.ToString();

                        }
                        else
                        {
                            string data = stateResponse.Content.ReadAsStringAsync().Result;
                            string rcvstate = Controllers.dateFormat.getStateFromJSON(data);
                            state = Convert.ToInt16(rcvstate);
                        }

                    }
                    else
                    {
                        string data = stateResponse.Content.ReadAsStringAsync().Result;
                        string rcvstate = Controllers.dateFormat.getStateFromJSON(data);
                        state = Convert.ToInt16(rcvstate);
                    }


                    // Get available dates
                    WebClient com = new WebClient();
                    string correctDate = Controllers.dateFormat.getFormatedDateNow();
                    string result = com.getDatesRequest(correctDate);
                    
                    string[] output = Controllers.dateFormat.getTimeslotsPlainText(result);
                    for (int i = 0; i < output.Length; i++)
                    {
                        reply = activity.CreateReply($"{output[i]}");
                        await connector.Conversations.ReplyToActivityAsync(reply);
                    }
                    //reply = activity.CreateReply($"{activity.ChannelId}");                                // Send to Matthias
                }
                else
                {
                    // return our reply to the user
                    reply = activity.CreateReply($"You sent {activity.Text} which was {length} characters");
                }
                
                await connector.Conversations.ReplyToActivityAsync(reply);
            }
            else
            {
                HandleSystemMessage(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }
}