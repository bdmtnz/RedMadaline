using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTITY
{
    public enum FUNCIONES
    {
        Lineal,
        Sigmoide,
        TangenteHip,
        Gaussiana
    }

    public class Activacion
    {
        public FUNCIONES Funcion { get; set; }

        public Activacion(FUNCIONES Funcion)
        {
            this.Funcion = Funcion;
        }

        public double Activar(double SalidaSoma)
        {
            if (Funcion == FUNCIONES.Lineal)
                return SalidaSoma;
            else if (Funcion == FUNCIONES.Sigmoide)
                return 1 / 1 + Math.Pow(Math.E, -1 * SalidaSoma);
            else if (Funcion == FUNCIONES.TangenteHip)
                return Math.Tanh(SalidaSoma);
            else if (Funcion == FUNCIONES.Gaussiana)
                return SalidaSoma >= 0 ? 1 : 0;
            else
                return 1 / 1 + Math.Pow(Math.E, -1 * SalidaSoma);
        }
    }
}
