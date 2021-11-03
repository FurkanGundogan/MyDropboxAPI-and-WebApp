using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WebTest.Models;
using WebTest.Services;

namespace WebTest.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {

            return View();
        }


        public async Task<ActionResult> Management()
        {

            
                
            var prevtkn = Session["mytoken"];
            if (prevtkn is null)
                return RedirectToAction("LoginAsync");

            using (HttpClient client = new HttpClient())
            {
                string apiUrl = ApiUrlControl.GetFileListUrl(Session["mytoken"].ToString());
                client.BaseAddress = new Uri(apiUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = await client.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    var files = Newtonsoft.Json.JsonConvert.DeserializeObject<List<FileModel>>(data);
                    return View(files);
                }


            }

            return View();
        }

        public async Task<ActionResult> LoginAsync()
        {
            var prevtkn = Session["mytoken"];
            if (!(prevtkn is null))
            {
                return Redirect("Management");
            }
            else
            {
                //login url, control sınıfıyla web config'den getiriliyor.
                string apiUrl = ApiUrlControl.GetLoginLinkUrl();

                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri(apiUrl);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/text"));

                    HttpResponseMessage response = await client.GetAsync(apiUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        var data = await response.Content.ReadAsStringAsync();
                        return Redirect(data);
                    }

                    
                }
            }
            return Redirect("Index");

        }

        public async Task<ActionResult> DoAuth(string state, string code)
        {
            
            string apiUrl = ApiUrlControl.GetAuthUrl(code);

            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(apiUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/text"));

                HttpResponseMessage response = await client.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();

                    Session["mytoken"] = data;

                    return RedirectToAction("Management");
                }


            }
            return View("Management");
        }

        public async Task<ActionResult> UploadFile(FileModel file)
        {


            var fs = file.filedata.InputStream;
            var br = new BinaryReader(fs);
            byte[] bytes = br.ReadBytes((Int32)fs.Length);
            FileModel fm = new FileModel();
            fm.filename = file.filedata.FileName;
            fm.filesize = bytes.Length.ToString();
            fm.filebytes = bytes;
            fm.filepath = file.filepath;

           
            string apiUrl = ApiUrlControl.GetUploadUrl(Session["mytoken"].ToString());


            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(apiUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                var seriobj = JsonConvert.SerializeObject(fm);
                var content = new StringContent(seriobj, System.Text.Encoding.UTF8, "application/json");
                var response = await client.PostAsync(apiUrl, content);
                if (response.IsSuccessStatusCode)
                {

                    return RedirectToAction("Management");
                }


                return RedirectToAction("Management");
            }

        }
        public ActionResult LogOut()
        {
            Session["mytoken"] = null;
            return RedirectToAction("Index");
        }


    }
}