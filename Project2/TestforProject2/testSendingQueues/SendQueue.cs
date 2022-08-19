using System;
using System.Collections.Generic;
using System.Text;
using Amazon;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.SQS;
using Amazon.SQS.Model;

namespace testSendingQueues
{
    class SendQueue
    {
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


        public SendQueue (String message)
        {
            //setup credentials, SQSClient, URL, and message to send
            AWSCredentials credentials = GetAWSCredentialsByName("default");
            //AmazonSQSClient client = new AmazonSQSClient(credentials, RegionEndpoint.USEast1);
            string InputQueueUrl = "https://sqs.us-east-1.amazonaws.com/256444621120/upwardqueue.fifo";

            try
            {
                /*
                //create new send message request with URL and message
                SendMessageRequest request = new SendMessageRequest
                {
                    MessageGroupId = "Group2",
                    MessageDeduplicationId = "GroupIDMessage2",
                    QueueUrl = InputQueueUrl,
                    MessageBody = message
                };
                //send the message
                //var sendMessage = client.SendMessageAsync(request);
                SendMessageResponse sendMessage = client.SendMessageAsync(request);
                sendMessage.Wait();
                
                //SendMessageResponse response =sendMessage.SendMessageResponse();
                */

                //var amazonSQSConfig = new AmazonSQSConfig();
                //amazonSQSConfig.ServiceURL = "https://sqs.eu-west-1.amazonaws.com";

                var amazonSQSClient = new AmazonSQSClient(credentials, RegionEndpoint.USEast1);
                var sendRequest = new SendMessageRequest()
                {
                    MessageGroupId = "Group2",
                    MessageDeduplicationId = "GroupIDMessage2",
                    QueueUrl = InputQueueUrl,
                    MessageBody = message
                };
                //sendRequest.QueueUrl = InputQueueUrl;
                //sendRequest.MessageBody = message;

                var sendMessageResponse = amazonSQSClient.SendMessageAsync(sendRequest);
                //sendMessageResponse.Status
                String resultMessage = sendMessageResponse.Result.MD5OfMessageBody;


                /*
                var credentials = new Amazon.Runtime.BasicAWSCredentials(accessKey, secretKey);
                AmazonSQSClient amazonSQSClient = new AmazonSQSClient(credentials, Amazon.RegionEndpoint.USEast2);
                SendMessageRequest sendMessageRequest = new SendMessageRequest();
                sendMessageRequest.QueueUrl = InputQueueUrl;
                sendMessageRequest.MessageBody = "{YOUR_QUEUE_MESSAGE}";
                SendMessageResponse sendMessageResponse =await client.SendMessage(sendMessageRequest);
                */
                Console.WriteLine("Message sent");


            }
            catch (AmazonSQSException ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
