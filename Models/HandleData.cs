namespace nsquare.Models;

public class HandleData
{
    public double Azimuth { get; set; }
    public double Elevation { get; set; }
    public bool IsFiring { get; set; }
    public double Voltage { get; set; }

    public HandleData(double azimuth, double elevation, bool isFiring, double voltage)
    {
        Azimuth = azimuth;
        Elevation = elevation;
        IsFiring = isFiring;
        Voltage = voltage;
    }
}
