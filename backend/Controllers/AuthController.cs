using Google.Apis.Auth;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using IntegrationWithGoogleCalendarAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using RestSharp;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using static Google.Apis.Requests.BatchRequest;

namespace IntegrationWithGoogleCalendarAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private static List<User> UserList = new List<User>();
        private IConfiguration _configuration;
        private readonly IHttpClientFactory _factory;

        public AuthController(IConfiguration configuration, IHttpClientFactory factory)
        {
            _configuration = configuration;
            _factory = factory;
        }

        [HttpPost("LoginWithGoogle")]
        public async Task<IActionResult> LoginWithGoogle([FromBody] string credential)
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings()
            {
                Audience = new List<string> { _configuration["google:client_id"] },
            };

            var payload = await GoogleJsonWebSignature.ValidateAsync(credential, settings);

            var user = UserList.Where(x => x.UserName == payload.Email).FirstOrDefault();

            if(user != null)
            {
                return Ok(JWTGenerator(user));
            }

            return BadRequest();
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] Register model)
        {
            var user = new User { UserName = model.UserName, BirthDay = model.BirthDay, Role = model.Role };
            using (HMACSHA256 hmac = new HMACSHA256())
            {
                user.PasswordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(model.Password));
            }

            UserList.Add(user);

            return Ok(user);
        }

        private dynamic JWTGenerator(User user)
        {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes("d53f4746334584121df7c6617e7b43cfd8e653155af70e0507277e3dc256de2b");

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new System.Security.Claims.ClaimsIdentity(new[] { new Claim("id", user.UserName), new Claim(ClaimTypes.Role, user.Role), new Claim(ClaimTypes.DateOfBirth, user.BirthDay) }),
                    Expires = DateTime.UtcNow.AddDays(7),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha512Signature)
                };
                var token = tokenHandler.CreateToken(tokenDescriptor);
                var encriptedToken = tokenHandler.WriteToken(token);

                return new { token = encriptedToken, username = user.UserName };
        }

        [HttpPost("exchange")]
        public async Task<IActionResult> ExchangeCodeForToken([FromBody] string authorizationCode)
        {
            var body = new StringContent(
                $"code={authorizationCode}&client_id={_configuration["google:client_id"]}&client_secret={_configuration["google:client_secret"]}&redirect_uri={_configuration["google:redirect_uri"]}&grant_type=authorization_code",
                Encoding.UTF8,
                "application/x-www-form-urlencoded");

            try
            {
                var _httpClient = _factory.CreateClient();
                var response = await _httpClient.PostAsync("https://oauth2.googleapis.com/token", body);
                if (!response.IsSuccessStatusCode)
                {
                    return BadRequest("Error exchanging code for token");
                }

                var content = await response.Content.ReadAsStringAsync();
                return Ok(content);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, $"Error exchanging code for token: {ex.Message}");
            }
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshAccessToken([FromBody] string refreshToken)
        {
            var body = new StringContent(
                $"client_id={_configuration["google:client_id"]}&client_secret={_configuration["google:client_secret"]}&refresh_token={refreshToken}&grant_type=refresh_token",
                Encoding.UTF8,
                "application/x-www-form-urlencoded");

            try
            {
                var _httpClient = _factory.CreateClient();
                var response = await _httpClient.PostAsync("https://oauth2.googleapis.com/token", body);
                if (!response.IsSuccessStatusCode)
                {
                    return BadRequest("Error refreshing token");
                }

                var content = await response.Content.ReadAsStringAsync();
                return Ok(content);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, $"Error refreshing token: {ex.Message}");
            }
        }

        [HttpPost("create-appointment")]
        public async Task<IActionResult> CreateEvent([FromBody] Consulta model)
        {
            var accessToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            string apiKey = _configuration["google:api_key"];
           
            string requestUri = $"https://www.googleapis.com/calendar/v3/calendars/primary/events";

            RestClient restClient = new RestClient(requestUri);
            RestRequest request = new RestRequest();
            var guid = Guid.NewGuid().ToString("N");

            request.AddQueryParameter("key", apiKey);
            request.AddHeader("Authorization", "Bearer " + accessToken);
            request.AddHeader("Accept", "application/json");
            request.AddHeader("Content-Type", "application/json");

            model.ConferenceData = new ConferenceData()
            {
                CreateRequest = new CreateConferenceRequest()
                {
                    RequestId = guid,
                    ConferenceSolutionKey = new ConferenceSolutionKey()
                    {
                        Type = "hangoutsMeet"
                    }
                }
            };

            request.AddJsonBody(model);

            var response = await restClient.PostAsync(request);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var createdEvent = JsonConvert.DeserializeObject<JObject>(response.Content);
                Console.WriteLine(createdEvent);
                var eventId = createdEvent["id"].ToString();
                requestUri = $"https://www.googleapis.com/calendar/v3/calendars/primary/events/{eventId}";
                request = new RestRequest(requestUri);
                request.AddQueryParameter("key", apiKey);
                request.AddQueryParameter("conferenceDataVersion", 1);
                request.AddHeader("Authorization", "Bearer " + accessToken);
                request.AddHeader("Accept", "application/json");

                response = await restClient.GetAsync(request);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var eventDetails = JsonConvert.DeserializeObject<JObject>(response.Content);
                    var hangoutLink = eventDetails["hangoutLink"]?.ToString();
                    var conferenceURI = eventDetails["conferenceData"]?["entryPoints"]?[0]["uri"]?.ToString();
                    if (hangoutLink != null)
                    {
                        return Ok(new { message = "Event created", meetLink = hangoutLink });
                    }
                    else
                    {
                        return BadRequest("Failed to retrieve Hangouts Meet link");
                    }
                }
                else
                {
                    return StatusCode((int)response.StatusCode);
                }
            }
            else
            {
                return StatusCode((int)response.StatusCode);
            }
        }
        
        [HttpGet("{id}")]
        public async Task<IActionResult> getEvent(string access_Token, string id)
        {
            var eventId = id;
            var accessToken = access_Token;
            string apiKey = _configuration["google:api_key"];

            string requestUri = $"https://www.googleapis.com/calendar/v3/calendars/primary/events/{eventId}";
            RestClient restClient = new RestClient(requestUri);
            RestRequest request = new RestRequest();
            request = new RestRequest(requestUri);
            request.AddQueryParameter("key", apiKey);
            request.AddQueryParameter("conferenceDataVersion", 1);
            request.AddHeader("Authorization", "Bearer " + accessToken);
            request.AddHeader("Accept", "application/json");

            var response = await restClient.GetAsync(request);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var eventDetails = JsonConvert.DeserializeObject<JObject>(response.Content);
                var hangoutLink = eventDetails["hangoutLink"]?.ToString();
                var conferenceURI = eventDetails["conferenceData"]?["entryPoints"]?[0]["uri"]?.ToString();
                if (hangoutLink != null)
                {
                    return Ok(new { message = "Event created", meetLink = hangoutLink });
                }
                else
                {
                    return BadRequest("Failed to retrieve Hangouts Meet link");
                }
            }
            else
            {
                return StatusCode((int)response.StatusCode);
            }
        }
    }
}