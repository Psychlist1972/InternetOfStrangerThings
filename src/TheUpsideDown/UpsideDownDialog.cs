using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace TheUpsideDown
{
    // Reference: https://docs.botframework.com/en-us/csharp/builder/sdkreference/dialogs.html

    // Partial class is excluded from project. It contains keys:
    // 
    // [LuisModel("model id", "subscription key")]
    // public partial class UpsideDownDialog
    // {
    // }
    // 
    [Serializable]
    public partial class UpsideDownDialog : LuisDialog<object>
    {
        //
        // TODO: Each of the LuisIntent attributes here must match exactly the
        //       Intents you have set up on LUIS.ai
        //

        // None
        [LuisIntent("")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            string message = $"Eh";
            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }

        [LuisIntent("CheckCreator")]
        public async Task CheckCreator(IDialogContext context, LuisResult result)
        {
            string message = $"Pete";
            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }

        [LuisIntent("CheckPresence")]
        public async Task CheckPresence(IDialogContext context, LuisResult result)
        {
            string message = $"Yes";
            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }

        [LuisIntent("AskName")]
        public async Task AskName(IDialogContext context, LuisResult result)
        {
            string message = $"Will";
            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }

        [LuisIntent("FavoriteColor")]
        public async Task FavoriteColor(IDialogContext context, LuisResult result)
        {
            string message = $"Blue ... no Gr..ahhhhh";
            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }

        [LuisIntent("WhatIShouldDoNow")]
        public async Task WhatIShouldDoNow(IDialogContext context, LuisResult result)
        {
            string message = $"Run";
            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }

        [LuisIntent("CheckEnvironment")]
        public async Task CheckEnvironment(IDialogContext context, LuisResult result)
        {
            string message = $"Cold, and Dark";
            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }

        [LuisIntent("KnockKnock")]
        public async Task KnockKnock(IDialogContext context, LuisResult result)
        {
            string message = $"Bang";
            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }

        [LuisIntent("AskLocation")]
        public async Task AskLocation(IDialogContext context, LuisResult result)
        {
            string message = $"Upside Down";
            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }

        [LuisIntent("CheckSafety")]
        public async Task CheckSafety(IDialogContext context, LuisResult result)
        {
            string message = $"Scared";
            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }

    }
}