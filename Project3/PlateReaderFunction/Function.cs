using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Amazon.Lambda.Core;
using Amazon.Lambda.S3Events;
using Amazon.S3;
using Amazon.S3.Util;
using System.Data;
using Amazon.S3.Model;
using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon.Rekognition;
using Amazon.Rekognition.Model;


// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace PlateReaderFunction
{
    public class Function
    {
        private const string manualProcessBucket = "p3-manual-processing-bucket";

        IAmazonS3 S3Client { get; set; }

        IAmazonRekognition RekognitionClient { get; set; }

        /// <summary>
        /// Default constructor. This constructor is used by Lambda to construct the instance. When invoked in a Lambda environment
        /// the AWS credentials will come from the IAM role associated with the function and the AWS region will be set to the
        /// region the Lambda function is executed in.
        /// </summary>
        public Function()
        {
            S3Client = new AmazonS3Client();
            RekognitionClient = new AmazonRekognitionClient();
        }

        /// <summary>
        /// Constructs an instance with a preconfigured S3 client. This can be used for testing the outside of the Lambda environment.
        /// </summary>
        /// <param name="s3Client"></param>
        /// <param name="rekognitionClient"></param>
        public Function(IAmazonS3 s3Client, IAmazonRekognition rekognitionClient)
        {
            this.S3Client = s3Client;
            this.RekognitionClient = rekognitionClient;
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

                Image image = new Image
                {
                    S3Object = new Amazon.Rekognition.Model.S3Object
                    {
                        Bucket = s3Event.Bucket.Name,
                        Name = s3Event.Object.Key
                    }
                };

                ProcessLicenseImage process = new ProcessLicenseImage();
                String[] plateTexts = process.processImage(this.RekognitionClient, image);
                Console.WriteLine("plate texts: {0}", plateTexts);

                //Get tags
                GetObjectTaggingRequest getTagsRequest = new GetObjectTaggingRequest
                {
                    BucketName = bucketName,
                    Key = objectKey
                };
                GetObjectTaggingResponse objectTags = await S3Client.GetObjectTaggingAsync(getTagsRequest);
                string tag0 = objectTags.Tagging[0].Value;
                string tag1 = objectTags.Tagging[1].Value;
                string tag2 = objectTags.Tagging[2].Value;
                Console.WriteLine("Tag0: {0}", tag0);
                Console.WriteLine("Tag1: {0}", tag1);
                Console.WriteLine("Tag2: {0}", tag2);


                //Check for california
                Boolean isCalPlate = process.isPlateCalifornia(plateTexts);
                Console.WriteLine("Plate is from California = {0}", isCalPlate);

                String plateNumber = "";
                //if plate is california send to s3 else send to manual processing bucket
                if (isCalPlate == false)
                { //send to manual processing bucket
                    var requestXml = new PutObjectRequest
                    {
                        BucketName = manualProcessBucket,
                        Key = s3Event.Object.Key,
                        //ContentType = "application/" + fileType,
                        //FilePath = filePath,
                        TagSet = new List<Tag>
                            {
                                new Tag {Key = "DateTime", Value = tag0},
                                new Tag {Key = "Type", Value = tag1},
                                new Tag {Key = "Location", Value = tag2}
                            }
                    };
                    Console.WriteLine("Loading... ");
                    await S3Client.PutObjectAsync(requestXml);
                    Console.WriteLine("Loading complete to manual processing bucket . ");
                }
                else
                {
                    //retreive license plate number
                    for (int i = 0; i < plateTexts.Length; i++)
                    {
                        if (process.IsCapitalLettersAndNumbers(plateTexts[i]))
                        {
                            plateNumber = plateTexts[i];
                        }
                    }
                    //Store data to PlateData object
                    PlateData data = new PlateData();
                    data.plate = plateNumber;
                    data.date = tag1;
                    data.violation = tag0;
                    data.address = tag2;

                    //Serialize patient class/data to json 
                    JsonSerializerOptions options = new JsonSerializerOptions();
                    options.WriteIndented = true;
                    string plateJson = JsonSerializer.Serialize(data, options);

                    //send json data to downwardQueue
                    SendQueue queue = new SendQueue(plateJson);
                    Console.WriteLine(plateJson);
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
