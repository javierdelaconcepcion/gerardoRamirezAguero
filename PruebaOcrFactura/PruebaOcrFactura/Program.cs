using System;
using System.Threading.Tasks;
using Azure;
using System.IO;
using Azure.AI.FormRecognizer;
using Azure.AI.FormRecognizer.Models;
using Azure.AI.FormRecognizer.Training;
using System.Threading;
using System.Globalization;

namespace PruebaOcrFactura
{
    public class Factura {

        public string Empresa { get; set; }
        public string Gasto { get; set; }

        public string Consumo { get; set; }

        public string Periodo { get; set; }

        //public static implicit operator Factura(Task<object> v)
        //{
        //    throw new NotImplementedException();
        //}
    }
    class Program
    {

        private static readonly string endPoint = "https://geryocrtfg.cognitiveservices.azure.com/";
        private static readonly string apiKey = "b86582409b0a4cb0b0d7c1bc74300250";
        private static AzureKeyCredential credential = new AzureKeyCredential(apiKey);

        private static string sasDatos = "https://storagepapurri.blob.core.windows.net/prueba2?sp=racwdl&st=2022-08-22T16:05:05Z&se=2022-08-28T00:05:05Z&spr=https&sv=2021-06-08&sr=c&sig=8uEiYknrI2XrBT%2B3LPbQZBgtfMBRUWDWAt8B9jKemfk%3D";
        private static string modelID = String.Empty;
        private static string facturaSas = "https://storagepapurri.blob.core.windows.net/prueba1/prueba1.jpeg?sp=racwdyt&st=2022-08-10T14:05:28Z&se=2022-08-12T22:05:28Z&spr=https&sv=2021-06-08&sr=b&sig=1wpgmnRO%2FvBQ6ondcMWaVmdBf7CH1uZi%2BQSt6alSUSg%3D";
        private static Stream factura = new FileStream("C:/Users/gerardiweb/Desktop/TFG/DESARROLLO/WhatsApp Image 2022-07-24 at 4.56.52 PM.jpeg", FileMode.Open, FileAccess.Read);
        

        static void Main(string[] args)
        {
            var clienteFR = AutenticarCliente();
            var clienteEntrenador = AutenticarClienteEntrenador();
            Factura objFactura = new Factura();
            //var consulta = ConsultarModelos(clienteEntrenador);
            //consulta.Wait();
            
            var taskEntrenador = EntrenarModelo(clienteEntrenador, sasDatos, true);
            taskEntrenador.Wait();

            modelID = taskEntrenador.Result;

            var taskAnalizar =  AnalizarDocumento(clienteFR, modelID, factura, objFactura);
            taskAnalizar.Wait();
            objFactura = (Factura)taskAnalizar.Result;

            if (objFactura != null)
            {
                Console.WriteLine("Empresa: "+objFactura.Empresa);
                Console.WriteLine("Consumo: "+objFactura.Consumo);
                Console.WriteLine("Gasto: "+objFactura.Gasto);
                Console.WriteLine("Periodo consumo: "+objFactura.Periodo);
            }
            // Console.ReadLine();

        }
        private static FormRecognizerClient AutenticarCliente()
        {
            return new FormRecognizerClient(endpoint: new Uri(endPoint), credential: new AzureKeyCredential(apiKey));
        }
        private static FormTrainingClient AutenticarClienteEntrenador()
        {
            return new FormTrainingClient(endpoint: new Uri(endPoint), credential: new AzureKeyCredential(apiKey));
        }

        private static async Task<string> EntrenarModelo(FormTrainingClient clienteEntrenador, string urlSasContenedor, bool etiquetas = false)
        {
            CustomFormModel customFormModel = await clienteEntrenador.StartTrainingAsync(new Uri(urlSasContenedor), etiquetas).WaitForCompletionAsync();

            Console.WriteLine("Información modelo:");
            Console.WriteLine("ID model: " +customFormModel.ModelId );
            Console.WriteLine("ID model status: " + customFormModel.Status);

            if (customFormModel.Errors.Count > 0)
            {
                foreach (var err in customFormModel.Errors)
                {
                    Console.WriteLine("  Error: " + err.ErrorCode + " - " + err.Message);
                }
            }
            return customFormModel.ModelId;
        }

        private static async Task<object> AnalizarDocumento(FormRecognizerClient cliente, string modelo, Stream urlFichero, Factura factura)
        {
            string etiqueta = String.Empty;
            RecognizeCustomFormsOptions recognizeCustomFormsOptions = new RecognizeCustomFormsOptions();
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;
            //RecognizedFormCollection forms = await cliente.StartRecognizeCustomFormsFromUri(modelo, new Uri(urlFichero)).WaitForCompletionAsync();

         RecognizedFormCollection forms = await cliente.StartRecognizeCustomFormsAsync(modelo, urlFichero).WaitForCompletionAsync();

            foreach (RecognizedForm form in forms)
            {
                Console.WriteLine($"Tipo de documento: {form.FormType}");
                foreach (FormField field in form.Fields.Values)
                {

                    if (field.Name != null)
                    {
                        

                        etiqueta =(String) $"{field.Name}";
                        if (etiqueta != "")
                        {
                            switch (etiqueta)
                            {
                                case "Empresa":
                                    factura.Empresa = field.ValueData.Text;
                                    break;
                                case "Consumo":
                                    factura.Consumo = field.ValueData.Text;
                                    break;

                                case "Gasto":
                                    factura.Gasto = field.ValueData.Text;
                                    break;
                                case "Periodo consumo":
                                    factura.Periodo = field.ValueData.Text;
                                    break;
                                default:
                                    break;
                            }
                        }

                    }
                }
            }

            return MapeaoDatos(factura);
        }

        

        private static async Task ConsultarModelos(FormTrainingClient cliente)
        {
            AccountProperties accountProperties =  cliente.GetAccountProperties();
          
            Console.WriteLine($"La cuenta tiene {accountProperties.CustomModelCount} modelos.");
            Console.WriteLine($"Límite de modelos en la cuenta {accountProperties.CustomModelLimit} modelos.");

            Pageable<CustomFormModelInfo> models =  cliente.GetCustomModels();

            foreach (CustomFormModelInfo modelInfo in models)
            {
                Console.WriteLine($"Información del modelo:");
                Console.WriteLine($"    Id Modelo: {modelInfo.ModelId}");
                Console.WriteLine($"    Estado: {modelInfo.Status}");
                Console.WriteLine($"    Iniciado: {modelInfo.TrainingStartedOn}");
                Console.WriteLine($"    Finalizado: {modelInfo.TrainingCompletedOn}");
            }


        }

        private static Factura MapeaoDatos(Factura datosFactura)
        {
            Console.WriteLine("Mapeo de datos");

            var cultureInfo = new CultureInfo("de-DE");

            String[] dateString = datosFactura.Periodo.Split(' ');

            String[] consumoString = datosFactura.Consumo.Split(' ');

            String[] gastoString = datosFactura.Gasto.Split(' ');

            datosFactura.Periodo = dateString[2].ToString().Trim();

            datosFactura.Consumo = consumoString[0].ToString().Trim();

            datosFactura.Gasto = gastoString[0].ToString().Trim();

            
            return datosFactura;

        }
    }
}
