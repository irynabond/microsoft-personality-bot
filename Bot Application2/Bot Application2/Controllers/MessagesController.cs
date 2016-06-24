using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;

using Newtonsoft.Json;
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
        List<Slide> slideCollection=null;

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<Message> argument)
        {
            var messageStop = await argument;
            if (messageStop.Text == "no" || messageStop.Text == "No"||messageStop.Text == "stop")
            {
                await context.PostAsync("Ok! Thank you. See you next time.");
                context.Wait(MessageReceivedAsync);
            }
            else
            {
                if (showDecks == false)
                {
                    showDecks = true;
                    TestType names = new TestType();
                    var listOfDecks = string.Join(", ", names.GetNames().ToArray());
                    await context.PostAsync("Please, choose the test: " + listOfDecks);
                    context.Wait(MessageReceivedAsync);

                }
                else
                {
                    if (slideStarted == false)
                    {
                        slideStarted = true;
                        var message = await argument;
                        TestType slides = new TestType();
                        test_name = message.Text;
                        slideCollection = slides.GetSlides(test_name);
                        await context.PostAsync("Your test is ready! For the following questions type 1 if your answer is yes, type 2 if your answer is no. In order to start type Ready or Start.");
                        context.Wait(MessageReceivedAsync);

                    }
                    else
                    {
                        if (index == 0)
                        {
                            string name = slideCollection[index].caption;
                            index++;
                            await context.PostAsync(name);
                            await context.PostAsync("![Show picture](" + slideCollection[index - 1].image_desktop + ")");
                            context.Wait(MessageReceivedAsync);
                        }
                        else
                        {
                            if (index < slideCollection.Count)
                            {
                                var message = await argument;
                                response = (message.Text == "1") ? true : false;
                                string name = slideCollection[index].caption;
                                slideCollection[index - 1].response = response;
                                index++;
                                await context.PostAsync(name);     
                                await context.PostAsync("![Show picture](" + slideCollection[index - 1].image_desktop + ")");                        
                                context.Wait(MessageReceivedAsync);
                            }
                            else if (index == slideCollection.Count)
                            {
                                var message = await argument;
                                response = (message.Text == "2") ? false : true;
                                slideCollection[index - 1].response = response;
                                TestType result = new TestType();
                                string personality_type = result.Result("test", slideCollection);
                                showDecks = false;
                                slideStarted = false;
                                index = 0;
                                await context.PostAsync(personality_type);
                                await context.PostAsync("Would you like to take one more test?");
                                context.Wait(MessageReceivedAsync);
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
    
    
