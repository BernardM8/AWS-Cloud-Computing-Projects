using System;
using System.Collections.Generic;
using System.Text;
using Amazon;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;

namespace TicketProcessingFunction
{
    class SendSNS
    {
        public void sendNotification(String message) {            
            var snsClient = new AmazonSimpleNotificationServiceClient(RegionEndpoint.USEast1);          
            var request = new PublishRequest()
            {
                Subject = "Ticket from camera",
                Message = message,
                TopicArn = "arn:aws:sns:us-east-1:256444621120:SendTicketMessage"
            };
            Console.WriteLine("Message in progress");
            var response = snsClient.PublishAsync(request);
            response.Wait();
            Console.WriteLine("Message sent!");           
        }
    
    }
    
}
