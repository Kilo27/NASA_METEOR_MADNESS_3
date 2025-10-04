import requests
from astropy import units as u
from astropy.time import Time
from poliastro.bodies import Sun, Earth
from poliastro.twobody import Orbit
import numpy as np

# Example: Astronomy Picture of the Day (APOD)
api_key = "ESoNRl5hTTqAWrQAHMoIcCcaIgJwCm5vT2LfoUih"  # replace with your own API key
start_date = "2023-10-01"
end_date = "2024-10-07"
url = f"https://api.nasa.gov/neo/rest/v1/feed?start_date=2025-10-01&end_date=2025-10-03&api_key={api_key}"

