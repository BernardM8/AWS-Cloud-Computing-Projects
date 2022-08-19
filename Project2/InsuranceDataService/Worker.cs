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

namespace InsuranceDataService
{
    class PatientInput
    {
        public string id { get; set; }
        public string name { get; set; }

    }

    class InsuranceDB
    {
        public string id { get; set; }
        public string policyNumber { get; set; }
        public string provider { get; set; }

    }

    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private const string logPath = @"C:\Users\Bernard M\Desktop\TempForWinServ\CS455InsuranceDataService.log";
        CredentialProfileOptions credentials = new CredentialProfileOptions()
        {
            AccessKey = "ASIATXNKKMFAASDA562T",
            SecretKey = "qFUq3TSYjrMVnKZejkmIkAY1YJhYqCDCy90vigtd",
            Token = "FwoGZXIvYXdzEGMaDEDlnpHcSIgmzpxlAiLTAVzEAgqK9b/4b7qKJtlMw4Lx7sNfE9wvWyNBSmUzwpz/s370691zSo2VkhmwHWo2HRqDV+yaWiui4DDZvgpS+Kr19nvtbHbJZqI5q1DY/e3dfy75+x9VS3ThsJiUaUzvaMIgz2bsIAWE3qBw6v8cEcuZOC41c38E5Mvb1oggxCa6mKfXWKeIWwwiYAn3M232w8xLnWFaEEPN/lVms9u4q+DsfAeDlpibSHBLjc/jAuha7itbjZwdYJSJNcUvefkHJ9XCZh2LzEDUjssR4bR8PYQOi7covpyehgYyLWp1CG7niJ15rEJmaYX4jw2wmiGFrEICzSzwpPvgmhQK//eq5da6WjU3VAx9dw=="
        };
        

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            //do anything at start of service
            //String startTime = String.Format("{0}:/t{1}", DateTimeOffset.Now);
            //WriteToLog(String.Format("Date:{0} Start time", startTime));
            await base.StartAsync(cancellationToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            //do anything at stop of service
            //String stopTime = String.Format("{0}:/t{1}", DateTimeOffset.Now);
            //WriteToLog(String.Format("Date:{0} Stop time", stopTime));
            await base.StopAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                Run();              
                await Task.Delay(1000, stoppingToken);
            }
        }

        public void Run() {
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
                    PatientInput patient = JsonSerializer.Deserialize<PatientInput>(message);
                    try
                    {
                    //string fileName = "InsuranceDatabase.xml";
                        string fileName = @"C:\Users\Bernard M\Desktop\TempForWinServ\InsuranceDatabase.xml";
                        string content = System.IO.File.ReadAllText(fileName);
                        InsuranceDB insurance = new InsuranceDB();
                        XmlDocument xmlDoc = new XmlDocument();
                        xmlDoc.LoadXml(content);
                        //Console.WriteLine(content);

                        XmlElement root = xmlDoc.DocumentElement;
                        XmlNodeList customer = root.SelectNodes("*[@id='" + patient.id + "']");
                        //Parse xml database file and add values into InsuranceDB object
                        if (customer.Count > 0)
                        {
                            foreach (XmlNode element in customer)
                            {
                                XmlAttribute policy = (XmlAttribute)element.SelectSingleNode("policy/@policyNumber");
                                XmlNode provider = policy.SelectSingleNode("provider");

                                insurance.id = patient.id;
                                insurance.policyNumber = policy.InnerText;
                                insurance.provider = policy.OwnerElement.InnerText;
                            }
                        }
                        else
                        {
                            insurance.id = patient.id;
                            insurance.policyNumber = "none";
                            insurance.provider = "none";
                        }
                        //Serialize to JSON and queueupward
                        JsonSerializerOptions options = new JsonSerializerOptions();
                        options.WriteIndented = true;
                        string insuranceJson = JsonSerializer.Serialize(insurance, options);
                        String postTime = String.Format("{0}:\t", DateTimeOffset.Now);
                        WriteToLog(String.Format("Date:{0} Posted message:{1}", postTime, insuranceJson));
                        
                        //send insuranceJson to long queue
                        sendUpwardQueue upQueue = new sendUpwardQueue(insuranceJson, credentials);
                    }
                    catch (XmlException ex)
                    {
                        Console.WriteLine(ex);
                    }
                //}
            }
            else
            {
                Console.WriteLine("message from queue was empty");
            }
        }


        public void WriteToLog(String message) {
            using StreamWriter writer = new StreamWriter(logPath, append: true);
            writer.WriteLine(message);
        }

    }
}
