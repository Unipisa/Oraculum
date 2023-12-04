using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Oraculum;

public class GetDestinationOfMission : IFunction
{

    private readonly Sibylla sibylla;

    public GetDestinationOfMission(Sibylla sibylla)
    {
        this.sibylla = sibylla;
    }

    public object Execute(Dictionary<string, object> args)
    {
        return GetDestinationOfMissionFunction(args);
    }

    private static object GetDestinationOfMissionFunction(Dictionary<string, object> args)
    {
        // retuns a random destination for the mission of today based on the current date
        var date = DateTime.Now;
        var destinations = new List<string>() { "Moon", "Mars", "Jupiter", "Saturn", "Pluto" };
        var destination = destinations[date.Day % destinations.Count];
        return destination;
    }
}
