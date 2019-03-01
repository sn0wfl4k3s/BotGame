using System.Collections.Generic;

namespace BotGame.Resource
{
    public class PromptType
    {
        public const string Text = "texto";
        public const string Number = "number";
        public const string Choice = "choice";
    }

    public class WaterFallDialogID
    {
        public const string BeginProgram = "beginGame";
        public const string StartGame = "startGame";
    }

    public class ConversationMessages
    {
        public const string welcome = "Bem-Vindo";
        public static string askName = $"Olá, eu me chamo {ConversationInfoData.NameBot}, Qual o seu nome?";
        public static string askPlay (string name){return $"Então {name}, Gostaria de tentar advinhar em que número estou pensando?";}
        public static string tryAgain (string temperatura, string nome){return $"Tá {temperatura} {nome}, Tente novamente";}
        public const string letsPlay = "Certo, então vamos jogar! =D";
        public const string dontLetsPlay = "Ok, tudo bem então.. =/";
        public const string askWhatNumber = "Qual número estou pensando?";
        public const string win = "Parabéns, você acertou! =D";
    }

    public class ConversationInfoData
    {
        public const string NameBot = "RicarBot";
        public static List<string> SelectOption = new List<string> { "Sim", "Não" };
    }
}
