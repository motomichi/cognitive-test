using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using FacebookAPIModel;
using System.Configuration;
using System.Net.Http.Headers;
using Core.Azure;

namespace cognitive_test.Controllers
{
    /// <summary>
    /// Users not authorized will be redirected to the Facebook login page
    /// </summary>
    [Authorize]
    public class TestController : Controller
    {        
        #region const
        const string PROVIDER = "facebook";
        #endregion
        
        #region Test Index 【Facebookユーザ情報表示】
        [Authorize]
        public async System.Threading.Tasks.Task<ActionResult> Index()
        {   
                        
            //Get Access Token
            string accessToken = GetAccessToken(PROVIDER);            
            UserInfo userModel = new UserInfo();

            using (HttpClient client = new HttpClient())
            {
                //Facebook get fundamental information using【/me?～】
                using (HttpResponseMessage response = await client.GetAsync("https://graph.facebook.com/me" + "?access_token=" + accessToken))
                {
                    var o = JObject.Parse(await response.Content.ReadAsStringAsync());
                    userModel.name = o["name"].ToString();
                    userModel.id = o["id"].ToString();                    
                }
                //Facebook get profile image using 【/me/picture?～】
                using (HttpResponseMessage response = await client.GetAsync("https://graph.facebook.com/me" + "/picture?redirect=false&access_token=" + accessToken))
                {
                    var x = JObject.Parse(await response.Content.ReadAsStringAsync());
                    userModel.ImageUri = x["data"]["url"].ToString();
                }
            }
            
            //Set Params for view
            @ViewBag.userName = userModel.name;
            @ViewBag.ImageUri = userModel.ImageUri;
            @ViewBag.Title = "Facebook Information";
            @ViewBag.FBActive = "active";

            return View(userModel);
        }
        #endregion

        #region Test photo 【画像一覧取得(アルバム単位)】
        public async System.Threading.Tasks.Task<ActionResult> Photo()
        {
            //Get Access Token
            string accessToken = GetAccessToken(PROVIDER);

            //Prepare Model used in view
            var userModel = new UserInfo();            
            var viewModel = new List<Album>();

            //Create a StorageUtil Instance
            var storageUtilClient = new StorageUtil();

            using (HttpClient client = new HttpClient())
            {
                //Facebook get fundamental information using【/me?～】
                using (HttpResponseMessage responseUser = await client.GetAsync("https://graph.facebook.com/me" + "?access_token=" + accessToken))
                {
                    var userInfo = JObject.Parse(await responseUser.Content.ReadAsStringAsync());
                    userModel.name = userInfo["name"].ToString();
                    userModel.id = userInfo["id"].ToString();
                }

                //Facebook get albums using【/me/albums?～】
                using (HttpResponseMessage response = await client.GetAsync("https://graph.facebook.com/me/albums" + "?access_token=" + accessToken))
                {
                    var albums = JObject.Parse(await response.Content.ReadAsStringAsync());
                    System.Diagnostics.Trace.TraceInformation("album accessToken：" + accessToken);
                    System.Diagnostics.Trace.TraceInformation("album result：" + albums.ToString());
                    foreach (JToken album in albums["data"].Children())
                    {
                        if (!(album["name"].ToString().Equals("Profile Pictures")) && (!album["name"].ToString().Equals("Timeline Photos")))
                        {
                            var albumtemp = new Album() { albumId = album["id"].ToString(),albumName = album["name"].ToString(), photo = new List<Photo>() };

                            //Facebook get photos in each albums using【/[album_id]/photos?fields=images～】
                            using (HttpResponseMessage responsephoto = await client.GetAsync("https://graph.facebook.com/" + album["id"].ToString() + "/photos?fields=images&access_token=" + accessToken))
                            {
                                
                                var phpotoList = JObject.Parse(await responsephoto.Content.ReadAsStringAsync());
                                System.Diagnostics.Trace.TraceInformation(phpotoList.ToString());
                                foreach (JToken photo in phpotoList["data"].Children())
                                {
                                    var phototemp = new Photo { photoId = photo["id"].ToString(), source = new List<string>() };
                                    phototemp.source.Add(photo["images"].Children().First()["source"].ToString());

                                    albumtemp.photo.Add(phototemp);                                    

                                    //Upload photo into Azure blob
                                    storageUtilClient.UploadBlob(userModel.id, albumtemp.albumId, phototemp.photoId, photo["images"].Children().First()["source"].ToString());
                                }
                            }
                            viewModel.Add(albumtemp);
                        }
                    }                    
                }
            }

            //Set Params for view
            @ViewBag.userName = userModel.name;
            @ViewBag.FBActive = "active";

            return View(viewModel);
        }
        #endregion

