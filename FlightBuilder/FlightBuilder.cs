using System;
using System.Collections.Generic;
using System.Linq;

namespace Gridnine.FlightCodingTest
{
    public class Filters
    {
        /// <summary>
        /// 1.	Вылет до текущего момента времени
        /// </summary>
        public void PrintDepartureDateBeforeNow(IList<Flight> currentFlights) 
        {
            var incorrectFlights = (from flight in currentFlights
                                from segment in flight.Segments
                                where segment.DepartureDate < DateTime.Now
                                select flight).ToList();
            var filterFlights = currentFlights.Except(incorrectFlights).ToList();
            PrintFlights(filterFlights);
        }
        /// <summary>
        /// 2. Имеются сегменты с датой прилёта раньше даты вылета
        /// </summary>
        public void PrintArriveBeforeDeparture(IList<Flight> currentFlights)
        {
            var incorrectFlights = (from flight in currentFlights
                                 from segment in flight.Segments
                                 where segment.ArrivalDate < segment.DepartureDate
                                 select flight).ToList();
            var filterFlights = currentFlights.Except(incorrectFlights).ToList();
            PrintFlights(filterFlights);
        }
        /// <summary>
        /// 3.	Общее время, проведённое на земле превышает два часа 
        /// (время на земле — это интервал между прилётом одного сегмента и вылетом следующего за ним)
        /// </summary>
        public void PrintGroundTimeMoreTwoH(IList<Flight> currentFlights)
        {
            List<Flight> incorrectFlights = new List<Flight>();
            for (int i = 0; i < currentFlights.Count; i++)
            {
                int countOfSegments = currentFlights[i].Segments.Count();
                for (int k = 1; k < countOfSegments && countOfSegments > 1; k++)
                {
                    if ((currentFlights[i].Segments[k].DepartureDate - currentFlights[i].Segments[k - 1].ArrivalDate).Hours > 2)
                        incorrectFlights.Add(currentFlights[i]);
                }
            }
            var filterFlights = currentFlights.Except(incorrectFlights).ToList();
            PrintFlights(filterFlights);
        }
        /// <summary>
        /// 4. Все перелеты
        /// </summary>
        public void PrintAllFlights(IList<Flight> currentFlights)
        {
            PrintFlights(currentFlights);
        }
        /// <summary>
        /// Вывод на печать
        /// </summary>
        private void PrintFlights(IList<Flight> currentFlights)
        {
            Console.WriteLine("_________________________________________");
            for (int i = 0; i < currentFlights.Count; i++)
            {
                foreach (var s in currentFlights[i].Segments)
                {
                    Console.WriteLine($"{s.DepartureDate} - {s.ArrivalDate}");
                }
                Console.WriteLine("_________________________________________");
            }
            
        }
    }
    public class Program
    {
       private static void Main(string[] args)
        {
            FlightBuilder flightBuilder = new FlightBuilder();
            var AllFlights = flightBuilder.GetFlights();
            Filters filters = new Filters();
            bool flag = true;
            while (flag)
            {
                Console.WriteLine("0. Выход\n1. Исключить перелеты, где вылет до текущего момента времени\n2. Исключить перелеты, где имеются сегменты с датой прилёта раньше даты вылета\n3. Исключить перелеты, где общее время, проведённое на земле превышает два часа\n4. Все перелеты\n\nВведите команду одной цифрой:");
                string UserCommand = Console.ReadLine();
                switch (UserCommand)
                {
                    case "0": flag = !flag;  
                        break;
                    case "1":
                        filters.PrintDepartureDateBeforeNow(AllFlights); 
                        break;
                    case "2": filters.PrintArriveBeforeDeparture(AllFlights);
                        break;
                    case "3": filters.PrintGroundTimeMoreTwoH(AllFlights); 
                        break;
                    case "4": filters.PrintAllFlights(AllFlights); 
                        break;
                    default: Console.WriteLine("Нет такой команды");
                        break;
                }               
            }
        }
    }
    
    public class FlightBuilder
    {
        private DateTime _threeDaysFromNow;

        public FlightBuilder()
        {
            _threeDaysFromNow = DateTime.Now.AddDays(3);
        }

        public IList<Flight> GetFlights()
        {
            return new List<Flight>
            {
                //A normal flight with two hour duration
			    CreateFlight(_threeDaysFromNow, _threeDaysFromNow.AddHours(2)), 

                //A normal multi segment flight
			    CreateFlight(_threeDaysFromNow, _threeDaysFromNow.AddHours(2), _threeDaysFromNow.AddHours(3), _threeDaysFromNow.AddHours(5)), 
                
                //A flight departing in the past
                CreateFlight(_threeDaysFromNow.AddDays(-6), _threeDaysFromNow),

                //A flight that departs before it arrives
                CreateFlight(_threeDaysFromNow, _threeDaysFromNow.AddHours(-6)),

                //A flight with more than two hours ground time
                CreateFlight(_threeDaysFromNow, _threeDaysFromNow.AddHours(2), _threeDaysFromNow.AddHours(5), _threeDaysFromNow.AddHours(6)),

                 //Another flight with more than two hours ground time
                CreateFlight(_threeDaysFromNow, _threeDaysFromNow.AddHours(2), _threeDaysFromNow.AddHours(3), _threeDaysFromNow.AddHours(4), _threeDaysFromNow.AddHours(6), _threeDaysFromNow.AddHours(7))
            };
        }

        private static Flight CreateFlight(params DateTime[] dates)
        {
            if (dates.Length % 2 != 0) throw new ArgumentException("You must pass an even number of dates,", "dates");

            var departureDates = dates.Where((date, index) => index % 2 == 0); 
            var arrivalDates = dates.Where((date, index) => index % 2 == 1);

            var segments = departureDates.Zip(arrivalDates,
                                              (departureDate, arrivalDate) =>
                                              new Segment { DepartureDate = departureDate, ArrivalDate = arrivalDate }).ToList();

            return new Flight { Segments = segments };
        }
    }

    public class Flight
    {
        public IList<Segment> Segments { get; set; }
    }

    public class Segment
    {
        public DateTime DepartureDate { get; set; } 
        public DateTime ArrivalDate { get; set; } 
    }
}

