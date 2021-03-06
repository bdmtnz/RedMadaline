using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTITY
{
    public class Red
    {
        private const double Xo = 1;
        public List<Patron> Patrones { get; set; }
        public List<Capa> Capas { get; set; }
        public int Entradas => Patrones.Count > 0 ? Patrones[0].Entradas.Count : 0;
        public double Rata { get; set; }
        public int Iteraciones { get; set; }
        public int Entrenamientos { get; set; }
        public double Error { get; set; }
        public double ErrorMaxPermitido { get; set; }
        public int PatronIndex { get; set; }

        public Red()
        {
            Capas = new List<Capa>();
            Patrones = new List<Patron>();
            Normalizar();
        }

        private void Normalizar()
        {
            Rata = 1;
            Error = 1;
            ErrorMaxPermitido = 0.01;
            Patrones = new List<Patron>();
        }

        private void CargarYD(Patron patron)
        {
            for (int i = 0; i < patron.SalidasSupervisada.Count; i++)
            {
                Capas[Capas.Count - 1].Neuronas[i].Salida.YD = patron.SalidasSupervisada[i].YD;
            }
        }

        public double Entrenar()
        {            
            var ErrorIteracion = 0.0;

            //SE ITERA POR PATRÓN
            for (int i = 0; i < Patrones.Count; i++)
            {
                var ErrorPatron = 0.0;
                CargarYD(Patrones[i]);
                PatronIndex = i;
                //SE ITERA POR CAPAS
                for (int c = 0; c < Capas.Count; c++)
                {
                    //SE ITERA POR NEURONAS
                    for (int n = 0; n < Capas[c].Neuronas.Count; n++)
                    {
                        //SE ENTRENAN LA NEURONAS
                        if (c == 0)
                        {
                            //SE ENTRENA CON LAS ENTRADAS DE CADA PATRON
                            Capas[c].Neuronas[n].Activar(
                                Capas[c].Activacion,
                                Patrones[i].Entradas
                            );
                            //Capas[j].Entrenar(Patrones[i], Rata, Xo);
                        }
                        else
                        {
                            //SE ENTRENA CON LAS SALIDAS DE LA ANTERIOR CAPA
                            Capas[c].Neuronas[n].Activar(
                                Capas[c].Activacion,
                                Capas[c - 1].Neuronas.Select(x => x.Salida.Activado).ToList()
                            );
                        }

                    }

                }
                //CALCULAMOS EL ERROR DE CADA PATRON
                //var Activacion = Capas[Capas.Count - 1].Activacion;
                var Activacion = new Activacion(FUNCIONES.Sigmoide);
                Capas[Capas.Count - 1].Neuronas.ForEach(n => {
                    ErrorPatron += Math.Abs(n.Salida.YD - n.Salida.YR);
                });

                //CALCULAR EL ERROR ASIGNADO A CADA SALIDA
                Capas[Capas.Count - 1].Neuronas.ForEach(n => {
                    //var DerivadaActivacion = Activacion.Activar(n.Salida.YR) * (1 - Activacion.Activar(n.Salida.YR));
                    n.ErrorSalida(Capas[Capas.Count - 1].Activacion.Activar(n.Salida.YD - n.Salida.YR));
                });
                //IDENTIFICAMOS SI ES APLICABLE A BACK PROPAGATION                
                /*if (ErrorPatron > ErrorMaxPermitido)
                {
                    if (Capas.Count > 1)
                        //BACK PROPAGATION
                        Backpropagation(Capas[Capas.Count - 2], this);
                }*/
                ErrorPatron /= Capas[Capas.Count - 1].Neuronas.Count;
                ErrorIteracion += ErrorPatron;
                Backpropagation(Capas[Capas.Count - 2], ErrorPatron, this);

            }
            ErrorIteracion /= (Patrones.Count * Capas[Capas.Count - 1].Neuronas.Count);
            Console.WriteLine(ErrorIteracion);
            Console.WriteLine("_______________");
            return ErrorIteracion;
        }        
        
        private void Backpropagation(Capa Capa, double ErrorLinealPatron, Red Red)
        {
            //APLICAR LAS PARCIALES PARA CALCULAR ERRORES Y ENTRENAR PESOS Y UMBRALES
            for (int i = 0; i < Capa.Neuronas.Count; i++)
            {
                //ERRORES DE NEURONA Y UMBRA
                var Pesos = Red.Capas[Capa.Indice + 1].Neuronas.Select(n => n.Pesos.Valores[i].Valor).ToList();
                var Errores = Red.Capas[Capa.Indice + 1].Neuronas.Select(n => n.Error).ToList();
                var DerivadaActivacion = Capa.Activacion.Activar(Capa.Neuronas[i].Salida.YR) * (1 - Capa.Activacion.Activar(Capa.Neuronas[i].Salida.YR));
                Capa.Neuronas[i].ErrorOculto(Pesos, Errores, DerivadaActivacion);
                Capa.Neuronas[i].Umbral.Entrenar(Red.Rata, Capa.Neuronas[i].Error, ErrorLinealPatron, Xo);
                //ERRORES DE PESOS
                for (int j = 0; j < Capa.Neuronas[i].Pesos.Valores.Count; j++)
                {
                    if(Capa.Indice == 0)
                    {
                        Capa.Neuronas[i].Pesos.Valores[j].CalcularError(Capa.Neuronas[i].Error, Red.Patrones[Red.PatronIndex].Entradas[j]);
                        Capa.Neuronas[i].Pesos.Valores[j].Entrenar(Red.Rata,Capa.Neuronas[i].Error, ErrorLinealPatron, Red.Patrones[Red.PatronIndex].Entradas[j]);
                    }
                    else
                    {
                        Capa.Neuronas[i].Pesos.Valores[j].CalcularError(Capa.Neuronas[i].Error, Red.Capas[Capa.Indice - 1].Neuronas[j].Salida.Activado);
                        Capa.Neuronas[i].Pesos.Valores[j].Entrenar(Red.Rata, Capa.Neuronas[i].Error, ErrorLinealPatron, Red.Capas[Capa.Indice - 1].Neuronas[j].Salida.Activado);
                    }
                }
            }
            //CRITERIO DE PARO
            if (Capa.Indice > 0)
                Backpropagation(Capas[Capa.Indice - 1], ErrorLinealPatron, Red);
        }       
        
        
        public List<double> Generalizar(Patron Patron)
        {
            //SE ITERA POR CAPAS
            for (int c = 0; c < Capas.Count; c++)
            {
                //SE ITERA POR NEURONAS
                for (int n = 0; n < Capas[c].Neuronas.Count; n++)
                {
                    //SE ENTRENAN LA NEURONAS
                    if (c == 0)
                    {
                        //SE ENTRENA CON LAS ENTRADAS DE CADA PATRON
                        Capas[c].Neuronas[n].Activar(
                            Capas[c].Activacion,
                            Patron.Entradas
                        );
                    }
                    else
                    {
                        //SE ENTRENA CON LAS SALIDAS DE LA ANTERIOR CAPA
                        Capas[c].Neuronas[n].Activar(
                            Capas[c].Activacion,
                            Capas[c - 1].Neuronas.Select(x => x.Salida.Activado).ToList()
                        );
                    }
                }
            }
            //CALCULAR EL ERROR DE CADA SALIDA Y PATRON
            var Activacion = Capas[Capas.Count - 1].Activacion;
            var ErrorPatron = 0.0;
            //var Activacion = new Activacion(FUNCIONES.Sigmoide);
            Capas[Capas.Count - 1].Neuronas.ForEach(n => {
                var DerivadaActivacion = Activacion.Activar(n.Salida.YR) * (1 - Activacion.Activar(n.Salida.YR));
                ErrorPatron += n.ErrorSalida(DerivadaActivacion);
                // ErrorPatron += Math.Abs(n.ErrorSalida(Activacion.Activar(n.Salida.YR)));
            });
            Console.WriteLine("Error al generalizar: ", ErrorPatron);
            return Capas[Capas.Count - 1].Neuronas.Select(n => n.Salida.Activado).ToList();
        }

        public void ReiniciarRed()
        {
            Entrenamientos = 0;
            Error = 10;
            //GENERAR ALEATORIAMENTE LOS PESOS Y UMBRALES DE CADA NEURONA DE CADA CAPA
            //RESPETANDO LOS LIMITES EN CADA CASO
            for (int i = 0; i < Capas.Count; i++)
            {
                //SE RECORRE LAS NEURONAS DE CADA CAPA
                for (int j = 0; j < Capas[i].Neuronas.Count; j++)
                {
                    //GENERAMOS ALEATORIAMENTE LOS PESOS
                    Capas[i].Neuronas[j].Pesos.Valores = Capas[i].Neuronas[j].Pesos.Valores.Select(x => new Peso(Plataforma.Random())).ToList();
                    //GENERAMOS ALEATORIAMENTE LOS UMBRALES
                    Capas[i].Neuronas[j].Umbral.Valor = Plataforma.Random();
                }
            }
        }

        public void ReiniciarCapas(List<int> Neuronas, List<Activacion> Funciones)
        {
            Capas.Clear();
            for (int i = 0; i < Neuronas.Count; i++)
            {
                Capas.Add(new Capa(i));
                Capas[i].Activacion = Funciones[i];
                for (int j = 0; j < Neuronas[i]; j++)
                {
                    Capas[i].Neuronas.Add(new Neurona(j));
                    if (i == 0)
                    {
                        for (int p = 0; p < Patrones[0].Entradas.Count; p++)
                        {
                            Capas[i].Neuronas[j].Pesos.Valores.Add(new Peso(Plataforma.Random()));
                        }
                    }
                    else
                    {
                        for (int p = 0; p < Capas[i - 1].Neuronas.Count; p++)
                        {
                            Capas[i].Neuronas[j].Pesos.Valores.Add(new Peso(Plataforma.Random()));
                        }
                    }
                }
            }
            ReiniciarRed();
        }

    }
    
    
}
