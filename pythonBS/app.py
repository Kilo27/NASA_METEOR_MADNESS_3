from flask import Flask, request, jsonify
from src import getTrajectory
from API_calls import getData
from src import simulation

app=Flask(__name__)

@app.route("/getAsteroids") #http://localhost:5000/getAsteroids?start_date=2025-10-05&weeks=1
def get_asteroids(methods="POST"):
    start_date=request.args.get('start_date')
    weeks= int(request.args.get('weeks'))
    return jsonify(getData.getAsteroids(start_date, weeks))

@app.route("/calculateAsteroidTrajectory")
def get_trajectory():
    asteroid_id=int(request.args.get("asteroid_id"))
    elements = getData.orbitalData(asteroid_id)
    end_date=request.args.get("end_date")
    return jsonify(getTrajectory.calculate_asteroid_trajectory(elements, end_date))#http://localhost:5000/calculateAsteroidTrajectory?asteroid_id=2497232&end_date=2026-10-01

@app.route("/getAsteroidInfo")#http://localhost:5000/getAsteroidInfo?asteroid_id=2497232
def asteroid_info(methods="POST"):
    asteroid_id=int(request.args.get("asteroid_id"))
    return jsonify(getData.get_asteroid_info(asteroid_id))

@app.route("/getAsteroidInfoByDay")#http://localhost:5000/getAsteroidInfoByDay?asteroid_id=2497232&day=0
def asteroid_info_by_day(methods="POST"):
    asteroid_id=int(request.args.get("asteroid_id"))
    day=int(request.args.get("day"))
    return jsonify(getData.get_asteroid_info_with_day(asteroid_id, day))

@app.route("/simulate")# http://127.0.0.1:5000/simulate?diameter=19&velocity=19000&impact_angle=45
def simulation1():
    diameter = int(request.args.get("diameter"))
    velocity = float(request.args.get("velocity"))
    impact_angle = int(request.args.get("impact_angle"))

    return jsonify(simulation.simulate_asteroid_impact(diameter, velocity, impact_angle)) 

if __name__ == '__main__':
    app.run()