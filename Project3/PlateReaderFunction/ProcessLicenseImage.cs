using System;
using System.IO;
using Amazon;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.Rekognition;
using Amazon.Rekognition.Model;
using System.Text;
using System.Text.RegularExpressions;

namespace PlateReaderFunction
{
    class ProcessLicenseImage
    {

        private Image GetImage(Stream s)
        {
            Image image = new Image();
            MemoryStream ms = new MemoryStream();
            s.CopyTo(ms);
            image.Bytes = ms;
            return image;
        }

        public String[] processImage(IAmazonRekognition client, Image image1)
        {

            try
            {               
                /*using (FileStream fs = new FileStream(photo, FileMode.Open, FileAccess.Read))
                {
                    byte[] data = new byte[fs.Length];
                    Console.WriteLine("before fs.Read");
                    fs.Read(data, 0, (int)fs.Length);
                    image1.Bytes = new MemoryStream(data);
                    Console.WriteLine( "before fs.close");
                    fs.Close();
                }*/
                                       
                DetectTextRequest request = new DetectTextRequest();
                request.Image = image1;
                var response = client.DetectTextAsync(request);
                response.Wait();

                String[] resultString = new String[response.Result.TextDetections.Count];
                int count = 0;
                //response.Result.TextDetections.Select(x=>x.DetectedText).ToList();
                foreach (var detection in response.Result.TextDetections)
                {
                    resultString[count] = detection.DetectedText;
                    count++;
                    //Console.WriteLine("resultString={0}, count = {1}", resultString, count);
                }
                Console.WriteLine(resultString + "\n");
                return resultString;
            }
            catch (Exception)
            {
                Console.WriteLine("Failed to load image file");
                return null;
            }
        }

        //To identify if number is a plate number
        public bool IsCapitalLettersAndNumbers(string s)
        {           
            if (s.Length > 7 || s.Length < 6 || String.IsNullOrEmpty(s))
            {
                return false;
            }
            bool allDigits = true;
            foreach (char c in s)
            {
                if (!char.IsDigit(c))
                {
                    allDigits = false;
                    break;
                }
            }
            if (allDigits) return false;
            return System.Text.RegularExpressions.Regex.IsMatch(s, "^[A-Z0-9]*$");
        }

        //Check if plate is from california
        public Boolean isPlateCalifornia(String[] plateString)
        {
            String pattern = "((\"California\")|(\\\"California)|(dmv.ca+)|(California+))";
            Regex rg = new Regex(pattern);
            foreach (var element in plateString)
            {
                MatchCollection match = rg.Matches(element);
                if (match.Count > 0)
                {
                    return true;
                }
            }
            return false;
        }

    }
}
