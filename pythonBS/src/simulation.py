import math
import json


class AsteroidImpactSimulator:
    # constants,
    G = 6.67428e-11
    g0 = 9.80665
    h0 = 8000
    rho0 = 1.2
    Cd = 1.0  # cross check once with real asteroid data
    fp = 7.0

    def __init__(self, diameter, velocity, impact_angle, density=3100):
        self.diameter = diameter
        self.initial_velocity = velocity
        self.impact_angle_deg = impact_angle
        self.impact_angle_rad = math.radians(impact_angle)
        self.density = density

        self.mass = self.calculate_mass()
        self.kinetic_energy = self.calculate_kinetic_energy()

    def calculate_mass(self):
        radius = self.diameter / 2
        volume = (4 / 3) * math.pi * radius ** 3
        return self.density * volume

    def calculate_kinetic_energy(self):
        return 0.5 * self.mass * self.initial_velocity ** 2

    def calculate_yield_strength(self):
        return 10 ** (2.107 + 0.0624 * math.sqrt(self.density))

    def calculate_breakup_altitude(self):
        # 3.33
        Yi = self.calculate_yield_strength()

        # 3.34
        numerator = 4.07 * self.Cd * self.h0 * Yi
        denominator = self.density * self.diameter * self.initial_velocity ** 2 * math.sin(self.impact_angle_rad)
        If = numerator / denominator

        term1 = math.log(Yi / (self.rho0 * self.initial_velocity ** 2))
        term2 = 1.308 - 0.314 * If - 1.303 * math.sqrt(1 - If)

        breakup_altitude = -self.h0 * (term1 + term2)
        return max(0, breakup_altitude)

    def calculate_airburst_altitude(self, breakup_altitude):
        if breakup_altitude <= 0:
            return 0  # no breakup case

        # 3.37
        rho_z = self.rho0 * math.exp(-breakup_altitude / self.h0)
        L_disp = (self.diameter * math.sin(self.impact_angle_rad) *
                  math.sqrt(self.density / (self.Cd * rho_z)))

        alpha = math.sqrt(self.fp ** 2 - 1)
        term = 1 + (L_disp / (2 * self.h0)) * alpha

        airburst_altitude = breakup_altitude - 2 * self.h0 * math.log(term)
        return max(0, airburst_altitude)


class GroundImpactEffects:
    def __init__(self, simulator, target_density=2500):
        self.sim = simulator
        self.target_density = target_density

    def calculate_transient_crater(self):
        term1 = (self.sim.density / self.target_density) ** (1 / 3)
        term2 = self.sim.diameter ** 0.78
        term3 = self.sim.initial_velocity ** 0.44
        term4 = self.sim.g0 ** (-0.22)
        term5 = math.sin(self.sim.impact_angle_rad) ** (1 / 3)

        # 3.42
        D_tc = 1.161 * term1 * term2 * term3 * term4 * term5
        return D_tc

    def calculate_final_crater(self, transient_crater_diameter):
        return transient_crater_diameter * 1.25

    def calculate_seismic_magnitude(self):
        # 3.45
        return 0.67 * math.log10(self.sim.kinetic_energy) - 5.87

    def calculate_overpressure(self, distance):
        # 3.49
        E_kt = self.sim.kinetic_energy / (4.184e12)

        # yield scale distance (no idea what this is)
        D1 = distance / (E_kt ** (1 / 3))

        px = 75000
        Dx = 290

        pD = (px * Dx) / (4 * D1) * (1 + 3 * (Dx / D1) ** 1.3)
        return pD

    def calculate_wind_speed(self, overpressure, ambient_pressure=101325):
        # 3.55
        c0 = 340
        term1 = (5 * overpressure) / (7 * ambient_pressure)
        term2 = c0 / math.sqrt(1 + (6 * overpressure) / (7 * ambient_pressure))
        return term1 * term2

    def calculate_thermal_radiation(self, distance, luminous_efficiency=1e-3):
        # 3.58
        energy_flux = (luminous_efficiency * self.sim.kinetic_energy) / (2 * math.pi * distance ** 2)
        return energy_flux

    def calculate_fireball_radius(self):
        return 0.002 * (self.sim.kinetic_energy ** (1 / 3))

    def calculate_overpressure_radius(self, pressure_threshold=5000):
        # use inverse equation to calculate the radius
        E_kt = self.sim.kinetic_energy / (4.184e12)
        px = 75000
        Dx = 290

        D1_guess = 1000
        tolerance = 1
        max_iterations = 100

        # using a sort of differentiation approach
        for i in range(max_iterations):
            term1 = (px * Dx) / (4 * D1_guess)
            term2 = 1 + 3 * (Dx / D1_guess) ** 1.3
            calculated_pressure = term1 * term2

            if abs(calculated_pressure - pressure_threshold) < tolerance:
                break

            if calculated_pressure > pressure_threshold:
                D1_guess *= 1.1
            else:
                D1_guess *= 0.9

        distance = D1_guess * (E_kt ** (1 / 3))
        return distance

    def calculate_shockwave_radius(self, wind_threshold=50):

        # sorta similiar approach to overpressure
        distance_guess = 1000

        for i in range(100):
            overpressure = self.calculate_overpressure(distance_guess)
            wind_speed = self.calculate_wind_speed(overpressure)

            if abs(wind_speed - wind_threshold) < 1:
                break

            if wind_speed > wind_threshold:
                distance_guess *= 1.1
            else:
                distance_guess *= 0.9

        return distance_guess

    def calculate_thermal_damage_radius(self, flux_threshold=100000):
        luminous_efficiency = 1e-3
        radius = math.sqrt((luminous_efficiency * self.sim.kinetic_energy) / (2 * math.pi * flux_threshold))
        return radius


