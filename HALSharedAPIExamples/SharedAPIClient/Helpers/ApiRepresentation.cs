using WebApi.Hal;

namespace SharedAPIClient.Helpers
{
    /// <summary>
    /// The base class for deserializing response data from SharedAPI
    /// </summary>
    public class ApiRepresentation : Representation
    {
        protected override void CreateHypermedia()
        {
        }
    }
}