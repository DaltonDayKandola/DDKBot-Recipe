// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.BotBuilderSamples

{

    // Defines a state property used to track information about the user.

    public class UserProfile

    {

        public string Name { get; set; }
        public string ContactType { get; set; }
        public string ContactValue { get; set; }
        public bool ContactDialogueFlow { get; set; }

        

    }

}