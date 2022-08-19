using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;

namespace DMVService
{
    public class Worker : BackgroundService
    {

        private readonly ILogger<Worker> _logger;
        private const string logPath = @"C:\Users\Bernard M\Desktop\TempForWinServ\CS455DMVDataService.log";
        CredentialProfileOptions credentials = new CredentialProfileOptions()
        {
            AccessKey = "ASIATXNKKMFAAO6DP6PV",
            SecretKey = "SFCDJc8qqN49Ud6VCHXQC+tHdObR8kK9/cGJC6bU",
            Token = "FwoGZXIvYXdzEMP//////////wEaDKEIt1poFzp7SA8n9iLTAWIMVCVgw6iLFYEUzKU5EkMrvh6ZFs6DWseIU5UdpBb1VNbAT0xKtSOQG7uuS3qrHAWXnK6GXkpMEvFVBIsPjTOLHBVtvIBKzvYCGPR+lXdIpaJY5FTDkkTQCLtKCyaHOGtDAWn+sMd7QFHKyC5FXY1guqm8Lu2Kq83richDG92a4y4qANhiv91xg3Mv4qAH6x6nEyUscQ5yiDnlzq9EYuduYgcViciLramD9QmxLHgBv+e15XmFu7b/GctajsFFGt6Xyv2xrO20ySs0wNcLC01tR0cokbCzhgYyLb9IcjarNuLjQ+6T/c01Y71FMO6vDM9VRKvGdzV2ZJNZ+eJ8Dp8Ld1S/oWkPKA=="
        };
       

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                Run();
                await Task.Delay(1000, stoppingToken);
            }
        }

        public void Run()
        {
            String message = null;
            receiveDownwardQueue queue = new receiveDownwardQueue();
            message = queue.receiveQueue(credentials);

            if (message != null)
            {
                //foreach (string jsonMessage in message)
                //{
                String readTime = String.Format("{0}:\t", DateTimeOffset.Now);
                WriteToLog(String.Format("Date:{0} Read message:{1}", readTime, message));
                //deserialize.json message
                OwnerVehicleData vehicleData = JsonSerializer.Deserialize<OwnerVehicleData>(message);
                try
                {
                    //string fileName = "InsuranceDatabase.xml";
                    string fileName = @"C:\Users\Bernard M\Desktop\TempForWinServ\DMVDatabase.xml";
                    string content = System.IO.File.ReadAllText(fileName);
                    
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(content);
                    //Console.WriteLine(content);

                    XmlElement root = xmlDoc.DocumentElement;
                    XmlNodeList vehicle = root.SelectNodes("vehicle[@plate='" + vehicleData.plate + "']");
                    //Parse xml database file and add values into InsuranceDB object
                    if (vehicle.Count > 0)
                    {
                        foreach (XmlNode element in vehicle)
                        {
                            XmlNode make = element.SelectSingleNode("make");
                            XmlNode model = element.SelectSingleNode("model");
                            XmlNode color = element.SelectSingleNode("color");

                            XmlAttribute language = (XmlAttribute)element.SelectSingleNode("owner/@preferredLanguage");
                            XmlNode name = element.SelectSingleNode("owner/name");
                            XmlNode contact = element.SelectSingleNode("owner/contact");

                            vehicleData.vehicle = color.InnerText + " " + make.InnerText + " " + model.InnerText;
                            vehicleData.language = language.InnerText;
                            vehicleData.name = name.InnerText;
                            String phonewithPlus = contact.InnerText;
                            vehicleData.phone = phonewithPlus.Remove(0, 1);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Plate number not in system");
                    }
                    //Serialize to JSON and queueupward
                    JsonSerializerOptions options = new JsonSerializerOptions();
                    options.WriteIndented = true;
                    string dataJson = JsonSerializer.Serialize(vehicleData, options);
                    String postTime = String.Format("{0}:\t", DateTimeOffset.Now);
                    WriteToLog(String.Format("Date:{0} Posted message:{1}", postTime, dataJson));

                    //send insuranceJson to long queue
                    sendUpwardQueue upQueue = new sendUpwardQueue(dataJson, credentials);
                }
                catch (XmlException ex)
                {
                    Console.WriteLine(ex);
                }
                
            }
            else
            {
                Console.WriteLine("message from queue was empty");
            }
        }

        public void WriteToLog(String message)
        {
            using StreamWriter writer = new StreamWriter(logPath, append: true);
            writer.WriteLine(message);
        }

    }
}
