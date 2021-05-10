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

        private double ErrorPatron()
        {
            //OBTIENE EL ERROR GENERADO POR LA CAPA DE SALIDA
            var Errores = 0.0;
            Capas[Capas.Count - 1].Neuronas.ForEach(
                n => Errores += Math.Abs(n.Salida.Error)
            );
            return Errores / Capas[Capas.Count - 1].Neuronas.Count;
        }

        private double ErrorPatronTemp()
        {
            //OBTIENE EL ERROR GENERADO POR LA CAPA DE SALIDA
            var Errores = 0.0;
            Capas[Capas.Count - 1].Neuronas.ForEach(
                n => Errores += Math.Abs(n.SalidaTemp.Error)
            );
            return Errores / Capas[Capas.Count - 1].Neuronas.Count;
        }

        private void Normalizar()
        {
            Rata = 1;
            Error = 1;
            ErrorMaxPermitido = 0.1;
            Patrones = new List<Patron>();
        }

        private void cargarYD(Patron patron)
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
                cargarYD(Patrones[i]);
                PatronIndex = i;
                var ErrorPatron = 0.0;
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
                                Capas[c - 1].Neuronas.Select( x => x.Salida.YR).ToList()
                            );
                        }
                        
                    }
                    
                }
                // SE CALCULA LOS ERRORES LINEALES DE LAS SALIDAS DEL PATRON
                for (int k = 0; k < Capas[Capas.Count-1].Neuronas.Count; k++)
                {
                    Capas[Capas.Count - 1].Neuronas[k].Salida.Error = Capas[Capas.Count - 1].Neuronas[k].Salida.YD -
                        Capas[Capas.Count - 1].Neuronas[k].Salida.YR;
                }
                //SE CALCULA LOS ERRORES NO LINEALES DE LAS CAPAS
                for (int t = Capas.Count - 2; t >=0 ; t--)
                {
                    for (int k = 0; k < Capas[t].Neuronas.Count; k++)
                    {
                        Capas[t].Neuronas[k].CalcularError(
                            Capas[t+1].Neuronas.Select( n => n.Pesos.Valores[k].Valor ).ToList(),
                            Capas[t+1].Neuronas.Select( x => x.Salida.Error ).ToList()
                        );
                    }
                }
                //SUMA DE ERROR DE PATRON
               
                var Errores = 0.0;
                Capas[Capas.Count - 1].Neuronas.ForEach(x =>
                {
                    Errores += Math.Abs(x.Salida.Error);
                });
                ErrorPatron += Errores / Capas[Capas.Count - 1].Neuronas.Count;
                ErrorIteracion += ErrorPatron;
                //MODIFICAR PESOS Y UMBRALES VALIDADOOOOO
                //Recorro las capas
                for (int j = 0; j < Capas.Count; j++)
                {
                    //Recorro las neuronas
                    for (int h = 0; h < Capas[j].Neuronas.Count; h++)
                    {
                        if (j==Capas.Count-1)
                        {
                            //public void EntrenarPesos(List<double> Entradas, double Rata, double ErrorPatron)
                            Capas[j].Neuronas[h].EntrenarPesos(
                                Capas[j - 1].Neuronas.Select(x => x.Salida.YR).ToList(),
                                Rata, 
                                Capas[j].Neuronas[h].Salida.Error
                            );
                            //public void Entrenar(double AnteriorValor, double Rata, double ErrorSalida, double Entrada)
                            Capas[j].Neuronas[h].Umbral.Entrenar(
                                Capas[j].Neuronas[h].Umbral.Valor, 
                                Rata,
                                Capas[j].Neuronas[h].Salida.Error, 
                                Xo
                            );
                        }
                        else if(j==0)
                        {
                            Capas[j].Neuronas[h].EntrenarPesos(
                                Patrones[PatronIndex].Entradas,
                                Rata, 
                                ErrorPatron
                            );
                            Capas[j].Neuronas[h].Umbral.Entrenar(
                                Capas[j].Neuronas[h].Umbral.Valor, 
                                Rata,
                                ErrorPatron, 
                                Xo
                            );
                        }
                        else
                        {
                            Capas[j].Neuronas[h].EntrenarPesos(
                                Capas[j - 1].Neuronas.Select(x => x.Salida.YR).ToList(),
                                Rata, 
                                ErrorPatron
                            );
                            Capas[j].Neuronas[h].Umbral.Entrenar(
                                Capas[j].Neuronas[h].Umbral.Valor, 
                                Rata,
                                ErrorPatron, 
                                Xo
                            );
                        }
                        //Se llenan los pesosy umbrales temporales con los permanenetes
                        Capas[j].Neuronas[h].PesosTemp.Valores = Capas[j].Neuronas[h].Pesos.Valores.Select(x => new Peso(x.Valor)).ToList();
                        Capas[j].Neuronas[h].UmbralTemp.Valor = Capas[j].Neuronas[h].Umbral.Valor;
                    }
                
                }
                
                //BACK FOWARD
                RecurrenteCapa(Capas[Capas.Count - 1]);
                
            }
            ErrorIteracion = ErrorIteracion / Patrones.Count;
            Console.WriteLine(ErrorIteracion);
            Console.WriteLine("_______________");
            return ErrorIteracion;
        }

        public void ReEntrenarTemp(Patron Patron)
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
                        Capas[c].Neuronas[n].ActivarTemp(
                            Capas[c].Activacion,
                            Patron.Entradas
                        );
                    }
                    else
                    {
                        //SE ENTRENA CON LAS SALIDAS DE LA ANTERIOR CAPA
                        Capas[c].Neuronas[n].ActivarTemp(
                            Capas[c].Activacion,
                            Capas[c - 1].Neuronas.Select(x => x.Salida.YR).ToList()
                        );
                    }

                }

            }

            // SE CALCULA LOS ERRORES LINEALES DE LAS SALIDAS DEL PATRON
            for (int k = 0; k < Capas[Capas.Count - 1].Neuronas.Count; k++)
            {
                Capas[Capas.Count - 1].Neuronas[k].SalidaTemp.Error = Capas[Capas.Count - 1].Neuronas[k].SalidaTemp.YD -
                    Capas[Capas.Count - 1].Neuronas[k].SalidaTemp.YR;
            }

            //SE CALCULA LOS ERRORES NO LINEALES DE LAS CAPAS
            for (int t = Capas.Count - 2; t >= 0; t--)
            {
                for (int k = 0; k < Capas[t].Neuronas.Count; k++)
                {
                    Capas[t].Neuronas[k].CalcularErrorTemp(
                        Capas[t + 1].Neuronas.Select(n => n.PesosTemp.Valores[k].Valor).ToList(),
                        Capas[t + 1].Neuronas.Select(x => x.SalidaTemp.Error).ToList()
                    );
                }
            }
        }

        public void ReEntrenar(Patron Patron)
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
                            Capas[c - 1].Neuronas.Select(x => x.Salida.YR).ToList()
                        );
                    }

                }

            }

            // SE CALCULA LOS ERRORES LINEALES DE LAS SALIDAS DEL PATRON
            for (int k = 0; k < Capas[Capas.Count - 1].Neuronas.Count; k++)
            {
                Capas[Capas.Count - 1].Neuronas[k].Salida.Error = Capas[Capas.Count - 1].Neuronas[k].Salida.YD -
                    Capas[Capas.Count - 1].Neuronas[k].Salida.YR;
            }

            //SE CALCULA LOS ERRORES NO LINEALES DE LAS CAPAS
            for (int t = Capas.Count - 2; t >= 0; t--)
            {
                for (int k = 0; k < Capas[t].Neuronas.Count; k++)
                {
                    Capas[t].Neuronas[k].CalcularErrorTemp(
                        Capas[t + 1].Neuronas.Select(n => n.Pesos.Valores[k].Valor).ToList(),
                        Capas[t + 1].Neuronas.Select(x => x.Salida.Error).ToList()
                    );
                }
            }
        }

        private void RecurrenteCapa(Capa Capa)
        {
            BuscarNeurona(Capa);
            //CRITERIO DE PARADA (PRIMERA CAPA)
            if (Capa.Indice > 0)
                RecurrenteCapa(Capas[Capa.Indice - 1]);  
        }
        
        public void BuscarNeurona(Capa capa)
        {
            //SE ORDENAN SEGUN LOS ERRORES MAS CERCANOS A CERO
            var NeuronasOrdenadas = capa.Neuronas.Select(n => n);
            NeuronasOrdenadas = NeuronasOrdenadas.OrderBy(x => x.Salida.Error);

            foreach (var item in NeuronasOrdenadas)
            {
                List<double> Entradas;

                if (capa.Indice == 0)
                    Entradas = Patrones[PatronIndex].Entradas;
                else
                    Entradas = Capas[capa.Indice - 1].Neuronas.Select(x => x.Salida.YR).ToList();

                var Neurona = capa.Neuronas.Find(x => x.Indice == item.Indice);
                RecalcularPesosUmbrales(Neurona, Entradas);
                ReEntrenarTemp(Patrones[PatronIndex]);

                //REVISO SI ACEPTO O NO ACEPTO LOS CAMBIOS
                //var ErrorTemp = Neurona.SalidaTemp.Error;
                var ErrorTemp = ErrorPatronTemp();
                if (ErrorTemp == 0)
                    break;
                if (ErrorTemp < Neurona.Salida.Error)
                {
                    Neurona.AceptarPesos();
                    Neurona.AceptaUmbrales();
                    Neurona.Salida.Error = ErrorTemp;
                    Neurona.Usada = true;
                    ReEntrenar(Patrones[PatronIndex]);
                    //Neurona.Activar(capa.Activacion, entradas);
                    //capa.Neuronas[indices[i]].Salida.Error = ErrorTemp;
                }


            }
        }
        
        private double RecalcularPesosUmbrales(Neurona Min, List<double> Entradas)
        {
            //RECALCULAMOS PESOS DE LA SELECCIONADA
            var ErrorAnt = Min.Salida.Error;
            Min.EntrenarPesosTemp(Entradas, Rata, ErrorPatronTemp());
            Min.UmbralTemp.Entrenar(Min.Umbral.Valor, Rata, ErrorPatronTemp(), 1);
            return ErrorAnt;
        }

        public List<double> Generalizar(Patron Patron)
        {
            //SE ITERA POR PATRÓN
            for (int i = 0; i < Patrones.Count; i++)
            {
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
                        }
                        else
                        {
                            //SE ENTRENA CON LAS SALIDAS DE LA ANTERIOR CAPA
                            Capas[c].Neuronas[n].Activar(
                                Capas[c].Activacion,
                                Capas[c - 1].Neuronas.Select(x => x.Salida.YR).ToList()
                            );
                        }
                    }
                }                
            }
            return Capas[Capas.Count - 1].Neuronas.Select(n => n.Salida.YR).ToList();
        }

        public void ReiniciarRed()
        {
            Entrenamientos = 0;
            Error = 10;
            var R = new Random();
            //GENERAR ALEATORIAMENTE LOS PESOS Y UMBRALES DE CADA NEURONA DE CADA CAPA
            //RESPETANDO LOS LIMITES EN CADA CASO
            for (int i = 0; i < Capas.Count; i++)
            {
                //SE RECORRE LAS NEURONAS DE CADA CAPA
                for (int j = 0; j < Capas[i].Neuronas.Count; j++)
                {
                    //GENERAMOS ALEATORIAMENTE LOS PESOS
                    Capas[i].Neuronas[j].Pesos.Valores = Capas[i].Neuronas[j].Pesos.Valores.Select(x => new Peso(R.NextDouble())).ToList();
                    Capas[i].Neuronas[j].PesosTemp.Valores.Clear();
                    //GENERAMOS ALEATORIAMENTE LOS UMBRALES
                    Capas[i].Neuronas[j].Umbral.Valor = R.NextDouble();
                    Capas[i].Neuronas[j].UmbralTemp.Valor = 0;
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
                }
            }
            ReiniciarRed();
        }
        
    }
    
    
}