class TsunamiGenerator:
    def __init__(self, simulator, ocean_depth=4000):
        self.sim = simulator
        self.ocean_depth = ocean_depth  # this is in METERS NOT KILOMETRES

    def calculate_tsunami_wave_amplitude(self, distance_from_impact):
        # 3.66
        term1 = (self.sim.density / 1000) ** (1 / 3)
        term2 = self.sim.diameter ** 0.78
        term3 = self.sim.initial_velocity ** 0.44
        term4 = self.sim.g0 ** (-0.22)
        term5 = math.sin(self.sim.impact_angle_rad) ** (1 / 3)

        D_tc_water = 1.365 * term1 * term2 * term3 * term4 * term5

        A_initial = min(0.14 * D_tc_water, self.ocean_depth)
        amplitude = A_initial * (D_tc_water / (2 * distance_from_impact))

        return amplitude

    def calculate_runup_height(self, wave_amplitude, beach_slope=0.02):
        # 3.71
        D_tc = 1.365 * (self.sim.density / 1000) ** (1 / 3) * \
               self.sim.diameter ** 0.78 * self.sim.initial_velocity ** 0.44 * \
               self.sim.g0 ** (-0.22) * math.sin(self.sim.impact_angle_rad) ** (1 / 3)

        runup = 2 * beach_slope * wave_amplitude * (wave_amplitude / D_tc) ** (-0.5)
        return runup

    def calculate_max_tsunami_range(self, amplitude_threshold=0.1):
        term1 = (self.sim.density / 1000) ** (1 / 3)
        term2 = self.sim.diameter ** 0.78
        term3 = self.sim.initial_velocity ** 0.44
        term4 = self.sim.g0 ** (-0.22)
        term5 = math.sin(self.sim.impact_angle_rad) ** (1 / 3)

        D_tc_water = 1.365 * term1 * term2 * term3 * term4 * term5
        A_initial = min(0.14 * D_tc_water, self.ocean_depth)

        max_range = A_initial * D_tc_water / (2 * amplitude_threshold)
        return max_range


