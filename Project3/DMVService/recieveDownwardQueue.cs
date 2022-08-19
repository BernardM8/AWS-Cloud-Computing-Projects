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
    class receiveDownwardQueue
    {
        public String receiveQueue(CredentialProfileOptions options)
        {
        //setup credentials, SQSClient, URL, and message to send           
        AWSCredentials credentials = AWSCredentialsFactory.GetAWSCredentials(options, null);
        AmazonSQSClient client = new AmazonSQSClient(credentials, RegionEndpoint.USEast1);
        string InputQueueUrl = "https://sqs.us-east-1.amazonaws.com/256444621120/P3downwardqueue";
        String resultMessage = null;
        try
        {
            //Receiving a message           
            var request = new ReceiveMessageRequest
            {              
                MaxNumberOfMessages = 1,              
                QueueUrl = InputQueueUrl,
                WaitTimeSeconds = 20
            };

            var response = client.ReceiveMessageAsync(request);
            response.Wait();

            if (response.Result.Messages.Count != 0)
            {
                resultMessage = response.Result.Messages[0].Body;
                Console.WriteLine("Message succesfully received");
                Console.WriteLine(resultMessage);

                //delete queue message after retreival
                var deleteMessageRequest = new DeleteMessageRequest();
                deleteMessageRequest.QueueUrl = InputQueueUrl;
                deleteMessageRequest.ReceiptHandle = response.Result.Messages[0].ReceiptHandle;
                var deleteResponse = client.DeleteMessageAsync(deleteMessageRequest);
                deleteResponse.Wait();

                return resultMessage;
            }
            else
            {
                Console.WriteLine(" No Messages in Queue");
                return resultMessage;
            }
        }
        catch (AmazonSQSException ex)
        {
            Console.WriteLine(ex);
            return resultMessage;
        }
    }
    }
}
