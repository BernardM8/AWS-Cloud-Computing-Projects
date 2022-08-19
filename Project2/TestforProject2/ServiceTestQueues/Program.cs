
using System;
using Amazon;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.SQS;
using Amazon.SQS.Model;
using System.Xml;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ServiceTestQueues
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
        //method to verify and get AWS Credentials
        private static AWSCredentials GetAWSCredentialsByName(String profileName)
        {
            if (String.IsNullOrEmpty(profileName))
            {
                throw new ArgumentNullException("profileNames cannot be bull or empty");
            }

            SharedCredentialsFile credFile = new SharedCredentialsFile();
            CredentialProfile profile = credFile.ListProfiles().Find(p => p.Name.Equals(profileName));
            if (profile == null)
            {
                throw new Exception(String.Format("Profile named {0} not found", profileName));
            }
            return AWSCredentialsFactory.GetAWSCredentials(profile, new SharedCredentialsFile());
        }



        static void Main(string[] args)
        {
            //setup credentials, SQSClient, URL, and message to send
            AWSCredentials credentials = GetAWSCredentialsByName("default");
            AmazonSQSClient client = new AmazonSQSClient(credentials, RegionEndpoint.USEast1);
            string InputQueueUrl = "https://sqs.us-east-1.amazonaws.com/256444621120/downwardqueue.fifo";

            try
            {               
                    string jsonString = System.IO.File.ReadAllText("test3.json");
                    PatientInput patient = JsonSerializer.Deserialize<PatientInput>(jsonString);
                    Console.WriteLine(jsonString);

                    string content = System.IO.File.ReadAllText("InsuranceDatabase.xml");
                    InsuranceDB insurance = new InsuranceDB();
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(content);
                    Console.WriteLine(content);

                    XmlElement root = xmlDoc.DocumentElement;
                    XmlNodeList customer = root.SelectNodes("*[@id='" + patient.id + "']");
                    //Parse xml database file and add values into InsuranceDB object
                    if (customer.Count > 0)
                    {
                        foreach (XmlNode element in customer)
                        {
                            //XmlNode name = element.SelectSingleNode("patient id");
                            //string policy = element.Attributes["policyNumber"].Value;
                        
                            XmlAttribute policy = (XmlAttribute)element.SelectSingleNode("policy/@policyNumber");
                            XmlNode provider = policy.SelectSingleNode("provider");

                            insurance.id = patient.id;
                            insurance.policyNumber = policy.InnerText;                           
                            //insurance.provider = provider.InnerText;
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
                    Console.WriteLine(insuranceJson);
                    
                    //send insuranceJson to long queue
                    sendUpwardQueue upQueue = new sendUpwardQueue(insuranceJson);              
            }
            catch (XmlException ex)
            {
                Console.WriteLine(ex);
            }
        }
    }           

}
               
            

