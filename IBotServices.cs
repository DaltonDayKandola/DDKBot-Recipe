
// Copyright (c) Microsoft Corporation. All rights reserved.

// Licensed under the MIT License.

using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;


namespace Microsoft.BotBuilderSamples

{

    public interface IBotServices

    {

        LuisRecognizer Dispatch { get; }
        QnAMaker DDKQnA { get; }
        BotState conversationState { get; }
        BotState userState { get; }
    }

}