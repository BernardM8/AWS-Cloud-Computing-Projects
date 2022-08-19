using System;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace UploadDataFile
{
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

        //constant to identify bucket name
        private const string bucketName = "project2s3bucket";

        static void Main(string[] args)
        {
            //Get credentials, authenticate and get object to interact with AWS S3
            AWSCredentials credentials = GetAWSCredentialsByName("default");
            using (AmazonS3Client S3Client = new AmazonS3Client(credentials, RegionEndpoint.USEast1))
            {
                try
                {
                    Console.Write("UploadData.exe ");
                    //Input from user 
                    String userInput = Console.ReadLine();

                    //To retreive file type split string by spaces and retrieve last array                  
                    String[] splitSpaceStrings = userInput.Split(@" ");
                    string fileType = splitSpaceStrings[splitSpaceStrings.Length - 1];

                    //To retreive file name split first space split string by "\" and retrieve last array                   
                    String remainingString1 = "";
                    for (int i = 0; i < splitSpaceStrings.Length - 1; i++)
                    {
                        remainingString1 += splitSpaceStrings[i];
                    }
                    String[] splitStrDirectory = remainingString1.Split(@"\");
                    string fileName = splitStrDirectory[splitStrDirectory.Length - 1];

                    //To retreive file path split userInput and rebuild string without last array 
                    String[] splitStrDirectory2 = userInput.Split(@"\");
                    String filePath = "";
                    for (int i = 0; i < splitStrDirectory2.Length - 1; i++)
                    {   //rebuild directory without last array
                        if (i == splitStrDirectory2.Length - 2)
                        {
                            filePath += splitStrDirectory2[i];
                        }
                        else
                        {
                            filePath += splitStrDirectory2[i] + "\\";
                        }
                    }
                    //section to call put object request if file type is xml
                    if (fileType == "xml")
                    {
                        String pathOfFile = filePath + "\\" + fileName;
                        var requestXml = new PutObjectRequest
                        {
                            BucketName = bucketName,
                            Key = fileName,
                            ContentType = "application/" + fileType,
                            FilePath = pathOfFile                            
                        };

                        Console.WriteLine("Loading... ");                       
                        var request = S3Client.PutObjectAsync(requestXml);
                        request.Wait();
                        Console.WriteLine("Loading Complete. ");
                    }
                    else //section to display errors, file type is not xml or json 
                    {
                        Console.WriteLine("File type or input format is not correct");
                        Console.WriteLine("Please upload only xml files");
                        Console.WriteLine("use input format: C:\\path\\filename.type type ");
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
        

    }
}
