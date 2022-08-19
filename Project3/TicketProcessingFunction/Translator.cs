using System;
using Amazon;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.Translate;
using Amazon.Translate.Model;

namespace TicketProcessingFunction
{
    class Translator
    {
        public String translateString(String input, String language) {
            String code= convertToCode(language);

            AmazonTranslateClient client = new AmazonTranslateClient( RegionEndpoint.USEast1);
           
            TranslateTextRequest request = new TranslateTextRequest();
            request.SourceLanguageCode = "en";
            request.TargetLanguageCode = code;
            request.Text = input;
            var response = client.TranslateTextAsync(request);
            response.Wait();
            Console.WriteLine("Translated text: {0}", response.Result.TranslatedText);
            return response.Result.TranslatedText;
        }

        public String convertToCode(String language)
        {
            switch (language) {
                case "spanish":
                    return "es";
                case "russian":
                    return "ru";
                case "french":
                    return "fr";
            }
            return "en";
        } 
    }
}
