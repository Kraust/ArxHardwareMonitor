using Arx;
using ArxHardwareMonitor.Properties;
using OpenHardwareMonitor.Hardware;
using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace ArxHardwareMonitor
{
   public partial class ArxGUI : Form
   {
      private static NotifyIcon trayIcon;
      private static ContextMenu trayMenu;
      static Computer myComputer = new Computer();
      System.Timers.Timer MainTimer = new System.Timers.Timer();


      public ArxGUI()
      {
         this.Opacity = 0;
         InitializeComponent();
         trayMenu = new ContextMenu();
         trayMenu.MenuItems.Add("Exit", Exit);

         trayIcon = new NotifyIcon();
         trayIcon.Text = "ArxHardwareMonitor";
         trayIcon.Icon = Resources.arxhw;
         trayIcon.ContextMenu = trayMenu;
         trayIcon.Visible = true;

         Start();

         myComputer.CPUEnabled = true;
         myComputer.GPUEnabled = true;
         myComputer.RAMEnabled = true;
         myComputer.Open();

         MainTimer.Interval = 1000;
         MainTimer.Elapsed += MainHandler;
         MainTimer.Enabled = true;


      }

      static void MainHandler(Object source, System.Timers.ElapsedEventArgs e)
      {
         string outStr = "";
         foreach (var hardwareItem in myComputer.Hardware)
         {
            hardwareItem.Update();
            hardwareItem.GetReport();

            outStr += String.Format("<div class='row'><div class='col-xs-12'><h1>{0}</h1></div></div>", hardwareItem.Name);

            foreach (var sensor in hardwareItem.Sensors)
            {
               if (sensor.SensorType == SensorType.Temperature)
               {
                  outStr += String.Format("<div class='row'><div class='col-xs-8'>{0}</div><div class='col-xs-4'>{1}C</div></div>", sensor.Name, sensor.Value);
               }

               if (sensor.SensorType == SensorType.Load)
               {
                  outStr += String.Format("<div class='row'><div class='col-xs-8'>{0}</div><div class='col-xs-4 vcenter'><div class='progress'><div class='progress-bar' role='progressbar' aria-valuenow='{1}' aria-minvalue='0' aria-maxvalue='100' style='width:{1}%'></div></div></div></div>", sensor.Name, Math.Round(Convert.ToDouble(sensor.Value)));
               }

            }
         }
         LogitechArx.LogiArxSetTagContentById("test", outStr);
      }

      static void SDKCallback(int eventType, int eventValue, System.String eventArg, System.IntPtr context)
      {
         if (eventType == LogitechArx.LOGI_ARX_EVENT_MOBILEDEVICE_ARRIVAL)
         {
            // Initilize Environment
            if (!LogitechArx.LogiArxAddFileAs("view.html", "view.html"))
            {
               int retCode = LogitechArx.LogiArxGetLastError();
               Debug.WriteLine("exec failed: " + retCode);
            }

            if (!LogitechArx.LogiArxAddFileAs("bootstrap.min.css", "bootstrap.min.css"))
            {
               int retCode = LogitechArx.LogiArxGetLastError();
               Debug.WriteLine("exec failed: " + retCode);
            }

            if (!LogitechArx.LogiArxAddFileAs("bootstrap.min.js", "bootstrap.min.js"))
            {
               int retCode = LogitechArx.LogiArxGetLastError();
               Debug.WriteLine("exec failed: " + retCode);
            }

            if (!LogitechArx.LogiArxSetIndex("view.html"))
            {
               int retCode = LogitechArx.LogiArxGetLastError();
               Debug.WriteLine("exec failed: " + retCode);
            }
         }
         else if (eventType == LogitechArx.LOGI_ARX_EVENT_MOBILEDEVICE_REMOVAL)
         {
            // Handle Disconnect
         }
         else if (eventType == LogitechArx.LOGI_ARX_EVENT_TAP_ON_TAG)
         {
            // Handle Inputs
         }
      }

      static readonly LogitechArx.logiArxCB arxCallback = new LogitechArx.logiArxCB(SDKCallback);
      static void Start()
      {
         LogitechArx.logiArxCbContext contextCallback;
         contextCallback.arxCallBack = arxCallback;
         contextCallback.arxContext = System.IntPtr.Zero;
         bool retVal = LogitechArx.LogiArxInit("arx.hardware.monitor", "ArxHWM", ref
         contextCallback);
         if (!retVal)
         {
            int retCode = LogitechArx.LogiArxGetLastError();
            Console.WriteLine("loading sdk failed: " + retCode);
         }
      }

      static void Exit(object sender, EventArgs e)
      {
         trayIcon.Visible = false;

         Application.Exit();
      }
   }
}
