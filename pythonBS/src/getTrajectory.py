import requests
import sys
from API_calls.getData import orbitalData, getAsteroids
from API_calls import config
import json
from astropy import units as u
from astropy.time import Time
from poliastro.bodies import Sun, Earth
from poliastro.twobody import Orbit
import numpy as np
from math import radians, degrees, cos, sin, atan2, sqrt, tan
from datetime import datetime

#Asteroid asteroids[] =getData.getAsteroids(mm-dd-yyyy, mm-dd-yyyy)

#Class Asteroid:
AU_TO_M = 149597870000.7  # 1 AU in meters
def solve_kepler(M, e, tolerance=1e-6):
    """Solve Kepler's Equation M = E - e*sin(E) using Newton-Raphson method."""
    E = M if e < 0.8 else np.pi
    while True:
        delta = E - e * np.sin(E) - M
        if abs(delta) < tolerance:
            break
        E -= delta / (1 - e * np.cos(E))
    return E
def asteroid_position(elements, target_date):
    # Orbital elements (example for asteroid 2009 JR5)
    a = float(elements['a'])                     # semi-major axis in AU
    e = float(elements['e'])                     # eccentricity
    i = radians(float(elements['i']))            # inclination in radians
    Omega = radians(float(elements['Omega']))    # longitude of ascending node
    omega = radians(float(elements['omega']))    # argument of perihelion
    M0 = radians(float(elements['M0']))          # mean anomaly at epoch in radians
    epoch = float(elements['epoch'])             # epoch in Julian date

    # Time conversion
    t = Time(target_date, format='iso', scale='utc').jd
    n = 360 / float(elements['period'])          # mean motion in deg/day
    M = radians((degrees(M0) + n * (t - epoch)) % 360)  # updated mean anomaly

    # Solve for eccentric anomaly E
    E = solve_kepler(M, e)

    # True anomaly ν
    nu = 2 * atan2(sqrt(1 + e) * sin(E / 2), sqrt(1 - e) * cos(E / 2))

    # Distance r
    r = a * (1 - e * cos(E))

    # Position in orbital plane
    x_orb = r * cos(nu)
    y_orb = r * sin(nu)

    # Rotate to heliocentric ecliptic coordinates
    x = r * (cos(Omega) * cos(omega + nu) - sin(Omega) * sin(omega + nu) * cos(i))
    y = r * (sin(Omega) * cos(omega + nu) + cos(Omega) * sin(omega + nu) * cos(i))
    z = r * (sin(omega + nu) * sin(i))
    return x*AU_TO_M, y*AU_TO_M, z*AU_TO_M

current_date = datetime.now().strftime("%Y-%m-%d")

def calculate_asteroid_trajectory(elements, end_date, start_date=current_date, step_days=1):
    dates = np.arange(np.datetime64(start_date), np.datetime64(end_date), np.timedelta64(step_days, 'D'))
    trajectory = [list(asteroid_position(elements, str(date))) for date in dates]
    return trajectory
#TODO: Replace with actual elements from getAsteroidData()
elements = orbitalData(2000719)

