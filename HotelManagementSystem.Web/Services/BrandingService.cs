namespace HotelManagementSystem.Web.Services;

using HotelManagementSystem.Web.Models;

public class BrandingService
{
    private readonly ConfigurationService _configService;
    private HotelConfiguration? _dbConfig;

    public BrandingService(ConfigurationService configService)
    {
        _configService = configService;
    }

    public async Task InitializeAsync()
    {
        try
        {
            _dbConfig = await _configService.GetConfigurationAsync();
        }
        catch
        {
            // Fallback if DB is not available or other error
            _dbConfig = new HotelConfiguration();
        }
    }

    public string SystemName => !string.IsNullOrEmpty(_dbConfig?.NomeSistema)
        ? _dbConfig.NomeSistema
        : "Hotel Management System";

    public string PageTitle =>
        !string.IsNullOrEmpty(_dbConfig?.NomeSistema)
            ? _dbConfig.NomeSistema
            : "HotelSys"; // Using SystemName as PageTitle fallback

    public string LoginTitle =>
        !string.IsNullOrEmpty(_dbConfig?.LoginTitle) ? _dbConfig.LoginTitle : "Bem-vindo ao HotelSys";

    public string LogoUrl => !string.IsNullOrEmpty(_dbConfig?.LogoUrl) ? _dbConfig.LogoUrl : "/images/logo.png";
    public string FaviconUrl => "favicon.ico"; // Keep static for now or add to DB later
}
