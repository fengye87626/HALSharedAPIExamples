using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RestSharp;
using SharedAPIClient.Helpers;
using SharedAPIClient.Models;
using RestClient = SharedAPIClient.Helpers.RestClient;

namespace SharedAPIClient.UnitTests
{
    [TestClass]
    public class SharedAPIClientSample
    {
        // Authenticate a shared API user
        // GET a Vehicle from the API(using a hardcoded GUID)
        // Generate an input object (used in a subsequent PUT request)
        // Modify the vehicle's name in that input object
        // PUT the input object back to the server
        [TestMethod]
        public void Sample_Authenticate_GetMetadata_FormUrlBasedOnMetadata_GetVehicle_UpdateVehicle()
        {
            var userName = ConfigurationManager.AppSettings[Constants.UserNameConfigurationKey];
            var testPassword = ConfigurationManager.AppSettings[Constants.TestPasswordConfigurationKey];

            var clientId = ConfigurationManager.AppSettings[Constants.AuthSecretPairConfigurationKey];
            var secretPair = ConfigurationManager.AppSettings[Constants.AuthClientIdConfigurationKey];


            var baseUrl = ConfigurationManager.AppSettings[Constants.BaseSharedApiUrlConfigurationKey];
            var vehicleGuid = ConfigurationManager.AppSettings[Constants.TestVehicleGuidConfigurationKey];
            var rootApiUrl = ConfigurationManager.AppSettings[Constants.RootApiUrlConfigurationKey];
            var requestClient = new RestClient(baseUrl);
            Task<Token> token = requestClient.GetAccessToken(rootApiUrl, clientId, secretPair, userName, testPassword);

            Assert.IsNotNull(token.Result);
            Assert.IsTrue(!string.IsNullOrEmpty(token.Result.AccessToken));

            //GetHypermediaLinks
            var hypermediaLinksResult = requestClient.MakeRequest<ApiRepresentation>(rootApiUrl,
                Method.GET,
                token.Result.AccessToken, null, null, null);
            Assert.IsNotNull(hypermediaLinksResult);
            Assert.IsNotNull(hypermediaLinksResult.Result);
            Assert.IsTrue(hypermediaLinksResult.Result.Links != null && hypermediaLinksResult.Result.Links.Count != 0);


            //form url for get vehicle by guid
            var links = hypermediaLinksResult.Result.Links;
            var vehicleListTemplate = links.FirstOrDefault(l => System.String.Compare(l.Rel.ToLower(), "vehicle-list", System.StringComparison.Ordinal) == 0);
            Assert.IsTrue(vehicleListTemplate!=null &&  !string.IsNullOrWhiteSpace(vehicleListTemplate.Href));

            //Get vehicle info by guid
            var vehicleResult = requestClient.MakeRequest<VehicleRepresentation>(string.Concat(vehicleListTemplate.Href, "/{id}"),
                Method.GET, token.Result.AccessToken, null, new[] {new UrlParameter {Name = "id", Value = vehicleGuid}}, null);
            
            Assert.IsNotNull(vehicleResult.Result);
            VehicleRepresentation vehicle = vehicleResult.Result;
            
            //form url to update changed vehicle
            var selfLink = vehicle.Href;
            Assert.IsTrue(!string.IsNullOrWhiteSpace(selfLink));

            //perform changes in vehicle
            VehicleData inputVehicleData = new VehicleData()
            {
                Model = vehicle.Model,
                Vin = vehicle.Vin,
                Make = vehicle.Make,
                Year = vehicle.Year.HasValue ? vehicle.Year.Value : 0,
                Nickname = string.IsNullOrWhiteSpace(vehicle.Nickname)? "UNKNOWN" : vehicle.Nickname,
                Description = string.IsNullOrWhiteSpace(vehicle.Description) ? "UNKNOWN" : vehicle.Description,
            };
            //update vehicle
            var vehicleUpdateResult = requestClient.MakeRequest<VehicleRepresentation>(selfLink, Method.PUT, token.Result.AccessToken, null, null, null, true, inputVehicleData);

            Assert.IsNotNull(vehicleUpdateResult.Result);
            Assert.IsFalse(vehicleUpdateResult.Result.Id.Equals(Guid.Empty));
        }

    }
}
