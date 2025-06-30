using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class Reactor : MonoBehaviour
{
    // Reaction 
    public int IdleNeutrons = 100000000; //Simulates random nuclear decay from reactor (1000)
    public float NumberOfNeutrons; //Current reactor power
    private float oldNumberOfNeutrons; //Previous reactor power
    private const long MaxNeutrons = 100000000000; //Maximum reactor power, this value dictates how much neutrons are needed to reach 100% power
    public float ReactorPower;
    public double ReactorPeriod;
    public float RodsPulled; // Set by user
    private const float FuelRemaining = 0.95f; // Amount in percent(%) of reactive fuel left in reactor
    private bool IPR_Status;
    private int IPR_Level = 1;

    // Radioactive decay
    private float iodine;
    public float xenon;
    private float iodineDecay;
    private const float decayScaleFactor = 0.00025f; // How much iodine decays from reactor; 1 = All neutron reaction decays to iodine

    // Water and temperature of system
    public float WaterAmount;
    public float SteamAmount;
    public float WaterDensity = 1000;
    public float WaterTemperature = 30;
    public float TemperatureSurplus;
    public float SaturationTemperature;
    private const float BoilingCoefficient = 2; // tune this
    public float AmountBoiled;
    public float VoidCoefficient;

    // Cooling system
    private float PumpFlow;
    private const float MixingParameter = 0.1f; // Tune this
    private float FeedwaterTemperature;
    private const int minPressure = 1;
    private const int maxPressure = 10000;
    public float Pressure;
    private float SteamPressureCoefficient;
    private const int HeatPerPower = 10; // Degrees per timestep for 100% reactor power

    

    private const float TimeStep = 1f; // How fast reactor calculations are updated (do NOT put lower than 1 second, causes lag)
    public float reactorScaleFactors;

    // Script referencing
    // public DialCode RodsCtrlDial;
    // public DialCode AutoSetpointDial;
    // public Recirculation Recirculation;
    // public MCC MCC;

    // GameObject Signs to display information
    [Space(10),Header("Reactor")]
    public TextMeshProUGUI ReactorPowerSign;
    public TextMeshProUGUI ReactorPeriodSign;
    public TextMeshProUGUI FuelRemainingSign;
    public TextMeshProUGUI WaterTemperatureSign;
    public TextMeshProUGUI SteamOutputSign;
    public TextMeshProUGUI SourceRangeMonitorSign;
    public TextMeshProUGUI InterRangeMonitorSign;
    public TextMeshProUGUI InterRangeLevelSign;
    public TextMeshProUGUI IodineSign;
    public TextMeshProUGUI XenonSign;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        StartCoroutine(ReactorUpdateLoop(TimeStep));
        FuelRemainingSign.text = FuelRemaining.ToString("0.00" + "%");
    }

    public void UpdateIPR_Status()
    {
        IPR_Status = true;
    }

    public void UpdateIPR_Level(bool status)
    {
        if (!IPR_Status) return;
        IPR_Status = false;
        if (status)
        {
            IPR_Level += 1;
        }
        else
        {
            IPR_Level -= 1;
        }
        IPR_Level = Mathf.Clamp(IPR_Level, 1, 6);
    }


    // Coroutines
    private IEnumerator ReactorUpdateLoop(float timestep)
    {
        while (true)
        {
            UpdateReactor();
            yield return new WaitForSeconds(timestep); // Wait for 1 second before running again
        }
        // ReSharper disable once IteratorNeverReturns
    }

    


    // Preload functions
    private static float CalculateBoilingPoint(float pressure) // Pressure in Pa
    {
        //Limit pressure to prevent insane numbers
        var newPressure = Math.Clamp(pressure, minPressure, maxPressure); 
    
        var temperature = (49.161 * Math.Log(newPressure)) + 44.932;

        return (float)temperature; // Temperature in degrees Celsius
    }
    private static float CalculateWaterDensity(float temperature)
    {
        // Clamp temperature between 0°C and 300°C to stay within valid range
        temperature = Math.Clamp(temperature, 0, 300);

        // Use the high-precision polynomial equation
        var value = 999.83952 
                    + (16.945176 * temperature) 
                    - (7.9870401e-3 * Math.Pow(temperature, 2)) 
                    - (46.170461e-6 * Math.Pow(temperature, 3)) 
                    + (105.56302e-9 * Math.Pow(temperature, 4)) 
                    - (280.54253e-12 * Math.Pow(temperature, 5));
        return (float)value;
    }

    // Input from game using functions


    // Coroutine update loops
    // ReSharper disable Unity.PerformanceAnalysis
    private void UpdateReactor()
    {
        // Reactor reactivity calculation
        RodsPulled = RodsCtrlDial.Value;
        NumberOfNeutrons = reactorScaleFactors * (oldNumberOfNeutrons + IdleNeutrons);
        ReactorPower = NumberOfNeutrons / MaxNeutrons;
        VoidCoefficient = 1 - 0.05f * Mathf.Lerp(ReactorPower, 0,(Recirculation.Valve1 + Recirculation.Valve2) / 2);
        
        // Calculate Reactor Period
        if (NumberOfNeutrons > 0 && oldNumberOfNeutrons > 0)
        {
            ReactorPeriod = 1 / Math.Log(NumberOfNeutrons / oldNumberOfNeutrons) / TimeStep;
        }

        // Reactor decay byproduct calculation
        iodine += NumberOfNeutrons / MaxNeutrons * decayScaleFactor;
        iodineDecay = iodine * decayScaleFactor;
        iodine -= iodineDecay;
        xenon += iodineDecay;
        xenon -= (float)(2.0 / 3.0 * xenon * decayScaleFactor * TimeStep);
        xenon -= xenon * NumberOfNeutrons / MaxNeutrons * decayScaleFactor;
        
        // the factor of all the combined variables that could affect core reactivity
        reactorScaleFactors = (float)(2 * (0.2 + RodsPulled * 0.8 / 100) * 
                                      (WaterDensity / 1000.0 ) * 
                                      FuelRemaining * 
                                      ( 1 - xenon / 4.5 ) * 
                                      VoidCoefficient); 
        
        // Update neutrons for next timestep
        oldNumberOfNeutrons = NumberOfNeutrons;
        
        // Reactor water boiling simulation
        WaterAmount = 1000000;
        if (WaterTemperature > SaturationTemperature) 
        {
            TemperatureSurplus = WaterTemperature - SaturationTemperature;
            AmountBoiled = TemperatureSurplus * BoilingCoefficient;
            // Adjust water and steam amounts
            WaterTemperature = SaturationTemperature;
            WaterAmount -= AmountBoiled;
            SteamAmount += AmountBoiled;
            // Update pressure
            Pressure = SteamAmount * SteamPressureCoefficient * (WaterTemperature + 273) / 373;
            // Update saturation temperature for the next timestep
            SaturationTemperature = CalculateBoilingPoint(Pressure);
        }
        
        // Reactor water temp 
        WaterDensity = CalculateWaterDensity(WaterTemperature);
        WaterTemperature += NumberOfNeutrons / MaxNeutrons * HeatPerPower;
        WaterTemperature = (WaterTemperature + MCC.DeaeratorOutflow * FeedwaterTemperature * MixingParameter) / (1 + PumpFlow * MixingParameter);
        WaterAmount += MCC.DeaeratorOutflow;
    
        // Update display signs
        ReactorPowerSign.text = ReactorPower.ToString("0.0000");
        ReactorPeriodSign.text = ReactorPeriod is <= 9999f and >= 0.001f ? ReactorPeriod.ToString("0.000") : "Inf";
        WaterTemperatureSign.text = WaterTemperature.ToString("0") + "\u00b0C";
        SteamOutputSign.text = AmountBoiled.ToString("0");
        SourceRangeMonitorSign.text = NumberOfNeutrons < 1000000 ? NumberOfNeutrons.ToString("e1"): "Saturated";
        InterRangeLevelSign.text = IPR_Level.ToString();
        InterRangeMonitorSign.text = Mathf.Clamp((float)(NumberOfNeutrons/Math.Pow(50,IPR_Level)),0,100).ToString("0.00") + "%";
        IodineSign.text = (iodine / MaxNeutrons).ToString("0.00") + "%";
        XenonSign.text = (xenon / MaxNeutrons).ToString("0.00") + "%";
        
    }

    
    // Update is called once per frame
    private void Update()
    {
        
    }
}

