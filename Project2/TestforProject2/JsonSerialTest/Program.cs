using System;
using System.Text.Json;
using System.Text.Json.Serialization;
namespace JsonSerialTest
{
    class PatientInsuranceData
    {
        public string id { get; set; }
        public string policyNumber { get; set; }
        public string provider { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            PatientInsuranceData patientbuild = new PatientInsuranceData();
            patientbuild.id = "10001";
            patientbuild.policyNumber = "AFF-234-667788";
            patientbuild.provider = "Liberty Mutual";
            //Serialize to JSON
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.WriteIndented = true;

            string patientJson = JsonSerializer.Serialize(patientbuild, options);
            Console.WriteLine(patientJson);




            //Get the content from file test2uppward.json
            string jsonString = System.IO.File.ReadAllText("test2uppward.json");          

            //deserialize.json message
            PatientInsuranceData patient = JsonSerializer.Deserialize<PatientInsuranceData>(jsonString);
            //Print data
            Console.WriteLine("Patient with ID {0}: policyNumber={1}, provider={2}", patient.id, patient.policyNumber, patient.provider);
        }
    }
}
