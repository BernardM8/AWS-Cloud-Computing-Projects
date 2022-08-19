using System;
using System.Collections.Generic;
using System.Text;
using Amazon;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.SQS;
using Amazon.SQS.Model;

namespace ServiceTestQueues
{
    class sendUpwardQueue
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

        public sendUpwardQueue(String message) 
        {
            //setup credentials, SQSClient, URL, and message to send
            AWSCredentials credentials = GetAWSCredentialsByName("default");
            AmazonSQSClient client = new AmazonSQSClient(credentials, RegionEndpoint.USEast1);
            string InputQueueUrl = "https://sqs.us-east-1.amazonaws.com/256444621120/UpwardQueue.fifo";

            try
            {   //create new send message request with URL and message
                SendMessageRequest request = new SendMessageRequest
                {
                    MessageGroupId = "Group2",
                    MessageDeduplicationId = "GroupIDMessage2",
                    QueueUrl = InputQueueUrl,
                    MessageBody = message
                };
                //send the message
                var sendMessage = client.SendMessageAsync(request);
                sendMessage.Wait();
                Console.WriteLine("Message sent");              

                /*
                //To create new queue 
                // var request = new CreateQueueRequest

                var request = new SetQueueAttributesRequest
                {
                    Attributes = new Dictionary<string, string>
                    {
                        { "ReceiveMessageWaitTimeSeconds", "20"}
                    },
                    QueueUrl = InputQueueUrl,
                    MessageBody = message
                };
                var response = client.SetQueueAttributesAsync(request);
                response.Wait();
                Console.WriteLine("Created a queue with URL : {0}", response.QueueUrl);
                */
            }
            catch (AmazonSQSException ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
