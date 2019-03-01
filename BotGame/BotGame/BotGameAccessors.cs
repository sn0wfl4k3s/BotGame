using BotGame.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using System;

namespace BotGame
{
    public class BotGameAccessors
    {
        public ConversationState ConversationState { get; }
        public UserState UserState { get; }

        // Initializes a new instance of the class.
        public BotGameAccessors(ConversationState conversationState, UserState userState)
        {
            ConversationState = conversationState ?? throw new ArgumentNullException(nameof(conversationState));
            UserState = userState ?? throw new ArgumentNullException(nameof(userState));
        }

        public IStatePropertyAccessor<DialogState> ConversationDialogState { get; set; }
        public IStatePropertyAccessor<Usuario> Usuario { get; set; }
        public IStatePropertyAccessor<Game> Game { get; set; }

    }
}
