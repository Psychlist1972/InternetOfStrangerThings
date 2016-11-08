using Microsoft.Bot.Connector.DirectLine;
using Microsoft.Bot.Connector.DirectLine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Web.Http;
using Windows.Web.Http.Headers;

namespace AlphabetRemoteArduino
{
    class BotInterface
    {
        // Thanks to Shen's Hunt the Wumpus for help here
        // https://github.com/shenchauhan/HuntTheWumpus/


        #region Secret
        const string _directLineSecret = "TODO: Your Direct Line Secret";
        #endregion


        // TODO: Change the URL to match your bot
        private const string _botBaseUrl = "https://YOUR_BOT_HOST/api/messages";

        private DirectLineClient _directLine;
        private string _conversationId;
        public BotInterface()
        {
        }


        public async Task ConnectAsync()
        {
            _directLine = new DirectLineClient(_directLineSecret);

            var conversation = await _directLine.Conversations.NewConversationWithHttpMessagesAsync();
            _conversationId = conversation.Body.ConversationId;

            System.Diagnostics.Debug.WriteLine("Bot connection set up.");
        }

        private async Task<string> GetResponse()
        {
            try
            {
                var httpMessages = await _directLine.Conversations.GetMessagesWithHttpMessagesAsync(_conversationId);
                var messages = httpMessages.Body.Messages;

                // our bot only returns a single message, so we won't loop through
                // First message is the question, second message is the response
                if (messages?.Count > 1)
                {
                    // select latest message -- the response
                    var text = messages[messages.Count-1].Text;
                    System.Diagnostics.Debug.WriteLine("Response from bot was: " + text);

                    return text;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Response from bot was empty.");
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());

                throw;
            }

        }


        public async Task<string> TalkToTheUpsideDownAsync(string message)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Sending bot message");

                var msg = new Message();
                msg.Text = message;


                System.Diagnostics.Debug.WriteLine("Posting");

                await _directLine.Conversations.PostMessageAsync(_conversationId, msg);

                System.Diagnostics.Debug.WriteLine("Post complete");

                return await GetResponse();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());

                throw;
            }
        }


    }
}
