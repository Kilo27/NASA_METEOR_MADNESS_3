from datetime import datetime, timedelta
import requests
import API_calls.config as config

def getAsteroids(start_date, week):
    start_date_dt = datetime.strptime(start_date, "%Y-%m-%d")

    asteroid_list = []

    for i in range(week):
        start_str = start_date_dt.strftime("%Y-%m-%d")
        end_str = (start_date_dt + timedelta(days=7)).strftime("%Y-%m-%d")

        response = requests.get(
            f"https://api.nasa.gov/neo/rest/v1/feed?start_date={start_str}&end_date={end_str}&api_key={config.API_KEY}"
        ).json()

        for date, asteroids in response["near_earth_objects"].items():
            for asteroid in asteroids:
                asteroid_list.append({"date": date, "id": asteroid["id"], "name": asteroid["name"]})

        start_date_dt += timedelta(days=7)

    asteroid_list = sorted(asteroid_list, key=lambda x: x['date'])

    return asteroid_list

def orbitalData(asteroid_id):
    response = requests.get(f"https://api.nasa.gov/neo/rest/v1/neo/{asteroid_id}?api_key={config.API_KEY}").json()
    orbital_data = response["orbital_data"]
    return {"a": orbital_data["semi_major_axis"],"e" : orbital_data["eccentricity"], "i":orbital_data["inclination"], "Omega": orbital_data["ascending_node_longitude"], "omega": orbital_data["perihelion_argument"], "M0": orbital_data["mean_anomaly"], "epoch": orbital_data["epoch_osculation"], "period": orbital_data["orbital_period"]}

def get_asteroid_info(asteroid_id):
    response = requests.get(f"https://api.nasa.gov/neo/rest/v1/neo/{asteroid_id}?api_key={config.API_KEY}").json()
    asteroid_data = response["close_approach_data"]
    diameter_data = response["estimated_diameter"]
    print(asteroid_data[0]["relative_velocity"]["kilometers_per_second"])

    return {"velocity": (float(asteroid_data[0]["relative_velocity"]["kilometers_per_second"])*1000),"mean_diameter": ((diameter_data["meters"]["estimated_diameter_max"]+diameter_data["meters"]["estimated_diameter_min"])/2) }
def get_asteroid_info_with_day(asteroid_id, day):
    response = requests.get(f"https://api.nasa.gov/neo/rest/v1/neo/{asteroid_id}?api_key={config.API_KEY}").json()
    asteroid_data = response["close_approach_data"]
    return {"velocity": (float(asteroid_data[day]["relative_velocity"]["kilometers_per_second"])*1000)}
if __name__ == '__main__':
    asteroids = getAsteroids(start_date, weeks)
    for a in asteroids:
        print(a)

    print(orbitalData(asteroids[0]["id"]))
