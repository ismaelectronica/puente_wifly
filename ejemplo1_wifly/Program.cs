using System;
using System.Collections;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Controls;
using Microsoft.SPOT.Presentation.Media;
using Microsoft.SPOT.Presentation.Shapes;
using Microsoft.SPOT.Touch;
using System.Threading;

using Gadgeteer.Networking;
using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;
using Gadgeteer.Modules.GHIElectronics;
using Gadgeteer.Interfaces;

namespace ejemplo1_wifly
{
    public partial class Program
    {
        private static byte[] bufferRxDebug = new byte[200];
        private static byte[] bufferRxWifly = new byte[200];
        private static byte[] bufferTxWifly = new byte[200];
        private static string mensajeDebug = null;
        private const string GET_MAC = "get mac\r";
        private const string RUN_WAPP = "run web_app\r";
        private const string RUN_SYS = "set sys launch_string web_app\r"; 
        private const string CMD_MODE = "$$$";
        private static string mensajeWifly = null;
        private static DigitalOutput Reset;
        private static GT.Socket sckWifly;
        // This method is run when the mainboard is powered up or reset.  
        void ProgramStarted()
        {
           
            GT.Timer timer = new GT.Timer(1000); // every second (1000ms)
            timer.Tick += timer_Tick;
            timer.Start();

            //configuracion de los puertos seriales para el debug y la antena  Wifly
            rsWifly.Initialize(9600, Serial.SerialParity.None, Serial.SerialStopBits.One, 8, Serial.HardwareFlowControl.NotRequired);
            rsDebug.Initialize(115200, GT.Interfaces.Serial.SerialParity.None, GT.Interfaces.Serial.SerialStopBits.One, 8, GT.Interfaces.Serial.HardwareFlowControl.NotRequired);
            rsWifly.serialPort.DataReceived += rsWifly_DataReceived;
            rsDebug.serialPort.DataReceived += rsDebug_DataReceived;
            
            //configura el pin del reset de la antena de Wifly
            sckWifly = GT.Socket.GetSocket(2, true, null, null);
            Reset = new DigitalOutput(sckWifly, GT.Socket.Pin.Three, false, null);

            //abre lo puertos seriales del debug y del wifly
            rsDebug.serialPort.Open();
            rsWifly.serialPort.Open();

            //resetea la antena de Wifly
            Reset.Write(false);
            Thread.Sleep(5000);
            Reset.Write(true);

            Thread.Sleep(500);
            Escribir_Comando(CMD_MODE);             //Pasa la antena de Wifly a Modo de comandos
            Thread.Sleep(1000);
            Escribir_Comando(GET_MAC);              //solicita la direccion MAC de la antena
        }

        void timer_Tick(GT.Timer timer)
        {
            //Escribir_Comando(GET_MAC);
        }

        // evento de recepcion de datos del puerto debug
        void rsDebug_DataReceived(GT.Interfaces.Serial sender, System.IO.Ports.SerialData data)
        {
            int largo = 0;

            //recibe datos del puerto de debug y los envia al puerto de wifly
            if ((largo = rsDebug.serialPort.BytesToRead) > 0)
            {
                rsDebug.serialPort.Read(bufferRxDebug, 0, largo);
                //mensajeDebug = new string(System.Text.UTF8Encoding.UTF8.GetChars(bufferRxDebug, 0, largo));
                rsWifly.serialPort.Write(bufferRxDebug, 0, largo);
            }
        }

        // evento de recepcion de datos del puerto antena de wifly
        void rsWifly_DataReceived(GT.Interfaces.Serial sender, System.IO.Ports.SerialData data)
        {
            int largo = 0;

            //recibe datos del puerto de wifly y los envia al puerto de debug
            if ((largo = rsWifly.serialPort.BytesToRead) > 0)
            {
                rsWifly.serialPort.Read(bufferRxWifly, 0, largo);
                //mensajeWifly = new string(System.Text.UTF8Encoding.UTF8.GetChars(bufferRxWifly, 0, largo));
                rsDebug.serialPort.Write(bufferRxWifly, 0, largo);
            }
        }

        //Metodo para escribir un comando en la antena Wifly
        void Escribir_Comando(string comando)
        {
            byte[] bComando = System.Text.UTF8Encoding.UTF8.GetBytes(comando);
            rsWifly.serialPort.Write(bComando);
        }
    }
}
