using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using comment_y_aller.Models;
using System.Net;
using Newtonsoft.Json;

namespace comment_y_aller.Controllers
{
    public class CommentYAllerController : Controller
    {
        public static List<Record> GetPoints(String type = "velib", bool depart = true)
        {
            List<Record> result = new List<Record>();

            WebClient client = new WebClient();

            String response;

            if (type == "velib")
            {

                if (depart)
                {
                    response = client.DownloadString("https://opendata.paris.fr/api/records/1.0/search/?dataset=stations-velib-disponibilites-en-temps-reel&facet=banking&facet=bonus&facet=status&facet=contract_name&facet=available_bikes&refine.status=OPEN&exclude.available_bikes=0&rows=-1");
                }
                else
                {
                    response = client.DownloadString("https://opendata.paris.fr/api/records/1.0/search/?dataset=stations-velib-disponibilites-en-temps-reel&facet=banking&facet=bonus&facet=status&facet=contract_name&facet=available_bikes&refine.status=OPEN&exclude.available_bike_stands=0&rows=-1");
                }
            }
            else if (type == "autolib")
            {
                if (depart)
                {
                    response = client.DownloadString("https://opendata.paris.fr/api/records/1.0/search/?dataset=autolib-disponibilite-temps-reel&rows=-1&facet=charging_status&facet=kind&facet=postal_code&facet=slots&facet=status&facet=subscription_status&refine.status=ok&exclude.cars=0");
                }
                else
                {
                    response = client.DownloadString("https://opendata.paris.fr/api/records/1.0/search/?dataset=autolib-disponibilite-temps-reel&rows=-1&facet=charging_status&facet=kind&facet=postal_code&facet=slots&facet=status&facet=subscription_status&refine.status=ok&exclude.charge_slots=0");
                }
            }
            else
            {
                response = "test";
            }

            var requestObject = JsonConvert.DeserializeObject<ParisRootObject>(response);

            foreach (Record record in requestObject.records)
            {
                result.Add(record);
            }

            return result;
        }



        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Coordinates(IFormCollection form)
        {
            double latitude_depart = Convert.ToDouble(form["latitude_depart"]);
            double longitude_depart = Convert.ToDouble(form["longitude_depart"]);
            double latitude_arriv = Convert.ToDouble(form["latitude_arriv"]);
            double longitude_arriv = Convert.ToDouble(form["longitude_arriv"]);

            //ViewData["latitude_depart"] = latitude_depart.ToString();
            //ViewData["longitude_depart"] = longitude_depart.ToString();
            //ViewData["latitude_arriv"] = latitude_arriv.ToString();
            //ViewData["longitude_arriv"] = longitude_arriv.ToString();


            return View();
        }

        public IActionResult Debug()
        {
            List<Record> Points = GetPoints();
            List<String> Result = new List<String>();
            foreach (Record point in Points)
            {
                Result.Add(point.fields.address) ;
            }
            ViewData["trace"] = Result;
            return View();
        }
        
    }
}