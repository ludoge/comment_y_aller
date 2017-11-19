using System;
using System.Diagnostics;
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
        //Google Maps API key
        public const string Key = "AIzaSyCKE-qC-H_55fQY-A6T9htgSbvLPVHmyrw";

        //Obtains all stations with available vehicles (if depart) or available slots (otherwise) of the specified type.
        //1 call to OpenDataParis API
        public static List<Record> GetPoints(VehiculeLib Type, bool depart = true)
        {
            List<Record> result = new List<Record>();

            WebClient client = new WebClient();


            String response;

            if (Type == VehiculeLib.vélib)
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
            else if (Type == VehiculeLib.autolib)
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
                response = ""; //This can't happen, the method would throw an error earlier...
            }

            var requestObject = JsonConvert.DeserializeObject<ParisRootObject>(response);

            foreach (Record record in requestObject.records)
            {
                result.Add(record);
            }

            return result;
        }

        //Distance between any two points on Earth
        static double GeometricDistance(Record Point1, Record Point2)
        {

            double lat1; double lat2; double lon1; double lon2;

            if (Point1.fields.position != null && Point2.fields.position != null)
            {
                lat1 = Point1.fields.position[0];
                lon1 = Point1.fields.position[1];
                lat2 = Point2.fields.position[0];
                lon2 = Point2.fields.position[1];
            }
            else if (Point1.fields.geo_point != null && Point2.fields.geo_point != null)
            {
                lat1 = Point1.fields.geo_point[0];
                lon1 = Point1.fields.geo_point[1];
                lat2 = Point2.fields.geo_point[0];
                lon2 = Point2.fields.geo_point[1];
            }
            else
            {
                lat1 = 0;
                lon1 = 0;
                lat2 = 0;
                lon2 = 0;
            }

            var rlat1 = Math.PI * lat1 / 180;
            var rlat2 = Math.PI * lat2 / 180;
            var rlon1 = Math.PI * lon1 / 180;
            var rlon2 = Math.PI * lon2 / 180;

            var theta = lon1 - lon2;
            var rtheta = Math.PI * theta / 180;

            var dist = Math.Sin(rlat1) * Math.Sin(rlat2) + Math.Cos(rlat1) * Math.Cos(rlat2) * Math.Cos(rtheta);
            dist = Math.Acos(dist);
            dist = dist * 180 / Math.PI;
            dist = dist * 60 * 1.1515 * 1.609344 * 1000;
            return dist;
        }

        static List<Record> NPlusProches(List<Record> Stations, Record Location, int numberOfPoints)
        {
            List<Record> Result = new List<Record>();

            Result = Stations.OrderBy(keySelector: Station => GeometricDistance(Station, Location)).ToList();

            Result = Result.Take(numberOfPoints).ToList();

            return Result;
        }

        public static MapsRootObject GetRoute(Record Depart, Record Arrivee, Mode mode)
        {
            string depart;
            string arrivee;
            try
            {
                depart = Depart.fields.position[0].ToString().Replace(',', '.') + "," + Depart.fields.position[1].ToString().Replace(',', '.');
                
            }
            catch (NullReferenceException)
            {
                depart = Depart.fields.geo_point[0].ToString().Replace(',', '.') + "," + Depart.fields.geo_point[1].ToString().Replace(',', '.');
            }
            try
            {
                arrivee = Arrivee.fields.position[0].ToString().Replace(',', '.') + "," + Arrivee.fields.position[1].ToString().Replace(',', '.');
            }
            catch (NullReferenceException)
            {
                arrivee = Arrivee.fields.geo_point[0].ToString().Replace(',', '.') + "," + Arrivee.fields.geo_point[1].ToString().Replace(',', '.');
            }

            //string depart = Depart.fields.position[0].ToString();
            //string arrivee = Arrivee.fields.position[0].ToString();

            return GetRoute(depart, arrivee, mode);
        }
        public static MapsRootObject GetRoute(List<Double> Depart, List<Double> Arrivee, Mode mode)
        {
            string depart = Depart[0].ToString().Replace(',', '.') + "," + Depart[1].ToString().Replace(',', '.');
            string arrivee = Arrivee[0].ToString().Replace(',', '.') + "," + Arrivee[1].ToString().Replace(',', '.');

            return GetRoute(depart, arrivee, mode);
        }
        public static MapsRootObject GetRoute(string depart, string arrivee, Mode mode)
        {

            string url = "https" + "://maps.googleapis.com/maps/api/directions/json?origin=" + depart + "&destination=" + arrivee + "&mode=" + mode.ToString() + "&key=" + Key +"&language=fr";
            url = string.Format(url);

            //Console.WriteLine(url);

            WebClient client = new WebClient();
            string response;
            response = client.DownloadString(url);
            //Console.WriteLine(response);
            var requestObject = JsonConvert.DeserializeObject<MapsRootObject>(response);

            try
            {
                Double dummy = requestObject.routes[0].legs[0].start_location.lat;
            }
            catch (IndexOutOfRangeException)
            {
                System.Diagnostics.Debug.WriteLine(url);
                throw;
            }

            return requestObject;
        }
        public static MapsRootObject GetRoute(List<Double> Depart, Record Arrivee, Mode mode)
        {
            string depart = Depart[0].ToString().Replace(',', '.') + "," + Depart[1].ToString().Replace(',', '.');
            string arrivee;
            try
            {
                arrivee = Arrivee.fields.position[0].ToString().Replace(',', '.') + "," + Arrivee.fields.position[1].ToString().Replace(',', '.');
            }
            catch (NullReferenceException)
            {
                arrivee = Arrivee.fields.geo_point[0].ToString().Replace(',', '.') + "," + Arrivee.fields.geo_point[1].ToString().Replace(',', '.');
            }
            return GetRoute(depart, arrivee, mode);
        }
        public static MapsRootObject GetRoute(Record Depart, List<Double> Arrivee, Mode mode)
        {
            string depart;
            try
            {
                depart = Depart.fields.position[0].ToString().Replace(',', '.') + "," + Depart.fields.position[1].ToString().Replace(',', '.');
            }
            catch (NullReferenceException)
            {
                depart = Depart.fields.geo_point[0].ToString().Replace(',', '.') + "," + Depart.fields.geo_point[1].ToString().Replace(',', '.');
            }
            string arrivee = Arrivee[0].ToString().Replace(',', '.') + "," + Arrivee[1].ToString().Replace(',', '.');
            return GetRoute(depart, arrivee, mode);
        }
        public static MapsRootObject GetRoute(List<Double> Depart, string arrivee, Mode mode)
        {
            string depart = Depart[0].ToString().Replace(',', '.') + "," + Depart[1].ToString().Replace(',', '.');
            return GetRoute(depart, arrivee, mode);
        }
        public static MapsRootObject GetRoute(string depart, List<Double> Arrivee, Mode mode)
        {
            string arrivee = Arrivee[0].ToString().Replace(',', '.') + "," + Arrivee[1].ToString().Replace(',', '.');
            return GetRoute(depart, arrivee, mode);
        }

        public static List<MapsRootObject> GetPossibleRoutes(List<Record> DeparturePoints, List<Record> ArrivalPoints)
        {
            List<MapsRootObject> Routes = new List<MapsRootObject>();

            //Velib

            if (DeparturePoints[0].fields.position != null)//THe API occasionally returns weird data or empty routes, this accounts for that
            {
                foreach (Record DeparturePoint in DeparturePoints)
                {
                    foreach (Record ArrivalPoint in ArrivalPoints)
                    {
                        Routes.Add(GetRoute(DeparturePoint, ArrivalPoint, Mode.bicycling));
                    }
                }
            }

            //Autolib
            else if (DeparturePoints[0].fields.geo_point != null)
            {
                foreach (Record DeparturePoint in DeparturePoints)
                {
                    foreach (Record ArrivalPoint in ArrivalPoints)
                    {
                        Routes.Add(GetRoute(DeparturePoint, ArrivalPoint, Mode.driving));
                    }
                }
            }


            return Routes;
        }

        public static double GetPrecipitation(Record Depart, String rayon)
        {
            DateTime date = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, 00, 00);

            List<OpenDataSoftRecord> result = new List<OpenDataSoftRecord>();

            string depart = Depart.fields.position[0].ToString().Replace(',', '.') + "," + Depart.fields.position[1].ToString().Replace(',', '.');

            string url = string.Format("https" + "://data.opendatasoft.com/api/records/1.0/search/?dataset=arome-0025-sp1_sp2_paris%40public&rows=-1&sort=forecast&geofilter.distance=" + depart + "," + rayon);

            url = string.Format(url);

            WebClient client = new WebClient();

            string response = client.DownloadString(url);

            var requestObject = JsonConvert.DeserializeObject<OpenDataSoftRootObject>(response);

            foreach (OpenDataSoftRecord record in requestObject.records)
            {

                if (record.fields.forecast == date)
                {
                    result.Add(record);
                }
            }
            Double precipitation = result.Max(a => a.fields.total_water_precipitation);
            return precipitation;
        }

        public static double RouteCost(MapsRootObject Route, Record depart, Record arrivee, decimal poids)
        {
            double precipitation;
            try
            {
                precipitation = GetPrecipitation(depart, "1800");
            }
            catch (Exception)
            {
                precipitation = 0;
            }

            int cost = 0;
            foreach (Route route in Route.routes)
            {
                foreach (Leg leg in route.legs)
                {
                    foreach (Step step in leg.steps)
                    {
                        if (step.travel_mode == "walking")
                        {
                            cost += (int)Math.Floor((double)leg.duration.value * (1 + precipitation / 10) * (1+Convert.ToInt32(poids)/10));
                        }
                        else if (step.travel_mode == "bicycling" && (precipitation > 5.0 || poids > 10))
                        {
                            cost += 999999999;
                        }
                        else if (step.travel_mode == "bicycling" && precipitation <= 5.0 && poids <= 10)
                        {
                            cost += (int)Math.Floor((double)leg.duration.value * (1 + precipitation/10) * (1 + Convert.ToInt32(poids) / 10));
                        }
                        else
                        {
                            cost += step.duration.value;
                        }
                    }
                }
            }
            return cost;
        }

        //Adds walk to/from Vélib/Autolib point
        public static MapsRootObject CompleteRoute(MapsRootObject Route, Record start, Record finish)
        {
            try
            {
                if (Route.routes[0].legs[0].steps[1].travel_mode == "TRANSIT" || Route.routes[0].legs[0].steps[0].travel_mode == "TRANSIT")
                {
                }
                else
                {
                    List<Double> RouteStartingPoint = new List<double> { Route.routes[0].legs[0].start_location.lat, Route.routes[0].legs[0].start_location.lng };
                    List<Double> RouteArrivalPoint = new List<double> { Route.routes.Last().legs.Last().end_location.lat, Route.routes.Last().legs.Last().end_location.lng };
                    MapsRootObject RouteBefore = GetRoute(start, RouteStartingPoint, Mode.walking);
                    MapsRootObject RouteAfter = GetRoute(RouteArrivalPoint, finish, Mode.walking);

                    Route.routes[0].legs.Insert(0, RouteBefore.routes[0].legs[0]);
                    Route.routes[0].legs.Add(RouteAfter.routes[0].legs[0]);
                }
            }
            catch (Exception)
            {
            }
            return Route;
        }


        static MapsRootObject MeilleureRoute(List<MapsRootObject> Routes, Record depart, Record arrivee, decimal poids)
        {
            List<MapsRootObject> Result = new List<MapsRootObject>();

            Result = Routes.OrderBy(keySelector: Route => RouteCost(Route, depart, arrivee, poids)).ToList();

            Result = Result.Take(1).ToList();

            return CompleteRoute(Result[0], depart, arrivee);
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Coordinates(IFormCollection form)
        {
            //Get all the data from form
            decimal latitude_depart = Convert.ToDecimal(form["latitude_depart"]);
            decimal longitude_depart = Convert.ToDecimal(form["longitude_depart"]);
            decimal latitude_arriv = Convert.ToDecimal(form["latitude_arriv"]);
            decimal longitude_arriv = Convert.ToDecimal(form["longitude_arriv"]);
            decimal poids_porte = Convert.ToDecimal(form["poids_porte"]);
            bool autolib = (form["autolib"]=="on");
            bool velib = (form["velib"]=="on");
            bool metro = (form["metro"]=="on");


            Record Departure = new Record(latitude_depart, longitude_depart);
            Record Arrival = new Record(latitude_arriv, longitude_arriv);

            List<Record> PossibleDeparturePointsVelib = GetPoints(VehiculeLib.vélib, true);
            List<Record> PossibleArrivalPointsVelib = GetPoints(VehiculeLib.vélib, false);
            List<Record> DeparturePointsVelib = NPlusProches(PossibleDeparturePointsVelib, Departure, 1);
            List<Record> ArrivalPointsVelib = NPlusProches(PossibleDeparturePointsVelib, Arrival, 1);

            List<Record> PossibleDeparturePointsAutolib = GetPoints(VehiculeLib.autolib, true);
            List<Record> PossibleArrivalPointsAutolib = GetPoints(VehiculeLib.autolib, false);
            List<Record> DeparturePointsAutolib = NPlusProches(PossibleDeparturePointsAutolib, Departure, 1);
            List<Record> ArrivalPointsAutolib = NPlusProches(PossibleDeparturePointsAutolib, Arrival, 1);

            List<MapsRootObject> Routes = new List<MapsRootObject>();
            if (velib)
            {
                Routes = Routes.Concat(GetPossibleRoutes(DeparturePointsVelib, ArrivalPointsVelib)).ToList();
            }
            if (autolib)
            {
                Routes = Routes.Concat(GetPossibleRoutes(DeparturePointsAutolib, ArrivalPointsAutolib)).ToList();
            }
            if (metro)
            {
                MapsRootObject MetroRoute = GetRoute(Departure, Arrival, Mode.transit);
                Routes.Add(MetroRoute);
            }

            MapsRootObject BestRoute = MeilleureRoute(Routes, Departure, Arrival, poids_porte);

            //Extracting primary travel mode from route
            String mode;
            List<String> Instructions = new List<String>();
            foreach (Leg leg in BestRoute.routes[0].legs)
            {
                foreach (Step step in leg.steps)
                {
                    if(step.travel_mode.ToLower() != "walking")
                    {
                        mode = step.travel_mode;
                        goto exitLoop;
                    }
                
                }
            }
            mode = "walking";
            exitLoop: { }
                
            ViewData["Route"] = BestRoute;

            ViewData["mode"] = mode.ToLower();
            return View();
        }        
    }
}