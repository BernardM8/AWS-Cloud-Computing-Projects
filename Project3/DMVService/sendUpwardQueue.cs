using System;
using System.Collections.Generic;
using System.Text;
using Amazon;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.SQS;
using Amazon.SQS.Model;

namespace DMVService
{
    class sendUpwardQueue
    {
        public sendUpwardQueue(string message, CredentialProfileOptions options)
        {
            //setup credentials, SQSClient, URL, and message to send
            AWSCredentials credentials = AWSCredentialsFactory.GetAWSCredentials(options, null);
            AmazonSQSClient client = new AmazonSQSClient(credentials, RegionEndpoint.USEast1);
            string InputQueueUrl = "https://sqs.us-east-1.amazonaws.com/256444621120/P3upwardqueue";

            try
            {   //create new send message request with URL and message
                SendMessageRequest request = new SendMessageRequest
                {
                    QueueUrl = InputQueueUrl,
                    MessageBody = message
                };

                //send the message
                var sendMessage = client.SendMessageAsync(request);
                sendMessage.Wait();
                Console.WriteLine("Message sent");  
            }
            catch (AmazonSQSException ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}