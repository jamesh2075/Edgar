namespace Edgar.Razor
{
    /// <summary>
    /// Represents the project data retrieved from the API (i.e. Author, ASP.NET Core Version, Pipeline, etc)
    /// Initially, I did not intend to create multiple UI's to display data from the API
    /// I only intended to create one UI just to prove that the API worked.
    /// This class/record is only created to group the multiple API calls into one object.
    /// </summary>
    public record WebApiProjectSettings
    {
        /// <summary>
        /// The name of the author
        /// </summary>
        public string? Author { get; init; }
        /// <summary>
        /// My website
        /// </summary>
        public string? Bio { get; init; }
        /// <summary>
        /// The project's Wiki page
        /// </summary>
        public string? Wiki { get; init; }
        /// <summary>
        /// The code's repository
        /// </summary>
        public string? Repository { get; init; }
        /// <summary>
        /// The code's build pipeline
        /// </summary>
        public string? Pipeline { get; init; }
        public string? RawDataUrl { get; init; }
        public string? RequirementsUrl { get; init; }
        public string? SwaggerUrl { get; init; }
        /// <summary>
        /// The ASP.NET Core Version of the API (not this Razor app)
        /// </summary>
        public string? AspNetVersion { get; init; }
    }
}