def simulate_asteroid_impact(mean_diameter, velocity, impact_angle):
    """
    mean_diameter: asteroid diameter in meters
    velocity: initial velocity in m/s
    impact_angle: pretty self-explanatory
    """

    asteroid = AsteroidImpactSimulator(
        diameter=mean_diameter,
        velocity=velocity,
        impact_angle=impact_angle
    )

    mass = asteroid.calculate_mass()
    kinetic_energy = asteroid.kinetic_energy
    kinetic_energy_kt_tnt = kinetic_energy / (4.184e12)  # Convert to kilotons TNT

    breakup_altitude = asteroid.calculate_breakup_altitude()
    airburst_altitude = asteroid.calculate_airburst_altitude(breakup_altitude)

    if breakup_altitude > 0:
        if airburst_altitude > 0:
            impact_scenario = "airburst"
            scenario_description = "Asteroid will disintegrate in airburst"
        else:
            impact_scenario = "fragmentation"
            scenario_description = "Asteroid breaks up but fragments reach surface"
    else:
        impact_scenario = "surface_impact"
        scenario_description = "Asteroid impacts surface intact"

    ground_effects = GroundImpactEffects(asteroid)
    D_tc = ground_effects.calculate_transient_crater()
    D_final = ground_effects.calculate_final_crater(D_tc)
    seismic_magnitude = ground_effects.calculate_seismic_magnitude()

    fireball_radius = ground_effects.calculate_fireball_radius()
    overpressure_radius_5kpa = ground_effects.calculate_overpressure_radius(5000)
    overpressure_radius_20kpa = ground_effects.calculate_overpressure_radius(20000)
    shockwave_radius_50ms = ground_effects.calculate_shockwave_radius(50)
    thermal_damage_radius = ground_effects.calculate_thermal_damage_radius(100000)

    overpressure_crater_rim = ground_effects.calculate_overpressure(D_final / 2)
    wind_speed_crater_rim = ground_effects.calculate_wind_speed(overpressure_crater_rim)
    thermal_flux_crater_rim = ground_effects.calculate_thermal_radiation(D_final / 2)

    tsunami = TsunamiGenerator(asteroid)
    max_tsunami_range = tsunami.calculate_max_tsunami_range(0.1)

    wave_amplitude_100km = tsunami.calculate_tsunami_wave_amplitude(100000)
    runup_height_100km = tsunami.calculate_runup_height(wave_amplitude_100km)

    if runup_height_100km < 1:
        tsunami_category = "Minor"
    elif runup_height_100km < 5:
        tsunami_category = "Moderate"
    elif runup_height_100km < 15:
        tsunami_category = "Major"
    else:
        tsunami_category = "Catastrophic"

    results = {
        "input_parameters": {
            "mean_diameter_m": mean_diameter,
            "velocity_km_s": velocity / 1000,
            "impact_angle_deg": impact_angle
        },
        "asteroid_properties": {
            "mass_kg": mass,
            "kinetic_energy_joules": kinetic_energy,
            "kinetic_energy_kt_tnt": kinetic_energy_kt_tnt,
            "equivalent_nuclear_yield": f"{kinetic_energy_kt_tnt:.1f} kt TNT"
        },
        "atmospheric_passage": {
            "breakup_altitude_km": breakup_altitude / 1000,
            "airburst_altitude_km": airburst_altitude / 1000,
            "impact_scenario": impact_scenario,
            "scenario_description": scenario_description
        },
        "impact_crater": {
            "transient_crater_diameter_km": D_tc / 1000,
            "final_crater_diameter_km": D_final / 1000,
            "crater_radius_km": (D_final / 2) / 1000
        },
        "effect_radii": {
            "fireball_radius_km": fireball_radius / 1000,
            "overpressure_radius_5kpa_km": overpressure_radius_5kpa / 1000,
            "overpressure_radius_20kpa_km": overpressure_radius_20kpa / 1000,
            "shockwave_radius_50ms_km": shockwave_radius_50ms / 1000,
            "thermal_damage_radius_km": thermal_damage_radius / 1000,
            "max_tsunami_range_km": max_tsunami_range / 1000
        },
        "effects_at_crater_rim": {
            "overpressure_kpa": overpressure_crater_rim / 1000,
            "wind_speed_m_s": wind_speed_crater_rim,
            "thermal_radiation_mj_m2": thermal_flux_crater_rim / 1e6
        },
        "tsunami_effects": {
            "wave_amplitude_at_100km_m": wave_amplitude_100km,
            "runup_height_at_100km_m": runup_height_100km,
            "tsunami_category": tsunami_category
        },
        "seismic_effects": {
            "richter_magnitude": seismic_magnitude
        },
        "impact_severity_classification": {
            "energy_class": classify_energy(kinetic_energy_kt_tnt),
            "crater_class": classify_crater(D_final),
            "tsunami_class": tsunami_category
        }
    }
    obj = json.dumps(results, indent=2)
    obj.replace("\n","")
    return obj


def classify_energy(energy_kt_tnt):
    if energy_kt_tnt < 0.1:
        return "Very Small"
    elif energy_kt_tnt < 10:
        return "Small"
    elif energy_kt_tnt < 1000:
        return "Medium"
    elif energy_kt_tnt < 10000:
        return "Large"
    else:
        return "Very Large"


def classify_crater(crater_diameter_m):
    crater_diameter_km = crater_diameter_m / 1000
    if crater_diameter_km < 0.1:
        return "Very Small"
    elif crater_diameter_km < 1:
        return "Small"
    elif crater_diameter_km < 10:
        return "Medium"
    elif crater_diameter_km < 100:
        return "Large"
    else:
        return "Very Large"


# Example usage
if __name__ == "__main__":
    results_json = simulate_asteroid_impact(
        mean_diameter=150,  # 20 meters
        velocity=21000,  # 19 km/s
        impact_angle=45,  # 20 degrees
    )

    print("ASTEROID IMPACT SIMULATION RESULTS")
    print("==================================")
    print(results_json)