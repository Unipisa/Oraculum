using System;
using System.Collections.Generic;
using LibreHardwareMonitor.Hardware;

public class PowerSensor
{
    internal PowerSensor(IHardware hardware, ISensor sensor)
    {
        Hardware = hardware;
        Sensor = sensor;
        Name = $"{Hardware.Name} - {Sensor.Name}";
        CurrentReading = Sensor.Value;
    }
    internal IHardware Hardware { get; set; }
    internal ISensor Sensor { get; set; }
    public string Name { get; internal set; }
    public float? CurrentReading { get; internal set; }
}

public class PowerSensorManager
{
    private Computer computer;
    private Dictionary<string, List<(long Tick, float Reading)>> sensorReadings;
    private List<PowerSensor> sensors = new List<PowerSensor>();
    private bool isRecording = false;

    public PowerSensorManager()
    {
        sensorReadings = new Dictionary<string, List<(long Tick, float Reading)>>();
        computer = new Computer()
        {
            IsBatteryEnabled = true,
            IsGpuEnabled = true,
            IsMotherboardEnabled = true
        };

        computer.Open();
        foreach (IHardware hardware in computer.Hardware)
        {
            hardware.Update();
            foreach (ISensor sensor in hardware.Sensors)
            {
                if (sensor.SensorType == SensorType.Power)
                {
                    AddSensor(new PowerSensor(hardware, sensor));
                }
            }
        }
    }

    public void Update()
    {
        foreach (var hw in computer.Hardware)
        {
            hw.Update();
        }
        foreach (PowerSensor sensor in sensors)
        {
            sensor.CurrentReading = sensor.Sensor.Value;
            UpdateReading(sensor, sensor.CurrentReading);
        }
    }

    internal void AddSensor(PowerSensor sensor)
    {
        if (sensors.Contains(sensor))
        {
            return;
        }
        sensors.Add(sensor);
    }

    internal void UpdateReading(PowerSensor sensor, float? newReading)
    {
        if (!sensorReadings.ContainsKey(sensor.Name))
        {
            sensorReadings[sensor.Name] = new List<(long Tick, float Reading)>();
        }

        if (newReading.HasValue)
        {
            long currentTick = DateTime.Now.Ticks;
            sensorReadings[sensor.Name].Add((currentTick, newReading.Value));
        }
    }

    public void Reset()
    {
        sensorReadings.Clear();
    }

    public Task<(double Duration, Dictionary<string, float> Energy)> RecordEnergy()
    {
        return Task.Run(() =>
        {
            if (isRecording)
            {
                throw new InvalidOperationException("Already recording");
            }
            isRecording = true;
            Reset();
            var tick = DateTime.Now.Ticks;
            while (isRecording)
            {
                Update();
            }
            var duration = (DateTime.Now.Ticks - tick) / 1e7;
            Update(); // Ensure we get at least two readings
            return (duration, GetEnergy());
        }
        );
    }

    public void StopRecording()
    {
        isRecording = false;
    }

    public Dictionary<string, float> GetEnergy()
    {
        var energy = new Dictionary<string, float>();
        foreach (var sr in sensorReadings)
        {
            float totalEnergy = 0;
            for (int i = 1; i < sr.Value.Count; i++)
            {
                var prev = sr.Value[i - 1];
                var current = sr.Value[i];
                totalEnergy += (current.Reading + prev.Reading) / 2 * (current.Tick - prev.Tick) / 1e7f;
            }
            energy[sr.Key] = totalEnergy;
        }
        return energy;
    }
}
