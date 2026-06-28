namespace SeptaRail.ClientApp;
public static class Constants
{
    // URL of REST service (Android does not use localhost)
    public static string LocalhostUrl = DeviceInfo.Platform == DevicePlatform.Android ? "10.0.2.2" : "localhost";
    public static string Scheme = "https";
    public static string Port = "7143";
    public static string RestUrl = $"{Scheme}://{LocalhostUrl}:{Port}/api/NextThreeTrainFunction";
    public static string ProdRestUrl = "https://gettrainfunction.azurewebsites.net/api/NextThreeTrainFunction?code=SMbQgIYzKjbnf7pkqZxAxwGzJizD-pvgIu0XF7PEk9KsAzFu7zNbow%3D%3D";
}
