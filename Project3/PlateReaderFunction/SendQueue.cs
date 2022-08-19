using System;
using System.Collections.Generic;
using System.Text;
using Amazon;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.SQS;
using Amazon.SQS.Model;

namespace PlateReaderFunction
{
    class SendQueue
    {
        public SendQueue(string message)
        {
            //setup credentials, SQSClient, URL, and message to send           
            AmazonSQSClient client = new AmazonSQSClient(RegionEndpoint.USEast1);
            string InputQueueUrl = "https://sqs.us-east-1.amazonaws.com/256444621120/P3downwardqueue";

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
