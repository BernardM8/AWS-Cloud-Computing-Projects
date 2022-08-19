using System;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace UploadData
{
    class Program
    {

        //constant to identify bucket name
        private const string bucketName = "s3-project3-input";


        //generate random tag for date, location, and type. 
        public static String[] generateRandomViolation()
        {
            String[] result = new String[3];
            result[0] = String.Format("{0}", DateTimeOffset.Now);
            Random rand = new Random();
            int num1 = rand.Next(1, 3);
            int num2 = rand.Next(1, 3);

            switch (num1)
            {
                case 1:
                    result[1] = "no stop";
                    break;
                case 2:
                    result[1] = "no full stop on right";
                    break;
                case 3:
                    result[1] = "no right on red";
                    break;
            }

            switch (num2)
            {
                case 1:
                    result[2] = "Main St and 116th AVE intersection. Bellevue";
                    break;
                case 2:
                    result[2] = "Harvard Street Anaheim. CA 92805";
                    break;
                case 3:
                    result[2] = "Water Drive San Diego. CA 92117";
                    break;
            }
            foreach (string element in result)
            {
                Console.WriteLine("Violation results = {0}", element);
            }
            return result;
        }

        static async Task Main(string[] args)
        {
            //Get credentials, authenticate and get object to interact with AWS S3
            AWSCredentials credentials = GetAWSCredentialsByName("default");

            using (AmazonS3Client S3Client = new AmazonS3Client(credentials, RegionEndpoint.USEast1))
            {
                try
                {
                    Console.Write("Upload from Camera, enter directory: ");
                    //Input from user 
                    String filePath = Console.ReadLine();

                    //To retreive file type split string by periods and retrieve last array                  
                    String[] splitPeriodStrings = filePath.Split(@".");
                    string fileType = splitPeriodStrings[splitPeriodStrings.Length - 1];

                    //To retreive file name split string by slashes and retrieve last array                  
                    String[] splitSlashStrings = filePath.Split(@"\");
                    string fileName = splitSlashStrings[splitSlashStrings.Length - 1];

                    //section to call put object request if file type is jpg
                    if (fileType == "jpg")
                    {                       
                        String[] violationData = generateRandomViolation();

                        var requestXml = new PutObjectRequest
                        {
                            BucketName = bucketName,
                            Key = fileName,
                            ContentType = "application/" + fileType,
                            FilePath = filePath,
                            TagSet = new List<Tag>
                            {
                                new Tag {Key = "DateTime", Value = violationData[0]},
                                new Tag {Key = "Type", Value = violationData[1]},
                                new Tag {Key = "Location", Value = violationData[2]}
                            }
                        };
                        Console.WriteLine("Loading... ");
                        await S3Client.PutObjectAsync(requestXml);
                        Console.WriteLine("Loading complete to PlateReaderFunction . ");
                    }
                    else //section to display errors, file type is not xml or json 
                    {
                        Console.WriteLine("File type or input format is not correct");
                        Console.WriteLine("Please upload only jpg file types");
                        Console.WriteLine("use input format: C:\\path\\filename.type ");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Caught exception when uploading file to bucket:");
                    Console.WriteLine(e.Message);
                }
                S3Client.Dispose();
            }
        }


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
    }
}

