﻿using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Bot.Connector;

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

            try
            {

                if (activity.Type == ActivityTypes.Message)
                {
                    ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));


                    // calculate something for us to return
                    int length = (activity.Text ?? string.Empty).Length;
                    Activity reply = null;

                    //Bot
                    int state = 5555;

                    //HttpResponseMessage response = client.GetAsync(backendIP + serverHello).Result;                         //GET STATUS RESPONSE
                    //string statusCode = response.StatusCode.ToString();   
                    if (activity.Text != null && activity.Text.Contains("test"))
                    {
                        WebClient com = new WebClient();
                        string result = com.getRequest();
                        reply = activity.CreateReply($"{result}");

                    }
                    else //if (activity.Text.ToLower().Contains("wash"))
                    {
                        //Check State
                        WebClient stateCom = new WebClient();
                        HttpResponseMessage stateResponse = stateCom.getConversationRequest(activity.Conversation.Id);
                        string statusCode = stateResponse.StatusCode.ToString();                                                     //GET STATUS RESPONSE

                        //state NONE
                        if (statusCode != "OK")
                        {
                            state = 5555;
                            // check for user existence
                            WebClient userCom = new WebClient();
                            HttpResponseMessage userResponse = userCom.getUserRequest(activity.ChannelId, activity.From.Id);
                            string statusCodeUser = userResponse.StatusCode.ToString();                                                     //GET STATUS RESPONSE

                            //No user -> state = 0
                            if (statusCodeUser != "OK")
                            {
                                state = 5555;

                                WebClient editState = new WebClient();
                                string editresponse = await editState.putStateRequest(activity.Conversation.Id, "0");
                                string statusPost = userResponse.StatusCode.ToString();
                                reply = activity.CreateReply("You are currently not registered. Please send me your mobile phone number, so I can register you.");
                            }
                            //Yes, user exists -> state = 1
                            else
                            {
                                WebClient editState = new WebClient();
                                string editresponse = await editState.putStateRequest(activity.Conversation.Id, "1");
                                string statusPost = userResponse.StatusCode.ToString();
                                reply = activity.CreateReply("When do you want to do your laundry?");
                            }
                        }
                        //some state betw 0 and 3
                        else
                        {
                            //get state 
                            string data = stateResponse.Content.ReadAsStringAsync().Result;
                            string rcvstate = Controllers.dateFormat.getStateFromJSON(data);
                            state = Convert.ToInt16(rcvstate);
                            
                            if (activity.Attachments != null && activity.Attachments.Count > 0)
                            {
                                Attachment attachedPic = activity.Attachments[0];
                                string url = attachedPic.ContentUrl;

                                WebClient _editState = new WebClient();
                                string _reserveResponse = await _editState.postPictureRequest(url, activity.From.Id, activity.ChannelId);

                                if (state == -1)
                                {
                                    string _editresponse = await _editState.putStateRequest(activity.Conversation.Id, "1");
                                    reply = activity.CreateReply("Thank you! When do you want to do your laundry?");
                                }
                                else
                                {
                                    reply = activity.CreateReply("Thank you for the additional picture!\n" + createRandomCompliment());
                                }

                            }
                            else
                            {
                                switch (state)
                                {
                                    case -1:

                                        if (activity.Attachments != null && activity.Attachments.Count > 0)
                                        {
                                            Attachment attachedPic = activity.Attachments[0];
                                            string url = attachedPic.ContentUrl;

                                            WebClient _editState = new WebClient();
                                            string _reserveResponse = await _editState.postPictureRequest(url, activity.From.Id, activity.ChannelId);

                                            string _editresponse = await _editState.putStateRequest(activity.Conversation.Id, "1");
                                            reply = activity.CreateReply("Thank you! When do you want to do your laundry?");
                                        }
                                        else
                                        {
                                            reply = activity.CreateReply("Don't be so shy :) Please send me a picture.");
                                            // twilio calls you and plays song "Don't be so shy"
                                        }
                                        break;
                                    //expect phone number + create new user
                                    case 0:
                                        //get phone number 
                                        string phoneNumber = activity.Text;
                                        string userName = activity.From.Name;
                                        string userId = activity.From.Id;
                                        string userChat = activity.ChannelId;

                                        WebClient editState = new WebClient();
                                        string editresponse = await editState.postNewUserRequest(phoneNumber, userName, userId, userChat);
                                        editresponse = await editState.putStateRequest(activity.Conversation.Id, "-1");
                                        reply = activity.CreateReply("Thank you! Please send me a picture to finish your registration.");
                                        break;
                                    //find available time slot for date
                                    case 1:
                                        WebClient com = new WebClient();
                                        string correctDate = "";
                                        if (activity.Text.ToLower().Contains("today") || activity.Text.ToLower().Contains("now"))
                                        {
                                            // Get available dates
                                            correctDate = Controllers.dateFormat.getFormatedDateNow();
                                        }
                                        else if (activity.Text.ToLower().Contains("tomorrow"))
                                        {
                                            correctDate = Controllers.dateFormat.getFormatedDateTomorrow();
                                        }
                                        else
                                        {
                                            reply = activity.CreateReply("Unfortunately there are only free spots available today or tomorrow. What would you prefer?");
                                            break;
                                        }

                                        //reply with all possible dates
                                        string result = com.getDatesRequest(correctDate);
                                        string[] output = Controllers.dateFormat.getTimeslotsPlainText(result);
                                        for (int i = 0; i < output.Length; i++)
                                        {
                                            reply = activity.CreateReply($"{output[i]}");
                                            await connector.Conversations.ReplyToActivityAsync(reply);
                                        }

                                        editresponse = await com.putStateTimeRequest(activity.Conversation.Id, "2", correctDate);
                                        reply = activity.CreateReply("Which hour do you wanna start?");
                                        break;
                                    //reserve a specific time slot
                                    case 2:
                                        string rcvdate = Controllers.dateFormat.getDateFromJSON(data);
                                        string[] elements = rcvdate.Split(' ');
                                        int month = int.Parse(elements[0]);
                                        int days = int.Parse(elements[1]);
                                        int year = int.Parse(elements[2]);
                                        int min = int.Parse(elements[3].Split(':')[1]);
                                        DateTime newDate = new DateTime(year, month, days, int.Parse(activity.Text.Trim()), min, 0);
                                        string formattedDate = Controllers.dateFormat.formatDateDot(newDate);

                                        WebClient reserveClient = new WebClient();
                                        string reserveResponse = await reserveClient.postReserveRequest(formattedDate, activity.From.Id, activity.ChannelId);

                                        editresponse = await reserveClient.putStateRequest(activity.Conversation.Id, "3");
                                        var room = Controllers.dateFormat.getRoomFromJSON(reserveResponse);
                                        reply = activity.CreateReply("Your slot has been reserved in Room " + room);

                                        break;
                                    //start over state=1
                                    default:
                                        WebClient newRound = new WebClient();
                                        editresponse = await newRound.putStateRequest(activity.Conversation.Id, "1");
                                        reply = activity.CreateReply("When do you want to do your laundry?");
                                        break;
                                }

                            }


                        }
                    }

                    await connector.Conversations.ReplyToActivityAsync(reply);
                }
                else
                {
                    HandleSystemMessage(activity);
                }


            }
            catch (Exception ex)
            {
                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                var reply = activity.CreateReply(ex.Message);
                await connector.Conversations.ReplyToActivityAsync(reply);
            }

            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private string createRandomCompliment()
        {
            Random rnd = new Random();
            int randomIndex = rnd.Next(0, 5);
            switch (randomIndex)
            {
                case 0:
                    return "You look really pretty today!";
                case 1:
                    return "That's a really nice photo!";
                case 3:
                    return "Oh you look beautiful on this one!";
                case 4:
                    return "But you can upload a nicer one :p";
                case 5:
                    return "That really makes me want to see more of you! ;)";
            }
            return "";
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