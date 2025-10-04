import math


def calculateCrater(velocity, mass):
    # Calculate kinetic energy in joules
    energy = 0.5 * mass * velocity ** 2

    # Crater diameter in km
    diameter = 1.8 * (energy / 1e12) ** 0.294

    # Richter magnitude
    magnitude = 0.67 * math.log10(energy) - 5.87

    return {"diameter": diameter, "richter scale": magnitude}