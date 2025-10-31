using System;
using System.Xml.Schema;
using System.Xml;
using Newtonsoft.Json;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Net;



/**
 * This template file is created for ASU CSE445 Distributed SW Dev Assignment 4.
 * Please do not modify or delete any existing class/variable/method names. However, you can add more variables and functions.
 * Uploading this file directly will not pass the autograder's compilation check, resulting in a grade of 0.
 * **/


namespace ConsoleApp1
{


    public class Program
    {
        // Replace these URLs with your actual deployed file URLs on GitHub or other server
        public static string xmlURL = "https://raw.githubusercontent.com/kokspro/cse445_assignment3/refs/heads/main/Hotels.xml";
        public static string xmlErrorURL = "https://raw.githubusercontent.com/kokspro/cse445_assignment3/refs/heads/main/HotelsErrors.xml";
        public static string xsdURL = "https://raw.githubusercontent.com/kokspro/cse445_assignment3/refs/heads/main/Hotels.xsd";

        public static void Main(string[] args)
        {
            string result = Verification(xmlURL, xsdURL);
            Console.WriteLine(result);


            result = Verification(xmlErrorURL, xsdURL);
            Console.WriteLine(result);


            result = Xml2Json(xmlURL);
            Console.WriteLine(result);
        }

        // Q2.1 - Verification method
        public static string Verification(string xmlUrl, string xsdUrl)
        {
            try
            {
                // Create XmlReaderSettings with validation
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.ValidationType = ValidationType.Schema;
                
                // List to collect validation errors
                List<string> validationErrors = new List<string>();
                
                // Add validation event handler
                settings.ValidationEventHandler += (sender, e) => 
                {
                    validationErrors.Add(e.Message);
                };
                
                // Load XSD from URL
                using (XmlReader xsdReader = XmlReader.Create(xsdUrl))
                {
                    XmlSchema schema = XmlSchema.Read(xsdReader, (sender, e) => 
                    {
                        if (e.Severity == XmlSeverityType.Error)
                            validationErrors.Add("Schema Error: " + e.Message);
                    });
                    settings.Schemas.Add(schema);
                }
                
                // Validate XML against XSD
                using (XmlReader xmlReader = XmlReader.Create(xmlUrl, settings))
                {
                    try
                    {
                        while (xmlReader.Read())
                        {
                            // Process the document - validation happens automatically
                        }
                    }
                    catch (XmlException xmlEx)
                    {
                        validationErrors.Add(xmlEx.Message);
                    }
                }
                
                // Return results
                if (validationErrors.Count == 0)
                {
                    return "No Error";
                }
                else
                {
                    // Return all validation errors
                    return string.Join("; ", validationErrors);
                }
            }
            catch (WebException webEx)
            {
                return "Error accessing files: " + webEx.Message;
            }
            catch (Exception ex)
            {
                return "Validation error: " + ex.Message;
            }
        }

        // Q2.2 - XML to JSON conversion method
        public static string Xml2Json(string xmlUrl)
        {
            try
            {
                string jsonText = "";
                
                // Download and load the XML
                using (WebClient client = new WebClient())
                {
                    string xmlContent = client.DownloadString(xmlUrl);
                    XDocument doc = XDocument.Parse(xmlContent);
                    
                    // Build JSON structure manually to ensure correct format
                    var hotels = new
                    {
                        Hotels = new
                        {
                            Hotel = new List<object>()
                        }
                    };
                    
                    // Process each Hotel element
                    foreach (var hotelElement in doc.Root.Elements("Hotel"))
                    {
                        // Create hotel object
                        var hotel = new Dictionary<string, object>();
                        
                        // Add Name
                        var nameElement = hotelElement.Element("Name");
                        if (nameElement != null)
                        {
                            hotel["Name"] = nameElement.Value;
                        }
                        
                        // Add Phone numbers (can be multiple)
                        var phoneElements = hotelElement.Elements("Phone");
                        if (phoneElements != null)
                        {
                            var phones = new List<string>();
                            foreach (var phone in phoneElements)
                            {
                                phones.Add(phone.Value);
                            }
                            if (phones.Count == 1)
                            {
                                hotel["Phone"] = phones[0];
                            }
                            else if (phones.Count > 1)
                            {
                                hotel["Phone"] = phones;
                            }
                        }
                        
                        // Add Address
                        var addressElement = hotelElement.Element("Address");
                        if (addressElement != null)
                        {
                            var address = new Dictionary<string, string>();
                            
                            // Add address sub-elements
                            if (addressElement.Element("Number") != null)
                                address["Number"] = addressElement.Element("Number").Value;
                            if (addressElement.Element("Street") != null)
                                address["Street"] = addressElement.Element("Street").Value;
                            if (addressElement.Element("City") != null)
                                address["City"] = addressElement.Element("City").Value;
                            if (addressElement.Element("State") != null)
                                address["State"] = addressElement.Element("State").Value;
                            if (addressElement.Element("Zip") != null)
                                address["Zip"] = addressElement.Element("Zip").Value;
                            
                            // Add NearestAirport attribute with underscore prefix
                            var nearestAirportAttr = addressElement.Attribute("NearestAirport");
                            if (nearestAirportAttr != null)
                            {
                                address["_NearestAirport"] = nearestAirportAttr.Value;
                            }
                            
                            hotel["Address"] = address;
                        }
                        
                        // Add Rating attribute with underscore prefix (if exists)
                        var ratingAttr = hotelElement.Attribute("Rating");
                        if (ratingAttr != null)
                        {
                            hotel["_Rating"] = ratingAttr.Value;
                        }
                        
                        hotels.Hotels.Hotel.Add(hotel);
                    }
                    
                    // Convert to JSON
                    jsonText = JsonConvert.SerializeObject(hotels, Formatting.Indented);
                }
                
                // The returned jsonText needs to be de-serializable by Newtonsoft.Json package
                return jsonText;
            }
            catch (Exception ex)
            {
                return "Error converting to JSON: " + ex.Message;
            }
        }
    }

}
