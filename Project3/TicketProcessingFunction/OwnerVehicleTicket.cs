using System;
using System.Collections.Generic;
using System.Text;

namespace TicketProcessingFunction
{
    class OwnerVehicleTicket
    {
        public String plate { get; set; }
        public String date { get; set; }
        public String violation { get; set; }
        public String address { get; set; }
        public String vehicle { get; set; }
        public String language { get; set; }
        public String name { get; set; }
        public String phone { get; set; }
        public String ticketAmount { get; set; }
        public String translatedMessage { get; set; }


        private String englishMessage = "Your vehicle was involved in a traffic violation. Please pay the specified ticket amount by 30 days:";
        public String getMessage() { return englishMessage; }


        public void setTicketAmount()
        {
            if (violation == "no stop") { ticketAmount = "$300"; }
            else if (violation == "no full stop on right") { ticketAmount = "$75"; }
            else if (violation == "no right on red") { ticketAmount = "$125"; }
        }


        public string generateTicket( ) {
            setTicketAmount();
            String introMessage;
            if (language != "english") { introMessage = translatedMessage; }
            else { introMessage = getMessage(); }
           
            String ticket = introMessage + "\n"+
                "Vehicle: " + vehicle + "\n" +
                "License plate: " + plate + "\n" +
                "Date: " + date + "\n" +
                "Violation address: " + address + "\n" +
                "Violation type: " + violation + "\n" +
                "Ticket amount: " + ticketAmount;
       
            return ticket;
        }
    }
}
