using System;
using System.Collections.Generic;
using System.Text;

namespace ProcessS3Event
{
    class VacSiteClass
    {
        private string date;
        private string siteID;
        private string siteName;
        private string zipCode;
        private int firstShot = 0;
        private int secondShot = 0;

        public void setDate(string dateMonth, string dateDay, string dateYear)
        {
            date = dateMonth + "/" + dateDay + "/" + dateYear; ;
        }

        public void setSiteId(string id)
        {
            siteID = id;
        }

        public void setSiteName(string name)
        {
            siteName = name;
        }

        public void setZipCode(string zipcode)
        {
            zipCode = zipcode;
        }

        public void setFirstShot(int shot)
        {
            firstShot += shot;
        }

        public void setSecondShot(int shot)
        {
            secondShot += shot;
        }

        public string getInsertTableQuery() 
        {
            return "create table if not exists Sites(SiteID int not null primary key, "
                    + "SiteName varchar(45), Zipcode varchar(45)); create table if not exists Data(SiteID int not null, Date date "
                    + "not null, FirstShot int, SecondShot int, foreign key(SiteID) references Sites(SiteID)); ";                   
        }


        public string getSiteQuery()
        {
            
            return "UPDATE Sites SET siteName = '" + siteName + " ', Zipcode = '" + zipCode + "' WHERE SiteID = " + siteID + ";"
            + "INSERT INTO Sites(SiteID, SiteName, Zipcode)"
            + "SELECT " + siteID + ", '" + siteName + "', '" + zipCode + "'"
            + "WHERE NOT EXISTS(SELECT 1 FROM sites WHERE SiteID = " + siteID + ");";

        }

        public string getDataQuery()
        {
            
            return "UPDATE Data SET FirstShot = "+ firstShot + ", SecondShot = "+ secondShot + " WHERE SiteID = "+ siteID + " and date = '"+ date + "';"
            +"INSERT INTO Data(SiteID, Date, FirstShot, SecondShot)"
            +"SELECT "+ siteID + ", '"+ date + "', "+ firstShot + ", "+ secondShot 
            +"WHERE NOT EXISTS(SELECT 2 FROM Data WHERE SiteID = " + siteID + " and date = '" + date + "');";

        }
    }
}
