using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Data;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.Lambda.Core;
using Amazon.Lambda.S3Events;
using Amazon.S3;
using Amazon.S3.Util;
using Amazon.S3.Model;
using Amazon.SQS;
using Amazon.SQS.Model;
using System.Text.Json;
using System.Text.Json.Serialization;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace PatientReaderFunction
{
    public class Function
    {
        IAmazonS3 S3Client { get; set; }

        /// <summary>
        /// Default constructor. This constructor is used by Lambda to construct the instance. When invoked in a Lambda environment
        /// the AWS credentials will come from the IAM role associated with the function and the AWS region will be set to the
        /// region the Lambda function is executed in.
        /// </summary>
        public Function()
        {
            S3Client = new AmazonS3Client();
        }

        /// <summary>
        /// Constructs an instance with a preconfigured S3 client. This can be used for testing the outside of the Lambda environment.
        /// </summary>
        /// <param name="s3Client"></param>
        public Function(IAmazonS3 s3Client)
        {
            this.S3Client = s3Client;
        }
        
        /// <summary>
        /// This method is called for every Lambda invocation. This method takes in an S3 event object and can be used 
        /// to respond to S3 notifications.
        /// </summary>
        /// <param name="evnt"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<string> FunctionHandler(S3Event evnt, ILambdaContext context)
        {
            var s3Event = evnt.Records?[0].S3;
            if(s3Event == null)
            {
                return null;
            }
            
            try
            {
                Console.WriteLine("Bucket: {0}", s3Event.Bucket.Name);
                Console.WriteLine("File: {0}", s3Event.Object.Key);
                string bucketName = s3Event.Bucket.Name;
                string objectKey = s3Event.Object.Key;               

                Stream stream = await S3Client.GetObjectStreamAsync(bucketName, objectKey, null);
                using (StreamReader reader = new StreamReader(stream))
                {
                    string content = reader.ReadToEnd();
                    Console.WriteLine(content);

                    //----------------Section to parse xml file-------------------
                    if (content != null)
                    {
                        try
                        {          
                            
                            XmlDocument xmlDoc = new XmlDocument();
                            xmlDoc.LoadXml(content);
                            XmlElement root = xmlDoc.DocumentElement;
                            Patient patient = new Patient();

                            if (root != null)
                            {
                                XmlNode id = root.SelectSingleNode("id");
                                XmlNode name = root.SelectSingleNode("name");

                                //add to patient class/data
                                patient.id = id.InnerText;
                                patient.name=name.InnerText;

                                Console.WriteLine("Patient id: {0}, Patient Name {1}", id.InnerText, name.InnerText);
                            }
                            //Serialize patient class/data to json 
                            JsonSerializerOptions options = new JsonSerializerOptions();
                            options.WriteIndented = true;
                            string insuranceJson = JsonSerializer.Serialize(patient, options);

                            //send json data to downwardQueue
                            SendQueue queue = new SendQueue(insuranceJson);
                                                                                   
                        }
                        catch (XmlException ex)
                        {
                            Console.WriteLine(ex);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Error file type incorrect or empty");
                    }
                    reader.Close();
                }
                return "OK";
            }
            catch(Exception e)
            {
                context.Logger.LogLine($"Error getting object {s3Event.Object.Key} from bucket {s3Event.Bucket.Name}. Make sure they exist and your bucket is in the same region as this function.");
                context.Logger.LogLine(e.Message);
                context.Logger.LogLine(e.StackTrace);
                throw;
            }
        }
    }
}
