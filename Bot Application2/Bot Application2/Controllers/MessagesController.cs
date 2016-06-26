﻿using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;

using Newtonsoft.Json;
using System.Collections;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using System.Collections.Generic;
using TraitifyAPI;
using com.traitify.net.TraitifyLibrary;


namespace Bot_Application2
{
    [Serializable]
    public class PersonalityDialog : IDialog<object>
    {
        bool showDecks = false;
        bool slideStarted = false;
        bool response;
        int index = 0;
        string test_name;
        List<Slide> slideCollection = null;
        Hashtable map = new Hashtable();


        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<Message> argument)
        {
            var message = await argument;
            TestType names = new TestType();
            string messageText = message.Text.ToLower();
            if (messageText == "no" || message.Text == "stop"||messageText=="nope"||messageText=="nop")
            {
                showDecks = false;
                slideStarted = false;
                index = 0;
                await context.PostAsync("Ok! Thank you. See you next time.");
                context.Wait(MessageReceivedAsync);
            }
            else
            {

                if (showDecks == false)
                {
                    showDecks = true;
                    int deckNum = 1;
                    string reply = "";
                    List<Deck> decks = names.GetNames();
                   
                    foreach (Deck deck in decks)
                    {                       
                        reply = reply + "\n" + deckNum + ". " + deck.id;
                        map.Add(deckNum.ToString(), deck.id);
                        deckNum++;

                    }
                    await context.PostAsync("Welcome to your upcoming adventure in personality exploring! Please, choose the test:\n" + reply + System.Environment.NewLine + "Type the *number* of chosen test");
                    context.Wait(MessageReceivedAsync);

                }
                else
                {
                    if (slideStarted == false)
                    {
                       
                        string deck_number = message.Text;
                        if (map.ContainsKey(deck_number))
                        {
                            slideStarted = true;
                            test_name = map[deck_number].ToString();
                            slideCollection = names.GetSlides(test_name);
                            await context.PostAsync("Your test is ready! For the following questions type 1 if you agree with statement, type 2 if you disagree. If you want to finish sesion type 'stop' any time. In order to start type Ready or Start.");
                            context.Wait(MessageReceivedAsync);
                        } else
                        {
                            await context.PostAsync("Sorry, I can't understand " + "'" + deck_number + "'" +". Please use appropriate number of the test.");
                            context.Wait(MessageReceivedAsync);
                        }                      
                    }
                    else
                    {
                        if (index == 0)
                        {
                            string name = slideCollection[index].caption;
                            index++;
                            string url = slideCollection[index - 1].image_desktop;
                            var reply = context.MakeMessage();
                            reply.Attachments = new List<Attachment>();
                            Attachment attachment = new Attachment()
                            {
                                ContentUrl = url,
                                ContentType = "image"
                            };
                            reply.Attachments.Add(attachment);
                            await context.PostAsync(name);
                            await context.PostAsync(reply);
                            context.Wait(MessageReceivedAsync);
                        }
                        else
                        {
                            if (index < slideCollection.Count)
                            {
                                if (message.Text=="1"||message.Text=="2")
                                {
                                    response = (message.Text == "1") ? true : false;
                                    string name = slideCollection[index].caption;
                                    slideCollection[index - 1].response = response;
                                    index++;
                                    string url = slideCollection[index - 1].image_desktop;
                                    var reply = context.MakeMessage();
                                    reply.Attachments = new List<Attachment>();
                                    Attachment attachment = new Attachment()
                                    {
                                        ContentUrl = url,
                                        ContentType = "image"
                                    };
                                    reply.Attachments.Add(attachment);
                                    await context.PostAsync(name);
                                    await context.PostAsync(reply);
                                    context.Wait(MessageReceivedAsync);
                                } else
                                {
                                    await context.PostAsync("Sorry, I can't understand " + "'" + message.Text + "'" + ". Please use numbers 1 (agree)  or 2 (disagree) in your answer");
                                    context.Wait(MessageReceivedAsync);
                                }
                                
                            }
                            else if (index == slideCollection.Count)
                            {
                                if (message.Text=="1"||message.Text=="2")
                                {
                                    response = (message.Text == "2") ? false : true;
                                    slideCollection[index - 1].response = response;
                                    string personality_type = names.Result("test", slideCollection);
                                    showDecks = false;
                                    slideStarted = false;
                                    index = 0;
                                    await context.PostAsync(personality_type);
                                    await context.PostAsync("Would you like to take one more test?");
                                    context.Wait(MessageReceivedAsync);
                                } else
                                {                               
                                    await context.PostAsync("Sorry, I can't understand " + "'" + message.Text + "'" + ". Please use numbers 1 (agree)  or 2 (disagree) in your answer");
                                    context.Wait(MessageReceivedAsync);                                    
                                }                              
                            }
                        }
                    }
                }
            }
        }
    }

    [BotAuthentication]
    public class MessagesController : ApiController
    {
        public async Task<Message> Post([FromBody]Message message)
        {
            if (message.Type == "Message")
            {
                return await Conversation.SendAsync(message, () => new PersonalityDialog());
            }
            else
            {
                return HandleSystemMessage(message);
            }
        }

        private Message HandleSystemMessage(Message message)
        {
            if (message.Type == "BotAddedToConversation")
            {
                Message reply = message.CreateReplyMessage("Hello and welcome to your upcoming adventure in personality exploring! Are you ready to start?");
                return reply;

            }

            if (message.Type == "BotRemovedFromConversation")
            {
                Message reply = message.CreateReplyMessage("Thank you! Hope to see you later.");
                return reply;
            }
            return null;
        }
    }
}


