using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTITY
{
    public class Peso
    {
        public double Valor { get; set; }

        public Peso(double Valor)
        {
            this.Valor = Valor;
        }

        public void Entrenar(double AnteriorValor, double Rata, double ErrorSalida, double Entrada)
        {
            Valor = AnteriorValor + (Rata * ErrorSalida * Entrada);
            //Valor = Valor > 1 ? 1 : Valor;
            //Valor = Valor < -1 ? -1 : Valor;
            Valor = Valor > .5 ? .5 : Valor;
            Valor = Valor < -.5 ? -.5 : Valor;
        }
    }
    
    public class Pesos
    {
        public List<Peso> Valores { get; set; }

        public Pesos()
        {
            Valores = new List<Peso>();
        }

        public Pesos(List<Peso> Valores)
        {
            this.Valores = new List<Peso>();
            Valores.ForEach( x => {
                this.Valores.Add(x);
            });
        }

        public Pesos(string Values)
        {
            if(Values != "")
            {
                Valores = new List<Peso>();
                Values = Values.Trim();
                var Split = Values.Split(';');
                foreach (var item in Split)
                {
                    Valores.Add(new Peso(Double.Parse(item)));
                }
            }
        }
    }
}
