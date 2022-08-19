using System;
using Amazon;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.SQS;
using Amazon.SQS.Model;
using System.Xml;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace testSendingQueues
{
    class InsuranceDB
    {
        public string id { get; set; }
        public string policyNumber { get; set; }
        public string provider { get; set; }
    }

    class PatientInput
    {
        public string id { get; set; }
        public string name { get; set; }
    }

    class Program
    {
        
        static void Main(string[] args)
        {
                   
            try
            {
                string jsonString = System.IO.File.ReadAllText(@"C:\Users\Bernard M\Desktop\Project2\DIYtestfiles\testup2.json");
                PatientInput patient = JsonSerializer.Deserialize<PatientInput>(jsonString);
                Console.WriteLine(jsonString);

                //send insuranceJson to long queue
                SendQueue upQueue = new SendQueue(jsonString);

            }
            catch (XmlException ex)
            {
                Console.WriteLine(ex);
            }
        }
    }

}




