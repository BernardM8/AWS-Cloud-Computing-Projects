using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;


// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace TicketProcessingFunction
{
    public class Function
    {
        /// <summary>
        /// Default constructor. This constructor is used by Lambda to construct the instance. When invoked in a Lambda environment
        /// the AWS credentials will come from the IAM role associated with the function and the AWS region will be set to the
        /// region the Lambda function is executed in.
        /// </summary>
        public Function()
        {

        }


        /// <summary>
        /// This method is called for every Lambda invocation. This method takes in an SQS event object and can be used 
        /// to respond to SQS messages.
        /// </summary>
        /// <param name="evnt"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task FunctionHandler(SQSEvent evnt, ILambdaContext context)
        {
            foreach(var message in evnt.Records)
            {
                await ProcessMessageAsync(message, context);
            }
        }

        private async Task ProcessMessageAsync(SQSEvent.SQSMessage message, ILambdaContext context)
        {
            context.Logger.LogLine($"Processed message {message.Body}");

            //deserialize.json message
            OwnerVehicleTicket violator = JsonSerializer.Deserialize<OwnerVehicleTicket>(message.Body);

            //Translate to preffered language
            String englishMessage =violator.getMessage();
            Translator translate = new Translator();
            if (violator.language != "english")
            {
                String translatedMessage = translate.translateString(englishMessage, violator.language);
                violator.translatedMessage = translatedMessage;
            }

            //combine and generate ticket
            String ticket = violator.generateTicket();
            Console.WriteLine("Ticket: {0}", ticket);

            //send ticket to email per SNS
            SendSNS sns = new SendSNS();
            sns.sendNotification(ticket);
            // TODO: Do interesting work based on the new message
            await Task.CompletedTask;
        }
    }
}
