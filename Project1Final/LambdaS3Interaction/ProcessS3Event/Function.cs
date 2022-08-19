using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Text.Json;
using System.Text.Json.Serialization;

using System.Data;
using Amazon.Lambda.Core;
using Amazon.Lambda.S3Events;
using Amazon.S3;
using Amazon.S3.Util;
using Amazon.S3.Model;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace ProcessS3Event
{

    public class date : Vaccinations
    {
        public int month { get; set; }
        public int day { get; set; }
        public int year { get; set; }
    }

    public class site : Vaccinations
    {
        public string id { get; set; }
        public string name { get; set; }
        public string zipCode { get; set; }
    }

    public class vaccines : Vaccinations
    {
        public string brand { get; set; }
        public int firstShot { get; set; }
        public int secondShot { get; set; }
    }

    public class Vaccinations
    {
        public date date { get; set; }
        public site site { get; set; }
        public List<vaccines> vaccines { get; set; }
    }


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
            if (s3Event == null)
            {
                return null;
            }
            VacSiteClass vacSite = new VacSiteClass();
            try
            {              
                Console.WriteLine("Bucket: {0}", s3Event.Bucket.Name);
                Console.WriteLine("File: {0}", s3Event.Object.Key);
                string bucketName = s3Event.Bucket.Name;
                string objectKey = s3Event.Object.Key;

                GetObjectTaggingRequest getTagsRequest = new GetObjectTaggingRequest
                {
                    BucketName = bucketName,
                    Key = objectKey
                };
                GetObjectTaggingResponse objectTags = await S3Client.GetObjectTaggingAsync(getTagsRequest);
                
                string fileType = objectTags.Tagging[0].Value;
                Console.WriteLine("Tag: {0}", fileType);            

                Stream stream = await S3Client.GetObjectStreamAsync(bucketName, objectKey, null);
                using (StreamReader reader = new StreamReader(stream))
                {
                    string content = reader.ReadToEnd();                   
                    //----------------Section for json files-------------------
                    if (fileType == "json") 
                    {
                        Vaccinations vaccines = JsonSerializer.Deserialize<Vaccinations>(content);
                        vacSite.setDate(vaccines.date.month.ToString(), vaccines.date.day.ToString(), vaccines.date.year.ToString());
                        vacSite.setSiteId(vaccines.site.id);
                        vacSite.setSiteName(vaccines.site.name);
                        vacSite.setZipCode(vaccines.site.zipCode);
                        foreach (var iter in vaccines.vaccines)
                        {
                            vacSite.setFirstShot(iter.firstShot);
                            vacSite.setSecondShot(iter.secondShot);
                        }
                        //query into Database
                        Database db1 = new Database();
                        db1.uploadToDataBase(vacSite);
                        Console.WriteLine(vacSite.getSiteQuery());
                        Console.WriteLine(vacSite.getDataQuery());
                        
                    }
                    //----------------Section for xml files-------------------
                    else if (fileType == "xml")
                    {
                        try
                        {
                            XmlDocument xmlDoc = new XmlDocument();
                            xmlDoc.LoadXml(content);
                            Console.WriteLine(content);
                            XmlElement root = xmlDoc.DocumentElement;
                            //Parse xml file and add values into vacSite object
                            if (root != null)
                            {
                                vacSite.setDate(root.Attributes["month"]?.InnerText, root.Attributes["day"]?.InnerText, root.Attributes["year"]?.InnerText);
                                XmlNodeList siteNode = root.SelectNodes("site");
                                foreach (XmlNode element in siteNode)
                                {
                                    XmlNode name = element.SelectSingleNode("name");
                                    XmlNode zip = element.SelectSingleNode("zipCode");

                                    vacSite.setSiteId(element.Attributes["id"]?.InnerText);
                                    vacSite.setSiteName(name.InnerText);
                                    vacSite.setZipCode(zip.InnerText);
                                }

                                XmlNodeList vacNode = root.SelectNodes("vaccines");
                                foreach (XmlNode element in vacNode)
                                {
                                    XmlNodeList brand = element.SelectNodes("brand");
                                    foreach (XmlNode shot in brand)
                                    {
                                        XmlNode total = shot.SelectSingleNode("total");
                                        XmlNode firstShot = shot.SelectSingleNode("firstShot");
                                        XmlNode secondShot = shot.SelectSingleNode("secondtShot");
                                        
                                        vacSite.setFirstShot(Int16.Parse(firstShot.InnerText));
                                        vacSite.setSecondShot(Int16.Parse(secondShot.InnerText));
                                    }
                                }
                                //query into Database
                                Console.WriteLine(vacSite.getSiteQuery());
                                Console.WriteLine(vacSite.getDataQuery());
                                Database db2 = new Database();
                                db2.uploadToDataBase(vacSite);
                            }
                            else
                            {
                                Console.WriteLine("Root of xml file seems to be empty");
                            }
                        }
                        catch (XmlException ex)
                        {
                            Console.WriteLine(ex);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Error Tag does not contain correct file type");
                    }
                    reader.Close();
                }
                return "OK";
            }
            catch (Exception e)
            {
                context.Logger.LogLine($"Error getting object {s3Event.Object.Key} from bucket {s3Event.Bucket.Name}. Make sure they exist and your bucket is in the same region as this function.");
                context.Logger.LogLine(e.Message);
                context.Logger.LogLine(e.StackTrace);
                throw;
            }
        }
    }
}
