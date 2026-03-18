import random
import datetime
from flask import Flask, request, jsonify

app = Flask(__name__)


def make_temperature_response(location, sensor_id):
    value = round(random.uniform(15.0, 35.0), 2)
    return {
        "value": value,
        "unit": "celsius",
        "timestamp": datetime.datetime.utcnow().strftime("%Y-%m-%dT%H:%M:%SZ"),
        "location": location,
        "status": "active",
        "sensor_id": sensor_id,
        "sensor_type": "temperature",
        "description": f"Temperature sensor at {location}",
    }


@app.route("/temperature")
def get_temperature():
    location = request.args.get("location", "unknown")
    return jsonify(make_temperature_response(location, location))


@app.route("/temperature/<sensor_id>")
def get_temperature_by_id(sensor_id):
    return jsonify(make_temperature_response(f"location-{sensor_id}", sensor_id))


if __name__ == "__main__":
    app.run(host="0.0.0.0", port=8081)
