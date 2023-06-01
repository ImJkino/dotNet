using ExampleAPI.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ExampleAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {

        private string ReadJsonDataFromFile(string filePath)
        {
            using (StreamReader reader = new StreamReader(filePath))
            {
                return reader.ReadToEnd();
            }
            
        } 

        [HttpGet("GetAllUsers")]
        public UserList GetAllUsers()
        {            
            
            UserList users = JsonConvert.DeserializeObject<UserList>(ReadJsonDataFromFile("Data/userData.json"));
            return users;

        }

        [HttpGet("GetAllAppData")]
        public AppList GetAllAppData()
        {
            using (StreamReader reader = new StreamReader("Data/appData.json"))
            {
                string jsonData = reader.ReadToEnd();
                AppList apps = JsonConvert.DeserializeObject<AppList>(jsonData);
                return apps;
            }
        }

        [HttpGet("GetAppDataByUser/{userId}")]
        public List<App> GetAppDataByUser(int userId)
        {
            using (StreamReader reader = new StreamReader("Data/userAppXref.json"))
            {
                string jsonData = reader.ReadToEnd();
                var userAppXrefObject = JsonConvert.DeserializeObject<JObject>(jsonData);
                var userAppXrefList = userAppXrefObject["userAppXref"].ToObject<List<UserAppXref>>();

                using (StreamReader appDataReader = new StreamReader("Data/appData.json"))
                {
                    string appJsonData = appDataReader.ReadToEnd();
                    
                    var userApps = (from xref in userAppXrefList
                                    where xref.UserId == userId
                                    join app in GetAllAppData().Apps
                                        on xref.AppId equals app.Id
                                    select app).ToList();
                    return userApps;
                }

            }
        }

        [HttpGet("GetUserDataByApp/{appId}")]
        public List<User> GetUserDataByApp(int appId)
        {
            using (StreamReader reader = new StreamReader("Data/userAppXref.json"))
            {
                string jsonData = reader.ReadToEnd();
                var userAppXrefObject = JsonConvert.DeserializeObject<JObject> (jsonData);
                var userAppXrefList = userAppXrefObject["userAppXref"].ToObject<List<UserAppXref>>();

                using (StreamReader userDataReader = new StreamReader("Data/userData.json"))
                {
                    string userJsonData = userDataReader.ReadToEnd();
                    
                    var apps = (from xref in userAppXrefList
                                where xref.AppId == appId
                                    join user in GetAllUsers().Users
                                        on xref.UserId equals user.Id
                                    select user).ToList();
                    return apps;


                }
            }
        }

        [HttpGet("GetAllUserAppDataByUser")]
        public IActionResult GetAllUserAppDataByUser()
        {
            using (StreamReader userAppXrefReader = new StreamReader("Data/userAppXref.json"))
            {
                string userAppXrefJsonData = userAppXrefReader.ReadToEnd();
                var userAppXrefObject = JObject.Parse(userAppXrefJsonData);
                var userAppXrefList = userAppXrefObject["userAppXref"].ToObject<List<UserAppXref>>();

                using (StreamReader userDataReader = new StreamReader("Data/userData.json"))
                {
                    string userDataJsonData = userDataReader.ReadToEnd();
                    var userDataObject = JObject.Parse(userDataJsonData);
                    var userDataList = userDataObject["users"].ToObject<List<User>>();

                    using (StreamReader appDataReader = new StreamReader("Data/appData.json"))
                    {
                        string appDataJsonData = appDataReader.ReadToEnd();
                        var appDataObject = JObject.Parse(appDataJsonData);
                        var appDataList = appDataObject["apps"].ToObject<List<App>>();

                        var userAppDataByUser = (from xref in userAppXrefList
                                                 join user in userDataList on xref.UserId equals user.Id
                                                 join app in appDataList on xref.AppId equals app.Id
                                                 group app by user into userGroup
                                                 select new
                                                 {
                                                     User = userGroup.Key,
                                                     Apps = userGroup.ToList()
                                                 }).ToList();
                        return Ok(userAppDataByUser);


                    }
                }
            }
        }
    }
}
