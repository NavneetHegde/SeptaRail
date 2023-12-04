namespace SeptaRail.ClientApp;
public static class Constants
{
    // URL of REST service (Android does not use localhost)
    public static string LocalhostUrl = DeviceInfo.Platform == DevicePlatform.Android ? "10.0.2.2" : "localhost";
    public static string Scheme = "https";
    public static string Port = "7143";
    public static string RestUrl = $"{Scheme}://{LocalhostUrl}:{Port}/api/NextThreeTrainFunction";
}