        #region Test Analysis 【画像分析】
        public async System.Threading.Tasks.Task<ActionResult> Analysis()
        {
            //Get Access Token
            string accessToken = GetAccessToken(PROVIDER);
            
            //Prepare Model used in view
            var userModel = new UserInfo();
            var viewModel = new List<Analysis>();

            //Create a StorageUtil Class Instance
            var storageUtilClient = new StorageUtil();
            DocumentDBRepository.Initialize();

            //Facebook get fundamental information using【/me?～】
            using (HttpClient httpclient = new HttpClient())
            {                
                using (HttpResponseMessage responseUser = await httpclient.GetAsync("https://graph.facebook.com/me" + "?access_token=" + accessToken))
                {
                    var userInfo = JObject.Parse(await responseUser.Content.ReadAsStringAsync());
                    userModel.name = userInfo["name"].ToString();
                    userModel.id = userInfo["id"].ToString();
                }
            }

            //Get Images From Container
            var imageList = storageUtilClient.DownloadBlob(userModel.id);

            //Try to Cognitive Services
            foreach (KeyValuePair<string, string> i in imageList)
            {                
                //Get Album & Photo ID
                var albumId = i.Key.Split('/')[0];
                var photoId = i.Key.Split('/')[1];

                //Set Temp Model
                var temp = new Analysis() { albumId = albumId, source = i.Value, photoId = photoId, userId = userModel.id };

                var result = storageUtilClient.DownloadBlob(userModel.id, i.Key);
                var client = new HttpClient();
                var queryString = HttpUtility.ParseQueryString(string.Empty);

                // Set CognitiveKey to Request headers
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", ConfigurationManager.AppSettings["CognitiveKey"]);

                // Request parameters
                // ★visualFeaturesに欲しい解析内容を指定
                queryString["visualFeatures"] = "Categories,Tags,Adult,Description,ImageType,Color,Faces";
                queryString["details"] = "Celebrities";
                var uri = "https://api.projectoxford.ai/vision/v1.0/analyze?" + queryString;

                HttpResponseMessage response;

                using (var content = new ByteArrayContent(result))
                {
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                    //execute vision api
                    response = await client.PostAsync(uri, content);
                }

                temp.visionResult = await response.Content.ReadAsStringAsync();                
                viewModel.Add(temp);

                //insert results to document db
                await DocumentDBRepository.CreateItemAsync(JObject.Parse(temp.visionResult));
                
                //※↓↓In case of inserting to SQL DB
                //dataAccessClient.RegisterData(temp.userId, temp.photoId, temp.visionResult);

            }

            //Set Params for view
            @ViewBag.userName = userModel.name;
            @ViewBag.FBActive = "active";
            return View(viewModel);
        }
        #endregion

        #region private 【アクセストークン取得】
        /// <summary>
        /// アクセストークン取得
        /// </summary>
        /// <param name="provider">認証プロバイダー</param>
        /// <returns></returns>
        private string GetAccessToken(string provider)
        {            
            /* アクセストークン取得 */
            return Request.Headers.GetValues("X-MS-TOKEN-FACEBOOK-ACCESS-TOKEN").FirstOrDefault();
        }
        #endregion

    }

}