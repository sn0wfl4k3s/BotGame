// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using BotGame.Models;
using BotGame.Resource;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;

namespace BotGame
{
    public class BotGameBot : IBot
    {
        private readonly DialogSet _dialogs;
        private readonly BotGameAccessors _accessors;

        public BotGameBot(BotGameAccessors accessors)
        {
            _accessors = accessors ?? throw new System.ArgumentException(nameof(accessors));
            _dialogs = new DialogSet(accessors.ConversationDialogState);

            _dialogs
                .Add(new TextPrompt(PromptType.Text))
                .Add(new NumberPrompt<int>(PromptType.Number))
                .Add(new ChoicePrompt(PromptType.Choice));

            var beginGame = new WaterfallStep[]
            {
                SolicitarNome,
                PerguntarSeQuerJogar,
                DecidirOQueFazer
            };
            var playGame = new WaterfallStep[]
            {
                SolicitarNumeroAoUsuario,
                VerificarSeAcertou
            };

            _dialogs.Add(new WaterfallDialog(WaterFallDialogID.BeginProgram, beginGame));
            _dialogs.Add(new WaterfallDialog(WaterFallDialogID.StartGame, playGame));
        }

        #region Começo do Programa
        private async Task<DialogTurnResult> SolicitarNome(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(PromptType.Text, new PromptOptions
            {
                Prompt = MessageFactory.Text(ConversationMessages.askName)
            }, cancellationToken);
        }

        private async Task<DialogTurnResult> PerguntarSeQuerJogar(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            Usuario usuario = await _accessors.Usuario.GetAsync(stepContext.Context, () => new Usuario(), cancellationToken);
            usuario.Nome = (string)stepContext.Result;
            return await stepContext.PromptAsync(PromptType.Choice, new PromptOptions
            {
                Prompt = MessageFactory.Text(ConversationMessages.askPlay(usuario.Nome)),
                Choices = ChoiceFactory.ToChoices(ConversationInfoData.SelectOption)
            }, cancellationToken);
        }

        private async Task<DialogTurnResult> DecidirOQueFazer(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string resposta_do_usuario = ((FoundChoice)stepContext.Result).Value;
            if (resposta_do_usuario.Equals(ConversationInfoData.SelectOption[0]))
            {
                Game game = await _accessors.Game.GetAsync(stepContext.Context, () => new Game(), cancellationToken);
                game.Gerar_Numero_Aleatorio();
                await stepContext.Context.SendActivityAsync(ConversationMessages.letsPlay);
                return await stepContext.ReplaceDialogAsync(WaterFallDialogID.StartGame);
            }
            await stepContext.Context.SendActivityAsync(ConversationMessages.dontLetsPlay);
            return await stepContext.EndDialogAsync();
        }
        #endregion

        #region Durante o Jogo
        private async Task<DialogTurnResult> SolicitarNumeroAoUsuario(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(PromptType.Number, new PromptOptions
            {
                Prompt = MessageFactory.Text(ConversationMessages.askWhatNumber)
            }, cancellationToken);
        }

        private async Task<DialogTurnResult> VerificarSeAcertou(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            int numero_sugerido_pelo_usuario = (int)stepContext.Result;
            Usuario usuario = await _accessors.Usuario.GetAsync(stepContext.Context);
            Game game = await _accessors.Game.GetAsync(stepContext.Context);
            string resultado = game.Verificar_Se_Chegou_Perto(numero_sugerido_pelo_usuario);
            if (resultado.Equals("acertou"))
            {
                await stepContext.Context.SendActivityAsync(ConversationMessages.win);
                return await stepContext.EndDialogAsync();
            }

            await stepContext.Context.SendActivityAsync(ConversationMessages.tryAgain(resultado, usuario.Nome));
            return await stepContext.ReplaceDialogAsync(WaterFallDialogID.StartGame);
        }
        #endregion

        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                var dialogContext = await _dialogs.CreateContextAsync(turnContext, cancellationToken);
                var results = await dialogContext.ContinueDialogAsync(cancellationToken);

                if (results.Status == DialogTurnStatus.Empty)
                {
                    await dialogContext.BeginDialogAsync(WaterFallDialogID.BeginProgram, null, cancellationToken);
                }
            }
            else if (turnContext.Activity.Type == ActivityTypes.ConversationUpdate)
            {
                foreach (var member in turnContext.Activity.MembersAdded)
                {
                    if (member.Id == turnContext.Activity.Recipient.Id)
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Text(ConversationMessages.welcome));
                    }
                }
            }

            await _accessors.ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            await _accessors.UserState.SaveChangesAsync(turnContext, false, cancellationToken);
        }
    }
}
