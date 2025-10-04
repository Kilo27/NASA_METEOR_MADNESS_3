using System.Collections.Generic;
[System.Serializable]

public class AsteroidDataClass
{
    public string id;
    public string name;
    public bool is_hazardous;
    public string next_close_approach_date;
    public string miss_distance_km;
}

[System.Serializable]
public class AsteroidList
{
    public List<AsteroidDataClass> asteroids = new List<AsteroidDataClass>();
}