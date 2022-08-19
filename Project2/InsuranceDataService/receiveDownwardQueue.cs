using System;
using System.Collections.Generic;
using System.Text;
using Amazon;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.SQS;
using Amazon.SQS.Model;

namespace InsuranceDataService
{
    class receiveDownwardQueue
    {          

        public String receiveQueue(CredentialProfileOptions options)
            {
            //setup credentials, SQSClient, URL, and message to send           
            AWSCredentials credentials = AWSCredentialsFactory.GetAWSCredentials(options, null);
            AmazonSQSClient client = new AmazonSQSClient(credentials, RegionEndpoint.USEast1);
            string InputQueueUrl = "https://sqs.us-east-1.amazonaws.com/256444621120/downwardqueue";
            String resultMessage = null;
            try
            {   
                //Receiving a message           
                var request = new ReceiveMessageRequest
                {                   
                    //AttributeNames = { "SentTimestamp" },
                    MaxNumberOfMessages = 1,
                    //MessageAttributeNames = { "All" },
                    QueueUrl = InputQueueUrl,
                    WaitTimeSeconds = 20                    
                };
               
                var response = client.ReceiveMessageAsync(request);
                response.Wait();
                
                if (response.Result.Messages.Count != 0)
                {                                     
                    //String[] resultMessage = new string[response.Result.Messages.Count];
                    /*int count = 0;
                    foreach(var element in response.Result.Messages)
                    {
                        resultMessage[count] = element.Body;
                        Console.WriteLine(resultMessage[count]);
                        count++;
                    }*/

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
                    //String[] resultMessage = new string[1];
                    //resultMessage[0] = null;
                    return resultMessage;
                }
            }
            catch (AmazonSQSException ex)
            {
                Console.WriteLine(ex);
                //String[] resultMessage = new string[1];
                //resultMessage[0] = null;
                return resultMessage;
            }
        }      
    }
}




