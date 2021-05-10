using System.Collections.Generic;
using System.Linq;

namespace ENTITY
{
    public class Capa
    {
        public List<Neurona> Neuronas { get; set; }
        public Activacion Activacion { get; set; }
        public int Indice { get; set; }
       

        public Capa(int Indice)
        {
            this.Indice = Indice;
            Normalizar();
        }

        private void Normalizar()
        {
            Neuronas = new List<Neurona>();
            Activacion = new Activacion(FUNCIONES.Sigmoide);
        }      

    }
}
