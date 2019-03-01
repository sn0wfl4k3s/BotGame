using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotGame.Models
{
    public class Game
    {
        public int Numero { get; set; }

        public void Gerar_Numero_Aleatorio ()
        {
            Numero = new Random().Next(0, 100);
        }

        public string Verificar_Se_Chegou_Perto (int numero_sugerido)
        {
            string resp = "";

            if (numero_sugerido == Numero) resp = "acertou";
            else if (Se_Esta_Entre(numero_sugerido, 3)) resp = "muito quente";
            else if (Se_Esta_Entre(numero_sugerido, 6)) resp = "quente";
            else if (Se_Esta_Entre(numero_sugerido, 12)) resp = "morno";
            else if (Se_Esta_Entre(numero_sugerido, 18)) resp = "frio";
            else resp = "muito frio";

            return resp;
        }

        private bool Se_Esta_Entre (int numero_sugerido, int distancia)
        {
            return (Numero + distancia) > numero_sugerido && (Numero - distancia) < numero_sugerido;
        }
    }
}
